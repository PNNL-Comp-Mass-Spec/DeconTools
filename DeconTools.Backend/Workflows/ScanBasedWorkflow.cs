using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters;
using DeconTools.Backend.ProcessingTasks.ResultExporters.PeakListExporters;
using DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.Workflows
{
    public abstract class ScanBasedWorkflow
    {
        private const int PeakListExporterTriggerValue = 10000;

        internal string PeakListOutputFileName;
        internal string IsosOutputFileName;
        internal string ScansOutputFileName;
        internal string LogFileName;
        internal string ParameterFileName;

        protected BackgroundWorker BackgroundWorker;


        protected Run Run;

        protected MSGenerator MSGenerator;
        protected PeakDetector PeakDetector;
        protected Deconvolutor Deconvolutor;
        protected ZeroFiller ZeroFiller;
        protected Smoother Smoother;

        protected ScanResultUpdater ScanResultUpdater;

        protected ResultValidatorTask ResultValidator;
        protected IPeakListExporter PeakListExporter;
        protected PeakToMSFeatureAssociator PeakToMSFeatureAssociator;

        protected IsosResultExporter IsosResultExporter;
        protected ScanResultExporter ScanResultExporter;
        protected DeconToolsFitScoreCalculator FitScoreCalculator;

        public OldDecon2LSParameters OldDecon2LsParameters { get; set; }



        #region Factory methods
        public static ScanBasedWorkflow CreateWorkflow(string datasetFileName, string parameterFile, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
        {

            Run run;

            try
            {
                run = new RunFactory().CreateRun(datasetFileName);
            }
            catch (Exception ex)
            {
                Logger.Instance.OutputFilename = datasetFileName + "_BAD_ERROR_log.txt";
                Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(ScanBasedWorkflow)));
                Logger.Instance.AddEntry("DeconEngine version = " + AssemblyInfoRetriever.GetVersion(typeof(DeconToolsV2.HornTransform.clsHornTransformParameters)));
                Logger.Instance.AddEntry("RapidEngine version = " + RapidDeconvolutor.getRapidVersion());
                Logger.Instance.AddEntry("UIMFLibrary version = " + AssemblyInfoRetriever.GetVersion(typeof(UIMFLibrary.DataReader)), Logger.Instance.OutputFilename);   //forces it to write out immediately and clear buffer
                Logger.Instance.AddEntry("ERROR details:" + ex.Message + "\n" + ex.StackTrace,
                                         Logger.Instance.OutputFilename);

                return null;
            }

            var parameters = new OldDecon2LSParameters();
            parameters.Load(parameterFile);

            return CreateWorkflow(run, parameters, outputFolderPath, backgroundWorker);

        }


        public static ScanBasedWorkflow CreateWorkflow(Run run, OldDecon2LSParameters parameters, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
        {

            switch (parameters.HornTransformParameters.ScanBasedWorkflowType.ToLower())
            {
                case "uimf_saturation_repair":
                    return new SaturationIMSScanBasedWorkflow(parameters, run, outputFolderPath, backgroundWorker);
                
                case "standard":
                    if (run is UIMFRun)
                    {
                        return new StandardIMSScanBasedWorkflow(parameters, run, outputFolderPath, backgroundWorker);
                    }
                    return new TraditionalScanBasedWorkflow(parameters, run, outputFolderPath, backgroundWorker);
                    
                case "run_merging_with_peak_export":
                    return new RunMergingPeakExportingWorkflow(parameters,null,outputFolderPath,backgroundWorker);
                    
                default:
                    throw new ArgumentOutOfRangeException("workflowType");
            }

          
        }

        #endregion

        #region Constructors

        public ScanBasedWorkflow(OldDecon2LSParameters parameters, Run run, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
        {
            OldDecon2LsParameters = parameters;
            Run = run;
            OutputFolderPath = outputFolderPath;    //path is null unless specified
            BackgroundWorker = backgroundWorker;   //null unless specified

            ExportData = true;
        }


        #endregion

        #region Properties

        internal WorkflowStats WorkflowStats { get; set; }

        public bool CanInitializeWorkflow
        {
            get { return (Run != null); }

        }

        public string OutputFolderPath { get; set; }

        public Globals.ExporterType ExporterType { get; set; }

        /// <summary>
        /// Controls whether or not data is exported. This is useful if you want programmatic 
        /// access to the IsosResults in the RunCollection (Exporters will clear the ResultCollection)
        /// Default = TRUE
        /// </summary>
        public bool ExportData { get; set; }


        #endregion

        #region Public Methods

        public virtual void InitializeWorkflow()
        {
            Check.Assert(Run != null, "Cannot initialize workflow. Run is null");
            Check.Assert(OldDecon2LsParameters != null, "Cannot initialize workflow. Parameters are null");

            Run.ResultCollection.ResultType = GetResultType();
       
            

            InitializeParameters();

            CreateOutputFileNames();

            WriteProcessingInfoToLog();

            try
            {
                CreateTargetMassSpectra();

                ExecutePreprocessHook();

                InitializeProcessingTasks();

            }
            catch (Exception ex)
            {
                string errorDetails = ex.Message + "; STACKTRACE= " + ex.StackTrace;

                Logger.Instance.AddEntry("Error - during workflow initialization", Logger.Instance.OutputFilename);
                Logger.Instance.AddEntry("Error details: " + errorDetails, Logger.Instance.OutputFilename);
                throw;
            }

           
            

        }


        protected virtual void InitializeProcessingTasks()
        {
            MSGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
            PeakDetector = PeakDetectorFactory.CreatePeakDetector(OldDecon2LsParameters);
            Deconvolutor = DeconvolutorFactory.CreateDeconvolutor(OldDecon2LsParameters);


            //Will initialize these but whether or not they are used are determined elsewhere
            ZeroFiller = new DeconToolsZeroFiller(OldDecon2LsParameters.HornTransformParameters.NumZerosToFill);
            Smoother = new DeconToolsSavitzkyGolaySmoother(OldDecon2LsParameters.HornTransformParameters.SGNumLeft,
                OldDecon2LsParameters.HornTransformParameters.SGNumRight,
                OldDecon2LsParameters.HornTransformParameters.SGOrder);

            FitScoreCalculator = new DeconToolsFitScoreCalculator();
            ScanResultUpdater = new ScanResultUpdater(OldDecon2LsParameters.HornTransformParameters.ProcessMSMS);
            ResultValidator = new ResultValidatorTask();

            IsosResultExporter = IsosExporterFactory.CreateIsosExporter(Run.ResultCollection.ResultType, ExporterType,
                                                                  IsosOutputFileName);

            ScanResultExporter = ScansExporterFactory.CreateScansExporter(Run.MSFileType, ExporterType,
                                                                          ScansOutputFileName);

            PeakListExporter = PeakListExporterFactory.Create(ExporterType, Run.MSFileType, PeakListExporterTriggerValue,
                                                              PeakListOutputFileName);
            PeakToMSFeatureAssociator = new PeakToMSFeatureAssociator();

        }

        /// <summary>
        /// Defines the scans that will be processed. 
        /// </summary>
        protected virtual void CreateTargetMassSpectra()
        {
            int minScan;
            int maxScan;

            if (OldDecon2LsParameters.HornTransformParameters.SumSpectra)
            {
                Run.ScanSetCollection = ScanSetCollection.Create(Run, true, false);
                return;
            }


            if (OldDecon2LsParameters.HornTransformParameters.UseScanRange)
            {
                minScan = Math.Max(Run.MinScan, OldDecon2LsParameters.HornTransformParameters.MinScan);
                maxScan = Math.Min(Run.MaxScan, OldDecon2LsParameters.HornTransformParameters.MaxScan);
            }
            else
            {
                minScan = Run.MinScan;
                maxScan = Run.MaxScan;
            }


            int numSummed;
            if (OldDecon2LsParameters.HornTransformParameters.SumSpectraAcrossScanRange)
            {
                numSummed = OldDecon2LsParameters.HornTransformParameters.NumScansToSumOver*2 + 1;
            }
            else
            {
                numSummed = 1;
            }
            
            Run.ScanSetCollection = ScanSetCollection.Create(Run, minScan, maxScan,numSummed,
                          OldDecon2LsParameters.HornTransformParameters.NumScansToAdvance,
                          OldDecon2LsParameters.HornTransformParameters.ProcessMSMS);
            
        }

        /// <summary>
        /// A hook that allows derived classes to do something before main processing is executed. See 'Template Method' design pattern
        /// </summary>
        protected virtual void ExecutePreprocessHook()
        {

        }



        public virtual void Execute()
        {
            InitializeWorkflow();

            WorkflowStats = new WorkflowStats();
            WorkflowStats.TimeStarted = DateTime.Now;


            IterateOverScans();

            WorkflowStats.TimeFinished = DateTime.Now;
            WorkflowStats.NumFeatures = Run.ResultCollection.MSFeatureCounter;

            WriteOutSummaryToLogfile();

        }

        protected virtual void WriteOutSummaryToLogfile()
        {
            Logger.Instance.AddEntry("Finished file processing", Logger.Instance.OutputFilename);

            string formattedOverallprocessingTime = string.Format("{0:00}:{1:00}:{2:00}",
                WorkflowStats.ElapsedTime.Hours, WorkflowStats.ElapsedTime.Minutes, WorkflowStats.ElapsedTime.Seconds);

            Logger.Instance.AddEntry("total processing time = " + formattedOverallprocessingTime);
            Logger.Instance.AddEntry("total features = " + WorkflowStats.NumFeatures);
            Logger.Instance.WriteToFile(Logger.Instance.OutputFilename);
            Logger.Instance.Close();
        }


        protected abstract void IterateOverScans();

        public abstract void ReportProgress();

        /// <summary>
        /// Executes the processing tasks on a given scan (or frame). 
        /// </summary>
        protected virtual void ExecuteProcessingTasks()
        {
            ExecuteTask(MSGenerator);

            if (OldDecon2LsParameters.HornTransformParameters.ZeroFill)
            {
                ExecuteTask(ZeroFiller);
            }

            if (OldDecon2LsParameters.HornTransformParameters.UseSavitzkyGolaySmooth)
            {
                ExecuteTask(Smoother);
            }

            ExecuteTask(PeakDetector);

            ExecuteTask(Deconvolutor);

            ExecuteTask(ResultValidator);

            ExecuteTask(ScanResultUpdater);

            if (OldDecon2LsParameters.HornTransformParameters.ReplaceRAPIDScoreWithHornFitScore)
            {
                ExecuteTask(FitScoreCalculator);
            }

            //Allows derived classes to execute additional tasks
            ExecuteOtherTasksHook();


            //the following exporting tasks should be last
            if (ExportData)
            {

                if (OldDecon2LsParameters.PeakProcessorParameters.WritePeaksToTextFile)
                {
                    ExecuteTask(PeakToMSFeatureAssociator);
                    ExecuteTask(PeakListExporter);

                }

                ExecuteTask(IsosResultExporter);

                ExecuteTask(ScanResultExporter);

            }

        }

        protected virtual void ExecuteOtherTasksHook() { }

        protected void ExecuteTask(Task processingTask)
        {

            try
            {
                processingTask.Execute(Run.ResultCollection);
            }
            catch (Exception ex)
            {
                string errorInfo = GetErrorInfo(Run, processingTask, ex);
                Logger.Instance.AddEntry(errorInfo, Logger.Instance.OutputFilename);

                throw;

            }

        }


        protected string GetErrorInfo(Run run, Task task, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ERROR THROWN. ProcessingTask = ");
            sb.Append(task);
            sb.Append("; ");
            sb.Append(run.GetCurrentScanOrFrameInfo());

            sb.Append("; ");
            sb.Append(DiagnosticUtilities.GetCurrentProcessInfo());
            sb.Append("; errorObject details: ");
            sb.Append(ex.Message);
            sb.Append("; ");
            sb.Append(ex.StackTrace);

            return sb.ToString();

        }


        #endregion

        #region Private Methods
        protected virtual Globals.ResultType GetResultType()
        {
            if (Run is UIMFRun) return Globals.ResultType.UIMF_TRADITIONAL_RESULT;
            if (Run is IMFRun) return Globals.ResultType.IMS_TRADITIONAL_RESULT;

            if (OldDecon2LsParameters.HornTransformParameters.O16O18Media)
            {
                return Globals.ResultType.O16O18_TRADITIONAL_RESULT;
            }
            else
            {
                return Globals.ResultType.BASIC_TRADITIONAL_RESULT;
            }

        }

        protected virtual void WriteProcessingInfoToLog()
        {
            Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(ScanBasedWorkflow)));
            Logger.Instance.AddEntry("ParameterFile = " + (OldDecon2LsParameters.ParameterFilename == null ? "[NONE]" : Path.GetFileName(OldDecon2LsParameters.ParameterFilename)));
            Logger.Instance.AddEntry("DeconEngine version = " + AssemblyInfoRetriever.GetVersion(typeof(DeconToolsV2.HornTransform.clsHornTransformParameters)));
            Logger.Instance.AddEntry("RapidEngine version = " + RapidDeconvolutor.getRapidVersion());
            Logger.Instance.AddEntry("UIMFLibrary version = " + AssemblyInfoRetriever.GetVersion(typeof(UIMFLibrary.DataReader)), Logger.Instance.OutputFilename);   //forces it to write out immediately and clear buffer
        }

        protected virtual void CreateOutputFileNames()
        {
            string basefileName = GetBaseFileName(Run);

            Logger.Instance.OutputFilename = basefileName + "_log.txt";


            switch (ExporterType)
            {
                case Globals.ExporterType.TEXT:
                    IsosOutputFileName = basefileName + "_isos.csv";
                    ScansOutputFileName = basefileName + "_scans.csv";
                    PeakListOutputFileName = basefileName + "_peaks.txt";
                    break;
                case Globals.ExporterType.SQLite:
                    IsosOutputFileName = basefileName + "_isos.db3";
                    ScansOutputFileName = basefileName + "_scans.db3";
                    PeakListOutputFileName = basefileName + "_peaks.db3";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private string GetBaseFileName(Run run)
        {
            //outputFilePath will be null if outputFilePath wasn't set using a constructor
            //So if null, will create the default outputPath
            if (OutputFolderPath == null)
            {
                return run.DataSetPath + "\\" + run.DatasetName;
            }
            else
            {
                return OutputFolderPath.TrimEnd(new char[] { '\\' }) + "\\" + run.DatasetName;
            }
        }


        protected void InitializeParameters()
        {
            //set exporter type. This property wraps the OldDeconTools parameter
            switch (OldDecon2LsParameters.HornTransformParameters.ExportFileType)
            {
                case DeconToolsV2.HornTransform.enmExportFileType.SQLITE:
                    ExporterType = Globals.ExporterType.SQLite;
                    break;
                case DeconToolsV2.HornTransform.enmExportFileType.TEXT:
                    ExporterType = Globals.ExporterType.TEXT;
                    break;
                default:
                    ExporterType = Globals.ExporterType.TEXT;
                    break;
            }






        }


        #endregion

    }
}
