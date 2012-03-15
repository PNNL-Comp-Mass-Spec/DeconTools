
using System.Text;

namespace DeconTools.Backend.Core.Results
{
    public class SipperLcmsTargetedResult : LcmsFeatureTargetedResult
    {

        #region Constructors

        public SipperLcmsTargetedResult(TargetBase target)
            : base(target)
        {

        }

        #endregion

        #region Properties
        public double AreaUnderRatioCurve { get; set; }

        public double AreaUnderDifferenceCurve { get; set; }

        public double SlopeOfRatioCurve { get; set; }

        public double InterceptOfRatioCurve { get; set; }

        public double RSquaredValForRatioCurve { get; set; }

        public double ChromCorrelationMax { get; set; }

        public double ChromCorrelationMin { get; set; }

        public double ChromCorrelationMedian { get; set; }

        public double ChromCorrelationAverage { get; set; }

        public double AreaUnderRatioCurveRevised { get; set; }

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            string delim = "; ";

            sb.Append(this.Target.ID);
            sb.Append(delim);
            sb.Append(((LcmsFeatureTarget)Target).FeatureToMassTagID);
            sb.Append(delim);
            sb.Append(ScanSet == null ? "null" : ScanSet.PrimaryScanNumber.ToString());
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : IsotopicProfile.MonoIsotopicMass.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : IsotopicProfile.MonoPeakMZ.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : Score.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : InterferenceScore.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : AreaUnderDifferenceCurve.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : AreaUnderRatioCurve.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : AreaUnderRatioCurveRevised.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : RSquaredValForRatioCurve.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : ChromCorrelationMin.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : ChromCorrelationMax.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : ChromCorrelationAverage.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? "null" : ChromCorrelationMedian.ToString("0.000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile == null ? ErrorDescription : "");

            return sb.ToString();
        }


    }
}
