using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class UIMF_TaskController : TaskController
    {
        const int DEFAULT_ISOSRESULT_THRESHOLD = 500000;
        const double DEFAULT_TIME_BETWEEN_LOGENTRIES = 15;    //number of minutes between log entries during processing

        private BackgroundWorker backgroundWorker;
        private IsosResultSerializer serializer;
        private int _frameCounter;


        public UIMF_TaskController(TaskCollection taskcollection)
        {
            this.IsosResultThresholdNum = DEFAULT_ISOSRESULT_THRESHOLD;
            this.TaskCollection = taskcollection;
        }


        public UIMF_TaskController(TaskCollection taskcollection, BackgroundWorker backgroundWorker)
            : this(taskcollection)
        {
            this.backgroundWorker = backgroundWorker;
        }



        public override void Execute(Run run)
        {

            if (run is UIMFRun)
            {
                UIMFRun uimfRun = (UIMFRun)run;

                serializer = null;
                //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                _frameCounter = 0;
                foreach (FrameSet frameset in uimfRun.FrameSetCollection.FrameSetList)
                {
                    _frameCounter++;
                    uimfRun.CurrentFrameSet = frameset;
                    //sw.Start();

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


                                throw ex;   // let something catch it
                            }

                        }

                        if (backgroundWorker != null)
                        {
                            if (backgroundWorker.CancellationPending)
                            {
                                return;
                            }
                        }
                        reportProgress(frameset, scanset, run);

                    }



                }

                run.Close();

            }

        }
        public override void Execute(List<Run> runCollection)
        {

            foreach (Run run in runCollection)
            {
                Execute(run);
            }
        }


        private string getOutputFileName(Run run)
        {
            return run.Filename + "_tmp.bin";
        }

        private void reportProgress(FrameSet frameset, ScanSet scanset, Run run)
        {
            ProjectFacade pf = new ProjectFacade();

            UIMFRun uimfRun = (UIMFRun)run;
            if (uimfRun.FrameSetCollection == null || uimfRun.FrameSetCollection.FrameSetList.Count == 0) return;

            UserState userstate = new UserState(run, scanset, frameset);
            int framenum = uimfRun.FrameSetCollection.FrameSetList.IndexOf(frameset);

            int scanNum = uimfRun.ScanSetCollection.ScanSetList.IndexOf(scanset);
            int scanTotal = uimfRun.ScanSetCollection.ScanSetList.Count;

            int frameTotal = uimfRun.FrameSetCollection.FrameSetList.Count;



            double percentDone = ((double)(framenum) / (double)frameTotal + ((double)scanNum / (double)scanTotal) / (double)frameTotal) * 100;
            userstate.PercentDone = (float)percentDone;


            string logText = "Scan/Frame= " + run.GetCurrentScanOrFrame() + "; PercentComplete= " + percentDone.ToString("0.0") + "; AccumlatedFeatures= " + run.ResultCollection.getTotalIsotopicProfiles();

            int numScansBetweenProgress = getNumScansBetweenProgress(this.TaskCollection);


            bool imsScanIsLastInFrame = run.ScanSetCollection.GetLastScanSet() == scanset.PrimaryScanNumber;
            if (imsScanIsLastInFrame)
            {
                Logger.Instance.AddEntry(logText, Logger.Instance.OutputFilename);
                Console.WriteLine(DateTime.Now + "\t" + logText);
            }

            if (backgroundWorker != null)
            {
                if (imsScanIsLastInFrame || scanset.PrimaryScanNumber % numScansBetweenProgress == 0)
                {
                    backgroundWorker.ReportProgress((int)percentDone, userstate);
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
                numScansBetweenProgress = 1;
            }
            return numScansBetweenProgress;
        }

        private void reportProgress(FrameSet frameset, Run run)
        {


        }
    }
}
