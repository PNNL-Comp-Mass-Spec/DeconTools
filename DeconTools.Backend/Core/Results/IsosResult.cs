using System;
using System.Collections.Generic;
using System.Text;

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
            if (this.IsotopicProfile == null) 
                return base.ToString();

            string delim = "; ";

            StringBuilder sb = new StringBuilder();
            sb.Append(this.MSFeatureID);
            sb.Append(delim);
            sb.Append(this.ScanSet.PrimaryScanNumber);
            sb.Append(delim);
            sb.Append(this.IsotopicProfile.MonoIsotopicMass.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(this.IsotopicProfile.ChargeState);
            sb.Append(delim);
            sb.Append(this.IsotopicProfile.MonoPeakMZ.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(this.IntensityAggregate);
            sb.Append(delim);
            sb.Append(this.IsotopicProfile.Score.ToString("0.0000"));		// Fit Score
            sb.Append(delim);
            sb.Append(this.InterferenceScore.ToString("0.0000"));
            // Uncomment to write out the fit_count_basis
            //sb.Append(delim);
            //sb.Append(this.IsotopicProfile.ScoreCountBasis);				// Number of points used for the fit score
            
            return sb.ToString();
        }
        
        public void Display()
        {
            Console.WriteLine(this.ToString());

        }


    }
}
