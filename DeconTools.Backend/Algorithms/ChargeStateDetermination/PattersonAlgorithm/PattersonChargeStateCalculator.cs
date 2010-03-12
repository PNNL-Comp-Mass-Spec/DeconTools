using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using Engine.PeakProcessing;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm
{
    public class PattersonChargeStateCalculator
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods


        private List<double> ACss(List<double> inData)
        {
            List<double> outData = new List<double>();

            double sum = 0;
            double average = 0;

            for (int i = 0; i < inData.Count; i++)
            {
                sum += inData[i];

            }
            average = sum / inData.Count;

            sum = 0;
            for (int i = 0; i < inData.Count; i++)
            {
                sum = 0;
                int j;
                for (j = 0; j < (inData.Count - i - 1); j++)
                {
                    sum += ((inData[i] - average) * (inData[i + j] - average));
                }

                if (j > 0)
                {
                    outData.Add(sum / inData.Count);
                }
                else
                {
                    outData.Add(0);
                }


            }

            return outData;

        }

        public short GetChargeState(XYData rawData, List<IPeak> peakList, MSPeak peak)
        {

            //look in rawData to the left (-0.1) and right (+1.1) of peak
            double minus = 0.1;
            double plus = 1.1;

            double fwhm = peak.Width;

            int leftIndex = MathUtils.BinarySearchWithTolerance(rawData.Xvalues, peak.XValue - fwhm - minus,
                0, rawData.Xvalues.Length - 1, 0.2);

            int rightIndex = MathUtils.BinarySearchWithTolerance(rawData.Xvalues, peak.XValue + fwhm + plus,
                leftIndex + 1, rawData.Xvalues.Length - 1, 0.2);

            //then 'Splint' and 'Spline' points to make them evenly spaced
            //Use ALGLIB to do this


            int numPoints = rightIndex - leftIndex + 1;
            int numL = numPoints;

            if (numPoints < 5)
                return -1;

            if (numPoints < 256)
                numL = 10 * numPoints;

            double[] filteredXValues = getFilteredXYData(rawData.Xvalues, leftIndex, rightIndex);
            double[] filteredYValues = getFilteredXYData(rawData.Yvalues, leftIndex, rightIndex);



            alglib.spline1d.spline1dinterpolant interpolant = new alglib.spline1d.spline1dinterpolant();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            alglib.spline1d.spline1dbuildcubic(filteredXValues, filteredYValues, filteredXValues.Length, 1, +1, 1, -1, ref interpolant);

            sw.Stop();
            Console.WriteLine("spline time = " + sw.ElapsedMilliseconds);





            //then extract Autocorrelation scores (ACss)

            //determine highest charge state peak (?)





            return 0;

        }

        private double[] getFilteredXYData(double[] inputVals, int leftIndex, int rightIndex)
        {
            int numPoints = rightIndex - leftIndex + 1;

            double[] outputVals = new double[numPoints];


            for (int i = 0; i < numPoints; i++)
            {
                outputVals[i] = inputVals[leftIndex + i];

            }

            return outputVals;
        }



        #endregion
    }
}
