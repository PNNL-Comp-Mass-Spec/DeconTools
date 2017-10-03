using System.Collections.Generic;

namespace DeconTools.Workflows.Backend.Results
{
    public class O16O18TargetedResultDTO : UnlabelledTargetedResultDTO
    {

        #region Properties
        public float IntensityI2 { get; set; }
        public float IntensityI4 { get; set; }
        public double IntensityI4Adjusted { get; set; }
        public double Ratio { get; set; }
        public double RatioFromChromCorr { get; set; }

        public float IntensityTheorI0 { get; set; }
        public float IntensityTheorI2 { get; set; }
        public float IntensityTheorI4 { get; set; }

        public float ChromCorrO16O18SingleLabel { get; set; }
        public float ChromCorrO16O18DoubleLabel { get; set; }



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
                IScore.ToString("0.0000"),
                IntensityTheorI0.ToString("0.000"),
                IntensityTheorI2.ToString("0.000"),
                IntensityTheorI4.ToString("0.000"),
                IntensityI0.ToString("0.000"),
                IntensityI2.ToString("0.000"),
                IntensityI4.ToString("0.000"),
                IntensityI4Adjusted.ToString("0.000"),
                ChromCorrO16O18SingleLabel.ToString("0.000"),
                ChromCorrO16O18DoubleLabel.ToString("0.000"),
                Ratio.ToString("0.0000"),
                RatioFromChromCorr.ToString("0.0000")
            };

            return string.Join("\t", data);

        }
    }
}
