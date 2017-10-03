using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core.Results
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
            var data = new List<string>() {  /// CheckListPopulate
            this.MassTagID,
            this.ChargeState,
            this.ScanLC,
            this.ScanLCStart,
            this.ScanLCEnd,
            this.NET.ToString("0.0000"),
            this.NumChromPeaksWithinTol,
            this.MonoMass.ToString("0.00000"),
            this.MonoMZ.ToString("0.00000"),
            this.FitScore.ToString("0.0000"),
            this.IScore.ToString("0.0000"),
            this.IntensityTheorI0,
            this.IntensityTheorI2,
            this.IntensityTheorI4,
            this.IntensityI0,
            this.IntensityI2,
            this.IntensityI4,
            this.IntensityI4Adjusted,
            this.Ratio.ToString("0.0000"))
            };

            return string.Join("\t", data);

        }
    }
}
