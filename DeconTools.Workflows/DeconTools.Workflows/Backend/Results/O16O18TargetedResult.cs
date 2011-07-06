using System.Text;

namespace DeconTools.Workflows.Backend.Results
{
    public class O16O18TargetedResult : UnlabelledTargetedResult
    {



        #region Properties
        public float IntensityI2 { get; set; }
        public float IntensityI4 { get; set; }
        public double IntensityI4Adjusted { get; set; }
        public double Ratio { get; set; }


        public float IntensityTheorI0 { get; set; }
        public float IntensityTheorI2 { get; set; }
        public float IntensityTheorI4 { get; set; }

        #endregion


        public override string ToStringWithDetailsAsRow()
        {
            StringBuilder sb = new StringBuilder();

            string delim = "\t";

            sb.Append(this.MassTagID);
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
            sb.Append(this.IntensityTheorI0);
            sb.Append(delim);
            sb.Append(this.IntensityTheorI2);
            sb.Append(delim);
            sb.Append(this.IntensityTheorI4);
            sb.Append(delim);
            sb.Append(this.IntensityI0);
            sb.Append(delim);
            sb.Append(this.IntensityI2);
            sb.Append(delim);
            sb.Append(this.IntensityI4);
            sb.Append(delim);
            sb.Append(this.IntensityI4Adjusted);
            sb.Append(delim);
            sb.Append(this.Ratio.ToString("0.0000"));


            return sb.ToString();

        }
    }
}
