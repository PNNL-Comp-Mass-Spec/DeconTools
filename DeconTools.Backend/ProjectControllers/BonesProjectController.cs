using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Deconvoluters;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    public class BonesProjectController : ProjectController
    {
        private List<string> m_inputDataFilenames;
        private string m_paramFilename;
        private string m_outputFilename;
        private DeconTools.Backend.Globals.MSFileType m_fileType;
        private BackgroundWorker m_backgroundWorker;

        #region Constructors

        public BonesProjectController(List<string> inputDataFilenames, DeconTools.Backend.Globals.MSFileType fileType, string paramFileName, int numScansToAdvance)
            : this(inputDataFilenames, fileType, paramFileName, numScansToAdvance, null)
        {

        }

        public BonesProjectController(List<string> inputDataFilenames, DeconTools.Backend.Globals.MSFileType fileType, string paramFileName, int numScansToAdvance, BackgroundWorker bw)
        {
            this.m_inputDataFilenames = inputDataFilenames;
            this.m_fileType = fileType;
            this.m_paramFilename = paramFileName;
            this.m_backgroundWorker = bw;

            Project.Reset();

            Project.getInstance().LoadOldDecon2LSParameters(this.m_paramFilename);

            RunFactory runfactory = new RunFactory();


            Globals.ResultType resultType = Globals.ResultType.BASIC_TRADITIONAL_RESULT;

            //Create runs and create ScanSets
            foreach (string filename in this.m_inputDataFilenames)
            {
                Run run;
                run = runfactory.CreateRun(fileType, filename, Project.getInstance().Parameters.OldDecon2LSParameters);
                run.ResultCollection.ResultType = GetResultType(run,Project.getInstance().Parameters.OldDecon2LSParameters);
                
                resultType= run.ResultCollection.ResultType;
                
                Check.Assert(run != null, "Processing aborted. Could not handle supplied File(s)");
                Project.getInstance().RunCollection.Add(run);

                ScanSetCollectionCreator scanSetCollectionCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan,
                    Project.getInstance().Parameters.NumScansSummed,
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance, false);
                scanSetCollectionCreator.Create();
            }

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


            //DeconvolutorFactory deconFactory = new DeconvolutorFactory();
            //Task deconvolutor = deconFactory.CreateDeconvolutor(Project.getInstance().Parameters.OldDecon2LSParameters);
            Task deconvolutor = new SimpleDecon(0.0005);
            Project.getInstance().TaskCollection.TaskList.Add(deconvolutor);



            Task isosResultExporter = new IsosExporterFactory(50000).CreateIsosExporter(resultType, Globals.ExporterType.TEXT, setIsosOutputFileName(Globals.ExporterType.TEXT));
            Project.getInstance().TaskCollection.TaskList.Add(isosResultExporter);

            Task scanResultExporter = new DeconTools.Backend.Data.ScansExporterFactory().CreateScansExporter(fileType, Globals.ExporterType.TEXT, setScansOutputFileName(Globals.ExporterType.TEXT));
            Project.getInstance().TaskCollection.TaskList.Add(scanResultExporter);


            Task scanResultUpdater = new ScanResultUpdater();
            Project.getInstance().TaskCollection.TaskList.Add(scanResultUpdater);




        }

        private string setScansOutputFileName(Globals.ExporterType exporterType)
        {
            string baseFileName = Project.getInstance().RunCollection[0].Filename.Substring(0, Project.getInstance().RunCollection[0].Filename.LastIndexOf('.'));

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
            string baseFileName = Project.getInstance().RunCollection[0].Filename.Substring(0, Project.getInstance().RunCollection[0].Filename.LastIndexOf('.'));

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

        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void Execute()
        {
            Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(OldSchoolProcRunner)));
            Logger.Instance.AddEntry("DeconEngine version = " + AssemblyInfoRetriever.GetVersion(typeof(DeconToolsV2.HornTransform.clsHornTransformParameters)));
            Logger.Instance.AddEntry("Deconvolution_Algorithm = " + Project.getInstance().TaskCollection.GetDeconvolutorType());
            Logger.Instance.AddEntry("Started file processing - using 'BonesProjectController'");

            TaskControllerFactory taskControllerFactory = new TaskControllerFactory(this.m_backgroundWorker);
            TaskController controller = taskControllerFactory.CreateTaskController(m_fileType, Project.getInstance().TaskCollection);
            controller.IsosResultThresholdNum = 50000;

            try
            {
                controller.Execute(Project.getInstance().RunCollection);

            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry(" ------------------------------- ERROR: " + ex.Message, Logger.Instance.OutputFilename);
                throw ex;   //throw the error again and let something else catch it. 
            }

            Logger.Instance.AddEntry("Finished file processing", Logger.Instance.OutputFilename);
            Logger.Instance.AddEntry("total processing time = " + Logger.Instance.GetTimeDifference("Started file processing", "Finished file processing"));

            ProjectFacade pf = new ProjectFacade();
            Logger.Instance.AddEntry("total features = " + pf.GetTotalFeaturesFromScanResultCollection());

            Logger.Instance.WriteToFile(Project.getInstance().RunCollection[0].Filename + "_log.txt");
            Logger.Instance.Close();

            TaskCleaner taskCleaner = new TaskCleaner(Project.getInstance().TaskCollection);
            taskCleaner.CleanTasks();

            Project.Reset();      //sets Project singleton to null;

        }
    }
}
