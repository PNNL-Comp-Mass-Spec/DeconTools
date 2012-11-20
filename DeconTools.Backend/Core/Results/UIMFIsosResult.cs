using System;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class UIMFIsosResult : IsosResult
    {

        public UIMFIsosResult()
        {

        }
        
        public UIMFIsosResult(Run run, ScanSet lcScanset, IMSScanSet scanset)
        {
            this.Run = run;
            this.ScanSet = lcScanset;
            this.IMSScanSet = scanset;
            
        }

        public IMSScanSet IMSScanSet { get; set; }
        
        public double DriftTime { get; set; }
    }
}
