using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Utilities
{
    [TestFixture]
    public class MathUtilsTests
    {
        [Test]
        public void MedianTest1()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var testValsEmpty = new List<double>();
            var median = MathUtils.GetMedian(testValsEmpty);
            Assert.AreEqual(double.NaN, median);

            var testValsOne = new List<double> { 5 };
            median = MathUtils.GetMedian(testValsOne);
            Assert.AreEqual(5, median);

            var testValsTwo = new List<double> { 5, 8 };
            median = MathUtils.GetMedian(testValsTwo);
            Assert.AreEqual(6.5, median);

            var testValsThree = new List<double> { 5, 300, 8 };
            median = MathUtils.GetMedian(testValsThree);
            Assert.AreEqual(8, median);

            var testVals = new List<double> { 5, 25, 4, 72, 50, 45, 50, 45, 32, 73, 8, 300, 400, 200, 3, 9 };
            median = MathUtils.GetMedian(testVals);
            Assert.AreEqual(45, median);
        }

        [Test]
        [TestCase(0, 10, 2, 6, 3, 4, 4)]
        [TestCase(0, 10, 2, 6, 7.5, -5, -5)]
        [TestCase(0, 10, 2, 6, -9, 28, 28)]
        [TestCase(0, 10, 0, 6, 0, 10, double.NaN)]
        [TestCase(0, 10, 0, 6, -5, 10, double.PositiveInfinity)]
        [TestCase(0, 10, 0, 6, 5, 10, double.PositiveInfinity)]
        [TestCase(0, 8, 0, 6, 5, 8, double.PositiveInfinity)]
        public void GetInterpolatedValue(double x1, double y1, double x2, double y2, double targetX, double mathUtilsExpectedY, double mathNetExpectedY)
        {
            // If x1 and x2 are the same, GetInterpolatedValue returns y2
            var mathUtilsResult = MathUtils.GetInterpolatedValue(x1, x2, y1, y2, targetX);

            var points = new List<double> { x1, x2 };

            var values = new List<double> { y1, y2 };

            var interpolationResult = MathNet.Numerics.Interpolate.Linear(points, values);

            // If x1 and x2 are the same, MathNet returns -infinity
            var mathNetResult = interpolationResult.Interpolate(targetX);

            Console.WriteLine("Interpolated value at x={0}\nMathUtils: {1,6}\nMathNet:   {2,6}",
                              targetX, mathUtilsResult, mathNetResult);
            Console.WriteLine();

            Assert.AreEqual(mathUtilsExpectedY, mathUtilsResult, Math.Abs(mathUtilsExpectedY / 10000.0));

            if (double.IsNaN(mathNetExpectedY)) return;

            if (double.IsNegativeInfinity(mathNetExpectedY) || double.IsPositiveInfinity(mathNetExpectedY))
            {
                Assert.True(double.IsNegativeInfinity(mathNetResult) || double.IsPositiveInfinity(mathNetResult),
                            "Expected the interpolated value from Math.NET to be positive or negative infinity, it is instead " + mathNetResult);
            }
            else
            {
                Assert.AreEqual(mathNetExpectedY, mathNetResult, Math.Abs(mathNetExpectedY / 10000.0));
            }
        }

        [Test]
        [TestCase(5, 10, 6.1137540601, 6.1137540601)]
        [TestCase(5, 100, 25.05171146, 25.05171146)]
        [TestCase(5, 1000, 5.6784198E+009, 5.6784198E+009)]
        [TestCase(5, 10000, 1.260347168E+105, 1.260347168E+105)]
        [TestCase(1500, 50, 3384.10223, 3384.10223)]
        [TestCase(1500, 100000, 2.84932984E+305, 2.84932984E+305)]
        [TestCase(43248398, 100000, 2.526209E+305, 2.526209E+305)]
        [TestCase(1E+50, 100000, 3.054249E+305, 3.054249E+305)]
        [TestCase(1E+200, 100000, 7.29530036E+305, 7.29530036E+305)]
        [TestCase(1E+307, 100000, 6.06371026E+307, 6.06371026E+307)]
        public void ComputeAverage(double startValue, int dataCount, double mathUtilsExpectedAverage, double mathNetExpectedAverage)
        {
            var values = GetLargeValuesList(startValue, dataCount);

            // If the sum of the values is larger than Double.MaxValue, GetAverage returns Infinity
            var mathUtilsAvgFromList = MathUtils.GetAverage(values);
            var mathUtilsAvgFromArray = MathUtils.GetAverage(values.ToArray());

            // MathNet properly computes the average if the sum of the values is larger than Double.MaxValue
            // ReSharper disable once InvokeAsExtensionMethod
            var mathNetAvg = MathNet.Numerics.Statistics.Statistics.Mean(values);

            // Linq reports infinity when the sum of the values is larger than Double.MaxValue
            var avgFromLinq = values.Average();

            Console.WriteLine("List:    {0,10:E7}\nArray:   {1,10:E7}\nMathNet: {2,10:E7}\nLinq:    {3,10:E7}",
                              mathUtilsAvgFromList, mathUtilsAvgFromArray, mathNetAvg, avgFromLinq);
            Console.WriteLine();

            Assert.AreEqual(mathUtilsExpectedAverage, mathUtilsAvgFromList, mathUtilsExpectedAverage / 10000.0);

            Assert.AreEqual(mathNetExpectedAverage, mathNetAvg, mathNetExpectedAverage / 10000.0);
        }

        [Test]
        [TestCase(5, 1, double.NaN)]
        [TestCase(5, 2, 0.17452651)]
        [TestCase(5, 3, 0.16567731)]
        [TestCase(5, 4, 0.19725679)]
        [TestCase(5, 10, 0.66639264)]
        [TestCase(5, 100, 16.763137)]
        [TestCase(5, 1000, 1.9075839E+010)]
        [TestCase(5, 10000, 1.3958984E+106)]
        [TestCase(1500, 50, 1.2239127E+003)]
        [TestCase(1500, 500, 4.8593655E+007)]
        [TestCase(43248398, 1000, 1.6499989E+017)]
        [TestCase(1E+40, 10000, 2.7917968E+145)]
        public void ComputeStDev(double startValue, int dataCount, double expectedStDev)
        {
            var values = GetLargeValuesList(startValue, dataCount);
            var mathUtilsStDevFromList = MathUtils.GetStDev(values);
            var mathUtilsStDevFromArray = MathUtils.GetStDev(values.ToArray());
            var mathNetStDev = MathNet.Numerics.Statistics.Statistics.StandardDeviation(values);

            Console.WriteLine("List:    {0,10:E7}\nArray:   {1,10:E7}\nMathNet: {2,10:E7}",
                              mathUtilsStDevFromList, mathUtilsStDevFromArray, mathNetStDev);
            Console.WriteLine();

            Assert.AreEqual(expectedStDev, mathUtilsStDevFromList, expectedStDev / 10000.0);
            Assert.AreEqual(expectedStDev, mathNetStDev, expectedStDev / 10000.0);
        }

        [Test]
        [TestCase(5, 0, double.NaN)]
        [TestCase(5, 1, 5.19194798)]
        [TestCase(5, 2, 5.31535686)]
        [TestCase(5, 10, 6.084867)]
        [TestCase(5, 100, 19.969399)]
        [TestCase(5, 1000, 900130.207205)]
        [TestCase(5, 10000, 1.03286E+54)]
        [TestCase(1500, 50, 3278.2780185)]
        [TestCase(1500, 100000, 3.4077159E+155)]
        [TestCase(43248398, 100000, 4.899073E+157)]
        [TestCase(1E+50, 100000, 6.13832008E+178)]
        [TestCase(1E+200, 100000, 1.0036869E+254)]
        [TestCase(1E+307, 100000, 4.607478E+307)]
        public void ComputeMedian(double startValue, int dataCount, double expectedMedian)
        {
            var values = GetLargeValuesList(startValue, dataCount);
            var mathUtilsMedianFromList = MathUtils.GetMedian(values);
            var mathUtilsMedianFromArray = MathUtils.GetMedian(values.ToArray());
            var mathNetMedian = MathNet.Numerics.Statistics.Statistics.Median(values);

            Console.WriteLine("List:    {0,10:E7}\nArray:   {1,10:E7}\nMathNet: {2,10:E7}",
                              mathUtilsMedianFromList, mathUtilsMedianFromArray, mathNetMedian);
            Console.WriteLine();

            Assert.AreEqual(expectedMedian, mathUtilsMedianFromList, expectedMedian / 10000.0);
            Assert.AreEqual(expectedMedian, mathNetMedian, expectedMedian / 10000.0);
        }

        [Test]
        [TestCase(5000, 8000, 6325.3, 0.75, 1325, 1324)]
        [TestCase(5000, 8000, 6300.8, 0.75, 1300, 1300)]
        [TestCase(5000, 8000, 5025, 0.75, 25, 24)]
        [TestCase(5000, 8000, 5000, 0.75, 0, -1)]
        [TestCase(5000, 8000, 8000, 0.75, 2999, 2999)]
        [TestCase(5000, 8000, 9000, 0.75, 3000, -1)]
        [TestCase(500, 6000, 1500, 0.75, 999, 999)]
        public void GetClosestDouble(int startIndex, int endIndex, double targetValue, double tolerance, int expectedClosestValueIndex, int expectedBinarySearchIndex)
        {
            var rand = new Random(314);
            var values = new List<double>();
            for (var i = startIndex; i <= endIndex; i++)
            {
                values.Add(i + rand.NextDouble());
            }

            // If the list does not contain the target value (within the tolerance), returns -1
            // GetClosest calls BinarySearchWithTolerance to find the index of a value within the tolerance
            // It then examines data near that index to find the value truly closest to the target value
            var closestValueIndex1 = MathUtils.GetClosest(values.ToArray(), targetValue, tolerance);

            var leftIndex = 0;
            var rightIndex = values.Count - 1;

            // If the list does not contain the target value (within the tolerance), returns -1
            // The binary search algorithm returns the first value found within tolerance of the targetValue, not necessarily the closest value
            var closestValueIndex2 = MathUtils.BinarySearchWithTolerance(values.ToArray(), targetValue, leftIndex, rightIndex, tolerance);

            Console.WriteLine("Index of value {0} is\n  {1} via GetClosest and\n  {2} via BinarySearchWithTolerance",
                              targetValue, closestValueIndex1, closestValueIndex2);
            Console.WriteLine();

            Assert.AreEqual(expectedClosestValueIndex, closestValueIndex1);
            Assert.AreEqual(expectedBinarySearchIndex, closestValueIndex2);
        }

        [Test]
        [TestCase(5000, 8000, 6318, -1)]
        [TestCase(5000, 8000, 6319, -1)]
        [TestCase(5000, 8000, 6320, -1)]
        [TestCase(5000, 8000, 6321, 453)]
        [TestCase(5000, 8000, 6322, -1)]
        [TestCase(5000, 8000, 6323, -1)]
        [TestCase(5000, 8000, 6324, -1)]
        [TestCase(5000, 8000, 6325, -1)]
        [TestCase(5000, 8000, 6326, 454)]
        [TestCase(5000, 8000, 6327, -1)]
        [TestCase(5000, 8000, 6328, -1)]
        [TestCase(5000, 8000, 6329, -1)]
        [TestCase(5000, 8000, 5000, 0)]
        [TestCase(5000, 8000, 8000, -1)]
        [TestCase(5000, 8000, 9000, -1)]
        public void GetClosestInt(int startIndex, int endIndex, int targetValue, int expectedClosestValueIndex)
        {
            var rand = new Random(314);
            var values = new List<int>();
            for (var i = startIndex; i <= endIndex; i++)
            {
                values.Add(i);
                i += rand.Next(0, 5);
            }

            var leftIndex = 0;
            var rightIndex = values.Count - 1;

            // If the list does not contain the target value, returns -1
            var valueIndex = MathUtils.BinarySearch(values.ToArray(), targetValue, leftIndex, rightIndex);

            Console.WriteLine("Index of value {0} is {1}", targetValue, valueIndex);
            Console.WriteLine();

            Assert.AreEqual(expectedClosestValueIndex, valueIndex);
        }

        [Test]
        [TestCase(0, 6000, 6.3, 25, 2.59574, 0.999947)]
        [TestCase(500000, 600000, 150, 1.5, -11.0744, 0.997615)]
        [TestCase(500000, 600000, 75, -8, -21.6384, 0.999664)]
        [TestCase(300000, 600000, 50, -8, -70.371, 0.999942)]
        [TestCase(300000, 600000, 2, -8, -1747.24, 0.964736)]
        public void LinearRegression(int startIndex, int endIndex, double addonDivisor, double expectedSlope, double expectedIntercept, double expectedRSquared)
        {
            var xVals = new List<double>();
            var yVals = new List<double>();
            var rand = new Random(314);

            for (var i = startIndex; i <= endIndex; i++)
            {
                xVals.Add(i);
                var addon = (int)Math.Round(i / addonDivisor);
                yVals.Add(i * expectedSlope + rand.Next(-addon, addon));
            }

            MathUtils.GetLinearRegression(xVals.ToArray(), yVals.ToArray(),
                                          out var mathUtilsSlope, out var mathUtilsIntercept, out var mathUtilsRSquared);

            var fitResult = MathNet.Numerics.Fit.Line(xVals.ToArray(), yVals.ToArray());
            var mathNetSlope = fitResult.Item2;
            var mathNetIntercept = fitResult.Item1;

            var mathNetRSquared = MathNet.Numerics.GoodnessOfFit.RSquared(xVals.Select(x => mathNetIntercept + mathNetSlope * x), yVals);

            Console.WriteLine("MathUtils:\nSlope:     {0,8:G6}\nIntercept: {1,8:G6}\nRSquared:  {2,8:G6}",
                              mathUtilsSlope, mathUtilsIntercept, mathUtilsRSquared);
            Console.WriteLine();
            Console.WriteLine("MathNet:\nSlope:     {0,8:G6}\nIntercept: {1,8:G6}\nRSquared:  {2,8:G6}",
                              mathNetSlope, mathNetIntercept, mathNetRSquared);
            Console.WriteLine();

            Assert.AreEqual(expectedSlope, mathUtilsSlope, Math.Abs(expectedSlope / 1000.0));
            Assert.AreEqual(expectedSlope, mathNetSlope, Math.Abs(expectedSlope / 1000.0));
            Assert.AreEqual(expectedIntercept, mathUtilsIntercept, Math.Abs(expectedIntercept / 10000.0));
            Assert.AreEqual(expectedIntercept, mathNetIntercept, Math.Abs(expectedIntercept / 10000.0));
            Assert.AreEqual(expectedRSquared, mathUtilsRSquared, Math.Abs(expectedRSquared / 10000.0));
            Assert.AreEqual(expectedRSquared, mathNetRSquared, Math.Abs(expectedRSquared / 10000.0));
        }

        private List<double> GetLargeValuesList(double startValue, int dataCount)
        {
            var rand = new Random(314);
            var currentValue = startValue;

            var values = new List<double>();
            for (var i = 0; i < dataCount; i++)
            {
                var nextValue = currentValue + currentValue * 0.05 * rand.NextDouble();
                if (nextValue >= double.MaxValue)
                    break;

                values.Add(nextValue);
                currentValue = nextValue;
            }

            return values;
        }
    }
}
