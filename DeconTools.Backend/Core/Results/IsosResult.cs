using System;
using System.Collections.Generic;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public abstract class IsosResult
    {
        public int MSFeatureID { get; set; }
        public IList<ResultFlag> Flags = new List<ResultFlag>();

        public Run Run { get; set; }

        public ScanSet ScanSet { get; set; }

        public IsotopicProfile IsotopicProfile { get; set; }

        public double InterferenceScore { get; set; }

        public double IntensityAggregate { get; set; }

        public override string ToString()
        {
            if (IsotopicProfile == null)
                return base.ToString();

            var data = new List<string>
            {
                MSFeatureID.ToString(),
                ScanSet.PrimaryScanNumber.ToString(),
                IsotopicProfile.MonoIsotopicMass.ToString("0.00000"),
                IsotopicProfile.ChargeState.ToString(),
                IsotopicProfile.MonoPeakMZ.ToString("0.00000"),
                IntensityAggregate.ToString("0.00"),
                IsotopicProfile.Score.ToString("0.0000"),
                InterferenceScore.ToString("0.0000")
            };

            // Fit Score

            // Uncomment to write out the fit_count_basis
            //
            //data.Add(this.IsotopicProfile.ScoreCountBasis);				// Number of points used for the fit score

            return string.Join("; ", data);
        }

        public void Display()
        {
            Console.WriteLine(ToString());

        }


    }
}
