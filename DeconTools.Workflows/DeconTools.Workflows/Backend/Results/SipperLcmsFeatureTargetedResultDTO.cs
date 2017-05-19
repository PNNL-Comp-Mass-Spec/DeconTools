
using System.Text;

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

        public double RSquaredValForRatioCurve { get; set; }

        /// <summary>
        /// Number of labeled carbons as a percent of the total number of carbons
        /// </summary>
        public double PercentCarbonsLabelled { get; set; }

        public int ContiguousnessScore { get; set; }

        #endregion


        public override string ToStringWithDetailsAsRow()
        {
            var sb = new StringBuilder();

            var delim = "\t";

            sb.Append(this.TargetID);
            sb.Append(delim);
            sb.Append(this.ChargeState);
            sb.Append(delim);
            sb.Append(this.ScanLC);
            sb.Append(delim);
            sb.Append(this.ScanLCStart);
            sb.Append(delim);
            sb.Append(this.ScanLCEnd);
            sb.Append(delim);
            sb.Append(this.NET.ToString("0.0000"));
            sb.Append(delim);
            sb.Append(this.NumChromPeaksWithinTol);
            sb.Append(delim);
            sb.Append(this.MonoMass.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(this.MonoMZ.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(this.FitScore.ToString("0.0000"));
            sb.Append(delim);
            sb.Append(FitScoreLabeledProfile.ToString("0.0000"));
            sb.Append(delim);
            sb.Append(ChromCorrelationMedian.ToString("0.000"));
            sb.Append(delim);
            sb.Append(this.IScore.ToString("0.0000"));
            sb.Append(delim);
            sb.Append(this.Intensity);
            sb.Append(delim);
            sb.Append(this.IntensityI0);
            sb.Append(delim);
            sb.Append(PercentCarbonsLabelled);
            sb.Append(delim);
            sb.Append(PercentPeptideLabelled);
            sb.Append(delim);
            sb.Append(ContiguousnessScore);

            return sb.ToString();
        }
         

    }
}
