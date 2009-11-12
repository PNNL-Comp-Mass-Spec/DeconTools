using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Utilities
{
    public class PeakUtilities
    {


        public DeconToolsV2.Peaks.clsPeak lookupPeak(double mz, DeconToolsV2.Peaks.clsPeak[] peaklist, double mzVar)
        {
            for (int i = 0; i < peaklist.Length; i++)
            {
                if (Math.Abs(mz - peaklist[i].mdbl_mz) <= mzVar)
                {
                    return peaklist[i];
                }
            }
            return null;      //couldn't find a peak
        }

        public int lookupPeakIndex(double mz, DeconToolsV2.Peaks.clsPeak[] peaklist, double mzVar)
        {
            for (int i = 0; i < peaklist.Length; i++)
            {
                if (Math.Abs(mz - peaklist[i].mdbl_mz) <= mzVar)
                {
                    return i;
                }
            }
            return -1;      //couldn't find a peak

        }


        

    }
}
