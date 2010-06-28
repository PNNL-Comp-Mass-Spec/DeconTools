using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using System.ComponentModel;
using DeconTools.Backend.Data;
using System.IO;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class UIMF_TaskController : TaskController
    {
        const int DEFAULT_ISOSRESULT_THRESHOLD = 500000;
        const double DEFAULT_TIME_BETWEEN_LOGENTRIES = 15;    //number of minutes between log entries during processing
        
        private BackgroundWorker backgroundWorker;
        private IsosResultSerializer serializer;


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

        public override void Execute(List<Run>runCollection)
        {

            foreach (Run run in runCollection)
            {
                if (run is UIMFRun)
                {
                    UIMFRun uimfRun = (UIMFRun)run;

                    serializer = null;
                    //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                    foreach (FrameSet frameset in uimfRun.FrameSetCollection.FrameSetList)
                    {
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
                        //Dec 2, 2009:  [gord].  Following code is decommissioned...  I rebuilt the exporters with triggers. No need for binary file output. 
                        //If numberOfResultsExceedsLimit, serialize
                        //if (run.ResultCollection.ResultList.Count > this.IsosResultThresholdNum)
                        //{
                        //    if (serializer == null)
                        //    {
                        //        string binaryOutputFilename = getOutputFileName(run);

                        //        bool deletePreviousBinaryFile = true;
                        //        serializer = new IsosResultSerializer(binaryOutputFilename, System.IO.FileMode.Append, deletePreviousBinaryFile);

                        //    }
                        //    serializer.Serialize(run.ResultCollection);

                        //    run.ResultCollection.ResultList.Clear();
                        //    run.AreRunResultsSerialized = true;   //Indicate if serialized; this flag will be used when exporting data

                        //}


                    }




                    run.Close();   

                }
                
                //See above... this has been decommissioned. 
                //Next take care of the last chunk of results that didn't exceed the threshold
                //if (run.AreRunResultsSerialized)
                //{
                //    serializer.Serialize(run.ResultCollection);
                //    serializer.Close();
                //}
                
            }
        }

   
        private string getOutputFileName(Run run)
        {
            return run.Filename + "_tmp.bin";
        }

        private void reportProgress(FrameSet frameset, ScanSet scanset, Run run)
        {
            ProjectFacade pf=new ProjectFacade();
            
            UIMFRun uimfRun = (UIMFRun)run;
            if (uimfRun.FrameSetCollection == null || uimfRun.FrameSetCollection.FrameSetList.Count == 0) return;

            UserState userstate = new UserState(run, scanset, frameset);
            int framenum = uimfRun.FrameSetCollection.FrameSetList.IndexOf(frameset);

            int scanNum = uimfRun.ScanSetCollection.ScanSetList.IndexOf(scanset);
            int scanTotal = uimfRun.ScanSetCollection.ScanSetList.Count;

            int frameTotal = uimfRun.FrameSetCollection.FrameSetList.Count;



            double percentDone = ((double)(framenum) / (double)frameTotal + ((double)scanNum / (double)scanTotal) / (double)frameTotal)*100;
            userstate.PercentDone = (float)percentDone;

            if (System.DateTime.Now.Subtract(Logger.Instance.TimeOfLastUpdate).TotalMinutes > DEFAULT_TIME_BETWEEN_LOGENTRIES)
            {
                Logger.Instance.AddEntry("Processed scan/frame " + uimfRun.GetCurrentScanOrFrame() + ", "
                    + percentDone.ToString("0.#") + "% complete, " + uimfRun.ResultCollection.getTotalIsotopicProfiles() + " accumulated features",Logger.Instance.OutputFilename);
            }



            int numScansBetweenProgress = getNumScansBetweenProgress(this.TaskCollection);

            if (run.ScanSetCollection.GetLastScanSet()==scanset.PrimaryScanNumber|| scanset.PrimaryScanNumber % numScansBetweenProgress == 0)
            {
                if (backgroundWorker != null)
                {
                    backgroundWorker.ReportProgress((int)percentDone, userstate);
                }
                else
                {

                    Console.WriteLine("Completed processing on frame " + frameset.PrimaryFrame + " Scan " + scanset.PrimaryScanNumber + "; Isotopic Profiles = " +scanset.NumIsotopicProfiles);
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
