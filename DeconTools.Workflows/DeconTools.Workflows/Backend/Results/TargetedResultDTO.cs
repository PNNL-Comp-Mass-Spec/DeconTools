using System.Text;

namespace DeconTools.Workflows.Backend.Results
{
    
    /// <summary>
    /// Result object for targeted workflows; free of any heavy objects
    /// </summary>
    public class TargetedResultDTO : DeconToolsResultDTO
    {

        #region Properties
        public long TargetID { get; set; }

        public int ScanLCStart { get; set; }
        public int ScanLCEnd { get; set; }

        public float NET { get; set; }
        public float NETError { get; set; }

        public int NumMSScansSummed { get; set; }
     
        public int NumChromPeaksWithinTol { get; set; }
        public int NumQualityChromPeaksWithinTol { get; set; }

        public string FailureType { get; set; }

        public string ErrorDescription { get; set; }


        public int MatchedMassTagID { get; set; }

        public string EmpiricalFormula { get; set; }

        #endregion

        #region Public Methods
        public virtual string ToStringWithDetailsAsRow()
        {
            StringBuilder sb = new StringBuilder();

            string delim = "\t";

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
            sb.Append(this.Intensity);
            sb.Append(delim);
            sb.Append(this.IntensityI0);


            return sb.ToString();
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
