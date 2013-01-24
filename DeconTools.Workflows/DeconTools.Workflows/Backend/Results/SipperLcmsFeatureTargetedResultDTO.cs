
namespace DeconTools.Workflows.Backend.Results
{
    public class SipperLcmsFeatureTargetedResultDTO : UnlabelledTargetedResultDTO
    {

        #region Constructors
        #endregion

        #region Properties

        public double AreaUnderRatioCurve { get; set; }

        public double AreaUnderDifferenceCurve { get; set; }
        
        public double ChromCorrelationMin { get; set; }

        public double ChromCorrelationMax { get; set; }

        public double ChromCorrelationAverage { get; set; }

        public double ChromCorrelationMedian { get; set; }

        public double AreaUnderRatioCurveRevised { get; set; }

        public double ChromCorrelationStdev { get; set; }

        public double PercentPeptideLabelled { get; set; }

        public double NumCarbonsLabelled { get; set; }

        public bool PassesFilter { get; set; }

        public int NumHighQualityProfilePeaks { get; set; }

        public double[] LabelDistributionVals { get; set; }

        public double FitScoreLabeledProfile { get; set; }



        /// <summary>
        /// Number of labeled carbons as a percent of the total number of carbons
        /// </summary>
        public double PercentCarbonsLabelled { get; set; }

        #endregion

         

    }
}
