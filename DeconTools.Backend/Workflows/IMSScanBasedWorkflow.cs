
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

            //TODO: update this so that FrameNum is changed to Frame_index

            bool sumAcrossLCFrames = OldDecon2LsParameters.HornTransformParameters.SumSpectraAcrossFrameRange;

            FrameSetCollectionCreator frameSetcreator;
            if (sumAcrossLCFrames)
            {
                frameSetcreator = new FrameSetCollectionCreator(Run, OldDecon2LsParameters.HornTransformParameters.MinScan,
                    OldDecon2LsParameters.HornTransformParameters.MaxScan, OldDecon2LsParameters.HornTransformParameters.NumFramesToSumOver, 1);
            }
            else
            {
                int numSummed = 1;   // this means we will NOT sum across LC Frames
                frameSetcreator = new FrameSetCollectionCreator(Run, OldDecon2LsParameters.HornTransformParameters.MinScan,
                    OldDecon2LsParameters.HornTransformParameters.MaxScan, numSummed, 1);
            }
            frameSetcreator.Create();


            bool sumAllIMSScansInAFrame = (OldDecon2LsParameters.HornTransformParameters.SumSpectra);
            if (sumAllIMSScansInAFrame)
            {
                int centerScan = (Run.MinScan + Run.MaxScan + 1) / 2;

                int numSummed = Run.MaxScan - Run.MinScan + 1;
                if (numSummed % 2 != 0)
                {
                    numSummed++;
                }

                uimfRun.ScanSetCollection.ScanSetList.Clear();
                ScanSet scanset = new ScanSet(centerScan, Run.MinScan, Run.MaxScan);
                uimfRun.ScanSetCollection.ScanSetList.Add(scanset);
            }
            else
            {

                bool sumAcrossIMSScans = OldDecon2LsParameters.HornTransformParameters.SumSpectraAcrossScanRange;

                ScanSetCollectionCreator scanSetCollectionCreator;
                if (sumAcrossIMSScans)
                {
                    int numIMSScanToSum = OldDecon2LsParameters.HornTransformParameters.NumScansToSumOver * 2 + 1;   //Old parameters report a +/- value for summing. But new code is different

                    scanSetCollectionCreator = new ScanSetCollectionCreator(Run, Run.MinScan, Run.MaxScan, numIMSScanToSum,
                    OldDecon2LsParameters.HornTransformParameters.NumScansToAdvance,
                    OldDecon2LsParameters.HornTransformParameters.ProcessMSMS);

                }
                else
                {
                    int numIMSScanToSum = 1;      // this means there is no summing

                    scanSetCollectionCreator = new ScanSetCollectionCreator(Run, Run.MinScan, Run.MaxScan, numIMSScanToSum,
                    OldDecon2LsParameters.HornTransformParameters.NumScansToAdvance,
                    OldDecon2LsParameters.HornTransformParameters.ProcessMSMS);


                }
                scanSetCollectionCreator.Create();

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

            foreach (var frameset in uimfRun.FrameSetCollection.FrameSetList)
            {
                uimfRun.CurrentFrameSet = frameset;

                foreach (var scanset in uimfRun.ScanSetCollection.ScanSetList)
                {
                    uimfRun.CurrentScanSet = scanset;
                    ExecuteProcessingTasks();
                }

                ReportProgress();
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
            if (uimfRun.FrameSetCollection == null || uimfRun.FrameSetCollection.FrameSetList.Count == 0) return;

            UserState userstate = new UserState(Run, Run.CurrentScanSet, uimfRun.CurrentFrameSet);
            int framenum = uimfRun.FrameSetCollection.FrameSetList.IndexOf(uimfRun.CurrentFrameSet);

            int scanNum = uimfRun.ScanSetCollection.ScanSetList.IndexOf(Run.CurrentScanSet);
            int scanTotal = uimfRun.ScanSetCollection.ScanSetList.Count;

            int frameTotal = uimfRun.FrameSetCollection.FrameSetList.Count;



            double percentDone = ((double)(framenum) / (double)frameTotal + ((double)scanNum / (double)scanTotal) / (double)frameTotal) * 100;
            userstate.PercentDone = (float)percentDone;


            string logText = "Scan/Frame= " + Run.GetCurrentScanOrFrame() + "; PercentComplete= " + percentDone.ToString("0.0") + "; AccumlatedFeatures= " + Run.ResultCollection.getTotalIsotopicProfiles();

            int numScansBetweenProgress = 20;


            bool imsScanIsLastInFrame = Run.ScanSetCollection.GetLastScanSet() == Run.CurrentScanSet.PrimaryScanNumber;
            if (imsScanIsLastInFrame)
            {
                Logger.Instance.AddEntry(logText, Logger.Instance.OutputFilename);
                Console.WriteLine(DateTime.Now + "\t" + logText);
            }

            if (BackgroundWorker != null)
            {

                BackgroundWorker.ReportProgress((int)percentDone, userstate);

            }

        }


    }
}
