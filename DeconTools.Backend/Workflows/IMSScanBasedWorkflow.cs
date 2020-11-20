
using System;
using System.ComponentModel;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Workflows
{
    public abstract class IMSScanBasedWorkflow : ScanBasedWorkflow
    {
        protected UIMFDriftTimeExtractor UimfDriftTimeExtractor;
        protected UIMF_TICExtractor UimfTicExtractor;
        protected SaturationDetector SaturationDetector;

        private string mCachedProgressMessage;
        private ScanBasedProgressInfo mCachedUserState;
        private DateTime mLastProgress;

        #region Constructors

        internal IMSScanBasedWorkflow(DeconToolsParameters parameters, Run run, string outputDirectoryPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputDirectoryPath, backgroundWorker)
        {
            DeconTools.Utilities.Check.Require(run is UIMFRun, "Cannot create workflow. Run is required to be a UIMFRun for this type of workflow");
            mCachedProgressMessage = string.Empty;
            mCachedUserState = new ScanBasedProgressInfo(Run, new ScanSet(), new IMSScanSet(0));
        }

        #endregion

        protected override void InitializeProcessingTasks()
        {
            base.InitializeProcessingTasks();
            UimfDriftTimeExtractor = new UIMFDriftTimeExtractor();
            UimfTicExtractor = new UIMF_TICExtractor();
            SaturationDetector = new SaturationDetector();
            mCachedProgressMessage = string.Empty;
            mLastProgress = DateTime.UtcNow;
        }

        protected override void CreateTargetMassSpectra()
        {
            var uimfRun = (UIMFRun)Run;
            uimfRun.ScanSetCollection = new ScanSetCollection();
            uimfRun.IMSScanSetCollection = new IMSScanSetCollection();

            int numFramesSummed;
            if (NewDeconToolsParameters.MSGeneratorParameters.SumSpectraAcrossLC)
            {
                numFramesSummed = NewDeconToolsParameters.MSGeneratorParameters.NumLCScansToSum;
            }
            else
            {
                numFramesSummed = 1;
            }

            if (NewDeconToolsParameters.MSGeneratorParameters.UseLCScanRange)   //Defines whether or not to use all LC time points, or a restricted range
            {
                uimfRun.ScanSetCollection.Create(uimfRun, NewDeconToolsParameters.MSGeneratorParameters.MinLCScan,
                   NewDeconToolsParameters.MSGeneratorParameters.MaxLCScan, numFramesSummed, 1, NewDeconToolsParameters.ScanBasedWorkflowParameters.ProcessMS2);
            }
            else
            {
                uimfRun.ScanSetCollection.Create(uimfRun, numFramesSummed, 1);
            }

            var sumAllIMSScansInAFrame = (NewDeconToolsParameters.MSGeneratorParameters.SumAllSpectra);
            if (sumAllIMSScansInAFrame)
            {
                var centerScan = (uimfRun.MinIMSScan + uimfRun.MaxIMSScan + 1) / 2;

                uimfRun.IMSScanSetCollection.ScanSetList.Clear();
                var scanSet = new IMSScanSet(centerScan, uimfRun.MinIMSScan, uimfRun.MaxIMSScan);
                uimfRun.IMSScanSetCollection.ScanSetList.Add(scanSet);
            }
            else
            {
                var sumAcrossIMSScans = NewDeconToolsParameters.MSGeneratorParameters.SumSpectraAcrossIms;

                int numIMSScanToSum;
                if (sumAcrossIMSScans)
                {
                    numIMSScanToSum = NewDeconToolsParameters.MSGeneratorParameters.NumImsScansToSum;
                }
                else
                {
                    numIMSScanToSum = 1;
                }

                uimfRun.IMSScanSetCollection.Create(Run, uimfRun.MinIMSScan, uimfRun.MaxIMSScan, numIMSScanToSum, 1);
            }
        }

        protected override void ExecutePreprocessHook()
        {
            base.ExecutePreprocessHook();
            ((UIMFRun)Run).GetFrameDataAllFrameSets();
            ((UIMFRun)Run).SmoothFramePressuresInFrameSets();
        }

        protected override void IterateOverScans()
        {
            mCachedProgressMessage = string.Empty;
            mLastProgress = DateTime.UtcNow;

            var uimfRun = (UIMFRun)Run;

            foreach (var frameSet in uimfRun.ScanSetCollection.ScanSetList)
            {
                foreach (var scanSet in uimfRun.IMSScanSetCollection.ScanSetList)
                {
                    var imsScanSet = (IMSScanSet)scanSet;
                    uimfRun.CurrentScanSet = frameSet;
                    uimfRun.CurrentIMSScanSet = imsScanSet;
                    ReportProgress();
                    ExecuteProcessingTasks();
                }
            }
        }

        protected override void ExecuteOtherTasksHook()
        {
            base.ExecuteOtherTasksHook();
            ExecuteTask(UimfDriftTimeExtractor);
            ExecuteTask(UimfTicExtractor);
            ExecuteTask(SaturationDetector);
        }

        protected override string GetProgressMessage(double percentDone)
        {
            var progressMessage = base.GetProgressMessage(percentDone);

            var elapsedTimeMinutes = Math.Max(DateTime.UtcNow.Subtract(WorkflowStats.TimeStarted).TotalMinutes, 0.1);
            var framesPerMinute = Run.GetCurrentScanOrFrame() / elapsedTimeMinutes;

            return string.Format("{0}; FramesPerMinute= {1:F1}", progressMessage, framesPerMinute);
        }

        public virtual void ReportProgress()
        {
            var uimfRun = (UIMFRun)Run;
            var imsScanIsLastInFrame = uimfRun.IMSScanSetCollection.GetLastScanSet() == uimfRun.CurrentIMSScanSet.PrimaryScanNumber;

            if (imsScanIsLastInFrame ||
                string.IsNullOrWhiteSpace(mCachedProgressMessage) ||
                DateTime.UtcNow.Subtract(mLastProgress).TotalSeconds >= 3)
            {
                if (uimfRun.ScanSetCollection == null || uimfRun.ScanSetCollection.ScanSetList.Count == 0) return;

                mCachedUserState = new ScanBasedProgressInfo(Run, uimfRun.CurrentScanSet, uimfRun.CurrentIMSScanSet);
                var frameNum = uimfRun.ScanSetCollection.ScanSetList.IndexOf(uimfRun.CurrentScanSet);

                var scanNum = uimfRun.IMSScanSetCollection.ScanSetList.IndexOf(uimfRun.CurrentIMSScanSet);
                var scanTotal = uimfRun.IMSScanSetCollection.ScanSetList.Count;

                var frameTotal = uimfRun.ScanSetCollection.ScanSetList.Count;

                var percentDone = (frameNum / (double)frameTotal + scanNum / (double)scanTotal / frameTotal) * 100;
                mCachedUserState.PercentDone = (float)percentDone;

                mCachedProgressMessage = GetProgressMessage(percentDone);
                mLastProgress = DateTime.UtcNow;
            }

            if (imsScanIsLastInFrame)
            {
                Logger.Instance.AddEntry(mCachedProgressMessage, true);
                Console.WriteLine(DateTime.Now + "\t" + mCachedProgressMessage);
            }

            BackgroundWorker?.ReportProgress((int)mCachedUserState.PercentDone, mCachedUserState);
        }
    }
}
