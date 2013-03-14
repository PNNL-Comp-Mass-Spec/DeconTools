using System;
using System.Linq;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakDetectors
{
    public class ChromPeakDetectorMedianBased : ChromPeakDetector
    {
        private ChromPeakDetector _chromPeakDetector;

        #region Constructors

        public ChromPeakDetectorMedianBased()
        {
            //set up the peak detector that is used for removing highest intensity peak. 
            _chromPeakDetector = new ChromPeakDetector();
            _chromPeakDetector.PeakToBackgroundRatio = 2;
            _chromPeakDetector.SignalToNoiseThreshold = 2;

            IsDataThresholded = false;

        }

        public ChromPeakDetectorMedianBased(double chromPeakDetectorPeakBr, double chromPeakDetectorSigNoise):this()
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
        

        protected override double GetBackgroundIntensity(double[] yvalues, double[]xvalues=null)
        {
            double[] copiedYValues = new double[yvalues.Length];
            Array.Copy(yvalues, copiedYValues, yvalues.Length);

            if (xvalues!=null)
            {
                //use a peak detector to find the largest peak
                var peaklist=  _chromPeakDetector.FindPeaks(xvalues, copiedYValues);

                var largestPeaks = peaklist.OrderByDescending(p => p.Height).Take(3);


                foreach (var peak in largestPeaks)
                {
                    if (peak != null)
                    {
                        var widthAtBase = peak.Width / 2.35 * 2;

                        double valuePeakStart = peak.XValue - widthAtBase;
                        double valuePeakEnd = peak.XValue + widthAtBase;

                        int indexOfStartOfPeak = MathUtils.GetClosest(xvalues, valuePeakStart, 1);
                        int indexOfEndOfPeak = MathUtils.GetClosest(xvalues, valuePeakEnd, 1);

                        for (int index = indexOfStartOfPeak; index < indexOfEndOfPeak; index++)
                        {
                            copiedYValues[index] = 0;     //zero-out the intensities of the largest peak. 
                        }

                    }
                }


             

            }

            var valuesAboveZero = copiedYValues.Where(p => p > 0).ToList();


            var medianIntensity = MathUtils.GetMedian(valuesAboveZero);

            return medianIntensity;


        }

    }
}
