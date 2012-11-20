
using System.Collections.Generic;

namespace DeconTools.Backend.Core
{
    public class LCScanSetIMS:ScanSet
    {

        #region Constructors

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


        #endregion

        #region Properties

        public  double FramePressureFront { get; set; }

        public double FramePressureBack { get; set; }

        public double AvgTOFLength { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
