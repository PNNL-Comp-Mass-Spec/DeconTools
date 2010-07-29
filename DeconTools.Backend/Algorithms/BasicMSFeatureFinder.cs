using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.Algorithms
{
    public class BasicMSFeatureFinder
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public IsotopicProfile FindMSFeature(List<IPeak> peakList, IsotopicProfile theorFeature, double toleranceInPPM, bool requireMonoPeak)
        {
            return null;



        }

        private IPeak findMostIntensePeak(List<IPeak> peaksWithinTol, double targetMZ)
        {
            double maxIntensity = 0;
            IPeak mostIntensePeak = null;

            for (int i = 0; i < peaksWithinTol.Count; i++)
            {
                float obsIntensity = peaksWithinTol[i].Height;
                if (obsIntensity > maxIntensity)
                {
                    maxIntensity = obsIntensity;
                    mostIntensePeak = peaksWithinTol[i];
                }
            }
            return mostIntensePeak;
        }


        private IPeak findClosestToXValue(List<IPeak> peaksWithinTol, double targetVal)
        {
            double diff = double.MaxValue;
            IPeak closestPeak = null;

            for (int i = 0; i < peaksWithinTol.Count; i++)
            {

                double obsDiff = Math.Abs(peaksWithinTol[i].XValue - targetVal);

                if (obsDiff < diff)
                {
                    diff = obsDiff;
                    closestPeak = peaksWithinTol[i];
                }

            }

            return closestPeak;
        }




        #endregion

        #region Private Methods
        #endregion
    }
}
