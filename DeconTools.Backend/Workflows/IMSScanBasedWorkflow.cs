
using System;
using System.ComponentModel;
using DeconTools.Backend.Core;
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

        #region Constructors

        internal IMSScanBasedWorkflow(OldDecon2LSParameters parameters, Run run, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputFolderPath, backgroundWorker)
        {
            DeconTools.Utilities.Check.Require(run is UIMFRun, "Cannot create workflow. Run is required to be a UIMFRun for this type of workflow");

        }



        #endregion

        protected override void InitializeProcessingTasks()
        {
            base.InitializeProcessingTasks();
            UimfDriftTimeExtractor = new UIMFDriftTimeExtractor();
            UimfTicExtractor = new UIMF_TICExtractor();
            SaturationDetector = new SaturationDetector();
        }

        protected override void CreateTargetMassSpectra()
        {
           


            UIMFRun uimfRun = (UIMFRun)Run;
            uimfRun.ScanSetCollection = new ScanSetCollection();
            uimfRun.IMSScanSetCollection = new IMSScanSetCollection();


            int numFramesSummed;
            if (OldDecon2LsParameters.HornTransformParameters.SumSpectraAcrossFrameRange)
            {
                numFramesSummed = OldDecon2LsParameters.HornTransformParameters.NumFramesToSumOver;
            }
            else
            {
                numFramesSummed = 1;
            }

            if (OldDecon2LsParameters.HornTransformParameters.UseScanRange)   //Defines whether or not to use all LC time points, or a restricted range
            {
                uimfRun.ScanSetCollection.Create(uimfRun, OldDecon2LsParameters.HornTransformParameters.MinScan,
                    OldDecon2LsParameters.HornTransformParameters.MaxScan, numFramesSummed, 1, OldDecon2LsParameters.HornTransformParameters.ProcessMSMS);

            }
            else
            {
                uimfRun.ScanSetCollection.Create(uimfRun, numFramesSummed, 1);
            }



            bool sumAllIMSScansInAFrame = (OldDecon2LsParameters.HornTransformParameters.SumSpectra);
            if (sumAllIMSScansInAFrame)
            {

                int centerScan = (uimfRun.MinIMSScan + uimfRun.MaxIMSScan + 1) / 2;

                uimfRun.IMSScanSetCollection.ScanSetList.Clear();
                var scanset = new IMSScanSet(centerScan, uimfRun.MinIMSScan, uimfRun.MaxIMSScan);
                uimfRun.IMSScanSetCollection.ScanSetList.Add(scanset);
            }
            else
            {

                bool sumAcrossIMSScans = OldDecon2LsParameters.HornTransformParameters.SumSpectraAcrossScanRange;

                int numIMSScanToSum;
                if (sumAcrossIMSScans)
                {
                    numIMSScanToSum = OldDecon2LsParameters.HornTransformParameters.NumScansToSumOver * 2 + 1;   //Old parameters report a +/- value for summing. But new code is different
                }
                else
                {
                    numIMSScanToSum = 1;
                }

                uimfRun.IMSScanSetCollection.Create(Run,uimfRun.MinIMSScan,uimfRun.MaxIMSScan,numIMSScanToSum,OldDecon2LsParameters.HornTransformParameters.NumScansToAdvance);

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
            var uimfRun = (UIMFRun)Run;

            foreach (var frameset in uimfRun.ScanSetCollection.ScanSetList)
            {
                foreach (IMSScanSet imsScanSet in uimfRun.IMSScanSetCollection.ScanSetList)
                {
                    uimfRun.CurrentFrameSet = frameset;
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

        public override void ReportProgress()
        {
            UIMFRun uimfRun = (UIMFRun)Run;
            if (uimfRun.ScanSetCollection == null || uimfRun.ScanSetCollection.ScanSetList.Count == 0) return;

            ScanBasedProgressInfo userstate = new ScanBasedProgressInfo(Run, uimfRun.CurrentFrameSet, uimfRun.CurrentIMSScanSet);
            int framenum = uimfRun.ScanSetCollection.ScanSetList.IndexOf(uimfRun.CurrentFrameSet);

            int scanNum = uimfRun.IMSScanSetCollection.ScanSetList.IndexOf(uimfRun.CurrentIMSScanSet);
            int scanTotal = uimfRun.IMSScanSetCollection.ScanSetList.Count;

            int frameTotal = uimfRun.ScanSetCollection.ScanSetList.Count;



            double percentDone = ((double)(framenum) / (double)frameTotal + ((double)scanNum / (double)scanTotal) / (double)frameTotal) * 100;
            userstate.PercentDone = (float)percentDone;


            string logText = "Scan/Frame= " + Run.GetCurrentScanOrFrame() + "; PercentComplete= " + percentDone.ToString("0.0") + "; AccumlatedFeatures= " + Run.ResultCollection.getTotalIsotopicProfiles();

            int numScansBetweenProgress = 1;

            bool imsScanIsLastInFrame = uimfRun.IMSScanSetCollection.GetLastScanSet() == uimfRun.CurrentIMSScanSet.PrimaryScanNumber;
            if (imsScanIsLastInFrame)
            {
                Logger.Instance.AddEntry(logText, Logger.Instance.OutputFilename);
                Console.WriteLine(DateTime.Now + "\t" + logText);
            }

            if (BackgroundWorker != null && scanNum % numScansBetweenProgress == 0)
            {

                BackgroundWorker.ReportProgress((int)percentDone, userstate);

            }

        }


    }
}
