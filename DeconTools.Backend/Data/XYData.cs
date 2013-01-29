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
            this.xvalues = new double[1];
            this.yvalues = new double[1];
        }

        private double[] xvalues;

        public double[] Xvalues
        {
            get { return xvalues; }
            set { xvalues = value; }
        }

        private double[] yvalues;

        public double[] Yvalues
        {
            get { return yvalues; }
            set { yvalues = value; }
        }




        public void GetXYValuesAsSingles(ref float[] xvals, ref float[] yvals)
        {
            //NOTE going from double to single variables could result in a loss of information

            {
                xvals = new float[this.xvalues.Length];
                yvals = new float[this.yvalues.Length];

                for (int i = 0; i < this.xvalues.Length; i++)
                {
                    xvals[i] = (float)this.xvalues[i];
                    yvals[i] = (float)this.yvalues[i];
                }
            }
        }

        public void GetXYValuesAsDoubles(ref double[] xvals, ref double[] yvals)
        {
            xvals = this.xvalues;
            yvals = this.yvalues;
        }

        public void SetXYValues(ref float[] xvals, ref float[] yvals)
        {
            if (xvals == null || yvals == null)
            {
                this.Xvalues = null;
                this.Yvalues = null;

                return;
            }



            this.xvalues = new double[xvals.Length];
            this.yvalues = new double[yvals.Length];
            for (int i = 0; i < xvals.Length; i++)
            {
                this.xvalues[i] = xvals[i];
                this.yvalues[i] = yvals[i];
            }

        }

        public void SetXYValues(ref double[] xvals, ref double[] yvals)
        {
            this.xvalues = xvals;
            this.yvalues = yvals;
        }

        public void SetXYValues(double[] xvals, float[] yvals)
        {
            if (xvals == null || yvals == null)
            {
                this.Xvalues = null;
                this.Yvalues = null;

                return;
            }


            this.xvalues = xvals;
            this.yvalues = new double[yvals.Length];
            for (int i = 0; i < yvals.Length; i++)
            {
                this.yvalues[i] = yvals[i];
            }
        }

        public static double[] ConvertFloatsToDouble(float[] inputVals)
        {
            if (inputVals == null) return null;
            if (inputVals.Length == 0) return null;
            double[] outputVals = new double[inputVals.Length];

            for (int i = 0; i < inputVals.Length; i++)
            {
                outputVals[i] = (double)inputVals[i];

            }
            return outputVals;
        }

        public static double[] ConvertIntsToDouble(int[] inputVals)
        {
            if (inputVals == null) return null;
            if (inputVals.Length == 0) return null;
            double[] outputVals = new double[inputVals.Length];

            for (int i = 0; i < inputVals.Length; i++)
            {
                outputVals[i] = (double)inputVals[i];

            }
            return outputVals;
        }

        public int GetClosestXVal(double targetXVal)
        {
            double minDiff = 1e10;


            int indexOfClosest = -1;
            int numWrongDirection = 0;

            for (int i = 0; i < this.xvalues.Length; i++)
            {
                double currentDiff = Math.Abs(targetXVal - this.xvalues[i]);


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
            double maxY = double.MinValue;


            for (int i = 0; i < this.xvalues.Length; i++)
            {
                if (this.yvalues[i] > maxY)
                {
                    maxY = this.Yvalues[i];
                }
            }
            return maxY;

        }


        public double getMaxY(double xMin, double xMax)
        {
            double maxY = double.MinValue;

            for (int i = 0; i < this.xvalues.Length; i++)
            {
                if (this.xvalues[i] >= xMin && this.xvalues[i] <= xMax)
                {
                    if (this.yvalues[i] > maxY)
                    {
                        maxY = this.Yvalues[i];
                    }

                }

            }
            return maxY;
        }

        public static XYData ConvertDrawingPoints(System.Drawing.PointF[] points)
        {
            XYData xydata = new XYData();
            xydata.Xvalues = new double[points.Length];
            xydata.Yvalues = new double[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                xydata.Xvalues[i] = (double)points[i].X;
                xydata.Yvalues[i] = (double)points[i].Y;
            }

            return xydata;
        }

        public XYData TrimData(double xmin, double xmax, double tolerance = 0.1)
        {

		
			if (xvalues == null || yvalues == null || xvalues.Length == 0 || yvalues.Length == 0) return this;

			double currentMinXValue = xvalues[0];
			double currentMaxXValue = xvalues[xvalues.Length - 1];

			//if it doesn't need trimming, return it.
			if (xmin < currentMinXValue && xmax > currentMaxXValue)
			{
				return this;
			}


			XYData data = new XYData();
			int indexClosestXValMin = MathUtils.GetClosest(xvalues, xmin, tolerance);

			int indexClosestXValMax = MathUtils.GetClosest(xvalues, xmax, tolerance);

			int numPoints = indexClosestXValMax - indexClosestXValMin + 1;

			if (numPoints <= 0)
				throw new InvalidOperationException("indexClosestXValMin > indexClosestXValMax in XYData.TrimData; xvalues are likely not sorted");

			data.Xvalues = new double[numPoints];
			data.Yvalues = new double[numPoints];

			for (int i = indexClosestXValMin; i <= indexClosestXValMax; i++)
			{

				data.Xvalues[i - indexClosestXValMin] = xvalues[i];
				data.Yvalues[i - indexClosestXValMin] = yvalues[i];

			}

			return data;

        }

        public XYData GetNonZeroXYData()
        {
            XYData data = new XYData();
            List<double> tempxvals = new List<double>();
            List<double> tempyvals = new List<double>();

            for (int i = 0; i < xvalues.Length; i++)
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
            double maxVal = this.getMaxY();

            for (int i = 0; i < this.Yvalues.Length; i++)
            {
                yvalues[i] = yvalues[i] / maxVal;

            }
        }

        public void OffSetXValues(double offset)
        {
            for (int i = 0; i < this.xvalues.Length; i++)
            {
                this.xvalues[i] = this.xvalues[i] + offset;

            }
        }


        public static XYData GetFilteredXYValues(XYData data, double minX, double maxX, int startingIndex)
        {
            //this assumes XY pairs with ordered x-values. 

            if (data == null || startingIndex > data.Xvalues.Length - 1) return null;

            List<double> xvals = new List<double>();
            List<double> yvals = new List<double>();


            for (int i = startingIndex; i < data.Xvalues.Length; i++)
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
                else
                {

                }

            }

            if (xvals.Count == 0) return null;

            XYData returnedData = new XYData();
            returnedData.Xvalues = xvals.ToArray();
            returnedData.Yvalues = yvals.ToArray();

            return returnedData;

        }

        public static XYData GetFilteredXYValues(XYData data, double minX, double maxX)
        {
            return GetFilteredXYValues(data, minX, maxX, 0);
        }

        public void Display()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("--------- XYData -----------------\n");
            for (int i = 0; i < this.Xvalues.Length; i++)
            {
                sb.Append(this.Xvalues[i]);
                sb.Append("\t");
                sb.Append(this.Yvalues[i]);
                sb.Append("\n");
            }
            sb.Append("--------------------------- end ---------------------------------------\n");

            Console.Write(sb.ToString());
        }


    }
}
