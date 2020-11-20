using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultValidators
{
    /// <summary>
    /// Calculates a score that indicates the amount of noise present in XYData.  Currently geared for MS data but could be used for other data
    /// </summary>
    public class InterferenceScorer
    {
        #region Constructors

        public InterferenceScorer(double minRelativeIntensity = 0.025)
        {
            MinRelativeIntensity = minRelativeIntensity;
        }

        #endregion

        #region Properties
        public double MinRelativeIntensity { get; set; }

        #endregion

        #region Public Methods

        public double GetInterferenceScore(IsotopicProfile observedIso, List<Peak> observedMSPeaks)
        {
            if (observedIso == null) return 1.0;

            if (!observedMSPeaks.Any()) return 1.0;

            var leftBoundary = observedIso.getMonoPeak().XValue - 1.1;
            var rightMostPeak = observedIso.Peaklist[observedIso.Peaklist.Count - 1];
            var rightBoundary = rightMostPeak.XValue + rightMostPeak.Width / 2.35 * 2;  // 2 * sigma

            return GetInterferenceScore(observedIso, observedMSPeaks, leftBoundary, rightBoundary);
        }

        public double GetInterferenceScore(IsotopicProfile observedIso, List<Peak> observedMSPeaks, double minMz, double maxMz)
        {
            if (observedIso == null) return 1.0;

            if (!observedMSPeaks.Any()) return 1.0;

            var scanPeaks = observedMSPeaks.Select(i => (MSPeak)i).ToList();

            return GetInterferenceScore(scanPeaks, observedIso.Peaklist, minMz, maxMz);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xyData">the raw data including noise and non-noise</param>
        /// <param name="peakList">the peak list representing the non-noise</param>
        /// <param name="leftBoundary">the left most x value to be considered</param>
        /// <param name="rightBoundary">the right most x value to be considered</param>
        /// <param name="startIndex">the index of the xyData from which to begin searching - for improving performance. Default is '0'</param>
        /// <returns></returns>
        public double GetInterferenceScore(XYData xyData, List<MSPeak> peakList, double leftBoundary, double rightBoundary, int startIndex = 0)
        {
            var currentIndex = startIndex;
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            double sumIntensities = 0;
            double sumPeakIntensities = 0;
            var currentPeakIndex = 0;

            while (xyData.Xvalues[currentIndex] < rightBoundary && currentPeakIndex < peakList.Count)
            {
                var isWithinRange = (!(xyData.Xvalues[currentIndex] < leftBoundary));

                if (isWithinRange)
                {
                    sumIntensities += xyData.Yvalues[currentIndex];

                    var sigma = peakList[currentPeakIndex].Width / 2.35;
                    var threeSigma = sigma * 3;

                    var leftPeakValue = peakList[currentPeakIndex].XValue - threeSigma;
                    var rightPeakValue = peakList[currentPeakIndex].XValue + threeSigma;

                    if (xyData.Xvalues[currentIndex] > leftPeakValue)
                    {
                        var wentPastPeak = (xyData.Xvalues[currentIndex] > rightPeakValue);
                        if (wentPastPeak)
                        {
                            currentPeakIndex++;
                        }
                        else
                        {
                            sumPeakIntensities += xyData.Yvalues[currentIndex];
                        }
                    }
                }

                currentIndex++;
                if (currentIndex >= xyData.Xvalues.Length) break;
            }

            var interferenceScore = 1 - (sumPeakIntensities / sumIntensities);
            return interferenceScore;
        }

        /// <summary>
        /// This calculates a score:  1- (I1/I2) where:
        /// I1 = sum of intensities of target peaks.
        /// I2 = sum of intensities of all peaks.
        /// </summary>
        /// <param name="allPeaks">all peaks, including noise and non-noise</param>
        /// <param name="targetPeaks">target peaks. </param>
        /// <param name="leftBoundary">the left-most x-value boundary. If any peak is less than this value, it isn't considered</param>
        /// <param name="rightBoundary">the right-most x-value boundary. If any peak is greater than this value, it isn't considered</param>
        /// <returns></returns>
        public double GetInterferenceScore(List<MSPeak> allPeaks, List<MSPeak> targetPeaks, double leftBoundary, double rightBoundary)
        {
            double sumAllPeakIntensities = 0;
            double sumTargetPeakIntensities = 0;

            var maxPeak = GetMaxPeak(targetPeaks);

            if (maxPeak == null) return -1;

            foreach (var currentPeak in allPeaks)
            {
                if (currentPeak.XValue <= leftBoundary || currentPeak.XValue >= rightBoundary)
                    continue;

                var currentRelIntensity = currentPeak.Height / maxPeak.Height;
                if (currentRelIntensity >= MinRelativeIntensity)
                {
                    sumAllPeakIntensities += currentPeak.Height;
                }
            }

            foreach (var currentPeak in targetPeaks)
            {
                if (currentPeak.XValue <= leftBoundary || currentPeak.XValue >= rightBoundary)
                    continue;

                var currentRelIntensity = currentPeak.Height / maxPeak.Height;
                if (currentRelIntensity >= MinRelativeIntensity)
                {
                    sumTargetPeakIntensities += currentPeak.Height;
                }
            }

            var interferenceScore = 1 - (sumTargetPeakIntensities / sumAllPeakIntensities);
            return interferenceScore;
        }

        #endregion

        #region Private Methods

        private MSPeak GetMaxPeak(IEnumerable<MSPeak> msPeakList)
        {
            MSPeak maxMsPeak = null;

            foreach (var msPeak in msPeakList)
            {
                if (maxMsPeak == null)
                {
                    maxMsPeak = msPeak;
                    continue;
                }

                if (msPeak.Height > maxMsPeak.Height)
                {
                    maxMsPeak = msPeak;
                }
            }

            return maxMsPeak;
        }

        #endregion

    }
}
