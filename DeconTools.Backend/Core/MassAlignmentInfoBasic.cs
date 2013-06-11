using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Core
{
    public class MassAlignmentInfoBasic:MassAlignmentInfo
    {
        
        public override double GetPpmShift(double mz, int scan=-1)
        {
            if (scan == -1) return AveragePpmShift;

            int indexOfClosestScanVal=  MathUtils.GetClosest(ScanAndPpmShiftVals.Xvalues, scan);
            return ScanAndPpmShiftVals.Yvalues[indexOfClosestScanVal];

        }

      
    }
}
