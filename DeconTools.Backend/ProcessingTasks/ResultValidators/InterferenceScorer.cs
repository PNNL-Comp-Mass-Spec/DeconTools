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


            var leftBoundary = observedIso.getMonoPeak().XValue-1.1;
            var rightMostPeak = observedIso.Peaklist[observedIso.Peaklist.Count - 1];

            var rightBoundary = rightMostPeak.XValue + rightMostPeak.Width/2.35 *2;  // 2 * sigma

            List<MSPeak> scanPeaks = observedMSPeaks.Select<Peak, MSPeak>(i => (MSPeak)i).ToList();

             return  GetInterferenceScore(scanPeaks, observedIso.Peaklist, leftBoundary, rightBoundary);



        }



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

            MSPeak maxPeak = GetMaxPeak(targetPeaks);
 
            if (maxPeak == null) return -1;


            for (int i = 0; i < allPeaks.Count; i++)
            {
                var currentPeak = allPeaks[i];

                if (currentPeak.XValue > leftBoundary && currentPeak.XValue < rightBoundary)
                {
                    var currentRelIntensity = currentPeak.Height/maxPeak.Height;
                    if (currentRelIntensity >= MinRelativeIntensity)
                    {
                        sumAllPeakIntensities += currentPeak.Height; 
                    }
                    
                    
                }
                
            }

            foreach (var peak in targetPeaks)
            {

                if (peak.XValue > leftBoundary && peak.XValue < rightBoundary)
                {
                    var currentRelIntensity = peak.Height / maxPeak.Height;
                    if (currentRelIntensity >= MinRelativeIntensity)
                    {
                        sumTargetPeakIntensities += peak.Height;
                    }
                    
                }
                
            }

            double interferenceScore = 1 - (sumTargetPeakIntensities / sumAllPeakIntensities);
            return interferenceScore;
        }

       
        #endregion

        #region Private Methods

        private MSPeak GetMaxPeak (IEnumerable<MSPeak>mspeakList)
        {

            MSPeak maxMsPeak=null;
            
            foreach (var msPeak in mspeakList)
            {
                if (maxMsPeak==null)
                {
                    maxMsPeak = msPeak;
                    continue;
                    
                }

                if (msPeak.Height>maxMsPeak.Height)
                {
                    maxMsPeak = msPeak;
                }
            }

            return maxMsPeak;



        }

        #endregion


    }
}
