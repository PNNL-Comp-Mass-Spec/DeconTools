using System.Collections.Generic;

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

        /// <summary>
        /// Target's code. For peptides this is the amino acid sequence.
        /// </summary>
        public string Code { get; set; }

        #endregion

        #region Public Methods
        public virtual string ToStringWithDetailsAsRow()
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
                IScore.ToString("0.0000"),
                Intensity.ToString("0.0000"),
                IntensityI0.ToString("0.0000")
            };

            return string.Join("\t", data);
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
