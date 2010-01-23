using System;
using System.Collections.Generic;
using System.Text;

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
            DeconTools.Utilities.Check.Require(xvals != null && yvals != null, "Cannot set XY Values; Input values are null");

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
            DeconTools.Utilities.Check.Require(xvals != null && yvals != null, "Cannot set XY Values; Input values are null");

            this.xvalues = xvals;
            this.yvalues = yvals;
            

        }

        internal static double[] ConvertFloatsToDouble(float[] inputVals)
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

        internal static double[] ConvertIntsToDouble(int[] inputVals)
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
                double currentDiff=Math.Abs(targetXVal-this.xvalues[i]);
                

                if (currentDiff< minDiff )
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

        public XYData TrimData(double xmin, double xmax)
        {
            XYData data = new XYData();
            List<double> xvals = new List<double>();
            List<double> yvals = new List<double>();

            for (int i = 0; i < xvalues.Length; i++)
            {
                if (xvalues[i] >= xmin && xvalues[i] <= xmax)
                {
                    xvals.Add(xvalues[i]);
                    yvals.Add(yvalues[i]);
                }
                
            }

            data.Xvalues = xvals.ToArray();
            data.Yvalues = yvals.ToArray();

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
    }
}
