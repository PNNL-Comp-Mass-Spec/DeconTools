using System;
using System.Collections.Generic;
using System.Linq;

namespace DeconTools.Backend.Utilities
{
    public class MathUtils
    {

        public static double GetInterpolatedValue(double x1, double x2, double y1, double y2, double targetXValue)
        {

            if (Math.Abs(x1 - x2) < double.Epsilon) return y1;

            var slope = (y2 - y1) / (x2 - x1);
            var yIntercept = y1 - (slope * x1);
            var interpolatedVal = targetXValue * slope + yIntercept;
            return interpolatedVal;
        }

        public static double GetAverage(List<double> values)
        {
            return MathNet.Numerics.Statistics.Statistics.Mean(values);
        }

        public static double GetAverage(double[] values)
        {
            return MathNet.Numerics.Statistics.Statistics.Mean(values);
        }

        public static double GetStDev(List<double> values)
        {
            return MathNet.Numerics.Statistics.Statistics.StandardDeviation(values);
        }

        public static double GetStDev(double[] values)
        {
            return MathNet.Numerics.Statistics.Statistics.StandardDeviation(values);
        }


        public static double GetMedian(double[] values)
        {
            return MathNet.Numerics.Statistics.Statistics.Median(values);
        }



        public static double GetMedian(List<double> values)
        {
            return MathNet.Numerics.Statistics.Statistics.Median(values);
        }

        /// <summary>
        /// Find the value in data that is closest to targetVal, within +/-tolerance
        /// </summary>
        /// <param name="data">List of values to search (must be sorted)</param>
        /// <param name="targetVal"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static int GetClosest(double[] data, double targetVal, double tolerance = 0.1)
        {
            if (data.Length == 0) return -1;

            var binarySearchIndex = BinarySearchWithTolerance(data, targetVal, 0, data.Length - 1, tolerance);
            if (binarySearchIndex == -1) binarySearchIndex = 0;

            var indexIsBelowTarget = (data[binarySearchIndex] < targetVal);

            var indexOfClosest = -1;


            if (indexIsBelowTarget)
            {
                var diff = double.MaxValue;
                for (var i = binarySearchIndex; i < data.Length; i++)
                {
                    var currentDiff = Math.Abs(data[i] - targetVal);
                    if (currentDiff < diff)
                    {
                        diff = currentDiff;
                        indexOfClosest = i;
                    }
                    else
                    {
                        break;
                    }



                }
            }
            else
            {
                var diff = double.MaxValue;
                for (var i = binarySearchIndex; i >= 0; i--)
                {
                    var currentDiff = Math.Abs(data[i] - targetVal);
                    if (currentDiff < diff)
                    {
                        diff = currentDiff;
                        indexOfClosest = i;
                    }
                    else
                    {
                        break;
                    }


                }


            }
            return indexOfClosest;


        }

        /// <summary>
        /// Use a binary search to find a value in data that is within +/-tolerance of the target value
        /// </summary>
        /// <param name="data">List of values to search (must be sorted)</param>
        /// <param name="targetVal"></param>
        /// <param name="leftIndex"></param>
        /// <param name="rightIndex"></param>
        /// <param name="tolerance"></param>
        /// <returns>Index of the matched value, or -1 if no match</returns>
        /// <remarks>Returns the first value found within tolerance of the targetValue, not necessarily the closest value</remarks>
        public static int BinarySearchWithTolerance(double[] data, double targetVal, int leftIndex, int rightIndex, double tolerance)
        {
            if (leftIndex <= rightIndex)
            {
                var middle = (leftIndex + rightIndex) / 2;
                if (Math.Abs(targetVal - data[middle]) <= tolerance)
                {
                    return middle;
                }

                if (targetVal < data[middle])
                {
                    return BinarySearchWithTolerance(data, targetVal, leftIndex, middle - 1, tolerance);
                }

                return BinarySearchWithTolerance(data, targetVal, middle + 1, rightIndex, tolerance);
            }

            return -1;

        }


        public static int BinarySearch(int[] data, int targetVal, int leftIndex, int rightIndex)
        {
            if (leftIndex <= rightIndex)
            {
                var middle = (leftIndex + rightIndex) / 2;
                if (targetVal == data[middle])
                {
                    return middle;
                }

                if (targetVal < data[middle])
                {
                    return BinarySearch(data, targetVal, leftIndex, middle - 1);
                }

                return BinarySearch(data, targetVal, middle + 1, rightIndex);
            }

            return -1;

        public static void GetLinearRegression(double[] xVals, double[] yVals, out double slope, out double intercept, out double rSquaredVal)
        {

            var fitResult = MathNet.Numerics.Fit.Line(xVals.ToArray(), yVals.ToArray());
            slope = fitResult.Item2;
            intercept = fitResult.Item1;

            rSquaredVal = MathNet.Numerics.GoodnessOfFit.RSquared(xVals.Select(x => fitResult.Item1 + fitResult.Item2 * x), yVals);
        }

        [Obsolete("Use GetLinearRegression which uses MathNet.Numerics")]
        public static void GetLinearRegression_Manual(double[] xVals, double[] yVals, out double slope, out double intercept, out double rSquaredVal)
        {
            var inputData = new double[xVals.Length, 2];

            for (var i = 0; i < xVals.Length; i++)
            {
                inputData[i, 0] = xVals[i];
                inputData[i, 1] = yVals[i];
            }

            var numIndependentVariables = 1;
            var numPoints = yVals.Length;
            alglib.lrbuild(inputData, numPoints, numIndependentVariables, out var _, out var linearModel, out var regressionReport);

            double[] regressionLineInfo;

            try
            {
                alglib.lrunpack(linearModel, out regressionLineInfo, out numIndependentVariables);

            }
            catch (Exception ex)
            {
                IqLogger.IqLogger.LogError(
                    "----> FATAL error occurred during linear regression. xVals length= " +
                    xVals.Length + "; yVals length= " + yVals.Length + "; " + regressionReport.cvavgerror + ", " + ex.Message, ex);

                slope = -99999999;
                intercept = -9999999;
                rSquaredVal = -9999999;
                return;
            }

            slope = regressionLineInfo[0];
            intercept = regressionLineInfo[1];

            var squaredResiduals = new List<double>();
            var squaredMeanResiduals = new List<double>();

            var averageY = yVals.Average();


            for (var i = 0; i < xVals.Length; i++)
            {
                var calcYVal = alglib.lrprocess(linearModel, new[] { xVals[i] });
                new List<double>().Add(calcYVal);

                var residual = yVals[i] - calcYVal;
                squaredResiduals.Add(residual * residual);

                var meanResidual = yVals[i] - averageY;
                squaredMeanResiduals.Add(meanResidual * meanResidual);
            }

            var sumSquaredMeanResiduals = squaredMeanResiduals.Sum();

            //check for sum=0
            if (Math.Abs(sumSquaredMeanResiduals) < double.Epsilon)
            {
                rSquaredVal = 0;
            }
            else
            {
                rSquaredVal = 1.0d - (squaredResiduals.Sum() / sumSquaredMeanResiduals);
            }


        }



    }
}
