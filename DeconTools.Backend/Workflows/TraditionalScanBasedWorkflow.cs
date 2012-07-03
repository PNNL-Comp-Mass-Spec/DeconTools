
using System;
using System.ComponentModel;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Workflows
{
    public class TraditionalScanBasedWorkflow : ScanBasedWorkflow
    {

        private O16O18PeakDataAppender _o16O18PeakDataAppender = new O16O18PeakDataAppender();
        private int _scanCounter = 1;
        private const int NumScansBetweenProgress = 10;

        #region Constructors

        public TraditionalScanBasedWorkflow(OldDecon2LSParameters parameters, Run run, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputFolderPath, backgroundWorker)
        {


        }


       

        #endregion


        #region Public Methods
        




        protected override void IterateOverScans()
        {
            _scanCounter = 1;
            foreach (ScanSet scanset in Run.ScanSetCollection.ScanSetList)
            {
                Run.CurrentScanSet = scanset;

                ExecuteProcessingTasks();

                if (BackgroundWorker != null)
                {
                    if (BackgroundWorker.CancellationPending)
                    {
                        return;
                    }

                }
                ReportProgress();

                _scanCounter++;
            }
        }


        protected string getErrorInfo(Run run, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ERROR THROWN. Scan/Frame = ");
            sb.Append(run.GetCurrentScanOrFrameInfo());
            sb.Append("; ");
            sb.Append(DiagnosticUtilities.GetCurrentProcessInfo());
            sb.Append("; errorObject details: ");
            sb.Append(ex.Message);
            sb.Append("; ");
            sb.Append(ex.StackTrace);

            return sb.ToString();

        }

        protected override void ExecuteOtherTasksHook()
        {
            base.ExecuteOtherTasksHook();
            if (OldDecon2LsParameters.HornTransformParameters.O16O18Media)
            {
                ExecuteTask(_o16O18PeakDataAppender);
            }


        }

      


        public override void ReportProgress()
        {
            if (Run.ScanSetCollection == null || Run.ScanSetCollection.ScanSetList.Count == 0) return;

            ScanBasedProgressInfo userstate = new ScanBasedProgressInfo(Run, Run.CurrentScanSet, null);

            float percentDone = (float)(_scanCounter) / (float)(Run.ScanSetCollection.ScanSetList.Count) * 100;
            userstate.PercentDone = percentDone;

            string logText = "Scan/Frame= " + Run.GetCurrentScanOrFrame() + "; PercentComplete= " + percentDone.ToString("0.0") + "; AccumlatedFeatures= " + Run.ResultCollection.getTotalIsotopicProfiles();

            if (BackgroundWorker != null)
            {
                BackgroundWorker.ReportProgress((int)percentDone, userstate);
            }

            if (_scanCounter % NumScansBetweenProgress == 0)
            {
                Logger.Instance.AddEntry(logText, Logger.Instance.OutputFilename);

                if (BackgroundWorker == null)
                {
                    Console.WriteLine(DateTime.Now + "\t" + logText);
                }

            }
        }


        #endregion

        #region Private Methods

        #endregion





    }
}
