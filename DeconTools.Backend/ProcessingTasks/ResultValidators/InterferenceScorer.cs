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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xydata">the raw data including noise and non-noise</param>
        /// <param name="peakList">the peak list representing the non-noise</param>
        /// <param name="leftBoundary">the left most xvalue to be considered</param>
        /// <param name="rightBoundary">the right most xvalue to be considered</param>
        /// <param name="startIndex">the index of the xydata from which to begin searching - for improving performance. Default is '0'</param>
        /// <returns></returns>
        public double GetInterferenceScore(XYData xydata, List<MSPeak> peakList, double leftBoundary, double rightBoundary, int startIndex = 0)
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
                    double threeSigma = sigma * 3;

                    double leftPeakValue = peakList[currentPeakIndex].XValue - threeSigma;
                    double rightPeakValue = peakList[currentPeakIndex].XValue + threeSigma;

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

        /// <summary>
        /// This calculates a score:  1- (I1/I2) where:
        /// I1= sum of intensities of target peaks.
        /// I2 = sum of intensities of all peaks.
        /// </summary>
        /// <param name="allPeaks">all peaks, including noise and non-noise</param>
        /// <param name="nonNoisePeaks">target peaks. </param>
        /// <param name="leftBoundary">the left-most x-value boundary. If any peak is less than this value, it isn't considered</param>
        /// <param name="rightBoundary">the right-most x-value boundary. If any peak is greater than this value, it isn't considered</param>
        /// <returns></returns>
        public double GetInterferenceScore(List<MSPeak> allPeaks, List<MSPeak> targetPeaks, double leftBoundary, double rightBoundary)
        {
            double sumAllPeakIntensities = 0;
            double sumTargetPeakIntensities = 0;

            for (int i = 0; i < allPeaks.Count; i++)
            {
                if (allPeaks[i].XValue > leftBoundary && allPeaks[i].XValue < rightBoundary)
                {
                    sumAllPeakIntensities += allPeaks[i].Height;
                }
                
            }

            foreach (var peak in targetPeaks)
            {
                if (peak.XValue > leftBoundary && peak.XValue < rightBoundary)
                {
                    sumTargetPeakIntensities += peak.Height;
                }
                
            }

            double interferenceScore = 1 - (sumTargetPeakIntensities / sumAllPeakIntensities);
            return interferenceScore;
        }

        #endregion

        #region Private Methods

        #endregion


    }
}
