using System;

namespace DeconTools.Backend.Workflows
{
    public class WorkflowStats
    {
        public int NumFeatures;
        public int NumPeaks;
        public DateTime TimeStarted;
        public DateTime TimeFinished;

        public TimeSpan ElapsedTime => TimeFinished - TimeStarted;
        public int ElapsedSeconds => (int)ElapsedTime.TotalSeconds;
    }
}