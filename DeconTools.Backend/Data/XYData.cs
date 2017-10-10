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
            Xvalues = new double[1];
            Yvalues = new double[1];
        }

        public double[] Xvalues { get; set; }

        public double[] Yvalues { get; set; }


        public void GetXYValuesAsSingles(out float[] xvals, out float[] yvals)
        {
            //NOTE going from double to single variables could result in a loss of information

            {
                xvals = new float[Xvalues.Length];
                yvals = new float[Yvalues.Length];

                for (var i = 0; i < Xvalues.Length; i++)
                {
                    xvals[i] = (float)Xvalues[i];
                    yvals[i] = (float)Yvalues[i];
                }
            }
        }

        public void GetXYValuesAsDoubles(out double[] xvals, out double[] yvals)
        {
            xvals = Xvalues;
            yvals = Yvalues;
        }

        public void SetXYValues(float[] xvals, float[] yvals)
        {
            if (xvals == null || yvals == null)
            {
                Xvalues = null;
                Yvalues = null;

                return;
            }

            Xvalues = new double[xvals.Length];
            Yvalues = new double[yvals.Length];
            for (var i = 0; i < xvals.Length; i++)
            {
                Xvalues[i] = xvals[i];
                Yvalues[i] = yvals[i];
            }

        }

        public void SetXYValues(double[] xvals, double[] yvals)
        {
            Xvalues = xvals;
            Yvalues = yvals;
        }

        public void SetXYValues(double[] xvals, float[] yvals)
        {
            if (xvals == null || yvals == null)
            {
                Xvalues = null;
                Yvalues = null;

                return;
            }


            Xvalues = xvals;
            Yvalues = new double[yvals.Length];
            for (var i = 0; i < yvals.Length; i++)
            {
                Yvalues[i] = yvals[i];
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

        public double getMaxY()
        {
            var maxY = double.MinValue;


            for (var i = 0; i < Xvalues.Length; i++)
            {
                if (Yvalues[i] > maxY)
                {
                    maxY = Yvalues[i];
                }
            }
            return maxY;

        }


        public double getMaxY(double xMin, double xMax)
        {
            var maxY = double.MinValue;

            for (var i = 0; i < Xvalues.Length; i++)
            {
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

            if (Xvalues == null || Yvalues == null || Xvalues.Length == 0 || Yvalues.Length == 0) return this;

            var currentMinXValue = Xvalues[0];
            var currentMaxXValue = Xvalues[Xvalues.Length - 1];

            //if it doesn't need trimming, return it.
            if (xmin < currentMinXValue && xmax > currentMaxXValue)
            {
                return this;
            }


            var data = new XYData();
            var indexClosestXValMin = MathUtils.GetClosest(Xvalues, xmin, tolerance);

            var indexClosestXValMax = MathUtils.GetClosest(Xvalues, xmax, tolerance);

            var numPoints = indexClosestXValMax - indexClosestXValMin + 1;

            if (numPoints <= 0)
                throw new InvalidOperationException("indexClosestXValMin > indexClosestXValMax in XYData.TrimData; xvalues are likely not sorted");

            data.Xvalues = new double[numPoints];
            data.Yvalues = new double[numPoints];

            for (var i = indexClosestXValMin; i <= indexClosestXValMax; i++)
            {

                data.Xvalues[i - indexClosestXValMin] = Xvalues[i];
                data.Yvalues[i - indexClosestXValMin] = Yvalues[i];

            }

            return data;

        }

        public XYData GetNonZeroXYData()
        {
            var data = new XYData();
            var tempxvals = new List<double>();
            var tempyvals = new List<double>();

            for (var i = 0; i < Xvalues.Length; i++)
            {
                if (Yvalues[i] > 0)
                {
                    tempxvals.Add(Xvalues[i]);
                    tempyvals.Add(Yvalues[i]);
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
                Yvalues[i] = Yvalues[i] / maxVal;

            }
        }

        public void OffSetXValues(double offset)
        {
            for (var i = 0; i < Xvalues.Length; i++)
            {
                Xvalues[i] = Xvalues[i] + offset;

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
