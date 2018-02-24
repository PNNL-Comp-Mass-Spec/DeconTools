using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Backend.Workflows;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using DeconTools.Workflows.Backend.Utilities;
namespace DeconTools.Workflows.Backend.Core
{
    public abstract class TargetedWorkflowExecutor : WorkflowBase
    {
        protected IsotopicDistributionCalculator IsotopicDistributionCalculator = IsotopicDistributionCalculator.Instance;


        protected string _loggingFileName;
        protected string _resultsFolder;
        protected TargetedResultRepository ResultRepository;


        protected List<long> MassTagIDsinTargets = new List<long>();

        protected WorkflowParameters _workflowParameters;

        protected BackgroundWorker _backgroundWorker;
        private readonly TargetedWorkflowExecutorProgressInfo _progressInfo = new TargetedWorkflowExecutorProgressInfo();
        private string _alignmentFolder;

        #region Constructors

        protected TargetedWorkflowExecutor(WorkflowParameters parameters, string datasetPath, BackgroundWorker backgroundWorker = null)
        {
            DatasetPath = datasetPath;
            RunIsDisposed = true;

            _backgroundWorker = backgroundWorker;

            WorkflowParameters = parameters;

            MsgfFdrScoreCutoff = 0.1;

            ResultRepository = new TargetedResultRepository();
            InitializeWorkflow();
        }

        protected TargetedWorkflowExecutor(WorkflowParameters parameters, TargetedWorkflow targetedWorkflow, string datasetPath, BackgroundWorker backgroundWorker = null)
        {
            DatasetPath = datasetPath;
            RunIsDisposed = true;

            _backgroundWorker = backgroundWorker;

            WorkflowParameters = parameters;

            TargetedWorkflow = targetedWorkflow;

            ResultRepository = new TargetedResultRepository();
            InitializeWorkflow();
        }


        protected TargetedWorkflowExecutor(WorkflowParameters parameters, WorkflowParameters workflowParameters, string datasetPath, BackgroundWorker backgroundWorker = null)
        {
            DatasetPath = datasetPath;
            RunIsDisposed = true;

            _backgroundWorker = backgroundWorker;

            WorkflowParameters = parameters;
            _workflowParameters = workflowParameters;

            ResultRepository = new TargetedResultRepository();
            InitializeWorkflow();
        }

        protected TargetedWorkflowExecutor(WorkflowParameters parameters, Run run, BackgroundWorker backgroundWorker = null)
        {
            Run = run;
            RunIsDisposed = true;

            if (Run != null)
                DatasetPath = Run.DataSetPath;

            _backgroundWorker = backgroundWorker;

            WorkflowParameters = parameters;

            ResultRepository = new TargetedResultRepository();
            InitializeWorkflow();
        }


        public void InitializeWorkflow()
        {
            if (string.IsNullOrEmpty(ExecutorParameters.OutputFolderBase))
            {
                _resultsFolder = RunUtilities.GetDatasetParentFolder(DatasetPath);
            }
            else
            {
                _resultsFolder = GetResultsFolder(ExecutorParameters.OutputFolderBase);
            }

#pragma warning disable 618
            if (ExecutorParameters.TargetedAlignmentIsPerformed)
#pragma warning restore 618
            {
                MassTagsForTargetedAlignment = GetMassTagTargets(GetTargetFilePathForIqAlignment());
            }


            var targetsFilePathIsEmpty = (string.IsNullOrEmpty(ExecutorParameters.TargetsFilePath));

            string currentTargetsFilePath;

            if (targetsFilePathIsEmpty)
            {
                currentTargetsFilePath = TryFindTargetsForCurrentDataset();   //check for a _targets file specifically associated with dataset
            }
            else
            {
                currentTargetsFilePath = ExecutorParameters.TargetsFilePath;
            }

            Targets = CreateTargets(ExecutorParameters.TargetType, currentTargetsFilePath);

            Check.Ensure(Targets != null && Targets.TargetList.Count > 0,
                         "Target massTags is empty (or all peptides contain unknown modifications). Check the path to the massTag data file.");

            if (Targets == null)
                return;

            IqLogger.LogMessage("Total targets loaded= " + Targets.TargetList.Count);


            if (ExecutorParameters.TargetType == Globals.TargetType.LcmsFeature)
            {
                UpdateTargetMissingInfo();
            }

            if (TargetedWorkflow == null)
            {
                if (_workflowParameters == null)
                {
                    _workflowParameters = WorkflowParameters.CreateParameters(ExecutorParameters.WorkflowParameterFile);
                    _workflowParameters.LoadParameters(ExecutorParameters.WorkflowParameterFile);
                }
                TargetedWorkflow = TargetedWorkflow.CreateWorkflow(_workflowParameters);
            }
            else
            {
                _workflowParameters = TargetedWorkflow.WorkflowParameters;
            }

#pragma warning disable 618
            if (ExecutorParameters.TargetedAlignmentIsPerformed)
#pragma warning restore 618
            {
                if (string.IsNullOrEmpty(ExecutorParameters.TargetedAlignmentWorkflowParameterFile))
                {
                    throw new FileNotFoundException(
                        "Cannot initialize workflow. TargetedAlignment is requested but TargetedAlignmentWorkflowParameter file is not found. Check path for the 'TargetedAlignmentWorkflowParameterFile' ");
                }


                TargetedAlignmentWorkflowParameters = new TargetedAlignerWorkflowParameters();
                TargetedAlignmentWorkflowParameters.LoadParameters(ExecutorParameters.TargetedAlignmentWorkflowParameterFile);

            }


        }



        #endregion

        #region Properties

        public string DatasetPath { get; set; }

        public TargetCollection MassTagsForTargetedAlignment { get; set; }

        public TargetCollection Targets { get; set; }

        public WorkflowParameters WorkflowParameters
        {
            get => ExecutorParameters;
            set => ExecutorParameters = value as WorkflowExecutorBaseParameters;
        }

        public WorkflowExecutorBaseParameters ExecutorParameters { get; set; }

        public TargetedAlignerWorkflowParameters TargetedAlignmentWorkflowParameters { get; set; }

        public TargetedAlignerWorkflow TargetedAlignmentWorkflow { get; set; }

        public TargetedWorkflow TargetedWorkflow { get; set; }

        /// <summary>
        /// These are database targets that are used for lookup when working on peak-matched LcmsFeatures
        /// </summary>
        public TargetCollection MassTagsForReference { get; set; }


        public double MsgfFdrScoreCutoff { get; set; }

        public bool RunIsDisposed { get; set; }


        #endregion



        #region Public Methods

        protected virtual void UpdateTargetMissingInfo()
        {

            var canUseReferenceMassTags = MassTagsForReference != null && MassTagsForReference.TargetList.Count > 0;

            var massTagIDsAvailableForLookup = new List<int>();

            if (canUseReferenceMassTags)
            {
                massTagIDsAvailableForLookup = MassTagsForReference.TargetList.Select(p => p.ID).ToList();
            }


            foreach (var targetBase in Targets.TargetList)
            {
                var target = (LcmsFeatureTarget)targetBase;

                var isMissingMonoMass = target.MonoIsotopicMass <= 0;

                if (string.IsNullOrEmpty(target.EmpiricalFormula))
                {
                    if (canUseReferenceMassTags && massTagIDsAvailableForLookup.Contains(target.FeatureToMassTagID))
                    {

                        var mt = MassTagsForReference.TargetList.First(p => p.ID == target.FeatureToMassTagID);

                        //in DMS, Sequest will put an 'X' when it can't differentiate 'I' and 'L'
                        //  see:   \\gigasax\DMS_Parameter_Files\Sequest\sequest_ETD_N14_NE.params
                        //To create the theoretical isotopic profile, we will change the 'X' to 'L'
                        if (mt.Code.Contains("X"))
                        {
                            mt.Code = mt.Code.Replace('X', 'L');
                            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
                        }

                        target.Code = mt.Code;
                        target.EmpiricalFormula = mt.EmpiricalFormula;
                    }
                    else if (!string.IsNullOrEmpty(target.Code))
                    {
                        //Create empirical formula based on code. Assume it is an unmodified peptide

                        if (target.Code.Contains("X"))
                        {
                            target.Code = target.Code.Replace('X', 'L');
                        }

                        target.EmpiricalFormula = new PeptideUtils().GetEmpiricalFormulaForPeptideSequence(target.Code);

                    }
                    else
                    {
                        if (isMissingMonoMass)
                        {
                            throw new ApplicationException(
                                "Trying to prepare target list, but Target is missing both the 'Code' and the Monoisotopic Mass. One or the other is needed.");
                        }
                        target.Code = "AVERAGINE";
                        target.EmpiricalFormula =
                            IsotopicDistributionCalculator.GetAveragineFormulaAsString(target.MonoIsotopicMass, false);
                    }
                }


                if (isMissingMonoMass)
                {
                    target.MonoIsotopicMass =
                        EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(target.EmpiricalFormula);



                    target.MZ = target.MonoIsotopicMass / target.ChargeState + DeconTools.Backend.Globals.PROTON_MASS;
                }



            }
        }

        protected string TryFindTargetsForCurrentDataset()
        {
            var datasetName = RunUtilities.GetDatasetName(DatasetPath);

            string[] possibleFileSuffixs = { "_iqTargets.txt", "_targets.txt", "_LCMSFeatures.txt", "_MSGFPlus.tsv", "_msgfplus_fht.txt", "_msgfdb_fht.txt" };

            var possibleTargetFiles = new List<FileInfo>();

            var targetsBaseFolder = ExecutorParameters.TargetsBaseFolder;
            if (string.IsNullOrEmpty(ExecutorParameters.TargetsBaseFolder))
            {
                targetsBaseFolder = Path.Combine(ExecutorParameters.OutputFolderBase, "Targets");
            }

            var dirinfo = new DirectoryInfo(targetsBaseFolder);
            foreach (var suffix in possibleFileSuffixs)
            {
                var fileInfos = dirinfo.GetFiles("*" + suffix);


                foreach (var fileInfo in fileInfos)
                {
                    if (fileInfo.Name.StartsWith(datasetName, StringComparison.OrdinalIgnoreCase))
                    {
                        possibleTargetFiles.Add(fileInfo);
                    }
                }
            }

            if (possibleTargetFiles.Count == 0)
            {
                return string.Empty;
            }

            if (possibleTargetFiles.Count == 1)
            {
                return possibleTargetFiles.First().FullName;
            }

            var sb = new StringBuilder();

            sb.Append("Error getting IQ target file. Multiple files were found for the dataset: " + datasetName + Environment.NewLine);
            sb.Append("Candidate IQ target files: " + Environment.NewLine);

            foreach (var possibleTargetFile in possibleTargetFiles)
            {
                sb.Append(possibleTargetFile.FullName + Environment.NewLine);
            }

            throw new NotSupportedException(sb.ToString());
        }


        public override void Execute()
        {
            try
            {
                SetupLogging();

                SetupAlignment();

                ReportGeneralProgress("Started Processing....");
                ReportGeneralProgress("Dataset = " + DatasetPath);
                ReportGeneralProgress("Parameters:" + "\n" + _workflowParameters.ToStringWithDetails());


                if (!RunIsInitialized)
                {
                    //create Run; load _peaks data; do alignment if desired
                    InitializeRun(DatasetPath);
                }

                ExecutePreProcessingHook();

                ProcessDataset();

                ExecutePostProcessingHook();

                ExportData();

                HandleAlignmentInfoFiles();

                if (RunIsDisposed)
                {
                    FinalizeRun();
                }

            }
            catch (Exception ex)
            {
                ReportGeneralProgress("--------------------------------------------------------------");
                ReportGeneralProgress("-------------------   ERROR    -------------------------------");
                ReportGeneralProgress("--------------------------------------------------------------");

                try
                {
                    FinalizeRun();
                }
                catch
                {
                    // Ignore errors
                }

                ReportGeneralProgress(ex.Message);
                ReportGeneralProgress(ex.StackTrace);


            }

        }

        protected void SetupAlignment()
        {
            string alignmentFolder;

            if (string.IsNullOrEmpty(ExecutorParameters.OutputFolderBase))
            {
                alignmentFolder = Path.Combine(RunUtilities.GetDatasetParentFolder(DatasetPath), "AlignmentInfo");
            }
            else
            {
                alignmentFolder = Path.Combine(ExecutorParameters.OutputFolderBase, "AlignmentInfo");
            }

            if (!Directory.Exists(alignmentFolder))
            {
                Directory.CreateDirectory(alignmentFolder);
            }

            _alignmentFolder = alignmentFolder;

        }


        protected virtual string GetTargetFilePathForIqAlignment()
        {
            if (Run == null)
            {
                IqLogger.LogError("Trying to get target file path for use in IqAlignment but Run is null.");
                return string.Empty;
            }

            if (string.IsNullOrEmpty(_alignmentFolder))
            {
                SetupAlignment();

            }

            //first look for _fht.txt file (MSGF output)
            var targetsForAlignmentFilePath = Path.Combine(_alignmentFolder, Run.DatasetName + "_msgf_fht.txt");

            if (File.Exists(targetsForAlignmentFilePath))
            {
                return targetsForAlignmentFilePath;
            }

            IqLogger.LogMessage("Trying to get target file path for use in IqAlignment, but no suitable targets file found. " +
                                "Suitable source files include: *_msgfplus_fht.txt");

            return string.Empty;
        }



        protected virtual void SetupLogging()
        {
            string loggingFolder;

            if (string.IsNullOrEmpty(ExecutorParameters.OutputFolderBase))
            {
                loggingFolder = RunUtilities.GetDatasetParentFolder(DatasetPath);
            }
            else
            {
                loggingFolder = Path.Combine(ExecutorParameters.OutputFolderBase, "Logs");
            }

            try
            {
                if (!Directory.Exists(loggingFolder))
                {
                    Directory.CreateDirectory(loggingFolder);
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Trying to set up logging folder but there was a critical error. Details:\n\n" + ex.Message, ex);
            }


            _loggingFileName = Path.Combine(loggingFolder, RunUtilities.GetDatasetName(DatasetPath) + "_log.txt");
        }


        public List<TargetedResultDTO> GetResults()
        {
            return ResultRepository.Results;
        }



        public bool RunIsInitialized
        {
            get
            {
                if (Run == null || Run.ResultCollection.MSPeakResultList.Count == 0)
                {
                    return false;
                }

                if (!DatasetPath.Contains(Run.DataSetPath))
                {
                    return false;
                }

                return true;
            }
        }


        public void ProcessDataset()
        {


            //apply mass calibration and NET alignment from .txt files, if they exist
            PerformAlignment();


            var runIsNotAligned = (!Run.MassIsAligned && !Run.NETIsAligned);     //if one of these two is aligned, the run is considered to be aligned

            //Perform targeted alignment if 1) run is not aligned  2) parameters permit it
#pragma warning disable 618
            if (runIsNotAligned && ExecutorParameters.TargetedAlignmentIsPerformed)
#pragma warning restore 618
            {
                Check.Ensure(MassTagsForTargetedAlignment != null && MassTagsForTargetedAlignment.TargetList.Count > 0, "MassTags for targeted alignment have not been defined. Check path within parameter file.");

                ReportGeneralProgress("Performing TargetedAlignment using mass tags from file: " + GetTargetFilePathForIqAlignment());
                ReportGeneralProgress("Total mass tags to be aligned = " + MassTagsForTargetedAlignment.TargetList.Count);

                TargetedAlignmentWorkflow = new TargetedAlignerWorkflow(TargetedAlignmentWorkflowParameters);
                TargetedAlignmentWorkflow.SetMassTags(MassTagsForTargetedAlignment.TargetList);
                TargetedAlignmentWorkflow.Run = Run;
                TargetedAlignmentWorkflow.Execute();

                ReportGeneralProgress("Targeted Alignment COMPLETE.");
                ReportGeneralProgress("Targeted Alignment Report: ");
                ReportGeneralProgress(TargetedAlignmentWorkflow.GetAlignmentReport1());

                PerformAlignment();     //now perform alignment, based on alignment .txt files that were outputted from the targetedAlignmentWorkflow




                TargetedAlignmentWorkflow.SaveFeaturesToTextfile(_alignmentFolder);
                if (Run.AlignmentInfo != null)
                {
                    TargetedAlignmentWorkflow.SaveAlignmentData(_alignmentFolder);
                }

                ReportGeneralProgress("MassAverage = \t" + TargetedAlignmentWorkflow.Aligner.Result.MassAverage.ToString("0.00000"));
                ReportGeneralProgress("MassStDev = \t" + TargetedAlignmentWorkflow.Aligner.Result.MassStDev.ToString("0.00000"));
                ReportGeneralProgress("NETAverage = \t" + TargetedAlignmentWorkflow.Aligner.Result.NETAverage.ToString("0.00000"));
                ReportGeneralProgress("NETStDev = \t" + TargetedAlignmentWorkflow.Aligner.Result.NETStDev.ToString("0.00000"));
                ReportGeneralProgress("---------------- END OF Alignment info -------------");
            }

            TargetedWorkflow.Run = Run;


            ResultRepository.Results.Clear();

            var mtCounter = 0;
            var totalTargets = Targets.TargetList.Count;

            ReportGeneralProgress("Processing...");

            foreach (var massTag in Targets.TargetList)
            {
                mtCounter++;

#if DEBUG
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

#endif


                Run.CurrentMassTag = massTag;
                try
                {
                    TargetedWorkflow.Execute();
                    ResultRepository.AddResult(TargetedWorkflow.Result);

                }
                catch (Exception ex)
                {
                    var errorString = "Error on MT\t" + massTag.ID + "\tchargeState\t" + massTag.ChargeState + "\t" + ex.Message + "\t" + ex.StackTrace;
                    ReportGeneralProgress(errorString, mtCounter);

                    throw;
                }

#if DEBUG
                stopwatch.Stop();
                Console.WriteLine(massTag.ID + "\tprocessing time = " + stopwatch.ElapsedMilliseconds);

#endif

                var progressString = "Percent complete = " + ((double)mtCounter / totalTargets * 100.0).ToString("0.0") + "\tTarget " + mtCounter + " of " + totalTargets;


                if (_backgroundWorker != null)
                {
                    if (_backgroundWorker.CancellationPending)
                    {
                        return;
                    }
                }

                ReportProcessingProgress(progressString, mtCounter);


            }

            ReportGeneralProgress("---- PROCESSING COMPLETE ---------------", 100);





        }


        protected virtual void ExportData()
        {

            var outputFileName = GetOutputFileName();
            backupResultsFileIfNecessary(Run.DatasetName, outputFileName);

            var exporter = TargetedResultToTextExporter.CreateExporter(_workflowParameters, outputFileName);
            exporter.ExportResults(ResultRepository.Results);
        }



        /// <summary>
        /// This hook allows inheriting class to execute post processing methods. e.g. see TopDownTargetedWorkflowExecutor
        /// </summary>
        protected virtual void ExecutePostProcessingHook() { }

        /// <summary>
        /// This hook allows inheriting class to execute pre processing methods.
        /// </summary>
        protected virtual void ExecutePreProcessingHook() { }

        /// <summary>
        /// Output file path
        /// </summary>
        /// <returns></returns>
        protected virtual string GetOutputFileName()
        {
            var outputFilePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(_resultsFolder))
                outputFilePath = _resultsFolder;

            return Path.Combine(outputFilePath, Run.DatasetName + "_results.txt");
        }

        #endregion

        #region Private Methods
        protected string GetResultsFolder(string baseFolder)
        {
            string resultsFolder;

            if (string.IsNullOrWhiteSpace(baseFolder))
            {
                if (Directory.Exists(DatasetPath))
                    resultsFolder = Path.Combine(DatasetPath, "Results");
                else
                {
                    var fiDatasetFile = new FileInfo(DatasetPath);
                    resultsFolder = Path.Combine(fiDatasetFile.Directory.FullName, "Results");
                }
            }
            else
            {
                resultsFolder = Path.Combine(baseFolder, "Results");
            }


            var dirinfo = new DirectoryInfo(resultsFolder);

            if (!dirinfo.Exists)
            {
                dirinfo.Create();
            }

            return dirinfo.FullName;

        }

        protected TargetCollection GetMassTagTargets(string massTagFileName)
        {
            return GetMassTagTargets(massTagFileName, new List<int>());
        }

        protected TargetCollection GetMassTagTargets(string massTagFileName, List<int> targetIDsToFilterOn)
        {
            if (string.IsNullOrEmpty(massTagFileName) || !File.Exists(massTagFileName))
            {
                return new TargetCollection();
            }


            if (massTagFileName.ToLower().Contains("_msgfplus.tsv"))
            {
                var iqTargetImporter = new BasicIqTargetImporter(massTagFileName);
                var iqTargets = iqTargetImporter.Import();

                var targetUtilities = new IqTargetUtilities();
                var targetCollection = new TargetCollection
                {
                    TargetList = new List<TargetBase>()
                };

                foreach (var iqTarget in iqTargets)
                {
                    if (iqTarget.QualityScore > MsgfFdrScoreCutoff) continue;
                    targetUtilities.UpdateTargetMissingInfo(iqTarget);

                    TargetBase oldStyleTarget = new PeptideTarget();
                    oldStyleTarget.ChargeState = (short)iqTarget.ChargeState;
                    oldStyleTarget.Code = iqTarget.Code;
                    oldStyleTarget.EmpiricalFormula = iqTarget.EmpiricalFormula;
                    oldStyleTarget.ID = iqTarget.ID;
                    oldStyleTarget.MZ = iqTarget.MZTheor;
                    oldStyleTarget.MonoIsotopicMass = iqTarget.MonoMassTheor;
                    oldStyleTarget.ScanLCTarget = iqTarget.ScanLC;
                    oldStyleTarget.NormalizedElutionTime = (float)iqTarget.ElutionTimeTheor;

                    oldStyleTarget.ElutionTimeUnit = DeconTools.Backend.Globals.ElutionTimeUnit.ScanNum;
                    targetCollection.TargetList.Add(oldStyleTarget);
                }

                return targetCollection;
            }


            var importer = new MassTagFromTextFileImporter(massTagFileName);
            return importer.Import(targetIDsToFilterOn);
        }

        protected string GetLogFileName(string folderPath)
        {
            var logfolderPath = new DirectoryInfo(folderPath);

            if (!logfolderPath.Exists) logfolderPath.Create();

            // The timestamp for the log file name is today's date, plus the number of milliseconds since midnight
            var logfilename = Path.Combine(logfolderPath.FullName,
                                           "logfile_" +
                                           DateTime.Now.ToString("yyyy-MM-dd") + "_" +
                                           DateTime.Now.Subtract(DateTime.Today).TotalMilliseconds.ToString("0") +
                                           ".txt");

            return logfilename;

        }

        protected List<string> GetListDatasetPaths(string fileContainingDatasetPaths)
        {
            var datasetPathList = new List<string>();

            using (var sr = new StreamReader(fileContainingDatasetPaths))
            {

                while (!sr.EndOfStream)
                {
                    datasetPathList.Add(sr.ReadLine());

                }
                sr.Close();
            }

            return datasetPathList;

        }

        protected void ReportGeneralProgress(string generalProgressString, int progressPercent = 0)
        {
            if (_backgroundWorker == null)
            {
                if (generalProgressString.IndexOf('\r') >= 0 ||
                    generalProgressString.IndexOf('\n') >= 0)
                {
                    IqLogger.LogMessage(generalProgressString);
                }
                else
                {
                    IqLogger.LogMessage(generalProgressString);
                    Console.WriteLine();
                }
            }
            else
            {
                _progressInfo.ProgressInfoString = generalProgressString;
                _progressInfo.IsGeneralProgress = true;
                _backgroundWorker.ReportProgress(progressPercent, _progressInfo);
            }

            writeToLogFile(DateTime.Now + "\t" + generalProgressString);
        }


        protected void ReportProcessingProgress(string reportString, int progressCounter)
        {

            if (_backgroundWorker == null)
            {
                if (progressCounter % 100 == 0)
                {
                    Console.WriteLine(DateTime.Now + "\t" + reportString);
                }

            }
            else
            {
                var progressPercent = (int)(progressCounter * 100 / (double)Targets.TargetList.Count);

                _progressInfo.ProgressInfoString = reportString;
                _progressInfo.IsGeneralProgress = false;
                _progressInfo.Result = TargetedWorkflow.Result;
                _progressInfo.Time = DateTime.Now;

                _progressInfo.ChromatogramXYData = new XYData
                {
                    Xvalues = TargetedWorkflow.ChromatogramXYData.Xvalues,
                    Yvalues = TargetedWorkflow.ChromatogramXYData.Yvalues
                };

                _progressInfo.MassSpectrumXYData = new XYData
                {
                    Xvalues = TargetedWorkflow.MassSpectrumXYData.Xvalues,
                    Yvalues = TargetedWorkflow.MassSpectrumXYData.Yvalues
                };

                _backgroundWorker.ReportProgress(progressPercent, _progressInfo);
            }

            if (progressCounter % 100 == 0)
            {
                writeToLogFile(DateTime.Now + "\t" + reportString);
            }

        }

        protected void writeToLogFile(string stringToWrite)
        {

            if (!string.IsNullOrEmpty(_loggingFileName))
            {
                using (var sw = new StreamWriter(new FileStream(_loggingFileName, FileMode.Append,
                              FileAccess.Write, FileShare.Read)))
                {
                    sw.AutoFlush = true;
                    sw.WriteLine(stringToWrite);
                    sw.Flush();

                }
            }

        }

        protected void HandleAlignmentInfoFiles()
        {
            var attr = File.GetAttributes(Run.Filename);


            FileInfo[] datasetRelatedFiles;

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var dirInfo = new DirectoryInfo(Run.Filename);
                datasetRelatedFiles = dirInfo.GetFiles(Run.DatasetName + "*.txt");



            }
            else
            {
                var fi = new FileInfo(Run.Filename);
                var dirInfo = fi.Directory;
                datasetRelatedFiles = dirInfo.GetFiles(Run.DatasetName + "*.txt");

            }

            foreach (var file in datasetRelatedFiles)
            {
                if (file.Name.Contains("_alignedFeatures") || file.Name.Contains("_MZAlignment") || file.Name.Contains("_NETAlignment"))
                {
                    var targetCopiedFilename = Path.Combine(_alignmentFolder, file.Name);

                    //upload alignment data only if it doesn't already exist
                    if (!File.Exists(targetCopiedFilename))
                    {
                        file.CopyTo(Path.Combine(_alignmentFolder, file.Name), overwrite: false);
                    }

                    if (ExecutorParameters.CopyRawFileLocal)
                    {
                        file.Delete();       //if things were copied locally, we are going to delete anything created.
                    }

                }

            }


        }

        public void InitializeRun(string dataset)
        {
            string runFilename;


            if (ExecutorParameters.CopyRawFileLocal)
            {
                ReportGeneralProgress("Started copying raw data to local folder: " + ExecutorParameters.FolderPathForCopiedRawDataset);

                var attr = File.GetAttributes(dataset);

                DirectoryInfo targetDirInfo;
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    var sourceDirInfo = new DirectoryInfo(dataset);
                    runFilename = Path.Combine(ExecutorParameters.FolderPathForCopiedRawDataset, sourceDirInfo.Name);
                    targetDirInfo = new DirectoryInfo(runFilename);
                    FileUtilities.CopyAll(sourceDirInfo, targetDirInfo);
                    ReportGeneralProgress("Copying complete.");
                }
                else
                {
                    var fileinfo = new FileInfo(dataset);
                    runFilename = Path.Combine(ExecutorParameters.FolderPathForCopiedRawDataset, Path.GetFileName(dataset));

                    targetDirInfo = new DirectoryInfo(ExecutorParameters.FolderPathForCopiedRawDataset);

                    if (!File.Exists(runFilename))
                    {
                        FileUtilities.CopyAll(fileinfo, targetDirInfo);
                        ReportGeneralProgress("Copying complete.");
                    }
                    else
                    {
                        ReportGeneralProgress("Datafile already exists on local drive. Using existing datafile.");

                    }

                }

            }
            else
            {
                runFilename = dataset;
            }

            //create Run
            var rf = new RunFactory();
            Run = rf.CreateRun(runFilename);

            var runInstantiationFailed = (Run == null);
            if (runInstantiationFailed)
            {
                ReportGeneralProgress("Run initialization FAILED. Likely a filename problem. Or missing manufacturer .dlls");
                return;
            }

            ReportGeneralProgress("Run initialized successfully.");


            //Retrieve alignment data if it exists
            CopyAlignmentInfoIfExists();




            //check and load chrom source data (_peaks.txt)
            var peaksFileExists = CheckForPeaksFile();
            if (!peaksFileExists)
            {
                ReportGeneralProgress("Creating _Peaks.txt file for extracted ion chromatogram (XIC) source data ... takes 1-5 minutes");

                CreatePeaksForChromSourceData();
                ReportGeneralProgress("Done creating _Peaks.txt file");
            }
            else
            {
                ReportGeneralProgress("Using existing _Peaks.txt file");
            }


            ReportGeneralProgress("Peak loading started...");


            var baseFileName = Path.Combine(Run.DataSetPath, Run.DatasetName);

            var possibleFilename1 = baseFileName + "_peaks.txt";

            if (File.Exists(possibleFilename1))
            {
                //create background worker so that updates don't go out to console.
                //BackgroundWorker bw = new BackgroundWorker();
                //bw.WorkerSupportsCancellation = true;
                //bw.WorkerReportsProgress = true;

                //TODO: keep an eye on errors connected to background worker here.
                var peakImporter = new PeakImporterFromText(possibleFilename1, _backgroundWorker);

                peakImporter.ImportPeaks(Run.ResultCollection.MSPeakResultList);
            }
            else
            {
                ReportGeneralProgress("CRITICAL FAILURE. Chrom source data (_peaks.txt) file not loaded.");
                return;
            }

            // Grab the primary LC Scan numbers if they are not already filled out
            if (!Run.PrimaryLcScanNumbers.Any())
            {
                Run.PrimaryLcScanNumbers = RunUtilities.FindPrimaryLcScanNumbers(Run.ResultCollection.MSPeakResultList);
            }

            ReportGeneralProgress("Peak Loading complete.");
        }

        private void CopyAlignmentInfoIfExists()
        {
            if (string.IsNullOrEmpty(_alignmentFolder)) return;

            var dirInfo = new DirectoryInfo(_alignmentFolder);

            if (dirInfo.Exists)
            {

                var datasetRelatedFiles = dirInfo.GetFiles(Run.DatasetName + "*.txt");

                foreach (var file in datasetRelatedFiles)
                {
                    if (file.Name.ToLower() == Run.DatasetName.ToLower() + "_mzalignment.txt" || file.Name.ToLower() == Run.DatasetName.ToLower() + "_netalignment.txt")
                    {
                        var targetFileName = Path.Combine(Run.DataSetPath, file.Name);
                        if (!File.Exists(targetFileName))
                        {
                            file.CopyTo(Path.Combine(Run.DataSetPath, file.Name), true);
                        }
                    }


                }


            }
        }

        protected void PerformAlignment()
        {
            if (!string.IsNullOrEmpty(_alignmentFolder))
            {
                RunUtilities.AlignRunUsingAlignmentInfoInFiles(Run, _alignmentFolder);
            }


            if (Run.MassIsAligned)
            {
                ReportGeneralProgress("Run has been mass aligned");
            }
            else
            {
                ReportGeneralProgress("FYI - Run has NOT been mass aligned.");
            }

            if (Run.NETIsAligned)
            {
                ReportGeneralProgress("Run has been NET aligned using info in either the _NETAlignment.txt file or the _UMCs.txt file");
            }
            else
            {
                ReportGeneralProgress("Warning - Run has NOT been NET aligned.");
            }
        }

        private void CreatePeaksForChromSourceData()
        {
            var parameters = new PeakDetectAndExportWorkflowParameters();
            var deconParam = (TargetedWorkflowParameters)_workflowParameters;

            parameters.PeakBR = deconParam.ChromGenSourceDataPeakBR;
            parameters.PeakFitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;
            parameters.SigNoiseThreshold = deconParam.ChromGenSourceDataSigNoise;
            parameters.ProcessMSMS = deconParam.ProcessMsMs;
            var peakCreator = new PeakDetectAndExportWorkflow(Run, parameters, _backgroundWorker);
            peakCreator.Execute();
        }

        private bool CheckForPeaksFile()
        {
            var baseFileName = Path.Combine(Run.DataSetPath, Run.DatasetName);

            var possibleFilename1 = baseFileName + "_peaks.txt";

            if (File.Exists(possibleFilename1))
            {
                return true;
            }

            return false;
        }

        protected TargetCollection CreateTargets(Globals.TargetType targetType, string targetFilePath)
        {
            if (string.IsNullOrEmpty(targetFilePath)) return null;

            switch (targetType)
            {
                case Globals.TargetType.LcmsFeature:
                    return GetLcmsFeatureTargets(targetFilePath);

                case Globals.TargetType.DatabaseTarget:
                    return GetMassTagTargets(targetFilePath);

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetType));
            }
        }


        protected virtual TargetCollection GetLcmsFeatureTargets(string targetsFilePath)
        {
            if (targetsFilePath.ToLower().Contains("_msgf"))
            {
                var iqTargetImporter = new BasicIqTargetImporter(targetsFilePath);
                var iqTargets = iqTargetImporter.Import();

                var targetUtilities = new IqTargetUtilities();
                var targetCollection = new TargetCollection { TargetList = new List<TargetBase>() };

                foreach (var iqTarget in iqTargets)
                {
                    if (iqTarget.QualityScore > MsgfFdrScoreCutoff) continue;
                    targetUtilities.UpdateTargetMissingInfo(iqTarget);

                    TargetBase oldStyleTarget = new LcmsFeatureTarget();
                    oldStyleTarget.ChargeState = (short)iqTarget.ChargeState;
                    oldStyleTarget.Code = iqTarget.Code;
                    oldStyleTarget.EmpiricalFormula = iqTarget.EmpiricalFormula;
                    oldStyleTarget.ID = iqTarget.ID;
                    oldStyleTarget.MZ = iqTarget.MZTheor;
                    oldStyleTarget.MonoIsotopicMass = iqTarget.MonoMassTheor;
                    oldStyleTarget.ScanLCTarget = iqTarget.ScanLC;
                    oldStyleTarget.NormalizedElutionTime = (float)iqTarget.ElutionTimeTheor;

                    oldStyleTarget.ElutionTimeUnit = DeconTools.Backend.Globals.ElutionTimeUnit.ScanNum;
                    targetCollection.TargetList.Add(oldStyleTarget);
                }

                return targetCollection;
            }

            var importer =
               new LcmsTargetFromFeaturesFileImporter(targetsFilePath);

            var lcmsTargetCollection = importer.Import();
            return lcmsTargetCollection;
        }

        protected virtual TargetedResultToTextExporter createExporter(string outputFileName)
        {
            throw new NotImplementedException();
        }


        protected void FinalizeRun()
        {

            var runfileName = Run.Filename;
            var datasetName = Run.DatasetName;

            Run.Close();
            Run = null;
            GC.Collect();

            if (!ExecutorParameters.CopyRawFileLocal || !ExecutorParameters.DeleteLocalDatasetAfterProcessing)
            {
                return;
            }

            var attr = File.GetAttributes(runfileName);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var dirInfo = new DirectoryInfo(runfileName);
                dirInfo.Delete(true);
            }
            else
            {
                var fileinfo = new FileInfo(runfileName);

                var fileSuffix = fileinfo.Extension;

                var dirInfo = fileinfo.Directory;

                var expectedPeaksFile = Path.Combine(dirInfo.FullName, datasetName + "_peaks.txt");

                if (File.Exists(expectedPeaksFile))
                {
                    //File.Delete(expectedPeaksFile);
                }

                var allRawDataFiles = dirInfo.GetFiles("*" + fileSuffix);
                if (allRawDataFiles.Length <= 35)
                {
                    return;
                }

                foreach (var file in allRawDataFiles)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception)
                    {
                        // Ignore errors here
                    }


                }
            }
        }

        protected void backupResultsFileIfNecessary(string datasetName, string outputFileName)
        {
            var outputFileInfo = new FileInfo(outputFileName);

            if (!outputFileInfo.Exists)
            {
                return;
            }

            var backupFolder = Path.Combine(_resultsFolder, "Backup");
            var backupFolderInfo = new DirectoryInfo(backupFolder);

            if (!backupFolderInfo.Exists)
            {
                backupFolderInfo.Create();
            }

            var backupFilename = Path.Combine(backupFolderInfo.FullName, datasetName + "_results.txt");
            outputFileInfo.CopyTo(backupFilename, true);

            outputFileInfo.Delete();
        }
        #endregion

    }
}
