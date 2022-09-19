using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Backend.Workflows;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class IqExecutor
    {
        protected BackgroundWorker _backgroundWorker;
        protected TargetedWorkflowExecutorProgressInfo _progressInfo;

        private readonly IqResultUtilities _iqResultUtilities = new IqResultUtilities();
        private readonly IqTargetUtilities _targetUtilities = new IqTargetUtilities();
        // Unused: private RunFactory _runFactory = new RunFactory();

        private string _resultsDirectory;
        private string _alignmentFolder;

        #region Constructors

        public IqExecutor(WorkflowExecutorBaseParameters parameters, Run run)
        {
            Results = new List<IqResult>();
            IsDataExported = true;
            DisposeResultDetails = true;
            _parameters = parameters;
            _run = run;
            SetupLogging();
            IqLogger.LogMessage("Log started for dataset: " + _run.DatasetName);
            Console.WriteLine();
            IqLogger.LogMessage("Parameters: " + Environment.NewLine + _parameters.ToStringWithDetails());
        }

        public IqExecutor(WorkflowExecutorBaseParameters parameters, Run run, BackgroundWorker backgroundWorker = null)
        {
            Results = new List<IqResult>();
            IsDataExported = true;
            DisposeResultDetails = true;
            _parameters = parameters;
            _run = run;
            SetupLogging();
            IqLogger.LogMessage("Log started for dataset: " + _run.DatasetName);
            Console.WriteLine();
            IqLogger.LogMessage("Parameters: " + Environment.NewLine + _parameters.ToStringWithDetails());

            _backgroundWorker = backgroundWorker;
            _progressInfo = new TargetedWorkflowExecutorProgressInfo();
        }

        #endregion

        #region Properties

        protected IqMassAndNetAligner IqMassAndNetAligner { get; set; }

        private Run _run;
        protected Run Run
        {
            get => _run;
            set
            {
                _run = value;
                SetupLogging();
            }
        }

        private WorkflowExecutorBaseParameters _parameters;

        protected WorkflowExecutorBaseParameters Parameters
        {
            get => _parameters;
            set
            {
                _parameters = value;
                SetupLogging();
            }
        }

        protected bool IsDataExported { get; set; }

        public bool DisposeResultDetails { get; set; }

        public IqTargetImporter TargetImporter { get; set; }

        public string ChromSourceDataFilePath { get; set; }

        public List<IqResult> Results { get; set; }

        public List<IqTarget> Targets { get; set; }

        protected IqResultExporter IqResultExporter { get; set; }

        public TargetedWorkflowParameters IqWorkflowParameters { get; set; }

        /// <summary>
        /// We need this setting when targets are being created. Indicates whether a target is cysteine-modified (reduced & alkylated, +57).
        /// TODO: move this to the IqTarget creator class, once developed.
        /// </summary>
        public bool IqTargetsAreCysteineModified { get; set; }

        protected bool ChromDataIsLoaded => Run?.ResultCollection.MSPeakResultList.Count > 0;

        protected bool RunIsInitialized => throw new NotImplementedException();

        #endregion

        #region Public Methods

        public void SetupMassAndNetAlignment(string alignmentFolder = "")
        {
            WorkflowExecutorBaseParameters massNetAlignerParameters = new BasicTargetedWorkflowExecutorParameters
            {
                OutputDirectoryBase = Parameters.OutputDirectoryBase
            };

            IqMassAndNetAligner = new IqMassAndNetAligner(massNetAlignerParameters, Run)
            {
                LoessBandwidthNetAlignment = 0.1
            };

            //check if alignment info exists already

            SetupAlignmentFolder(alignmentFolder);

            var expectedAlignmentFilename = Path.Combine(_alignmentFolder, Run.DatasetName + "_iqAlignmentResults.txt");
            var alignmentResultsExist = (File.Exists(expectedAlignmentFilename));

            if (alignmentResultsExist)
            {
                IqLogger.LogMessage("Using the IQ alignment results from here: " + expectedAlignmentFilename);
                IqMassAndNetAligner.LoadPreviousIqResults(expectedAlignmentFilename);

                SetMassTagReferencesForNetAlignment();
                return;
            }

            //Get a suitable targets file for alignment. These are grabbed from the ..\AlignmentInfo folder.
            var targetFileForAlignment = GetTargetFilePathForIqAlignment(out var candidateTargetsFilePath);

            if (string.IsNullOrEmpty(targetFileForAlignment))
            {
                IqLogger.LogMessage("Alignment not performed - No suitable target file found for use in alignment.");
                IqLogger.LogDebug("Considered file paths\n" +
                                  "  " + PRISM.PathUtils.CompactPathString(expectedAlignmentFilename, 130) + " and\n" +
                                  "  " + PRISM.PathUtils.CompactPathString(candidateTargetsFilePath, 130));
                return;
            }

            if (!File.Exists(targetFileForAlignment))
            {
                IqLogger.LogWarning("Alignment not performed - Target file for alignment has been specified but a FILE NOT FOUND error has occured.");
                return;
            }

            var isFirstHitsFile = targetFileForAlignment.EndsWith("_fht.txt");

            if (!isFirstHitsFile)
            {
                IqLogger.LogWarning("Alignment not performed - target file for alignment must be a first hits file (_fht.txt)");
                return;
            }

            IqMassAndNetAligner.LoadAndInitializeTargets(targetFileForAlignment);

            SetMassTagReferencesForNetAlignment();
        }

        private void SetMassTagReferencesForNetAlignment()
        {
            if (!string.IsNullOrEmpty(Parameters.ReferenceTargetsFilePath))
            {
                IqTargetImporter massTagImporter = new BasicIqTargetImporter(Parameters.ReferenceTargetsFilePath);
                var massTagRefs = massTagImporter.Import();

                IqMassAndNetAligner.SetMassTagReferences(massTagRefs);
                IqLogger.LogMessage("IQ Net aligner - " + massTagRefs.Count + " reference targets were loaded successfully.");
            }
            else
            {
                IqLogger.LogMessage("IQ Net aligner INACTIVE - no reference tags were loaded. You need to define 'TargetsUsedForLookupFilePath'");
            }
        }

        public void DoAlignment()
        {
            if (Parameters.IsMassAlignmentPerformed)
            {
                Run.MassAlignmentInfo = IqMassAndNetAligner.DoMassAlignment();
            }

            if (Parameters.IsNetAlignmentPerformed)
            {
                Run.NetAlignmentInfo = IqMassAndNetAligner.DoNetAlignment();
            }

            if (Parameters.IsMassAlignmentPerformed || Parameters.IsMassAlignmentPerformed)
            {
                var exportedAlignmentIqResultsFilename = Path.Combine(_alignmentFolder, Run.DatasetName + "_iqAlignmentResults.txt");

                IqMassAndNetAligner.ExportResults(exportedAlignmentIqResultsFilename);

                var exportedGraphBaseFilename = Path.Combine(_alignmentFolder, Run.DatasetName);

                IqMassAndNetAligner.ExportGraphs(exportedGraphBaseFilename);
            }
        }

        public void Execute()
        {
            Execute(Targets);
        }

        public void Execute(List<IqTarget> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                IqLogger.LogMessage("WARNING - No targets loaded.");
                return;
            }

            var totalTargets = targets.Count;
            var targetCount = 1;
            var lastProgress = DateTime.UtcNow;

            IqLogger.LogMessage("Total targets being processed: " + totalTargets);
            IqLogger.LogMessage("Processing...");

            foreach (var target in targets)
            {
                Run = target.GetRun();

                if (!ChromDataIsLoaded)
                {
                    LoadChromData(Run);
                }

                ReportGeneralProgress(targetCount, totalTargets, ref lastProgress);

                target.DoWorkflow();
                var result = target.GetResult();

                if (IsDataExported)
                {
                    ExportResults(result);
                }

                Results.Add(result);

                if (DisposeResultDetails)
                {
                    result.Dispose();
                }
                targetCount++;
            }

            IqLogger.LogMessage("Processing Complete!");
            Console.WriteLine();
            Console.WriteLine();
        }

        public virtual void LoadAndInitializeTargets()
        {
            LoadAndInitializeTargets(Parameters.TargetsFilePath);
        }

        public virtual void LoadAndInitializeTargets(string targetsFilePath)
        {
            if (TargetImporter == null)
            {
                TargetImporter = new BasicIqTargetImporter(targetsFilePath);
            }

            IqLogger.LogMessage("Target Loading Started...");

            Targets = TargetImporter.Import();

            foreach (var iqTarget in Targets)
            {
                iqTarget.ChargeState = 0;    //parent has a 0 charge state

                if (Parameters.TargetType == Globals.TargetType.LcmsFeature)
                {
                    iqTarget.ElutionTimeTheor = Run.NetAlignmentInfo.GetNETValueForScan(iqTarget.ScanLC);
                }
            }

            _targetUtilities.CreateChildTargets(Targets, Parameters.MinMzForDefiningChargeStateTargets,
                Parameters.MaxMzForDefiningChargeStateTargets, Parameters.MaxNumberOfChargeStateTargetsToCreate, IqTargetsAreCysteineModified);

            IqLogger.LogMessage("Targets Loaded Successfully. Total targets loaded= " + Targets.Count);
        }

        protected virtual void ExportResults(IqResult iqResult)
        {
            var resultsForExport = _iqResultUtilities.FlattenOutResultTree(iqResult);

            var orderedResults = resultsForExport.OrderBy(p => p.Target.ChargeState).ToList();

            var exportedResults = orderedResults.Where(orderedResult => orderedResult.IsExported).ToList();

            if (IqResultExporter == null)
            {
                IqResultExporter = iqResult.Target.Workflow.CreateExporter();
            }

            SetupResultsFolder();

            string resultsFileName;
            if (_parameters.AppendTargetsFileNameToResultFile && !string.IsNullOrWhiteSpace(_parameters.TargetsFilePath))
            {
                var targetsFileName = Path.GetFileNameWithoutExtension(_parameters.TargetsFilePath);

                // Compare targetsFileName to the dataset name
                // Skip any characters that are in common
                var startIndex = 0;
                for (var i = 1; i < targetsFileName.Length; i++)
                {
                    if (!Run.DatasetName.StartsWith(targetsFileName.Substring(0, i), StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }

                    startIndex = i;
                }

                if (startIndex > 0 && startIndex < targetsFileName.Length - 1)
                {
                    while (targetsFileName[startIndex] == '_' && startIndex < targetsFileName.Length - 1)
                    {
                        startIndex++;
                    }
                    resultsFileName = Run.DatasetName + "_" + targetsFileName.Substring(startIndex) + "_iqResults.txt";
                }
                else
                {
                    resultsFileName = Run.DatasetName + "_" + targetsFileName + "_iqResults.txt";
                }
            }
            else
            {
                resultsFileName = Run.DatasetName + "_iqResults.txt";
            }

            IqResultExporter.WriteOutResults(Path.Combine(_resultsDirectory, resultsFileName), exportedResults);
        }

        private void SetupAlignmentFolder(string alignmentFolder = "")
        {
            if (!string.IsNullOrEmpty(alignmentFolder))
            {
                _alignmentFolder = alignmentFolder;
            }
            else if (string.IsNullOrEmpty(Parameters.OutputDirectoryBase))
            {
                _alignmentFolder = Path.Combine(GetDefaultOutputDirectory(), "AlignmentInfo");
            }
            else
            {
                _alignmentFolder = Path.Combine(Parameters.OutputDirectoryBase, "AlignmentInfo");
            }

            if (!Directory.Exists(_alignmentFolder))
            {
                Directory.CreateDirectory(_alignmentFolder);
            }
        }

        /// <summary>
        /// Look for the target file path for use in IqAlignment
        /// </summary>
        /// <param name="candidateTargetsFilePath">File that this method looks for</param>
        /// <returns>File path if found, otherwise empty string</returns>
        protected virtual string GetTargetFilePathForIqAlignment(out string candidateTargetsFilePath)
        {
            if (Run == null)
            {
                IqLogger.LogError("Trying to get target file path for use in IqAlignment but Run is null.");
                candidateTargetsFilePath = string.Empty;
                return string.Empty;
            }

            if (string.IsNullOrEmpty(_alignmentFolder))
            {
                SetupAlignmentFolder(_alignmentFolder);
            }

            //first look for _fht.txt file (MSGF output)
            candidateTargetsFilePath = Path.Combine(_alignmentFolder, Run.DatasetName + "_msgfplus_fht.txt");

            if (File.Exists(candidateTargetsFilePath))
            {
                return candidateTargetsFilePath;
            }

            return string.Empty;
        }

        protected virtual void ReportGeneralProgress(int currentTarget, int totalTargets, ref DateTime lastProgress, int progressIntervalSeconds = 10)
        {
            var currentProgress = (currentTarget / (double)totalTargets);

            if (DateTime.UtcNow.Subtract(lastProgress).TotalSeconds >= progressIntervalSeconds)
            {
                lastProgress = DateTime.UtcNow;
                IqLogger.LogMessage("Processing target " + currentTarget + " of " + totalTargets + "; " + (Math.Round(currentProgress * 100, 1)) + "% Complete.");
            }

            _backgroundWorker?.ReportProgress(Convert.ToInt16(currentProgress * 100));
        }

        private void SetupResultsFolder()
        {
            if (string.IsNullOrEmpty(Parameters.OutputDirectoryBase))
            {
                _resultsDirectory = GetDefaultOutputDirectory();
            }
            else
            {
                _resultsDirectory = Path.Combine(Parameters.OutputDirectoryBase, "IqResults");
            }

            if (!Directory.Exists(_resultsDirectory))
            {
                Directory.CreateDirectory(_resultsDirectory);
            }
        }

        #endregion

        #region Private Methods

        private string CreatePeaksForChromSourceData()
        {
            var parameters = new PeakDetectAndExportWorkflowParameters
            {
                PeakBR = Parameters.ChromGenSourceDataPeakBR,
                PeakFitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC,
                SigNoiseThreshold = Parameters.ChromGenSourceDataSigNoise,
                ProcessMSMS = Parameters.ChromGenSourceDataProcessMsMs,
                IsDataThresholded = Parameters.ChromGenSourceDataIsThresholded,
                LCScanMin = Run.GetMinPossibleLCScanNum(),
                LCScanMax = Run.GetMaxPossibleLCScanNum()
            };

            var peakCreator = new PeakDetectAndExportWorkflow(Run, parameters, _backgroundWorker);
            peakCreator.Execute();

            var peaksFilename = Path.Combine(Run.DatasetDirectoryPath, Run.DatasetName + "_peaks.txt");
            return peaksFilename;
        }

        private string GetPossiblePeaksFile()
        {
            var baseFileName = Path.Combine(Run.DatasetDirectoryPath, Run.DatasetName);

            var possibleFilename1 = baseFileName + "_peaks.txt";

            if (File.Exists(possibleFilename1))
            {
                return possibleFilename1;
            }

            return string.Empty;
        }

        public void LoadChromData(Run run)
        {
            if (string.IsNullOrEmpty(ChromSourceDataFilePath))
            {
                ChromSourceDataFilePath = GetPossiblePeaksFile();
            }

            if (string.IsNullOrEmpty(ChromSourceDataFilePath))
            {
                Console.WriteLine("Creating _Peaks.txt file for " + Run.DatasetName + " at " + Run.DatasetDirectoryPath);
                IqLogger.LogMessage("Creating _Peaks.txt");
                ChromSourceDataFilePath = CreatePeaksForChromSourceData();
            }
            else
            {
                IqLogger.LogMessage("Using Existing _Peaks.txt");
            }

            IqLogger.LogMessage("Peak Loading Started...");

            var peakImporter = new PeakImporterFromText(ChromSourceDataFilePath, _backgroundWorker);
            peakImporter.ImportPeaks(Run.ResultCollection.MSPeakResultList);

            IqLogger.LogMessage("Peak Loading Complete. Number of peaks loaded= " + Run.ResultCollection.MSPeakResultList.Count.ToString("#,##0"));
        }

        private void SetupLogging()
        {
            string loggingDirectory;
            if (string.IsNullOrEmpty(Parameters.OutputDirectoryBase))
            {
                loggingDirectory = GetDefaultOutputDirectory();
            }
            else
            {
                loggingDirectory = Path.Combine(Parameters.OutputDirectoryBase, "IqLogs");
            }

            if (!Directory.Exists(loggingDirectory))
            {
                Directory.CreateDirectory(loggingDirectory);
            }

            IqLogger.InitializeIqLog(_run.DatasetName, loggingDirectory);
        }

        private string GetDefaultOutputDirectory()
        {
            var defaultOutputDirectory = _run.DatasetDirectoryPath;
            return defaultOutputDirectory;
        }

        #endregion

    }
}
