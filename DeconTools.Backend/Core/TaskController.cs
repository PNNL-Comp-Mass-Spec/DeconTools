using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Core
{
    public abstract class TaskController
    {

        private TaskCollection taskCollection;

        public TaskCollection TaskCollection
        {
            get { return taskCollection; }
            set { taskCollection = value; }
        }

        public abstract void Execute(List<Run> runCollection);

        protected BackgroundWorker backgroundWorker;

        private int percentDone;

        public int PercentDone
        {
            get { return percentDone; }
            set { percentDone = value; }
        }
        protected virtual int getNumScansBetweenProgress(TaskCollection taskCollection)
        {
            int numScansBetweenProgress;


            if (taskCollectionContainsRapidDeconvolutor(taskCollection))
            {
                numScansBetweenProgress = 50;
            }
            else
            {
                numScansBetweenProgress = 1;
            }
            return numScansBetweenProgress;
        }

        protected string getErrorInfo(Run run, Task task, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ERROR THROWN. ProcessingTask = ");
            sb.Append(task);
            sb.Append("; ");
            sb.Append(run.GetCurrentScanOrFrameInfo());

            sb.Append("; ");
            sb.Append(DiagnosticUtilities.GetCurrentProcessInfo());
            sb.Append("; errorObject details: ");
            sb.Append(ex.Message);
            sb.Append("; ");
            sb.Append(ex.StackTrace);

            return sb.ToString();

        }

        protected bool taskCollectionContainsRapidDeconvolutor(TaskCollection taskCollection)
        {
            foreach (Task task in taskCollection.TaskList)
            {
                if (task is RapidDeconvolutor)
                { return true; }
            }
            return false;
        }

        private int isosResultThresholdNum;

        public int IsosResultThresholdNum           //this value represents the limit that when exceeded may lead to serialization, according to the derived class; purpose is to prevent outOfMemory errors 
        {
            get { return isosResultThresholdNum; }
            set { isosResultThresholdNum = value; }
        }
    }



    public class UserState
    {
        public UserState(Run currentRun, ScanSet scanSet, FrameSet frameset)
        {
            this.currentRun = currentRun;
            this.currentScanSet = scanSet;
            this.currentFrameSet = frameset;

        }

        private float percentDone;

        public float PercentDone
        {
            get { return percentDone; }
            set { percentDone = value; }
        }
        
        private Run currentRun;

        public Run CurrentRun
        {
            get { return currentRun; }
            set { currentRun = value; }
        }

        private ScanSet currentScanSet;

        public ScanSet CurrentScanSet
        {
            get { return currentScanSet; }
            set { currentScanSet = value; }
        }
        private FrameSet currentFrameSet;

        public FrameSet CurrentFrameSet
        {
            get { return currentFrameSet; }
            set { currentFrameSet = value; }
        }

        public int getScanOrFrameNum()
        {
            if (this.currentRun == null) return -1;
            if (this.currentRun is UIMFRun)
            {
                if (currentFrameSet == null) return -1;
                else
                {
                    return currentFrameSet.PrimaryFrame;
                }
            }
            else
            {
                if (currentScanSet == null) return -1;
                else
                {
                    return currentScanSet.PrimaryScanNumber;
                }

            }
            
        }

  



      
    }
}
