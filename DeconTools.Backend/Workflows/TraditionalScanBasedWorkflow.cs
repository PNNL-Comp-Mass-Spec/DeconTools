
using System;
using System.ComponentModel;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Workflows
{
    public class TraditionalScanBasedWorkflow : ScanBasedWorkflow
    {

        private readonly O16O18PeakDataAppender _o16O18PeakDataAppender = new O16O18PeakDataAppender();
        private int _scanCounter = 1;
        private DateTime _lastProgressTime = DateTime.UtcNow;

        private const int NumScansBetweenProgress = 10;

        #region Constructors

        public TraditionalScanBasedWorkflow(DeconToolsParameters parameters, Run run, string outputDirectoryPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputDirectoryPath, backgroundWorker)
        {
        }
        #endregion

        #region Public Methods

        protected override void IterateOverScans()
        {
            var startTime = DateTime.UtcNow;
            var maxRuntimeHours = NewDeconToolsParameters.MiscMSProcessingParameters.MaxHoursPerDataset;
            if (maxRuntimeHours <= 0)
                maxRuntimeHours = int.MaxValue;

            _scanCounter = 1;
            foreach (var scanSet in Run.ScanSetCollection.ScanSetList)
            {
                Run.CurrentScanSet = scanSet;

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

                if (DateTime.UtcNow.Subtract(startTime).TotalHours >= maxRuntimeHours)
                {
                    Console.WriteLine(
                        "Aborted processing because {0} hours have elapsed; ScanCount processed = {1}",
                        (int)DateTime.UtcNow.Subtract(startTime).TotalHours,
                        _scanCounter);

                    break;
                }
            }
        }

        protected string GetErrorInfo(Run run, Exception ex)
        {
            var sb = new StringBuilder();
            sb.Append("ERROR THROWN. Scan/Frame = ");
            sb.Append(run.GetCurrentScanOrFrameInfo());
            sb.Append("; ");
            sb.Append(DiagnosticUtilities.GetCurrentProcessInfo());
            sb.Append("; errorObject details: ");
            sb.Append(ex.Message);
            sb.Append("; ");
            sb.Append(PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex));

            return sb.ToString();

        }

        protected override void ExecuteOtherTasksHook()
        {
            base.ExecuteOtherTasksHook();
            if (NewDeconToolsParameters.ThrashParameters.IsO16O18Data)
            {
                ExecuteTask(_o16O18PeakDataAppender);
            }
        }

        public virtual void ReportProgress()
        {
            if (Run.ScanSetCollection == null || Run.ScanSetCollection.ScanSetList.Count == 0) return;

            var userState = new ScanBasedProgressInfo(Run, Run.CurrentScanSet);

            var percentDone = _scanCounter / (float)(Run.ScanSetCollection.ScanSetList.Count) * 100;
            userState.PercentDone = percentDone;

            var progressMessage = GetProgressMessage(percentDone);

            BackgroundWorker?.ReportProgress((int)percentDone, userState);

            var reportProgress = _scanCounter % NumScansBetweenProgress == 0 ||
                                 DateTime.UtcNow.Subtract(_lastProgressTime).TotalMinutes > 5 ||
                                 mShowTraceMessages;
            if (!reportProgress)
                return;

            _lastProgressTime = DateTime.UtcNow;

            Logger.Instance.AddEntry(progressMessage, true);

            if (BackgroundWorker == null)
            {
                Console.WriteLine(DateTime.Now + "\t" + progressMessage);
            }
        }

        #endregion

    }
}
