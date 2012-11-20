
namespace DeconTools.Backend.Core
{
    public class IMSScanSet : ScanSet
    {

        #region Constructors


        public IMSScanSet(int primaryScanNum)
            : base(primaryScanNum)
        {

        }

        public IMSScanSet(int primaryScanNum, int[] indexArray)
            : base(primaryScanNum, indexArray)
        {

        }

        public IMSScanSet(int primaryScanNum, int lowerScan, int upperScan)
            : base(primaryScanNum, lowerScan, upperScan)
        {

        }

        #endregion

        #region Properties


        public double DriftTime { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
