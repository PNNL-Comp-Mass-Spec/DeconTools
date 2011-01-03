using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultValidators
{
    public class InterferenceScorer
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public double GetInterferenceScore(XYData xydata, List<MSPeak> peakList, double leftBoundary, double rightBoundary, int startIndex)
        {
            
            int currentIndex = startIndex;
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            double sumIntensities = 0;
            double sumPeakIntensities = 0;
            int currentPeakIndex = 0;

            while (xydata.Xvalues[currentIndex] < rightBoundary && currentPeakIndex < peakList.Count)
            {

                bool isWithinRange = (!(xydata.Xvalues[currentIndex] < leftBoundary));

                if (isWithinRange)
                {
                    sumIntensities += xydata.Yvalues[currentIndex];

                    double sigma = peakList[currentPeakIndex].Width / 2.35;
                    double twoSigma = sigma * 2;

                    double leftPeakValue = peakList[currentPeakIndex].XValue - twoSigma;
                    double rightPeakValue = peakList[currentPeakIndex].XValue + twoSigma;

                    if (xydata.Xvalues[currentIndex] > leftPeakValue)
                    {

                        bool wentPastPeak = (xydata.Xvalues[currentIndex] > rightPeakValue);
                        if (wentPastPeak)
                        {
                            currentPeakIndex++;
                        }
                        else
                        {
                            sumPeakIntensities += xydata.Yvalues[currentIndex];
                        }

                    }
                    
                }
              

                currentIndex++;
                if (currentIndex >= xydata.Xvalues.Length) break;
            }

            double interferenceScore = 1 - (sumPeakIntensities / sumIntensities);
            return interferenceScore;
        }


        #endregion

        #region Private Methods

        #endregion


    }
}
