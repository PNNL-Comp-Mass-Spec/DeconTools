using System;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class UimfScanResult : ScanResult
    {
        public UimfScanResult(ScanSet frameset)
        {
            this.ScanSet = frameset;
        }
        
        public int LCScanNum { get; set; }

       public double FramePressureFront { get; set; }

        public double FramePressureBack { get; set; }

      
    }
}
