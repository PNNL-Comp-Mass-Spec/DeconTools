using System;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class UIMFIsosResult : IsosResult
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public UIMFIsosResult()
        {
        }

        /// <summary>
        /// Constructor that takes run, LC scanset, and an IMS scan set
        /// </summary>
        /// <param name="run"></param>
        /// <param name="lcScanset"></param>
        /// <param name="scanset"></param>
        public UIMFIsosResult(Run run, ScanSet lcScanset, IMSScanSet scanset)
        {
            Run = run;
            ScanSet = lcScanset;
            IMSScanSet = scanset;

        }

        public IMSScanSet IMSScanSet { get; set; }

        public double DriftTime { get; set; }
    }
}
