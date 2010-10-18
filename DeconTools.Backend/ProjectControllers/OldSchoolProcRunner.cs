using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Data;
using DeconTools.Backend.Runs;
using System.IO;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using System.ComponentModel;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;

namespace DeconTools.Backend
{
    public class OldSchoolProcRunner : ProjectController
    {

        #region Member Variables

        private string inputDataFilename;
        private string paramFilename;
        private string outputFilepath;
        private Run m_run;

        private DeconTools.Backend.Globals.MSFileType fileType;
        private BackgroundWorker backgroundWorker;

        #endregion

        #region Properties
        private Project project;

        public Project Project
        {
            get { return project; }

        }


        private int isosResultThreshold;

        public int IsosResultThreshold
        {
            get { return isosResultThreshold; }
            set { isosResultThreshold = value; }
        }

        private Globals.ExporterType exporterType;

        public Globals.ExporterType ExporterType
        {
            get { return exporterType; }
            set { exporterType = value; }
        }


        #endregion

        #region Constructors

        public OldSchoolProcRunner(string inputDataFilename, DeconTools.Backend.Globals.MSFileType fileType, string paramFileName)
            : this(inputDataFilename, null, fileType, paramFileName)
        {

        }


        public OldSchoolProcRunner(string inputDataFilename, DeconTools.Backend.Globals.MSFileType fileType, string paramFileName, BackgroundWorker backgroundWorker)
            : this(inputDataFilename, fileType, paramFileName)
        {
            this.backgroundWorker = backgroundWorker;

        }

        public OldSchoolProcRunner(string inputDataFilename, string outputPath, DeconTools.Backend.Globals.MSFileType fileType, string paramFileName)
        {

            // Check.Require(validateFileExistance(inputDataFilename),"Could not process anything. Inputfile does not exist or is inaccessible");
            // Check.Require(validateFileExistance(paramFileName), "Could not process anything. Parameter file does not exist or is inaccessible");




            Project.Reset();
            project = Project.getInstance();
            this.inputDataFilename = inputDataFilename;
            this.fileType = fileType;
            this.paramFilename = paramFileName;
            this.project = Project.getInstance();
            Project.getInstance().LoadOldDecon2LSParameters(this.paramFilename);
            this.IsosResultThreshold = 25000;       // results will be serialized if count is greater than this number
            this.exporterType = getExporterTypeFromOldParameters(Project.getInstance().Parameters.OldDecon2LSParameters);

            RunFactory runfactory = new RunFactory();



            //Create run
            m_run = runfactory.CreateRun(fileType, this.inputDataFilename, Project.getInstance().Parameters.OldDecon2LSParameters);
            //Project.getInstance().RunCollection.Add(m_run);

            Check.Assert(m_run != null, "Processing aborted. Could not handle supplied File(s)");
            //Define ScansetCollection

            if (outputFilepath == null || outputFilepath.Length == 0 || outputFilepath == String.Empty)
            {
                this.outputFilepath = m_run.DataSetPath;
            }
            else
            {
                this.outputFilepath = outputPath;
            }




            Logger.Instance.OutputFilename = m_run.DataSetPath + "\\" + m_run.DatasetName + "_log.txt";
            Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(OldSchoolProcRunner)));
            Logger.Instance.AddEntry("ParameterFile = " + Path.GetFileName(this.paramFilename));
            Logger.Instance.AddEntry("DeconEngine version = " + AssemblyInfoRetriever.GetVersion(typeof(DeconToolsV2.HornTransform.clsHornTransformParameters)));
            Logger.Instance.AddEntry("RapidEngine version = " + RapidDeconvolutor.getRapidVersion());
            Logger.Instance.AddEntry("UIMFLibrary version = " + UIMFLibraryAdapter.getLibraryVersion(), Logger.Instance.OutputFilename);   //forces it to write out immediately and clear buffer


            if (m_run is UIMFRun)     // not pretty...  
            {
                UIMFRun uimfRun = (UIMFRun)m_run;
                FrameSetCollectionCreator frameSetcreator = new FrameSetCollectionCreator(m_run, uimfRun.MinFrame,
                      uimfRun.MaxFrame, Project.getInstance().Parameters.NumFramesSummed, 1);
                frameSetcreator.Create();

                uimfRun.GetFrameDataAllFrameSets();     //this adds avgTOFlength and framePressureBack to each frame's object data; I do this so it doesn't have to be repeated looked up.

            }

            try
            {
                ScanSetCollectionCreator scanSetCollectionCreator = new ScanSetCollectionCreator(m_run, m_run.MinScan, m_run.MaxScan,
                    Project.getInstance().Parameters.NumScansSummed,
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance,
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.ProcessMSMS);
                scanSetCollectionCreator.Create();
            }
            catch (Exception ex)
            {

                Logger.Instance.AddEntry("ERROR: " + ex.Message, Logger.Instance.OutputFilename);   //forces it to write out immediately and clear buffer
                throw ex;
            }



            //Create Tasks and add to task collection...

            MSGeneratorFactory msGeneratorFactory = new MSGeneratorFactory();
            Task msGen = msGeneratorFactory.CreateMSGenerator(fileType, Project.getInstance().Parameters.OldDecon2LSParameters);
            Project.getInstance().TaskCollection.TaskList.Add(msGen);

            if (Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.ZeroFill)
            {
                Task zeroFiller = new DeconToolsZeroFiller(Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumZerosToFill);
                Project.getInstance().TaskCollection.TaskList.Add(zeroFiller);
            }
            if (Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.UseSavitzkyGolaySmooth)
            {
                Task smoother = new DeconToolsSavitzkyGolaySmoother(
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.SGNumLeft,
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.SGNumRight,
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.SGOrder);
                Project.getInstance().TaskCollection.TaskList.Add(smoother);
            }

            Task peakDetector = new DeconToolsPeakDetector(Project.getInstance().Parameters.OldDecon2LSParameters.PeakProcessorParameters);
            Project.getInstance().TaskCollection.TaskList.Add(peakDetector);

            if (Project.getInstance().Parameters.OldDecon2LSParameters.PeakProcessorParameters.WritePeaksToTextFile == true)
            {
                DeconTools.Backend.ProcessingTasks.ResultExporters.PeakListExporters.PeakListExporterFactory peakexporterFactory = new DeconTools.Backend.ProcessingTasks.ResultExporters.PeakListExporters.PeakListExporterFactory();
                Task peakListTextExporter = peakexporterFactory.Create(this.exporterType, this.fileType, 10000, getPeakListFileName(this.exporterType));
                Project.getInstance().TaskCollection.TaskList.Add(peakListTextExporter);
            }

            DeconvolutorFactory deconFactory = new DeconvolutorFactory();
            Task deconvolutor = deconFactory.CreateDeconvolutor(Project.getInstance().Parameters.OldDecon2LSParameters);
            Project.getInstance().TaskCollection.TaskList.Add(deconvolutor);
            Logger.Instance.AddEntry("Deconvolution_Algorithm = " + Project.getInstance().TaskCollection.GetDeconvolutorType(), Logger.Instance.OutputFilename);

            Task resultFlagger = new DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidatorTask();
            Project.getInstance().TaskCollection.TaskList.Add(resultFlagger);


            if (Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.ReplaceRAPIDScoreWithHornFitScore == true)
            {
                Task fitscoreCalculator = new DeconToolsFitScoreCalculator();
                Project.getInstance().TaskCollection.TaskList.Add(fitscoreCalculator);
            }

            if (m_run is UIMFRun)     // not pretty...  but have to do this since the drift time extractor is a specialized task
            {
                Task driftTimeExtractor = new UIMFDriftTimeExtractor();
                Project.getInstance().TaskCollection.TaskList.Add(driftTimeExtractor);

                Task uimfTICExtractor = new UIMF_TICExtractor();
                Project.getInstance().TaskCollection.TaskList.Add(uimfTICExtractor);
            }

            if (m_run is UIMFRun || m_run is IMFRun)
            {
                Task originalIntensitiesExtractor = new OriginalIntensitiesExtractor();
                Project.getInstance().TaskCollection.TaskList.Add(originalIntensitiesExtractor);
            }

            Task scanResultUpdater = new ScanResultUpdater();
            Project.getInstance().TaskCollection.TaskList.Add(scanResultUpdater);

            Task isosResultExporter = new IsosExporterFactory(this.IsosResultThreshold).CreateIsosExporter(fileType, this.ExporterType, setIsosOutputFileName(exporterType));
            Project.getInstance().TaskCollection.TaskList.Add(isosResultExporter);

            Task scanResultExporter = new DeconTools.Backend.Data.ScansExporterFactory().CreateScansExporter(fileType, this.ExporterType, setScansOutputFileName(exporterType));
            Project.getInstance().TaskCollection.TaskList.Add(scanResultExporter);
        }

        private string createOutputPath(Run m_run)
        {
            return m_run.DataSetPath;
        }



        #endregion

        #region Public Methods

        public override void Execute()
        {
            Logger.Instance.AddEntry("Started file processing", Logger.Instance.OutputFilename);  //this will write out immediately

            TaskControllerFactory taskControllerFactory = new TaskControllerFactory(this.backgroundWorker);
            TaskController controller = taskControllerFactory.CreateTaskController(fileType, Project.getInstance().TaskCollection);
            controller.IsosResultThresholdNum = this.IsosResultThreshold;


            using (Run run = m_run)
            {

                try
                {
                    controller.Execute(run);

                }
                catch (Exception ex)
                {
                    Console.Write(ex.StackTrace);
                    throw ex;   //throw the error again and let something else catch it. 
                }
                Logger.Instance.AddEntry("Finished file processing", Logger.Instance.OutputFilename);


                TimeSpan overallProcessingTime = Logger.Instance.GetTimeDifference("Started file processing", "Finished file processing");
                string formattedOverallprocessingTime = string.Format("{0:00}:{1:00}:{2:00}", overallProcessingTime.TotalHours, overallProcessingTime.Minutes, overallProcessingTime.Seconds);

                Logger.Instance.AddEntry("total processing time = " + formattedOverallprocessingTime);

                ProjectFacade pf = new ProjectFacade();

                Logger.Instance.AddEntry("total features = " + m_run.ResultCollection.MSFeatureCounter);

                Logger.Instance.WriteToFile(m_run.Filename + "_log.txt", true);
                Logger.Instance.Close();

                TaskCleaner taskCleaner = new TaskCleaner(Project.getInstance().TaskCollection);
                taskCleaner.CleanTasks();

                Project.Reset();      //sets Project singleton to null;

            }
          
        }
        #endregion

        #region Private Methods

        private string getPeakListFileName(Globals.ExporterType exporterType)
        {
            string baseFileName = m_run.DataSetPath + "\\" + m_run.DatasetName;

            //string baseFileName = m_run.Filename.Substring(0, m_run.Filename.LastIndexOf('.'));
            switch (exporterType)
            {
                case Globals.ExporterType.TEXT:
                    return baseFileName += "_peaks.txt";
                case Globals.ExporterType.SQLite:
                    return baseFileName += "_peaks.sqlite";
                default:
                    return baseFileName += "_peaks.txt";
            }


        }




        private string setScansOutputFileName(Globals.ExporterType exporterType)
        {
            string baseFileName = m_run.DataSetPath + "\\" + m_run.DatasetName;

            // string baseFileName = m_run.Filename.Substring(0, m_run.Filename.LastIndexOf('.'));

            switch (exporterType)
            {
                case Globals.ExporterType.TEXT:
                    return baseFileName += "_scans.csv";
                    break;
                case Globals.ExporterType.SQLite:
                    return baseFileName += "_scans.sqlite";
                    break;
                default:
                    return baseFileName += "_scans.csv";
                    break;
            }
        }

        private string setIsosOutputFileName(Globals.ExporterType exporterType)
        {
            string baseFileName = getBaseFileName(m_run);

            //string baseFileName = m_run.Filename.Substring(0, m_run.Filename.LastIndexOf('.'));

            switch (exporterType)
            {
                case Globals.ExporterType.TEXT:
                    return baseFileName += "_isos.csv";
                    break;
                case Globals.ExporterType.SQLite:
                    return baseFileName += "_isos.sqlite";
                    break;
                default:
                    return baseFileName += "_isos.csv";
                    break;
            }


        }

        private string getBaseFileName(Run run)
        {
            //outputFilePath will be null if outputFilePath wasn't set using a constructor
            //So if null, will create the default outputPath
            if (outputFilepath == null)
            {
                return run.DataSetPath + "\\" + run.DatasetName;
            }
            else
            {
                return outputFilepath.TrimEnd(new char[] { '\\' }) + "\\" + run.DatasetName;
            }
        }



        private string setScansOutputFileName()
        {
            string baseFileName = Project.getInstance().RunCollection[0].Filename.Substring(0, Project.getInstance().RunCollection[0].Filename.LastIndexOf('.'));
            return baseFileName += "_scans.csv";
        }

        private string setIsosOutputFileName()
        {
            string baseFileName = Project.getInstance().RunCollection[0].Filename.Substring(0, Project.getInstance().RunCollection[0].Filename.LastIndexOf('.'));
            return baseFileName += "_isos.csv";
        }
        private bool validateFileExistance(string filename)
        {
            try
            {
                if (!File.Exists(inputDataFilename)) return false;

            }
            catch (Exception)
            {

                return false;
            }

            return true;
        }
        #endregion

    }
}
