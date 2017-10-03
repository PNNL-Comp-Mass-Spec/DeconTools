using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend
{
    public class XYData
    {

        public XYData()
        {
            xvalues = new double[1];
            yvalues = new double[1];
        }

        private double[] xvalues;

        public double[] Xvalues
        {
            get => xvalues;
            set => xvalues = value;
        }

        private double[] yvalues;

        public double[] Yvalues
        {
            get => yvalues;
            set => yvalues = value;
        }


        public void GetXYValuesAsSingles(out float[] xvals, out float[] yvals)
        {
            //NOTE going from double to single variables could result in a loss of information

            {
                xvals = new float[xvalues.Length];
                yvals = new float[yvalues.Length];

                for (var i = 0; i < xvalues.Length; i++)
                {
                    xvals[i] = (float)xvalues[i];
                    yvals[i] = (float)yvalues[i];
                }
            }
        }

        public void GetXYValuesAsDoubles(out double[] xvals, out double[] yvals)
        {
            xvals = xvalues;
            yvals = yvalues;
        }

        public void SetXYValues(float[] xvals, float[] yvals)
        {
            if (xvals == null || yvals == null)
            {
                Xvalues = null;
                Yvalues = null;

                return;
            }

            xvalues = new double[xvals.Length];
            yvalues = new double[yvals.Length];
            for (var i = 0; i < xvals.Length; i++)
            {
                xvalues[i] = xvals[i];
                yvalues[i] = yvals[i];
            }

        }

        public void SetXYValues(double[] xvals, double[] yvals)
        {
            xvalues = xvals;
            yvalues = yvals;
        }

        public void SetXYValues(double[] xvals, float[] yvals)
        {
            if (xvals == null || yvals == null)
            {
                Xvalues = null;
                Yvalues = null;

                return;
            }


            xvalues = xvals;
            yvalues = new double[yvals.Length];
            for (var i = 0; i < yvals.Length; i++)
            {
                yvalues[i] = yvals[i];
            }
        }

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

        public int GetClosestXVal(double targetXVal)
        {
            var minDiff = 1e10;


            var indexOfClosest = -1;
            var numWrongDirection = 0;

            for (var i = 0; i < xvalues.Length; i++)
            {
                var currentDiff = Math.Abs(targetXVal - xvalues[i]);


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

        public double getMaxY()
        {
            var maxY = double.MinValue;


            for (var i = 0; i < xvalues.Length; i++)
            {
                if (yvalues[i] > maxY)
                {
                    maxY = Yvalues[i];
                }
            }
            return maxY;

        }


        public double getMaxY(double xMin, double xMax)
        {
            var maxY = double.MinValue;

            for (var i = 0; i < xvalues.Length; i++)
            {
                if (xvalues[i] >= xMin && xvalues[i] <= xMax)
                {
                    if (yvalues[i] > maxY)
                    {
                        maxY = Yvalues[i];
                    }

                }

            }
            return maxY;
        }

        public static XYData ConvertDrawingPoints(System.Drawing.PointF[] points)
        {
            var xydata = new XYData
            {
                Xvalues = new double[points.Length],
                Yvalues = new double[points.Length]
            };

            for (var i = 0; i < points.Length; i++)
            {
                xydata.Xvalues[i] = points[i].X;
                xydata.Yvalues[i] = points[i].Y;
            }

            return xydata;
        }

        public XYData TrimData(double xmin, double xmax, double tolerance = 0.1)
        {

            if (xvalues == null || yvalues == null || xvalues.Length == 0 || yvalues.Length == 0) return this;

            var currentMinXValue = xvalues[0];
            var currentMaxXValue = xvalues[xvalues.Length - 1];

            //if it doesn't need trimming, return it.
            if (xmin < currentMinXValue && xmax > currentMaxXValue)
            {
                return this;
            }


            var data = new XYData();
            var indexClosestXValMin = MathUtils.GetClosest(xvalues, xmin, tolerance);

            var indexClosestXValMax = MathUtils.GetClosest(xvalues, xmax, tolerance);

            var numPoints = indexClosestXValMax - indexClosestXValMin + 1;

            if (numPoints <= 0)
                throw new InvalidOperationException("indexClosestXValMin > indexClosestXValMax in XYData.TrimData; xvalues are likely not sorted");

            data.Xvalues = new double[numPoints];
            data.Yvalues = new double[numPoints];

            for (var i = indexClosestXValMin; i <= indexClosestXValMax; i++)
            {

                data.Xvalues[i - indexClosestXValMin] = xvalues[i];
                data.Yvalues[i - indexClosestXValMin] = yvalues[i];

            }

            return data;

        }

        public XYData GetNonZeroXYData()
        {
            var data = new XYData();
            var tempxvals = new List<double>();
            var tempyvals = new List<double>();

            for (var i = 0; i < xvalues.Length; i++)
            {
                if (yvalues[i] > 0)
                {
                    tempxvals.Add(xvalues[i]);
                    tempyvals.Add(yvalues[i]);
                }

            }

            data.Xvalues = tempxvals.ToArray();
            data.Yvalues = tempyvals.ToArray();

            return data;

        }

        public void NormalizeYData()
        {
            var maxVal = getMaxY();

            for (var i = 0; i < Yvalues.Length; i++)
            {
                yvalues[i] = yvalues[i] / maxVal;

            }
        }

        public void OffSetXValues(double offset)
        {
            for (var i = 0; i < xvalues.Length; i++)
            {
                xvalues[i] = xvalues[i] + offset;

            }
        }


        public static XYData GetFilteredXYValues(XYData data, double minX, double maxX, int startingIndex)
        {
            //this assumes XY pairs with ordered x-values.

            if (data == null || startingIndex > data.Xvalues.Length - 1) return null;

            var xvals = new List<double>();
            var yvals = new List<double>();


            for (var i = startingIndex; i < data.Xvalues.Length; i++)
            {
                if (data.Xvalues[i] >= minX)
                {
                    if (data.Xvalues[i] <= maxX)
                    {
                        xvals.Add(data.Xvalues[i]);
                        yvals.Add(data.Yvalues[i]);

                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (xvals.Count == 0) return null;

            var returnedData = new XYData
            {
                Xvalues = xvals.ToArray(),
                Yvalues = yvals.ToArray()
            };

            return returnedData;

        }

        public static XYData GetFilteredXYValues(XYData data, double minX, double maxX)
        {
            return GetFilteredXYValues(data, minX, maxX, 0);
        }

        public void Display()
        {
            var sb = new StringBuilder();
            sb.Append("--------- XYData -----------------\n");
            for (var i = 0; i < Xvalues.Length; i++)
            {
                sb.Append(Xvalues[i]);
                sb.Append("\t");
                sb.Append(Yvalues[i]);
                sb.Append("\n");
            }
            sb.Append("--------------------------- end ---------------------------------------\n");

            Console.Write(sb.ToString());
        }


    }
}
