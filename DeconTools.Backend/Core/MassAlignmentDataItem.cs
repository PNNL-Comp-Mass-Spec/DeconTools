
namespace DeconTools.Backend.Core
{
    public class MassAlignmentDataItem
    {

    

        #region Constructors
        public MassAlignmentDataItem(float mz, float mzPPMCorrection, float scan, float scanPPMCorrection)
        {
            this.mz = mz;
            this.mzPPMCorrection = mzPPMCorrection;
            this.scan = scan;
            this.scanPPMCorrection = scanPPMCorrection;
        }

        #endregion

        #region Properties
        public float mz { get; set; }
        public float mzPPMCorrection { get; set; }
        public float scan { get; set; }
        public float scanPPMCorrection { get; set; }

        #endregion

    }
}
