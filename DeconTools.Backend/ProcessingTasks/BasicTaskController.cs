using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using System.ComponentModel;

namespace DeconTools.Backend.ProcessingTasks
{
    public class BasicTaskController : TaskController
    {
        const double DEFAULT_TIME_BETWEEN_LOGENTRIES = 15;    //number of minutes between log entries during processing


        public BasicTaskController(TaskCollection taskcollection)
        {
            this.TaskCollection = taskcollection;
        }

        public BasicTaskController(TaskCollection taskcollection, BackgroundWorker backgroundWorker)
            : this(taskcollection)
        {
            this.backgroundWorker = backgroundWorker;
        }


        public override void Execute(List<Run>runCollection)
        {


            foreach (Run run in runCollection)
            {
                int counter = 1;
                foreach (ScanSet scanset in run.ScanSetCollection.ScanSetList)
                {
                    run.CurrentScanSet = scanset;
                    foreach (Task task in this.TaskCollection.TaskList)
                    {
                        task.Execute(run.ResultCollection);
                    }

                    if (backgroundWorker != null)
                    {
                        if (backgroundWorker.CancellationPending)
                        {
                            return;
                        }

                    }
                    reportProgress(scanset, run);


                    counter++;
                }

            }

        }

        private void reportProgress(ScanSet scanset, Run run)
        {
            if (run.ScanSetCollection == null || run.ScanSetCollection.ScanSetList.Count == 0) return;

            UserState userstate = new UserState(run, scanset, null);

            int scannum = run.ScanSetCollection.ScanSetList.IndexOf(scanset) + 1;
            float percentDone = (float)(scannum) / (float)(run.ScanSetCollection.ScanSetList.Count) * 100;

            if (System.DateTime.Now.Subtract(Logger.Instance.TimeOfLastUpdate).TotalMinutes > DEFAULT_TIME_BETWEEN_LOGENTRIES)
            {
                Logger.Instance.AddEntry("Processed scan/frame " + run.GetCurrentScanOrFrame() + ", "
                    + percentDone.ToString("0.#") + "% complete, " + run.ResultCollection.getTotalIsotopicProfiles() + " accumulated features", Logger.Instance.OutputFilename);
            }

            
            int numScansBetweenProgress = getNumScansBetweenProgress(this.TaskCollection);

            if (scanset.PrimaryScanNumber % numScansBetweenProgress == 0)
            {
                if (backgroundWorker != null)
                {
                    backgroundWorker.ReportProgress((int)percentDone, userstate);
                }
                else
                {
                    Console.WriteLine("Completed processing on Scan " + scanset.PrimaryScanNumber);
                }
            }


        }

 

    }
}
