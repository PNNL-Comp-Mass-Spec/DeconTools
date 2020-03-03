
using System.Collections.Generic;

namespace DeconTools.Backend.Core
{
    public class IMSScanSet : ScanSet
    {

        public IMSScanSet(int primaryScanNum)
            : base(primaryScanNum)
        {

        }

        public IMSScanSet(int primaryScanNum, IEnumerable<int> indexArray)
            : base(primaryScanNum, indexArray)
        {

        }

        public IMSScanSet(int primaryScanNum, int lowerScan, int upperScan)
            : base(primaryScanNum, lowerScan, upperScan)
        {

        }

        public double DriftTime { get; set; }

    }
}
