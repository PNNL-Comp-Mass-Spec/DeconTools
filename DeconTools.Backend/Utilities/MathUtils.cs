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

            double slope = (y2 - y1) / (x2 - x1);
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


        public static int GetClosest(double[] data, double targetVal, double tolerance = 0.1)
        {
            int binarySearchIndex = BinarySearchWithTolerance(data, targetVal, 0, data.Length - 1, tolerance);
            if (binarySearchIndex == -1) binarySearchIndex = 0;

            bool indexIsBelowTarget = (data[binarySearchIndex] < targetVal);

            int indexOfClosest = -1;


            if (indexIsBelowTarget)
            {
                double diff = double.MaxValue;
                for (int i = binarySearchIndex; i < data.Length; i++)
                {
                    double currentDiff = Math.Abs(data[i] - targetVal);
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
                double diff = double.MaxValue;
                for (int i = binarySearchIndex; i >= 0; i--)
                {
                    double currentDiff = Math.Abs(data[i] - targetVal);
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


        public static int BinarySearchWithTolerance(double[] data, double targetVal, int leftIndex, int rightIndex, double tolerance)
        {
            if (leftIndex <= rightIndex)
            {
                int middle = (leftIndex + rightIndex) / 2;
                if (Math.Abs(targetVal - data[middle]) <= tolerance)
                {
                    return middle;
                }
                else if (targetVal < data[middle])
                {
                    return BinarySearchWithTolerance(data, targetVal, leftIndex, middle - 1, tolerance);
                }
                else
                {
                    return BinarySearchWithTolerance(data, targetVal, middle + 1, rightIndex, tolerance);
                }
            }
            return -1;


        }


        public static int BinarySearch(int[] data, int targetVal, int leftIndex, int rightIndex)
        {
            if (leftIndex <= rightIndex)
            {
                int middle = (leftIndex + rightIndex) / 2;
                if (targetVal== data[middle] )
                {
                    return middle;
                }
                else if (targetVal < data[middle])
                {
                    return BinarySearch(data, targetVal, leftIndex, middle - 1);
                }
                else
                {
                    return BinarySearch(data, targetVal, middle + 1, rightIndex);
                }
            }
            return -1;


        }



    }
}
