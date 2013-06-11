
namespace DeconTools.Backend.Core
{
    public abstract class MassAlignmentInfo
    {

        #region Constructors
        #endregion

        #region Properties

        public double AveragePpmShift { get; set; }
        public double StdevPpmShiftData { get; set; }
        public XYData ScanAndPpmShiftVals { get; set; }

        #endregion

        #region Public Methods

        public abstract double GetPpmShift(double mz, int scan = -1);


        public int GetNumPoints()
        {
            if (ScanAndPpmShiftVals == null) return 0;

            return ScanAndPpmShiftVals.Xvalues.Length;
        }


        #endregion

        #region Private Methods

        #endregion

    }
}
