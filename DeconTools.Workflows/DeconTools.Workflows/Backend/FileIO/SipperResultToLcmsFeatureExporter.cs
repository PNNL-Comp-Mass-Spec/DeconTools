using System.Collections.Generic;
using System.Text;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class SipperResultToLcmsFeatureExporter : TargetedResultToTextExporter
    {
        #region Constructors
        public SipperResultToLcmsFeatureExporter(string filename)
            : base(filename)
        {
        }

        #endregion

        protected override string addAdditionalInfo(TargetedResultDTO result)
        {
            var sipperResult = (SipperLcmsFeatureTargetedResultDTO)result;

            var data = new List<string>
            {
                sipperResult.MatchedMassTagID.ToString(),
                sipperResult.NumHighQualityProfilePeaks.ToString("0"),
                sipperResult.FitScoreLabeledProfile.ToString("0.0000"),
                sipperResult.AreaUnderDifferenceCurve.ToString("0.000"),
                sipperResult.AreaUnderRatioCurve.ToString("0.000"),
                sipperResult.AreaUnderRatioCurveRevised.ToString("0.000"),
                sipperResult.ChromCorrelationMin.ToString("0.00000"),
                sipperResult.ChromCorrelationMax.ToString("0.00000"),
                sipperResult.ChromCorrelationAverage.ToString("0.00000"),
                sipperResult.ChromCorrelationMedian.ToString("0.00000"),
                sipperResult.ChromCorrelationStdev.ToString("0.00000"),
                sipperResult.NumCarbonsLabeled.ToString("0.000"),
                sipperResult.PercentCarbonsLabeled.ToString("0.00"),
                sipperResult.PercentPeptideLabeled.ToString("0.00"),
                sipperResult.ContiguousnessScore.ToString("0"),
                sipperResult.RSquaredValForRatioCurve.ToString("0.0000"),
                sipperResult.ValidationCode == ValidationCode.None ? string.Empty : sipperResult.ValidationCode.ToString()
            };
            //data.Add(GetLabelDistributionDataAsString(sipperResult));
            //data.Add(Delimiter);

            return string.Join(Delimiter.ToString(), data);
        }

        private string GetLabelDistributionDataAsString(SipperLcmsFeatureTargetedResultDTO sipperResult)
        {
            if (sipperResult.LabelDistributionVals == null || sipperResult.LabelDistributionVals.Length == 0)
                return string.Empty;

            var sb = new StringBuilder();
            var delim = ',';
            foreach (var labelDistributionVal in sipperResult.LabelDistributionVals)
            {
                sb.Append(labelDistributionVal.ToString("0.#####"));
                sb.Append(delim);
            }

            return sb.ToString().TrimEnd(delim);
        }

        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                base.buildHeaderLine(),
                "MatchedMassTagID",
                "NumHQProfilePeaks",
                "FitScoreLabeled",
                "AreaDifferenceCurve",
                "AreaRatioCurve",
                "AreaRatioCurveRevised",
                "ChromCorrMin",
                "ChromCorrMax",
                "ChromCorrAverage",
                "ChromCorrMedian",
                "ChromCorrStdev",
                "NumCarbonsLabeled",
                "PercentCarbonsLabeled",
                "PercentPeptidesLabeled",
                "ContigScore",
                "RSquared",
                "ValidationCode"
            };

            return string.Join(Delimiter.ToString(), data);
        }
    }
}
