using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Utilities
{
    public class MathUtils 
    {

        public static double getInterpolatedValue(double x1, double x2, double y1, double y2, double targetXvalue)
        {

            if (x1 == x2) return y1;

            double slope = (y2 - y1) / (x2-x1);
            double yintercept = y1 - (slope * x1);
            double interpolatedVal = targetXvalue * slope + yintercept;
            return interpolatedVal;
        }

        public static double GetAverage(List<double> values)
        {
            return GetAverage(values.ToArray());
        }

        public static double GetAverage(double[] values)
        {
            if (values == null || values.Length == 0) return double.NaN;
            double sum = 0;
            foreach (double val in values)
            {
                sum += val;
            }
            return (sum / values.Length);

        }

        public static double GetStDev(List<double> values)
        {
            return GetStDev(values.ToArray());
        }

        public static double GetStDev(double[] values)
        {
            if (values.Length < 3)
            {
                return double.NaN;
            }

            double average = GetAverage(values);

            double sum = 0;

            foreach (double val in values)
            {
                sum += ((average - val) * (average - val));
            }

            return System.Math.Sqrt((sum / (values.GetLength(0) - 1)));

        }


    }
}
