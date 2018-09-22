using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters;
using DeconTools.Backend.ProcessingTasks.ResultExporters.PeakListExporters;
using DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Utilities;

namespace DeconTools.Backend.Workflows
{
    public abstract class ScanBasedWorkflow
    {
        private const int PeakListExporterTriggerValue = 10000;

        private bool _deconvolutorRequiresPeaksFile;

        internal string PeakListOutputFileName;
        internal string IsosOutputFileName;
        internal string ScansOutputFileName;
        //internal string LogFileName;
        //internal string ParameterFileName;

        internal bool NotifiedZeroFillAutoEnabled;

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

        protected bool mShowTraceMessages = false;

        public DeconToolsParameters NewDeconToolsParameters { get; set; }

        #region Factory methods
        public static ScanBasedWorkflow CreateWorkflow(string datasetFileName, string parameterFile, string outputFolderPath = null, BackgroundWorker backgroundWorker = null, bool useNewDeconToolsParameterObjects = true)
        {
            var datasetFile = new FileInfo(datasetFileName);
            if (!datasetFile.Exists)
            {
                var datasetFolder = new DirectoryInfo(datasetFileName);
                if (!datasetFolder.Exists)
                    throw new FileNotFoundException("Dataset file (or folder) not found: " + datasetFile);
            }

            var paramFile = new FileInfo(parameterFile);
            if (!paramFile.Exists)
                throw new FileNotFoundException("Parameter file not found: " + parameterFile);

            // Initialize a new Run
            Run run;

            try
            {
                run = new RunFactory().CreateRun(datasetFileName);
            }
            catch (Exception ex)
            {
                Logger.Instance.OutputFilename = datasetFileName + "_BAD_ERROR_log.txt";
                Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(ScanBasedWorkflow)));
                Logger.Instance.AddEntry("UIMFLibrary version = " + AssemblyInfoRetriever.GetVersion(typeof(UIMFLibrary.DataReader)), true);
                Logger.Instance.AddEntry("ERROR message= " + ex.Message);
                Logger.Instance.AddEntry("ERROR type= " + ex);
                Logger.Instance.AddEntry("STACKTRACE = " + PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex), true);

                throw new ApplicationException(
                    "A fatal error occured when connecting to the instrument data file. Could not create the Run object. Internal error message: " +
                    ex.Message + Environment.NewLine + Environment.NewLine + PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex));
            }

            DeconToolsParameters newParameters;

            try
            {
                newParameters = new DeconToolsParameters();
                newParameters.LoadFromOldDeconToolsParameterFile(parameterFile);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    "Error loading the parameter file: " +
                    ex.Message + Environment.NewLine + Environment.NewLine + PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex));
            }

            try
            {
                return CreateWorkflow(run, newParameters, outputFolderPath, backgroundWorker);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    "Error creating the workflow: " +
                    ex.Message + Environment.NewLine + Environment.NewLine + PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex));
            }

        }

        public static ScanBasedWorkflow CreateWorkflow(Run run, DeconToolsParameters parameters, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
        {
            switch (parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName.ToLower())
            {
                case "uimf_saturation_repair":
                    return new SaturationIMSScanBasedWorkflow(parameters, run, outputFolderPath, backgroundWorker);

                case "uimf_standard":
                    return new StandardIMSScanBasedWorkflow(parameters, run, outputFolderPath, backgroundWorker);

                case "standard":
                    if (run is UIMFRun)
                    {
                        return new StandardIMSScanBasedWorkflow(parameters, run, outputFolderPath, backgroundWorker);
                    }
                    return new TraditionalScanBasedWorkflow(parameters, run, outputFolderPath, backgroundWorker);

                case "run_merging_with_peak_export":
                    return new RunMergingPeakExportingWorkflow(parameters, null, outputFolderPath, backgroundWorker);

                case "deconmsn":
                    return new DeconMSnWorkflow(parameters, run, outputFolderPath, backgroundWorker);

                default:
                    throw new Exception("ScanBasedWorkflowName is unknown: " + parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName);
            }
        }



        #endregion

        #region Constructors

        protected ScanBasedWorkflow(DeconToolsParameters parameters, Run run, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
        {
            NewDeconToolsParameters = parameters;
            Run = run;
            OutputFolderPath = outputFolderPath;    //path is null unless specified
            BackgroundWorker = backgroundWorker;   //null unless specified

            ExportData = true;
        }



        #endregion

        #region Properties

        internal WorkflowStats WorkflowStats { get; set; }

        public bool CanInitializeWorkflow => (Run != null);

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
            if (Run == null)
                return;

            Check.Assert(NewDeconToolsParameters != null, "Cannot initialize workflow. Parameters are null");

            Run.ResultCollection.ResultType = GetResultType();

            InitializeParameters();

            CreateOutputFileNames();

            WriteProcessingInfoToLog();

            CreateTargetMassSpectra();

            ExecutePreprocessHook();

            InitializeProcessingTasks();

            if (_deconvolutorRequiresPeaksFile)
            {
                //new iThrash deconvolutor uses the _peaks.txt file. So need to check for it and create it if necessary
                var peaksFileExists = CheckForPeaksFile(OutputFolderPath);
                if (!peaksFileExists)
                {
                    IqLogger.LogMessage("Creating _peaks.txt file. Takes 1 to 5 minutes.");
                    CreatePeaksFile(NewDeconToolsParameters.PeakDetectorParameters, OutputFolderPath);
                }

                IqLogger.LogMessage("Loading _peaks.txt file into memory. Takes 0 - 30 seconds" + Environment.NewLine);
                LoadPeaks(OutputFolderPath);

            }


        }

        protected virtual void InitializeProcessingTasks()
        {
            MSGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
            PeakDetector = PeakDetectorFactory.CreatePeakDetector(NewDeconToolsParameters);
            Deconvolutor = DeconvolutorFactory.CreateDeconvolutor(NewDeconToolsParameters);

            //the new iThrash imports the _peaks.txt file
#pragma warning disable 618
            if (Deconvolutor is ProcessingTasks.Deconvoluters.InformedThrashDeconvolutor)
            {
                _deconvolutorRequiresPeaksFile = true;
            }
#pragma warning restore 618

            //Will initialize these but whether or not they are used are determined elsewhere
            ZeroFiller = new DeconToolsZeroFiller(NewDeconToolsParameters.MiscMSProcessingParameters.ZeroFillingNumZerosToFill);
            Smoother = new SavitzkyGolaySmoother(NewDeconToolsParameters.MiscMSProcessingParameters.SavitzkyGolayNumPointsInSmooth,
                NewDeconToolsParameters.MiscMSProcessingParameters.SavitzkyGolayOrder);

            FitScoreCalculator = new DeconToolsFitScoreCalculator();
            ScanResultUpdater = new ScanResultUpdater(NewDeconToolsParameters.ScanBasedWorkflowParameters.ProcessMS2);
            ResultValidator = new ResultValidatorTask();

            IsosResultExporter = IsosExporterFactory.CreateIsosExporter(Run.ResultCollection.ResultType, ExporterType,
                                                                  IsosOutputFileName);

            ScanResultExporter = ScansExporterFactory.CreateScansExporter(Run.MSFileType, ExporterType,
                                                                          ScansOutputFileName);

            if (!_deconvolutorRequiresPeaksFile)
            {
                PeakListExporter = PeakListExporterFactory.Create(ExporterType, Run.MSFileType, PeakListExporterTriggerValue,
                                                              PeakListOutputFileName);
            }

            PeakToMSFeatureAssociator = new PeakToMSFeatureAssociator();

        }

        /// <summary>
        /// Defines the scans that will be processed.
        /// </summary>
        protected virtual void CreateTargetMassSpectra()
        {
            int minScan;
            int maxScan;

            if (NewDeconToolsParameters.MSGeneratorParameters.SumAllSpectra)
            {
                Run.ScanSetCollection.Create(Run, true, false);
                return;
            }


            if (NewDeconToolsParameters.MSGeneratorParameters.UseLCScanRange)
            {
                minScan = Math.Max(Run.MinLCScan, NewDeconToolsParameters.MSGeneratorParameters.MinLCScan);
                maxScan = Math.Min(Run.MaxLCScan, NewDeconToolsParameters.MSGeneratorParameters.MaxLCScan);
            }
            else
            {
                minScan = Run.MinLCScan;
                maxScan = Run.MaxLCScan;
            }


            int numSummed;
            if (NewDeconToolsParameters.MSGeneratorParameters.SumSpectraAcrossLC)
            {
                numSummed = NewDeconToolsParameters.MSGeneratorParameters.NumLCScansToSum;
            }
            else
            {
                numSummed = 1;
            }

            Run.ScanSetCollection.Create(Run, minScan, maxScan, numSummed, 1,
                          NewDeconToolsParameters.ScanBasedWorkflowParameters.ProcessMS2);

        }

        /// <summary>
        /// A hook that allows derived classes to do something before main processing is executed. See 'Template Method' design pattern
        /// </summary>
        protected virtual void ExecutePreprocessHook()
        {

        }

        /// <summary>
        /// Execute the workflow
        /// </summary>
        public virtual void Execute()
        {
            try
            {
                InitializeWorkflow();
            }
            catch (Exception ex)
            {
                var simpleErrorMessage = "Error - during workflow initialization. Internal error message: " + ex.Message;
                LogError(ex, simpleErrorMessage);
                throw new ApplicationException(simpleErrorMessage, ex);
            }

            WorkflowStats = new WorkflowStats
            {
                TimeStarted = DateTime.Now
            };

            try
            {
                IterateOverScans();
            }
            catch (Exception ex)
            {
                var simpleErrorMessage = "A bad error happened while processing each scan in the dataset. Internal error message: " + ex.Message;
                LogError(ex, simpleErrorMessage);
                throw new ApplicationException(simpleErrorMessage, ex);
            }

            WorkflowStats.TimeFinished = DateTime.Now;
            WorkflowStats.NumFeatures = Run.ResultCollection.MSFeatureCounter;

            WriteOutSummaryToLogfile();

        }

        private void LogError(Exception ex, string simpleErrorMessage)
        {
            Logger.Instance.AddEntry(simpleErrorMessage);
            Logger.Instance.AddEntry(PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex), true);
        }

        private void LoadPeaks(string userProvidedOutputFolderPath = null)
        {
            string outputFolderPath;
            if (string.IsNullOrEmpty(userProvidedOutputFolderPath))
            {
                outputFolderPath = Run.DataSetPath;
            }
            else
            {
                outputFolderPath = userProvidedOutputFolderPath;
            }

            var expectedPeaksFile = Path.Combine(outputFolderPath, Run.DatasetName + "_peaks.txt");
            RunUtilities.GetPeaks(Run, expectedPeaksFile);
        }


        private bool CheckForPeaksFile(string userProvidedOutputFolderPath = null)
        {
            string outputFolderPath;
            if (string.IsNullOrEmpty(userProvidedOutputFolderPath))
            {
                outputFolderPath = Run.DataSetPath;
            }
            else
            {
                outputFolderPath = userProvidedOutputFolderPath;
            }

            var expectedPeaksFile = Path.Combine(outputFolderPath, Run.DatasetName + "_peaks.txt");

            if (!File.Exists(expectedPeaksFile))
                return false;

            // Open the file and confirm that it has at least one row of data
            var rowCount = 0;

            using (
                var peaksFile =
                    new StreamReader(new FileStream(expectedPeaksFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                while (!peaksFile.EndOfStream && rowCount < 2)
                {
                    var line = peaksFile.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                        rowCount++;
                }
            }

            return (rowCount > 1);
        }


        protected virtual void WriteOutSummaryToLogfile()
        {
            Logger.Instance.AddEntry("Finished file processing", true);

            var formattedOverallprocessingTime = string.Format("{0:00}:{1:00}:{2:00}",
                WorkflowStats.ElapsedTime.Hours, WorkflowStats.ElapsedTime.Minutes, WorkflowStats.ElapsedTime.Seconds);

            Logger.Instance.AddEntry("total processing time = " + formattedOverallprocessingTime);
            Logger.Instance.AddEntry("total features = " + WorkflowStats.NumFeatures, true);
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

            var isCentroided = Run.IsDataCentroided(Run.CurrentScanSet.PrimaryScanNumber);

            if (isCentroided && !NewDeconToolsParameters.MiscMSProcessingParameters.UseZeroFilling && !NotifiedZeroFillAutoEnabled)
            {
                // Log that ZeroFilling will be auto-enabled because centroided spectra are present
                Logger.Instance.AddEntry("Auto-enabled zero-filling for centroided scans");
                NotifiedZeroFillAutoEnabled = true;
            }

            if (isCentroided || NewDeconToolsParameters.MiscMSProcessingParameters.UseZeroFilling)
            {
                ExecuteTask(ZeroFiller);
            }

            if (NewDeconToolsParameters.MiscMSProcessingParameters.UseSmoothing)
            {
                ExecuteTask(Smoother);
            }

            ExecuteTask(PeakDetector);

            Deconvolutor.ShowTraceMessages = mShowTraceMessages;

            ExecuteTask(Deconvolutor);

            ExecuteTask(ResultValidator);

            ExecuteTask(ScanResultUpdater);

            if (NewDeconToolsParameters.ScanBasedWorkflowParameters.IsRefittingPerformed)
            {
                ExecuteTask(FitScoreCalculator);
            }

            ShowTraceMessageIfEnabled("ExecuteOtherTasksHook");

            //Allows derived classes to execute additional tasks
            ExecuteOtherTasksHook();


            //the following exporting tasks should be last
            if (ExportData)
            {
                if (NewDeconToolsParameters.ScanBasedWorkflowParameters.ExportPeakData)
                {
                    ExecuteTask(PeakToMSFeatureAssociator);

                    if (!_deconvolutorRequiresPeaksFile)  //if we are using the new iThrash, the _peaks file will have been already created
                    {
                        ExecuteTask(PeakListExporter);
                    }
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
                ShowTraceMessageIfEnabled("ExecuteTask " + processingTask.GetType().Name);

                processingTask.Execute(Run.ResultCollection);
            }
            catch (Exception ex)
            {
                var errorInfo = GetErrorInfo(Run, processingTask, ex);
                Logger.Instance.AddEntry(errorInfo, true);

                throw;

            }

        }


        protected string GetErrorInfo(Run run, Task task, Exception ex)
        {
            var sb = new StringBuilder();
            sb.Append("ERROR THROWN. ProcessingTask = ");
            sb.Append(task);
            sb.Append("; ");
            sb.Append(run.GetCurrentScanOrFrameInfo());

            sb.Append("; ");
            sb.Append(DiagnosticUtilities.GetCurrentProcessInfo());
            sb.Append("; errorObject details: ");
            sb.Append(ex.Message);
            sb.Append("; ");
            sb.Append(PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex));

            return sb.ToString();

        }

        private void ShowTraceMessageIfEnabled(string currentTask)
        {
            if (mShowTraceMessages)
                Console.WriteLine("Scan {0}: {1}", Run.CurrentScanSet, currentTask);
        }

        #endregion

        #region Private Methods

        private void CreatePeaksFile(PeakDetectorParameters peakDetectorParameters, string outputFolderPath)
        {


            var parameters = new PeakDetectAndExportWorkflowParameters
            {
                OutputFolder = outputFolderPath,
                LCScanMin = Run.MinLCScan,
                LCScanMax = Run.MaxLCScan,
                IsDataThresholded = Run.IsDataThresholded,
                ProcessMSMS = false,
                PeakBR = peakDetectorParameters.PeakToBackgroundRatio,
                Num_LC_TimePointsSummed = 1,
                SigNoiseThreshold = peakDetectorParameters.SignalToNoiseThreshold
            };

            var peakDetectAndExporter = new PeakDetectAndExportWorkflow(Run, parameters);
            peakDetectAndExporter.Execute();

        }


        protected virtual Globals.ResultType GetResultType()
        {
            if (Run is UIMFRun) return Globals.ResultType.UIMF_TRADITIONAL_RESULT;

#if !Disable_DeconToolsV2
            if (Run is IMFRun) return Globals.ResultType.IMS_TRADITIONAL_RESULT;
#endif

            if (NewDeconToolsParameters.ThrashParameters.IsO16O18Data)
            {
                return Globals.ResultType.O16O18_TRADITIONAL_RESULT;
            }

            return Globals.ResultType.BASIC_TRADITIONAL_RESULT;
        }

        protected virtual void WriteProcessingInfoToLog()
        {
            Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(ScanBasedWorkflow)));
            Logger.Instance.AddEntry("ParameterFile = " + (NewDeconToolsParameters.ParameterFilename == null ? "[NONE]" :
                Path.GetFileName(NewDeconToolsParameters.ParameterFilename)));
            Logger.Instance.AddEntry("UIMFLibrary version = " + AssemblyInfoRetriever.GetVersion(typeof(UIMFLibrary.DataReader)), true);
        }

        protected virtual void CreateOutputFileNames()
        {
            var basefileName = GetBaseFileName(Run);

            Logger.Instance.OutputFilename = basefileName + "_log.txt";


            switch (ExporterType)
            {
                case Globals.ExporterType.Text:
                    IsosOutputFileName = basefileName + "_isos.csv";
                    ScansOutputFileName = basefileName + "_scans.csv";
                    PeakListOutputFileName = basefileName + "_peaks.txt";
                    break;
                case Globals.ExporterType.Sqlite:
                    IsosOutputFileName = basefileName + "_isos.db3";
                    ScansOutputFileName = basefileName + "_scans.db3";
                    PeakListOutputFileName = basefileName + "_peaks.db3";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        protected string GetBaseFileName(Run run)
        {
            //outputFilePath will be null if outputFilePath wasn't set using a constructor
            //So if null, will create the default outputPath
            if (OutputFolderPath == null)
            {
                return Path.Combine(run.DataSetPath, run.DatasetName);
            }

            if (Directory.Exists(OutputFolderPath))
            {
                return Path.Combine(OutputFolderPath, run.DatasetName);
            }

            try
            {
                Directory.CreateDirectory(OutputFolderPath);
            }
            catch (Exception ex)
            {
                var errorMessage =
                    "Output folder does not exist. When we tried to create it there was an error: " + ex.Message;

                Logger.Instance.AddEntry(errorMessage);
                Logger.Instance.AddEntry(PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex), true);

                throw new DirectoryNotFoundException(
                    "Output folder does not exist. When we tried to create it there was an error: " + ex.Message,
                    ex);

            }

            return Path.Combine(OutputFolderPath, run.DatasetName);
        }


        protected void InitializeParameters()
        {
            //TODO: move this
            ExporterType = NewDeconToolsParameters.ScanBasedWorkflowParameters.ExportFileType;

        }


        #endregion

    }
}
