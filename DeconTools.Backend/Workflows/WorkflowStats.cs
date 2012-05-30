using System;

namespace DeconTools.Backend.Workflows
{
    public class WorkflowStats
    {
        public int NumFeatures;
        public int NumPeaks;
        public DateTime TimeStarted;
        public DateTime TimeFinished;

        public TimeSpan ElapsedTime { get { return TimeFinished - TimeStarted; } }
        public int ElapsedSeconds { get { return (int)ElapsedTime.TotalSeconds; } }
    }
}