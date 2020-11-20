using System.Collections.Generic;

namespace DeconTools.Workflows.Backend.Results
{
    public class SipperLcmsFeatureTargetedResultDTO : UnlabeledTargetedResultDTO
    {
        #region Properties

        public double AreaUnderRatioCurve { get; set; }

        public double AreaUnderDifferenceCurve { get; set; }

        public double ChromCorrelationMin { get; set; }

        public double ChromCorrelationMax { get; set; }

        public double ChromCorrelationAverage { get; set; }

        public double ChromCorrelationMedian { get; set; }

        public double AreaUnderRatioCurveRevised { get; set; }

        public double ChromCorrelationStdev { get; set; }

        public double PercentPeptideLabeled { get; set; }

        public double NumCarbonsLabeled { get; set; }

        public bool PassesFilter { get; set; }

        public int NumHighQualityProfilePeaks { get; set; }

        public double[] LabelDistributionVals { get; set; }

        public double FitScoreLabeledProfile { get; set; }

        public double RSquaredValForRatioCurve { get; set; }

        /// <summary>
        /// Number of labeled carbons as a percent of the total number of carbons
        /// </summary>
        public double PercentCarbonsLabeled { get; set; }

        public int ContiguousnessScore { get; set; }

        #endregion

        public override string ToStringWithDetailsAsRow()
        {
            var data = new List<string>
            {
                TargetID.ToString(),
                ChargeState.ToString(),
                ScanLC.ToString(),
                ScanLCStart.ToString(),
                ScanLCEnd.ToString(),
                NET.ToString("0.0000"),
                NumChromPeaksWithinTol.ToString(),
                MonoMass.ToString("0.00000"),
                MonoMZ.ToString("0.00000"),
                FitScore.ToString("0.0000"),
                FitScoreLabeledProfile.ToString("0.0000"),
                ChromCorrelationMedian.ToString("0.000"),
                IScore.ToString("0.0000"),
                Intensity.ToString("0.0000"),
                IntensityI0.ToString("0.0000"),
                PercentCarbonsLabeled.ToString("0.000"),
                PercentPeptideLabeled.ToString("0.000"),
                ContiguousnessScore.ToString()
            };

            return string.Join("\t", data);
        }
    }
}
