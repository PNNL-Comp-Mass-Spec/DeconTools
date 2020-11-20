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
        /// Constructor that takes run, LC scan set, and an IMS scan set
        /// </summary>
        /// <param name="run"></param>
        /// <param name="lcScanSet"></param>
        /// <param name="scanSet"></param>
        public UIMFIsosResult(Run run, ScanSet lcScanSet, IMSScanSet scanSet)
        {
            Run = run;
            ScanSet = lcScanSet;
            IMSScanSet = scanSet;
        }

        public IMSScanSet IMSScanSet { get; set; }

        public double DriftTime { get; set; }
    }
}
