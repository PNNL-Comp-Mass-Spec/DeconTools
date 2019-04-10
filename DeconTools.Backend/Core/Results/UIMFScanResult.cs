using System;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class UimfScanResult : ScanResult
    {
        public UimfScanResult(ScanSet frameSet)
        {
            ScanSet = frameSet;
        }

        public int LCScanNum { get; set; }

        public double FramePressureUnsmoothed { get; set; }

        public double FramePressureSmoothed { get; set; }


    }
}
