using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class BasicTaskController : TaskController
    {
        const double DEFAULT_TIME_BETWEEN_LOGENTRIES = 5;    //number of minutes between log entries during processing
        private int _scanCounter;


        public BasicTaskController(TaskCollection taskcollection)
        {
            this.TaskCollection = taskcollection;
        }

        public BasicTaskController(TaskCollection taskcollection, BackgroundWorker backgroundWorker)
            : this(taskcollection)
        {
            this.backgroundWorker = backgroundWorker;
        }

        public override void Execute(Run run)
        {
            _scanCounter = 1;
            foreach (ScanSet scanset in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanset;
                foreach (Task task in this.TaskCollection.TaskList)
                {

                    try
                    {
                        task.Execute(run.ResultCollection);

                    }
                    catch (Exception ex)
                    {
                        string errorInfo = getErrorInfo(run, task, ex);
                        Logger.Instance.AddEntry(errorInfo, Logger.Instance.OutputFilename);

                        throw;
                    }
                }

                if (backgroundWorker != null)
                {
                    if (backgroundWorker.CancellationPending)
                    {
                        return;
                    }

                }
                reportProgress(scanset, run);


                _scanCounter++;
            }
        }


        public override void Execute(List<Run> runCollection)
        {


            foreach (Run run in runCollection)
            {
                Execute(run);
            }

        }

        private void reportProgress(ScanSet scanset, Run run)
        {
            if (run.ScanSetCollection == null || run.ScanSetCollection.ScanSetList.Count == 0) return;

            UserState userstate = new UserState(run, scanset, null);

            
            float percentDone = (float)(_scanCounter) / (float)(run.ScanSetCollection.ScanSetList.Count) * 100;
            userstate.PercentDone = percentDone;

            string logText = "Scan/Frame= " + run.GetCurrentScanOrFrame() + "; PercentComplete= " + percentDone.ToString("0.0") + "; AccumlatedFeatures= " + run.ResultCollection.getTotalIsotopicProfiles();

            int numScansBetweenProgress = getNumScansBetweenProgress(this.TaskCollection);


            if (backgroundWorker != null)
            {
                backgroundWorker.ReportProgress((int)percentDone, userstate);
            }
            
            if (_scanCounter % numScansBetweenProgress == 0)
            {
                Logger.Instance.AddEntry(logText, Logger.Instance.OutputFilename);

                if (backgroundWorker == null)
                {
                    Console.WriteLine(DateTime.Now + "\t" + logText);
                }
                
            }


        }

        protected override int getNumScansBetweenProgress(TaskCollection taskCollection)
        {
            int numScansBetweenProgress;


            if (taskCollectionContainsRapidDeconvolutor(taskCollection))
            {
                numScansBetweenProgress = 10;
            }
            else
            {
                numScansBetweenProgress = 20;
            }
            return numScansBetweenProgress;
        }



    }
}
