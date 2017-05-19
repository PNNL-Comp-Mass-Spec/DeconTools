using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Runs;
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
        private RunFactory _runFactory = new RunFactory();

        private string _resultsFolder;
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
            IqLogger.Log.Info("Log started for dataset: " + _run.DatasetName);
            IqLogger.Log.Info(Environment.NewLine + "Parameters: " + Environment.NewLine + _parameters.ToStringWithDetails());
        }

        public IqExecutor(WorkflowExecutorBaseParameters parameters, Run run, BackgroundWorker backgroundWorker = null)
        {
            Results = new List<IqResult>();
            IsDataExported = true;
            DisposeResultDetails = true;
            _parameters = parameters;
            _run = run;
            SetupLogging();
            IqLogger.Log.Info("Log started for dataset: " + _run.DatasetName);
            IqLogger.Log.Info(Environment.NewLine + "Parameters: " + Environment.NewLine + _parameters.ToStringWithDetails());

            _backgroundWorker = backgroundWorker;
            _progressInfo = new TargetedWorkflowExecutorProgressInfo();
        }

        #endregion

        #region Properties


        protected IqMassAndNetAligner IqMassAndNetAligner { get; set; }

        private Run _run;
        protected Run Run { 
            get { return _run; }
            set 
            { 
                _run = value;
                SetupLogging();
            } 
        }

        private WorkflowExecutorBaseParameters _parameters;
        

        protected WorkflowExecutorBaseParameters Parameters 
        { 
            get { return _parameters; }
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

        protected bool ChromDataIsLoaded
        {
            get
            {
                if (Run != null)
                {
                    return Run.ResultCollection.MSPeakResultList.Count > 0;
                }

                return false;
            }
        }

        protected bool RunIsInitialized
        {
            get { throw new NotImplementedException(); }
        }

    
        #endregion

        #region Public Methods


        public void SetupMassAndNetAlignment(string alignmentFolder="")
        {
            WorkflowExecutorBaseParameters massNetAlignerParameters = new BasicTargetedWorkflowExecutorParameters();
            

            IqMassAndNetAligner = new IqMassAndNetAligner(massNetAlignerParameters, Run);
            IqMassAndNetAligner.LoessBandwidthNetAlignment = 0.1;

            //check if alignment info exists already

            SetupAlignmentFolder(alignmentFolder);

            var expectedAlignmentFilename = Path.Combine(_alignmentFolder, Run.DatasetName + "_iqAlignmentResults.txt");
            var alignmentResultsExist = (File.Exists(expectedAlignmentFilename));

            if (alignmentResultsExist)
            {
                IqLogger.Log.Info("Using the IQ alignment results from here: " + expectedAlignmentFilename);
                IqMassAndNetAligner.LoadPreviousIqResults(expectedAlignmentFilename);
                
                SetMassTagReferencesForNetAlignment();
                return;
            }

            //Get a suitable targets file for alignment. These are grabbed from the ..\AlignmentInfo folder. 
            var targetFileForAlignment = GetTargetFilePathForIqAlignment();


            if (string.IsNullOrEmpty(targetFileForAlignment))
            {
                IqLogger.Log.Info("Alignment not performed - No suitable target file found for use in alignment.");
                return;
            }

            if (!File.Exists(targetFileForAlignment))
            {
                IqLogger.Log.Info("Alignment not performed - Target file for alignment has been specified but a FILE NOT FOUND error has occured.");
                return;
            }

            var isFirstHitsFile = targetFileForAlignment.EndsWith("_fht.txt");

            if (!isFirstHitsFile)
            {
                IqLogger.Log.Info("Alignment not performed - target file for alignment must be a first hits file (_fht.txt)");
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
                IqLogger.Log.Info("IQ Net aligner - " + massTagRefs.Count + " reference targets were loaded successfully.");
            }
            else
            {
                IqLogger.Log.Info("IQ Net aligner INACTIVE - no reference tags were loaded. You need to define 'TargetsUsedForLookupFilePath'");
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
            if (targets==null || targets.Count==0)
            {
                IqLogger.Log.Info("WARNING - No targets loaded.");
                return;
            }

            var totalTargets = targets.Count;
            var targetCount = 1;
            IqLogger.Log.Info("Total targets being processed: " + totalTargets);
            IqLogger.Log.Info("Processing...");

            foreach (var target in targets)
            {
                Run = target.GetRun();

                if (!ChromDataIsLoaded)
                {
                    LoadChromData(Run);
                }

                ReportGeneralProgress(targetCount, totalTargets);

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

            IqLogger.Log.Info("Processing Complete!" + Environment.NewLine + Environment.NewLine);
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

            IqLogger.Log.Info("Target Loading Started...");

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

            IqLogger.Log.Info("Targets Loaded Successfully. Total targets loaded= "+ Targets.Count);
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

            IqResultExporter.WriteOutResults(Path.Combine(_resultsFolder, Run.DatasetName + "_iqResults.txt"), exportedResults);
        }

        private void SetupAlignmentFolder(string alignmentFolder= "")
        {
            if (!string.IsNullOrEmpty(alignmentFolder))
            {
                _alignmentFolder = alignmentFolder;
            }
            else if (string.IsNullOrEmpty(Parameters.OutputFolderBase))
            {
                _alignmentFolder = Path.Combine(GetDefaultOutputFolder(), "AlignmentInfo");
            }
            else
            {
                _alignmentFolder = Path.Combine(Parameters.OutputFolderBase, "AlignmentInfo");
            }

            if (!Directory.Exists(_alignmentFolder)) Directory.CreateDirectory(_alignmentFolder);

        }

        protected virtual string GetTargetFilePathForIqAlignment()
        {
            if (Run == null)
            {
                IqLogger.Log.Error("Trying to get target file path for use in IqAlignment but Run is null.");
                return string.Empty;
            }

            if (string.IsNullOrEmpty(_alignmentFolder))
            {
                SetupAlignmentFolder(_alignmentFolder);

            }

            //first look for _fht.txt file (MSGF output)
            var targetsForAlignmentFilePath = Path.Combine(_alignmentFolder, Run.DatasetName + "_msgfdb_fht.txt");

            if (File.Exists(targetsForAlignmentFilePath))
            {
                return targetsForAlignmentFilePath;
            }

            return string.Empty;
        }


        protected virtual void ReportGeneralProgress(int currentTarget, int totalTargets)
        {
            var currentProgress = (currentTarget / (double)totalTargets);

            if (currentTarget % 50 == 0)
            {
                IqLogger.Log.Info("Processing target " + currentTarget + " of " + totalTargets + "; " + (Math.Round(currentProgress * 100, 1)) + "% Complete.");
            }

            if (_backgroundWorker != null)
            {
                _backgroundWorker.ReportProgress(Convert.ToInt16(currentProgress * 100));
            }
        }


        private void SetupResultsFolder()
        {
            if (string.IsNullOrEmpty(Parameters.OutputFolderBase))
            {
                _resultsFolder = GetDefaultOutputFolder();
            }
            else
            {
                _resultsFolder = Path.Combine(Parameters.OutputFolderBase, "IqResults");
            }

            if (!Directory.Exists(_resultsFolder)) Directory.CreateDirectory(_resultsFolder);


        }

        #endregion

        #region Private Methods


        private string CreatePeaksForChromSourceData()
        {
            var parameters = new PeakDetectAndExportWorkflowParameters();

            parameters.PeakBR = Parameters.ChromGenSourceDataPeakBR;
            parameters.PeakFitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;
            parameters.SigNoiseThreshold = Parameters.ChromGenSourceDataSigNoise;
            parameters.ProcessMSMS = Parameters.ChromGenSourceDataProcessMsMs;
            parameters.IsDataThresholded = Parameters.ChromGenSourceDataIsThresholded;
            parameters.LCScanMin = this.Run.GetMinPossibleLCScanNum();
            parameters.LCScanMax = this.Run.GetMaxPossibleLCScanNum();

            var peakCreator = new PeakDetectAndExportWorkflow(this.Run, parameters, _backgroundWorker);
            peakCreator.Execute();

            var peaksFilename = Path.Combine(this.Run.DataSetPath, this.Run.DatasetName + "_peaks.txt");
            return peaksFilename;

        }


        private string GetPossiblePeaksFile()
        {
            string baseFileName;
            baseFileName = Path.Combine(this.Run.DataSetPath, this.Run.DatasetName);

            var possibleFilename1 = baseFileName + "_peaks.txt";

            if (File.Exists(possibleFilename1))
            {
                return possibleFilename1;
            }
            else
            {
                return string.Empty;
            }
        }



        public void LoadChromData(Run run)
        {
            if (string.IsNullOrEmpty(ChromSourceDataFilePath))
            {
                ChromSourceDataFilePath = GetPossiblePeaksFile();
            }

            if (string.IsNullOrEmpty(ChromSourceDataFilePath))
            {
                Console.WriteLine("Creating _Peaks.txt file for " + this.Run.DatasetName + " at " + this.Run.DataSetPath);
                IqLogger.Log.Info("Creating _Peaks.txt");
                ChromSourceDataFilePath = CreatePeaksForChromSourceData();
            }
            else
            {
                IqLogger.Log.Info("Using Existing _Peaks.txt");
            }

            IqLogger.Log.Info("Peak Loading Started...");

            var peakImporter = new PeakImporterFromText(ChromSourceDataFilePath, _backgroundWorker);
            peakImporter.ImportPeaks(this.Run.ResultCollection.MSPeakResultList);

            IqLogger.Log.Info("Peak Loading Complete. Number of peaks loaded= " + Run.ResultCollection.MSPeakResultList.Count);
        }


        
        private void SetupLogging()
        {
            string loggingFolder;
            if (string.IsNullOrEmpty(Parameters.OutputFolderBase))
            {
                loggingFolder = GetDefaultOutputFolder();
            }
            else
            {
                loggingFolder = Path.Combine(Parameters.OutputFolderBase, "IqLogs");
            }


            if (!Directory.Exists(loggingFolder)) Directory.CreateDirectory(loggingFolder);


            IqLogger.LogDirectory = loggingFolder;
            IqLogger.InitializeIqLog(_run.DatasetName);
        }

        private string GetDefaultOutputFolder()
        {
            var defaultOutputFolder = _run.DataSetPath;
            return defaultOutputFolder;
        }

        #endregion

    }
}
