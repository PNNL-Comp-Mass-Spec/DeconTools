
using System.Collections.Generic;

namespace DeconTools.Backend.Core
{
    public class LCScanSetIMS : ScanSet
    {
        public LCScanSetIMS(int primaryScanNum)
          : base(primaryScanNum)
        {
        }

        public LCScanSetIMS(int primaryScanNum, IEnumerable<int> indexArray)
            : base(primaryScanNum, indexArray)
        {
        }

        public LCScanSetIMS(int primaryScanNum, int lowerScan, int upperScan)
            : base(primaryScanNum, lowerScan, upperScan)
        {
        }

        public double FramePressureSmoothed { get; set; }

        public double FramePressureUnsmoothed { get; set; }

        public double AvgTOFLength { get; set; }
    }
}
