﻿
using System.Collections.Generic;

namespace DeconTools.Backend.Core.Results
{
    public class SipperLcmsTargetedResult : LcmsFeatureTargetedResult
    {
        #region Constructors

        public SipperLcmsTargetedResult(TargetBase target)
            : base(target)
        {
            ChromCorrelationStDev = -1;
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

        public double ChromCorrelationStDev { get; set; }

        public double PercentPeptideLabeled { get; set; }

        public double NumCarbonsLabeled { get; set; }

        public int NumHighQualityProfilePeaks { get; set; }

        /// <summary>
        /// Number of labeled carbons as a percent of the total number of carbons
        /// </summary>
        public double PercentCarbonsLabeled { get; set; }

        public List<double> LabelDistributionVals { get; set; }

        public int ContiguousnessScore { get; set; }

        public double RSquaredValForRatioCurve { get; set; }

        #endregion

        public override string ToString()
        {
            var target = Target as LcmsFeatureTarget;

            var data = new List<string>
            {
                Target.ID.ToString(),
                target?.FeatureToMassTagID.ToString() ?? "null",
                ScanSet?.PrimaryScanNumber.ToString("0") ?? "null"
            };

            if (IsotopicProfile == null)
            {
                for (var i = 0; i < 11; i++)
                {
                    data.Add("null");
                }
                data.Add(ErrorDescription);
            }
            else
            {
                data.Add(IsotopicProfile.MonoIsotopicMass.ToString("0.000"));
                data.Add(IsotopicProfile.MonoPeakMZ.ToString("0.000"));
                data.Add(Score.ToString("0.000"));
                data.Add(FitScoreLabeledProfile.ToString("0.000"));
                data.Add(NumHighQualityProfilePeaks.ToString("0"));
                data.Add(AreaUnderDifferenceCurve.ToString("0.000"));
                data.Add(AreaUnderRatioCurveRevised.ToString("0.000"));
                data.Add(ChromCorrelationMedian.ToString("0.000"));
                data.Add(NumCarbonsLabeled.ToString("0.000"));
                data.Add(PercentPeptideLabeled.ToString("0.000"));
                data.Add(PercentCarbonsLabeled.ToString("0.000"));
                data.Add("");   // ErrorDescription
            }

            return string.Join("; ", data);
        }
    }
}
