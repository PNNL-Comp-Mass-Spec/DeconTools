using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Utilities;

// ReSharper disable UnusedMember.Global

namespace DeconTools.Backend
{
    /// <summary>
    /// Class for tracking parallel arrays of data
    /// </summary>
    /// <remarks>Although the arrays will typically have the same number of data points, this is not a requirement</remarks>
    public class XYData
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public XYData()
        {
            Xvalues = new double[1];
            Yvalues = new double[1];
        }

        /// <summary>
        /// Array of x values
        /// </summary>
        public double[] Xvalues { get; set; }

        /// <summary>
        /// Array of y values
        /// </summary>
        public double[] Yvalues { get; set; }

        /// <summary>
        /// Convert the data in properties Xvalues and Yvalues to arrays of floats
        /// </summary>
        /// <param name="xVals"></param>
        /// <param name="yVals"></param>
        /// <remarks>Going from double to single variables could result in a loss of information</remarks>
        public void GetXYValuesAsSingles(out float[] xVals, out float[] yVals)
        {
            xVals = new float[Xvalues.Length];
            yVals = new float[Yvalues.Length];

            for (var i = 0; i < Xvalues.Length; i++)
            {
                xVals[i] = (float)Xvalues[i];
                yVals[i] = (float)Yvalues[i];
            }
        }

        /// <summary>
        /// Return the data in properties Xvalues and Yvalues as two arrays of doubles
        /// </summary>
        /// <param name="xVals"></param>
        /// <param name="yVals"></param>
        public void GetXYValuesAsDoubles(out double[] xVals, out double[] yVals)
        {
            xVals = Xvalues;
            yVals = Yvalues;
        }

        /// <summary>
        /// Store data in properties Xvalues and Yvalues
        /// </summary>
        /// <param name="xVals"></param>
        /// <param name="yVals"></param>
        public void SetXYValues(float[] xVals, float[] yVals)
        {
            if (xVals == null || yVals == null)
            {
                Xvalues = null;
                Yvalues = null;

                return;
            }

            Xvalues = new double[xVals.Length];
            Yvalues = new double[yVals.Length];
            for (var i = 0; i < xVals.Length; i++)
            {
                Xvalues[i] = xVals[i];
                Yvalues[i] = yVals[i];
            }

        }

        /// <summary>
        /// Store data in properties Xvalues and Yvalues
        /// </summary>
        /// <param name="xVals"></param>
        /// <param name="yVals"></param>
        public void SetXYValues(double[] xVals, double[] yVals)
        {
            Xvalues = xVals;
            Yvalues = yVals;
        }

        /// <summary>
        /// Store data in properties Xvalues and Yvalues
        /// </summary>
        /// <param name="xVals"></param>
        /// <param name="yVals"></param>
        public void SetXYValues(double[] xVals, float[] yVals)
        {
            if (xVals == null || yVals == null)
            {
                Xvalues = null;
                Yvalues = null;
                return;
            }

            Xvalues = xVals;
            Yvalues = new double[yVals.Length];
            for (var i = 0; i < yVals.Length; i++)
            {
                Yvalues[i] = yVals[i];
            }
        }

        /// <summary>
        /// Convert an array of floats to an array of doubles
        /// </summary>
        /// <param name="inputVals"></param>
        /// <returns></returns>
        public static double[] ConvertFloatsToDouble(float[] inputVals)
        {
            if (inputVals == null) return null;
            if (inputVals.Length == 0) return null;
            var outputVals = new double[inputVals.Length];

            for (var i = 0; i < inputVals.Length; i++)
            {
                outputVals[i] = inputVals[i];

            }

            return outputVals;
        }

        /// <summary>
        /// Convert an array of integers to an array of doubles
        /// </summary>
        /// <param name="inputVals"></param>
        /// <returns></returns>
        public static double[] ConvertIntsToDouble(int[] inputVals)
        {
            if (inputVals == null) return null;
            if (inputVals.Length == 0) return null;
            var outputVals = new double[inputVals.Length];

            for (var i = 0; i < inputVals.Length; i++)
            {
                outputVals[i] = inputVals[i];

            }

            return outputVals;
        }

        /// <summary>
        /// Get the x value in property Xvalues that is closest to the target x value
        /// </summary>
        /// <param name="targetXVal"></param>
        /// <returns></returns>
        public int GetClosestXVal(double targetXVal)
        {
            var minDiff = 1e10;

            var indexOfClosest = -1;
            var numWrongDirection = 0;

            for (var i = 0; i < Xvalues.Length; i++)
            {
                var currentDiff = Math.Abs(targetXVal - Xvalues[i]);

                if (currentDiff < minDiff)
                {
                    indexOfClosest = i;
                    minDiff = currentDiff;

                }
                else
                {
                    numWrongDirection++;
                    if (numWrongDirection > 3) break;    //three values in a row that indicate we are moving away from the target val
                }

            }

            return indexOfClosest;
        }

        /// <summary>
        /// Get the maximum Y value
        /// </summary>
        /// <returns>The maximum Y value, or double.MinValue if Xvalues or Yvalues is empty</returns>
        public double GetMaxY()
        {
            var maxY = double.MinValue;
            var yDataCount = Yvalues.Length;

            for (var i = 0; i < Xvalues.Length; i++)
            {
                if (i == yDataCount)
                    break;

                if (Yvalues[i] > maxY)
                {
                    maxY = Yvalues[i];
                }
            }

            return maxY;
        }

        /// <summary>
        /// Get maximum y value within the given x value range
        /// </summary>
        /// <param name="xMin"></param>
        /// <param name="xMax"></param>
        /// <returns>The maximum Y value, or double.MinValue if Xvalues or Yvalues is empty</returns>
        public double GetMaxY(double xMin, double xMax)
        {
            var maxY = double.MinValue;
            var yDataCount = Yvalues.Length;

            for (var i = 0; i < Xvalues.Length; i++)
            {
                if (i == yDataCount)
                    break;

                if (Xvalues[i] >= xMin && Xvalues[i] <= xMax)
                {
                    if (Yvalues[i] > maxY)
                    {
                        maxY = Yvalues[i];
                    }
                }
            }

            return maxY;
        }

        /// <summary>
        /// Convert an array of drawing points to arrays of doubles
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static XYData ConvertDrawingPoints(System.Drawing.PointF[] points)
        {
            var xyData = new XYData
            {
                Xvalues = new double[points.Length],
                Yvalues = new double[points.Length]
            };

            for (var i = 0; i < points.Length; i++)
            {
                xyData.Xvalues[i] = points[i].X;
                xyData.Yvalues[i] = points[i].Y;
            }

            return xyData;
        }

        /// <summary>
        /// Trim the Xvalues and Yvalues properties to only keep the data between xMin and xMax
        /// </summary>
        /// <param name="xMin"></param>
        /// <param name="xMax"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        /// <remarks>Assumes the data in Xvalues is sorted</remarks>
        public XYData TrimData(double xMin, double xMax, double tolerance = 0.1)
        {

            if (Xvalues == null || Yvalues == null || Xvalues.Length == 0 || Yvalues.Length == 0) return this;

            var currentMinXValue = Xvalues[0];
            var currentMaxXValue = Xvalues[Xvalues.Length - 1];

            if (xMin < currentMinXValue && xMax > currentMaxXValue)
            {
                // Nothing to trim
                return this;
            }

            var data = new XYData();
            var indexClosestXValMin = MathUtils.GetClosest(Xvalues, xMin, tolerance);

            var indexClosestXValMax = MathUtils.GetClosest(Xvalues, xMax, tolerance);

            var numPoints = indexClosestXValMax - indexClosestXValMin + 1;

            if (numPoints <= 0)
                throw new InvalidOperationException("indexClosestXValMin > indexClosestXValMax in XYData.TrimData; Xvalues is likely not sorted");

            data.Xvalues = new double[numPoints];
            data.Yvalues = new double[numPoints];

            for (var i = indexClosestXValMin; i <= indexClosestXValMax; i++)
            {
                data.Xvalues[i - indexClosestXValMin] = Xvalues[i];
                data.Yvalues[i - indexClosestXValMin] = Yvalues[i];
            }

            return data;
        }

        /// <summary>
        /// Get data in Xvalues and Yvalues where the y value is positive
        /// </summary>
        /// <returns></returns>
        public XYData GetNonZeroXYData()
        {
            var yDataCount = Yvalues.Length;

            var data = new XYData();
            var tempXVals = new List<double>();
            var tempYVals = new List<double>();

            for (var i = 0; i < Xvalues.Length; i++)
            {
                if (i == yDataCount)
                    break;

                if (Yvalues[i] > 0)
                {
                    tempXVals.Add(Xvalues[i]);
                    tempYVals.Add(Yvalues[i]);
                }

            }

            data.Xvalues = tempXVals.ToArray();
            data.Yvalues = tempYVals.ToArray();

            return data;
        }

        /// <summary>
        /// Normalize the y value to be between 0 and 1
        /// </summary>
        public void NormalizeYData()
        {
            var maxVal = GetMaxY();

            if (Math.Abs(maxVal) < double.Epsilon)
                return;

            for (var i = 0; i < Yvalues.Length; i++)
            {
                Yvalues[i] = Yvalues[i] / maxVal;

            }
        }

        /// <summary>
        /// Shift the x values by the given offset
        /// </summary>
        /// <param name="offset"></param>
        public void OffSetXValues(double offset)
        {
            for (var i = 0; i < Xvalues.Length; i++)
            {
                Xvalues[i] = Xvalues[i] + offset;
            }
        }

        /// <summary>
        /// Filter the data to only keep data with x values between minX and maxX, optionally specifying a startingIndex
        /// If startingIndex is greater than or equal to data.Xvalues.Length, returns null
        /// </summary>
        /// <param name="data"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="startingIndex"></param>
        /// <returns></returns>
        /// <remarks>Assumes the data in Xvalues is sorted</remarks>
        public static XYData GetFilteredXYValues(XYData data, double minX, double maxX, int startingIndex = 0)
        {
            if (data == null || startingIndex >= data.Xvalues.Length)
                return null;

            if (startingIndex < 0)
                startingIndex = 0;

            var yDataCount = data.Yvalues.Length;

            var xVals = new List<double>();
            var yVals = new List<double>();

            for (var i = startingIndex; i < data.Xvalues.Length; i++)
            {
                if (i == yDataCount)
                    break;

                if (data.Xvalues[i] < minX)
                    continue;

                if (data.Xvalues[i] > maxX)
                {
                    break;
                }

                xVals.Add(data.Xvalues[i]);
                yVals.Add(data.Yvalues[i]);
            }

            if (xVals.Count == 0)
                return null;

            var returnedData = new XYData
            {
                Xvalues = xVals.ToArray(),
                Yvalues = yVals.ToArray()
            };

            return returnedData;
        }

        /// <summary>
        /// Show the XY data at the console
        /// </summary>
        public void Display()
        {
            var yDataCount = Yvalues.Length;

            Console.WriteLine("--------- XYData -----------------");
            for (var i = 0; i < Xvalues.Length; i++)
            {
                if (i == yDataCount)
                    break;

                Console.WriteLine("{0}\t{1}", Xvalues[i], Yvalues[i]);
            }
            Console.WriteLine("--------------------------- end ---------------------------------------");
        }

    }
}
