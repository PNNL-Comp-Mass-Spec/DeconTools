using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core.Results
{
    public class TargetedResult : DeconToolsResult
    {

        #region Properties
        public long MassTagID { get; set; }

        public int ScanLCStart { get; set; }
        public int ScanLCEnd { get; set; }

        public float NET { get; set; }
        public float NETAligned { get; set; }

        public int NumChromPeaksWithinTol { get; set; }
        #endregion

        #region Public Methods
        public virtual string ToStringWithDetailsAsRow()
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
            this.Intensity,
            this.IntensityI0,
};
            return string.Join(delim, data);

        }

        #endregion

        #region Private Methods

        #endregion

    }
}
