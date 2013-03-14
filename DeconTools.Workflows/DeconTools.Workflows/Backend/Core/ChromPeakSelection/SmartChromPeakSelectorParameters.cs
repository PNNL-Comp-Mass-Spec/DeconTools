﻿
namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{


    public class SmartChromPeakSelectorParameters : ChromPeakSelectorParameters
    {

        #region Constructors

        public SmartChromPeakSelectorParameters()
            : this(new ChromPeakSelectorParameters())
        {
            
        }

        public SmartChromPeakSelectorParameters(ChromPeakSelectorParameters parameters)
            : base(parameters)
        {
            MSToleranceInPPM = 25;
            MSFeatureFinderType = DeconTools.Backend.Globals.TargetedFeatureFinderType.ITERATIVE;
            MSPeakDetectorPeakBR = 1.3;
            MSPeakDetectorSigNoiseThresh = 2;
            NumChromPeaksAllowed = 20;
            IterativeTffMinRelIntensityForPeakInclusion = 0.25;
            UpperLimitOfGoodFitScore = 0.15;
            NumMSSummedInSmartSelector = 1;

        }
        #endregion

        #region Properties
        public double MSToleranceInPPM { get; set; }

        public DeconTools.Backend.Globals.TargetedFeatureFinderType MSFeatureFinderType { get; set; }


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

        /// <summary>
        /// Iterative targeted feature finding has this parameter. So must be captured here
        /// </summary>
        public double IterativeTffMinRelIntensityForPeakInclusion { get; set; }


        /// <summary>
        /// Upper limit of what is considered a good fit score; Used in reporting number of High-quality chrom peaks.
        /// </summary>
        public double UpperLimitOfGoodFitScore { get; set; }


        public int NumMSSummedInSmartSelector { get; set; }


        #endregion


    }
}
