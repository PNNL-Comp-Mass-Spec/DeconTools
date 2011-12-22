using System;
using System.ComponentModel;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.ResultExporters.PeakListExporters;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

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
            this.IsosResultThreshold = 5000;
            this.exporterType = getExporterTypeFromOldParameters(Project.getInstance().Parameters.OldDecon2LSParameters);

            RunFactory runfactory = new RunFactory();



            //Create run
            m_run = runfactory.CreateRun(fileType, this.inputDataFilename, Project.getInstance().Parameters.OldDecon2LSParameters);
            //Project.getInstance().RunCollection.Add(m_run);


            m_run.ResultCollection.ResultType = GetResultType(m_run, Project.getInstance().Parameters.OldDecon2LSParameters);


            Check.Assert(m_run != null, "Processing aborted. Could not handle supplied File(s)");
            //Define ScansetCollection

            if (string.IsNullOrEmpty(outputFilepath))
            {
                this.outputFilepath = m_run.DataSetPath;
            }
            else
            {
                this.outputFilepath = outputPath;
            }

            Logger.Instance.OutputFilename = m_run.DataSetPath + "\\" + m_run.DatasetName + "_log.txt";

            try
            {
                if (File.Exists(Logger.Instance.OutputFilename))
                {
                    File.Delete(Logger.Instance.OutputFilename);
                }
            }
            catch (Exception)
            {

                throw;
            }



            Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(OldSchoolProcRunner)));
            Logger.Instance.AddEntry("ParameterFile = " + Path.GetFileName(this.paramFilename));
            Logger.Instance.AddEntry("DeconEngine version = " + AssemblyInfoRetriever.GetVersion(typeof(DeconToolsV2.HornTransform.clsHornTransformParameters)));
            Logger.Instance.AddEntry("RapidEngine version = " + RapidDeconvolutor.getRapidVersion());
            Logger.Instance.AddEntry("UIMFLibrary version = " + DeconTools.Backend.Utilities.AssemblyInfoRetriever.GetVersion(typeof(UIMFLibrary.DataReader)), Logger.Instance.OutputFilename);   //forces it to write out immediately and clear buffer


            //Define the Frames and Scans to analyze.
            //This defines the FrameSetCollection (for IMS data) and ScanSetCollection
            createTargetMassSpectra(m_run);


            //this is not in the right place... but must be done after the FrameSetCollection is created.   
            if (m_run is UIMFRun)
            {
                ((UIMFRun)m_run).GetFrameDataAllFrameSets();     //this adds avgTOFlength and framePressureBack to each frame's object data; I do this so it doesn't have to be repeated looked up.
                ((UIMFRun)m_run).SmoothFramePressuresInFrameSets();
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

            Task deconvolutor = DeconvolutorFactory.CreateDeconvolutor(Project.getInstance().Parameters.OldDecon2LSParameters);
            Project.getInstance().TaskCollection.TaskList.Add(deconvolutor);
            Logger.Instance.AddEntry("Deconvolution_Algorithm = " + Project.getInstance().TaskCollection.GetDeconvolutorType(), Logger.Instance.OutputFilename);

            //for exporting peaks.  Also add a task for associating peaks with MSFeatures 
            if (Project.getInstance().Parameters.OldDecon2LSParameters.PeakProcessorParameters.WritePeaksToTextFile == true)
            {
                Task peakListTextExporter = PeakListExporterFactory.Create(this.exporterType, this.fileType, 10000, getPeakListFileName(this.exporterType));
                Project.getInstance().TaskCollection.TaskList.Add(peakListTextExporter);

                Task peakToMSFeatureAssociator = new PeakToMSFeatureAssociator();
                Project.getInstance().TaskCollection.TaskList.Add(peakToMSFeatureAssociator);


            }



            Task resultFlagger = new DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidatorTask();
            Project.getInstance().TaskCollection.TaskList.Add(resultFlagger);


            if (m_run.ResultCollection.ResultType == Globals.ResultType.O16O18_TRADITIONAL_RESULT)
            {

                Task o16o18PeakDataAppender = new O16O18PeakDataAppender();
                Project.getInstance().TaskCollection.TaskList.Add(o16o18PeakDataAppender);
            }


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
                Task saturationDetector = new SaturationDetector();
                Project.getInstance().TaskCollection.TaskList.Add(saturationDetector);
            }

            Task scanResultUpdater = new ScanResultUpdater(Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.ProcessMSMS);
            Project.getInstance().TaskCollection.TaskList.Add(scanResultUpdater);

            Task isosResultExporter = IsosExporterFactory.CreateIsosExporter(m_run.ResultCollection.ResultType, this.ExporterType, setIsosOutputFileName(exporterType));
            Project.getInstance().TaskCollection.TaskList.Add(isosResultExporter);

            Task scanResultExporter = ScansExporterFactory.CreateScansExporter(fileType, this.ExporterType, setScansOutputFileName(exporterType));
            Project.getInstance().TaskCollection.TaskList.Add(scanResultExporter);


        }



        private void createTargetMassSpectra(Run m_run)
        {
            try
            {
                if (m_run is UIMFRun)
                {

                    //TODO: update this so that FrameNum is changed to Frame_index

                    UIMFRun uimfRun = (UIMFRun)m_run;

                    bool sumAcrossLCFrames = Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.SumSpectraAcrossFrameRange;
                    if (sumAcrossLCFrames)
                    {

                        uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, uimfRun.MinFrame,
                                                                               uimfRun.MaxFrame,
                                                                               Project.getInstance().Parameters.
                                                                                   NumFramesSummed, 1);

                    }
                    else
                    {
                        int numSummed = 1;   // this means we will NOT sum across LC Frames
                        uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, uimfRun.MinFrame,
                                                                               uimfRun.MaxFrame, numSummed, 1);
                    }



                    bool sumAllIMSScansInAFrame = (Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.SumSpectra == true);
                    if (sumAllIMSScansInAFrame)
                    {
                        int centerScan = (m_run.MinScan + m_run.MaxScan + 1) / 2;

                        int numSummed = m_run.MaxScan - m_run.MinScan + 1;
                        if (numSummed % 2 != 0)
                        {
                            numSummed++;
                        }

                        uimfRun.ScanSetCollection.ScanSetList.Clear();
                        ScanSet scanset = new ScanSet(centerScan, m_run.MinScan, m_run.MaxScan);
                        uimfRun.ScanSetCollection.ScanSetList.Add(scanset);
                    }
                    else
                    {

                        bool sumAcrossIMSScans = Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.SumSpectraAcrossScanRange;

                       
                        if (sumAcrossIMSScans)
                        {
                            int numIMSScanToSum = Project.getInstance().Parameters.NumScansSummed;

                            m_run.ScanSetCollection=ScanSetCollection.Create(m_run, m_run.MinScan, m_run.MaxScan, numIMSScanToSum,
                            Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance,
                            Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.ProcessMSMS);

                        }
                        else
                        {
                            int numIMSScanToSum = 1;      // this means there is no summing

                            m_run.ScanSetCollection=ScanSetCollection.Create(m_run, m_run.MinScan, m_run.MaxScan, numIMSScanToSum,
                            Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance,
                            Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.ProcessMSMS);


                        }
                        

                    }



                }
                else     //not a UIMF
                {
                    m_run.ScanSetCollection= ScanSetCollection.Create(m_run, m_run.MinScan, m_run.MaxScan,
                          Project.getInstance().Parameters.NumScansSummed,
                          Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance,
                          Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.ProcessMSMS);
                    
                }


            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("ERROR: " + ex.Message, Logger.Instance.OutputFilename);   //forces it to write out immediately and clear buffer
                throw ex;
            }



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
                Logger.Instance.AddEntry("total features = " + m_run.ResultCollection.MSFeatureCounter);
                Logger.Instance.WriteToFile(Logger.Instance.OutputFilename);
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

                case Globals.ExporterType.SQLite:
                    return baseFileName += "_scans.sqlite";

                default:
                    return baseFileName += "_scans.csv";

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

                case Globals.ExporterType.SQLite:
                    return baseFileName += "_isos.sqlite";

                default:
                    return baseFileName += "_isos.csv";

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
