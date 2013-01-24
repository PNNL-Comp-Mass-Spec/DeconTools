
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core.Results
{
    public class SipperLcmsTargetedResult : LcmsFeatureTargetedResult
    {

        #region Constructors

        public SipperLcmsTargetedResult(TargetBase target)
            : base(target)
        {
            ChromCorrelationStdev = -1;
        }

        #endregion

        #region Properties
        public double AreaUnderRatioCurve { get; set; }

        public double FitScoreLabeledProfile { get; set; }

        public double AreaUnderDifferenceCurve { get; set; }

        public double ChromCorrelationMax { get; set; }

        public double ChromCorrelationMin { get; set; }

        public double ChromCorrelationMedian { get; set; }

        public double ChromCorrelationAverage { get; set; }

        public double AreaUnderRatioCurveRevised { get; set; }

        public double ChromCorrelationStdev { get; set; }

        public double PercentPeptideLabelled { get; set; }

        public double NumCarbonsLabelled { get; set; }

        public int NumHighQualityProfilePeaks { get; set; }

        /// <summary>
        /// Number of labeled carbons as a percent of the total number of carbons
        /// </summary>
        public double PercentCarbonsLabelled { get; set; }

        public List<double> LabelDistributionVals { get; set; }

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            string delim = "; ";

            sb.Append(this.Target.ID);
            sb.Append(delim);
            sb.Append(((LcmsFeatureTarget)Target).FeatureToMassTagID);
            sb.Append(delim);
            sb.Append(ScanSet == null ? "null" : ScanSet.PrimaryScanNumber.ToString("0"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : IsotopicProfile.MonoIsotopicMass.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : IsotopicProfile.MonoPeakMZ.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : Score.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : FitScoreLabeledProfile.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : NumHighQualityProfilePeaks.ToString("0"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : AreaUnderDifferenceCurve.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : AreaUnderRatioCurveRevised.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : ChromCorrelationMedian.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : NumCarbonsLabelled.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : PercentPeptideLabelled.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : PercentCarbonsLabelled.ToString("0.000"));
            sb.Append(delim);


            sb.Append(IsotopicProfile == null ? ErrorDescription : "");

            return sb.ToString();
        }


    }
}
