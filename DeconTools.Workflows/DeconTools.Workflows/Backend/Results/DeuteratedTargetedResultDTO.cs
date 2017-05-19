using System.Text;

namespace DeconTools.Workflows.Backend.Results
{
    public class DeuteratedTargetedResultDTO : UnlabelledTargetedResultDTO
    {



        #region Properties
        //public float IntensityI2 { get; set; }
        //public float IntensityI4 { get; set; }
        //public double IntensityI4Adjusted { get; set; }
        //public double Ratio { get; set; }


        //public float IntensityTheorI0 { get; set; }
        //public float IntensityTheorI2 { get; set; }
        //public float IntensityTheorI4 { get; set; }

        
        public double HydrogenI0 { get; set; }
        public double HydrogenI1 { get; set; }
        public double HydrogenI2 { get; set; }
        public double HydrogenI3 { get; set; }
        public double HydrogenI4 { get; set; }
        public double DeuteriumI0 { get; set; }
        public double DeuteriumI1 { get; set; }
        public double DeuteriumI2 { get; set; }
        public double DeuteriumI3 { get; set; }
        public double DeuteriumI4 { get; set; }
        public double TheoryI0 { get; set; }
        public double TheoryI1 { get; set; }
        public double TheoryI2 { get; set; }
        public double TheoryI3 { get; set; }
        public double TheoryI4 { get; set; }
        public double RawI0 { get; set; }
        public double RawI1 { get; set; }
        public double RawI2 { get; set; }
        public double RawI3 { get; set; }
        public double RawI4 { get; set; }
        public double LabelingEfficiency {get;set;}
        public double RatioDH { get; set; }
        public double IntensityI0HydrogenMono { get; set; }

        /// <summary>
        /// area under the parabolic LC fit
        /// </summary>
        public double IndegratedLcAbundance { get; set; }

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
            sb.Append(this.IScore.ToString("0.0000"));
            sb.Append(delim);
            sb.Append(this.TheoryI0);
            sb.Append(delim);
            sb.Append(this.TheoryI1);
            sb.Append(delim);
            sb.Append(this.TheoryI2);
            sb.Append(delim);
            sb.Append(this.TheoryI3);
            sb.Append(delim);
            sb.Append(this.TheoryI4);
            sb.Append(delim);
            sb.Append(this.HydrogenI0);
            sb.Append(delim);
            sb.Append(this.HydrogenI1);
            sb.Append(delim);
            sb.Append(this.HydrogenI2);
            sb.Append(delim);
            sb.Append(this.HydrogenI3);
            sb.Append(delim);
            sb.Append(this.HydrogenI4);
            sb.Append(delim);
            sb.Append(this.DeuteriumI0);
            sb.Append(delim);
            sb.Append(this.DeuteriumI1);
            sb.Append(delim);
            sb.Append(this.DeuteriumI2);
            sb.Append(delim);
            sb.Append(this.DeuteriumI3);
            sb.Append(delim);
            sb.Append(this.DeuteriumI4);
            sb.Append(delim);
            sb.Append(this.RawI0);
            sb.Append(delim);
            sb.Append(this.RawI1);
            sb.Append(delim);
            sb.Append(this.RawI2);
            sb.Append(delim);
            sb.Append(this.RawI3);
            sb.Append(delim);
            sb.Append(this.RawI4);
            sb.Append(delim);
            sb.Append(this.LabelingEfficiency);
            sb.Append(delim);
            sb.Append(this.RatioDH.ToString("0.0000"));
            sb.Append(delim);
            sb.Append(this.IndegratedLcAbundance.ToString("0.0"));


            return sb.ToString();

        }
    }
}
