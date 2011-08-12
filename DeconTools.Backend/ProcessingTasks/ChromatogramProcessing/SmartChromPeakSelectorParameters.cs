
namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public class SmartChromPeakSelectorParameters
    {

        #region Constructors
        public SmartChromPeakSelectorParameters()
        {
            this.MSToleranceInPPM = 25;
            this.NETTolerance = 0.025f;
            this.MSFeatureFinderType = Globals.TargetedFeatureFinderType.ITERATIVE;
            this.MSPeakDetectorPeakBR = 1.3;
            this.MSPeakDetectorSigNoiseThresh = 2;
            this.NumChromPeaksAllowed = 20;
            this.NumScansToSum = 1;
            
        }
        #endregion

        #region Properties
        public double MSToleranceInPPM { get; set; }

        public Globals.TargetedFeatureFinderType MSFeatureFinderType { get; set; }

     
        public float NETTolerance { get; set; }

        public int NumScansToSum { get; set; }

        /// <summary>
        /// Number of chrom peaks allowed. For example if this is set to '5' 
        /// and '6' peaks were found within the tolerance, then the selected best peak is set to
        /// null indicating a failed execution
        /// 
        /// </summary>
        public int NumChromPeaksAllowed { get; set; }

        /// <summary>
        /// This is useful, especially in targetedAlignment, to allow strict selection of chrom peak.
        /// If true, the chromPeak that is most abundant will be allowed through (if the ms_fit value difference is small between first and second chrom peaks)
        /// If false, a null chromPeak will be reported. 
        /// </summary>
        public bool MultipleHighQualityMatchesAreAllowed { get; set; }


        public double MSPeakDetectorPeakBR { get; set; }

        public double MSPeakDetectorSigNoiseThresh { get; set; }
   

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
