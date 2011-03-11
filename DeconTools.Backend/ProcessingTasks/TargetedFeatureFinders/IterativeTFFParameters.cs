using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class IterativeTFFParameters
    {

        #region Constructors

        public IterativeTFFParameters()
        {
            this.PeakDetectorPeakBR = 5;
            this.PeakBRStep = 0.5;
            

            this.PeakDetectorSigNoiseRatioThreshold = 3;
            this.PeakDetectorPeakFitType = Globals.PeakFitType.QUADRATIC;
            this.PeakDetectorIsDataThresholded = false;
            
            this.IsotopicProfileType = ProcessingTasks.IsotopicProfileType.UNLABELLED;
            this.NumPeaksUsedInAbundance = 1;
            this.RequiresMonoIsotopicPeak = false;
            this.ToleranceInPPM = 25;
            
            
        }
        #endregion

        #region Properties

        /// <summary>
        /// The is the initial PeakBR the peakDetector starts with. Then iteratively, the PeakBR is stepped down.
        /// </summary>
        public double PeakDetectorPeakBR { get; set; }
        public double PeakDetectorSigNoiseRatioThreshold { get; set; }
        public DeconTools.Backend.Globals.PeakFitType PeakDetectorPeakFitType { get; set; }
        public bool PeakDetectorIsDataThresholded { get; set; }


        /// <summary>
        /// The PeakBR is stepped down by this value, in an effort to find lower intensity peaks, if not found on first iterations.
        /// </summary>
        public double PeakBRStep { get; set; }

        
        public double ToleranceInPPM { get; set; }
        public IsotopicProfileType IsotopicProfileType { get; set; }
        public int NumPeaksUsedInAbundance { get; set; }
        public bool RequiresMonoIsotopicPeak { get; set; }


        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
