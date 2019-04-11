using System;
using System.Linq;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakDetectors
{
    public class ChromPeakDetectorMedianBased : ChromPeakDetector
    {
        private readonly ChromPeakDetector _chromPeakDetector;

        #region Constructors

        public ChromPeakDetectorMedianBased()
        {
            //set up the peak detector that is used for removing highest intensity peak.
            _chromPeakDetector = new ChromPeakDetector
            {
                PeakToBackgroundRatio = 2,
                SignalToNoiseThreshold = 2
            };

            IsDataThresholded = false;

        }

        public ChromPeakDetectorMedianBased(double chromPeakDetectorPeakBr, double chromPeakDetectorSigNoise) : this()
        {
            PeakToBackgroundRatio = chromPeakDetectorPeakBr;
            SignalToNoiseThreshold = chromPeakDetectorSigNoise;
        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion


        protected override double GetBackgroundIntensity(double[] yValues, double[] xValues = null)
        {
            var copiedYValues = new double[yValues.Length];
            Array.Copy(yValues, copiedYValues, yValues.Length);

            if (xValues != null)
            {
                //use a peak detector to find the largest peak
                var peaklist = _chromPeakDetector.FindPeaks(xValues, copiedYValues);

                var largestPeaks = peaklist.OrderByDescending(p => p.Height).Take(3);


                foreach (var peak in largestPeaks)
                {
                    if (peak != null)
                    {
                        var widthAtBase = peak.Width / 2.35 * 2;

                        var valuePeakStart = peak.XValue - widthAtBase;
                        var valuePeakEnd = peak.XValue + widthAtBase;

                        var indexOfStartOfPeak = MathUtils.GetClosest(xValues, valuePeakStart, 1);
                        var indexOfEndOfPeak = MathUtils.GetClosest(xValues, valuePeakEnd, 1);

                        for (var index = indexOfStartOfPeak; index < indexOfEndOfPeak; index++)
                        {
                            copiedYValues[index] = 0;     //zero-out the intensities of the largest peak.
                        }

                    }
                }

            }

            var valuesAboveZero = copiedYValues.Where(p => p > 0).ToList();


            double medianIntensity = 0;
            if (valuesAboveZero.Count > 0)
            {
                medianIntensity = MathUtils.GetMedian(valuesAboveZero);
            }

            return medianIntensity;


        }

    }
}
