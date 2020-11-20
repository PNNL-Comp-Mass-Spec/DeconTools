using System;

namespace DeconTools.Backend.Workflows
{
    public class WorkflowStats
    {
        /// <summary>
        /// Number of deisotoped features
        /// </summary>
        public int NumFeatures;

        /// <summary>
        /// Number of peaks found
        /// </summary>
        public int NumPeaks;

        /// <summary>
        /// Number of scans processed
        /// </summary>
        /// <remarks>For UIMF files, this is not the number of frames; it is the number of scans deisotoped</remarks>
        public int NumScans;

        /// <summary>
        /// Start time
        /// </summary>
        public DateTime TimeStarted;

        /// <summary>
        /// Finish time
        /// </summary>
        public DateTime TimeFinished;

        /// <summary>
        /// Elapsed time
        /// </summary>
        public TimeSpan ElapsedTime => TimeFinished - TimeStarted;
    }
}