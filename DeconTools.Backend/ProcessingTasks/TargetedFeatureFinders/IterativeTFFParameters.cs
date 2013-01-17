using System;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    [Serializable]
    public class IterativeTFFParameters
    {

        #region Constructors

        public IterativeTFFParameters()
        {
            this.PeakDetectorPeakBR = 5;
            this.PeakBRStep = 0.5;
            PeakDetectorMinimumPeakBR = 0.5d;
            

            this.PeakDetectorSigNoiseRatioThreshold = 3;
            this.PeakDetectorPeakFitType = Globals.PeakFitType.QUADRATIC;
            this.PeakDetectorIsDataThresholded = false;

            this.IsotopicProfileType = Globals.IsotopicProfileType.UNLABELLED;
            this.NumPeaksUsedInAbundance = 1;
            this.RequiresMonoIsotopicPeak = false;
            this.ToleranceInPPM = 25;
            MinimumRelIntensityForForPeakInclusion = 0.02;


        }

       
        #endregion

        #region Properties

        /// <summary>
        /// The is the initial PeakBR the peakDetector starts with. Then iteratively, the PeakBR is stepped down.
        /// </summary>
        public double PeakDetectorPeakBR { get; set; }
        
        
        
        public double PeakDetectorMinimumPeakBR { get; set; }

        /// <summary>
        /// The PeakBR is stepped down by this value, in an effort to find lower intensity peaks, if not found on first iterations.
        /// </summary>
        public double PeakBRStep { get; set; }
        
        public double PeakDetectorSigNoiseRatioThreshold { get; set; }
        public DeconTools.Backend.Globals.PeakFitType PeakDetectorPeakFitType { get; set; }
        public bool PeakDetectorIsDataThresholded { get; set; }


       

        
        public double ToleranceInPPM { get; set; }
        public Globals.IsotopicProfileType IsotopicProfileType { get; set; }
        public int NumPeaksUsedInAbundance { get; set; }
        public bool RequiresMonoIsotopicPeak { get; set; }

        /// <summary>
        /// Iterator loops until the quality of the isotopic profile passes a certain level. This parameter 
        /// tells the iterator to loop until you have peaks that have a relative intensity
        /// below this threshold. Then iterator stops.
        /// </summary>
        public double MinimumRelIntensityForForPeakInclusion { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
