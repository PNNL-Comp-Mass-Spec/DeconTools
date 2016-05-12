using System;
using System.Collections.Generic;
using System.Linq;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ChargeDetermination
{
    public class Interpolation
    {
        // List to store the second derivatives at the knot points of the spline.
        private readonly List<double> _derivativesY2 = new List<double>();

        /// <summary>
        ///     Cubic Spline interpolation. This function does the actual interpolation at specified point, using provided second
        ///     derivatives at the knot points.
        /// </summary>
        /// <param name="xa">List of x values.</param>
        /// <param name="ya">List of y values.</param>
        /// <param name="x">is the value we want to find the interpolating y value at.</param>
        /// <returns>returns interpolated y at point x.</returns>
        public double Splint(List<double> xa, List<double> ya, double x)
        {
            var n = xa.Count;
            var klo = 0;
            var khi = n - 1;

            // binary search for khi, klo where xa[klo] <= x < xa[khi]
            while (khi - klo > 1)
            {
                var k = (khi + klo) >> 1;
                if (xa[k] > x)
                    khi = k;
                else
                    klo = k;
            }
            var h = xa[khi] - xa[klo];
            if (h.Equals(0))
                return -1;
            var a = (xa[khi] - x) / h;
            var b = (x - xa[klo]) / h;
            // cubic interpolation at x.
            var yaKlo = ya[klo];
            var yaKhi = ya[khi];
            var y2Klo = _derivativesY2[klo];
            var y2Khi = _derivativesY2[khi];
            var y = a * yaKlo + b * yaKhi + ((a * a * a - a) * y2Klo + (b * b * b - b) * y2Khi) * (h * h) / 6.0;

            return y;
        }

        /// <summary>
        ///     Cubic Spline interpolation. This function does the actual interpolation at specified point, using provided second
        ///     derivatives at the knot points.
        /// </summary>
        /// <param name="xa">List of x values.</param>
        /// <param name="ya">List of y values.</param>
        /// <param name="x">List of x values that we want to find the interpolating y value at.</param>
        /// <param name="y">List of interpolated y at each point of x.</param>
        public void Splint(List<double> xa, List<double> ya, List<double> x, out List<double> y)
        {
            var numX = x.Count;
            var n = xa.Count;
            y = new List<double>();

            if (numX == 0)
            {
                y.InsertRange(0, Enumerable.Repeat(0d, n));
                return;
            }

            var minXa = xa[0];
            var maxXa = xa[n - 1];

            for (var i = 0; i < numX; i++)
            {
                double y1;
                var x1 = x[i];

                if (x1 < minXa || x1 > maxXa)
                    y1 = 0;
                else
                {
                    var klo = 0;
                    var khi = n - 1;

                    // binary search for khi, klo where xa[klo] <= x < xa[khi]
                    while (khi - klo > 1)
                    {
                        var k = (khi + klo) >> 1;
                        if (xa[k] > x1)
                            khi = k;
                        else
                            klo = k;
                    }
                    var h = xa[khi] - xa[klo];
                    if (h.Equals(0))
                    {
                        break;
                    }

                    var a = (xa[khi] - x1) / h;
                    var b = (x1 - xa[klo]) / h;
                    // cubic interpolation at x.
                    var yaKlo = ya[klo];
                    var yaKhi = ya[khi];
                    var y2Klo = _derivativesY2[klo];
                    var y2Khi = _derivativesY2[khi];
                    y1 = a * yaKlo + b * yaKhi + ((a * a * a - a) * y2Klo + (b * b * b - b) * y2Khi) * (h * h) / 6.0;
                }
                y.Add(y1);
            }
        }

        /// <summary>
        ///     Cubic Spline interpolation. This function generates the second derivatives at the knot points.
        /// </summary>
        /// <param name="x">List of x values.</param>
        /// <param name="y">List of y values.</param>
        /// <param name="yp1">second derivative at first point.</param>
        /// <param name="ypn">second derivative at the nth point.</param>
        /// <remarks>
        ///     These algorithms are from: Numerical Recipes in C by William H. Press, Brian P. Flannery, Saul A. Teukolsky,
        ///     William T. Vetterling.
        /// </remarks>
        /// <remarks>
        ///     Given the arrays x[0..n-1] and y[0..n-1] containing the tabulated function, i.e., yi = f(xi),
        ///     with x0&lt;x1&lt;...&lt;xn-1, and given values yp1 and ypn for the first derivative of the
        ///     interpolating function at points 0 and n-1, respectively, this routine returns an array y2[1..n]
        ///     that contains the second derivatives of the interpolating function at the tabulated points xi.
        ///     If yp1 and/or ypn are equal to 1x10^30 or larger, the routine is signaled to set the corresponding
        ///     boundary condition for a natural spline, with zero second derivative on that boundary.
        /// </remarks>
        public void Spline(List<double> x, List<double> y, double yp1, double ypn)
        {
            // Unescaped version of above remark...
            // Given the arrays x[0..n-1] and y[0..n-1] containing the tabulated function, i.e., yi = f(xi), 
            // with x0<x1<...<xn-1, and given values yp1 and ypn for the first derivative of the interpolating
            // function at points 0 and n-1, respectively, this routine returns an array y2[1..n] that contains
            // the second derivatives of the interpolating function at the tabulated points xi. If yp1 and/or
            // ypn are equal to 1x10^30 or larger, the routine is signaled to set the corresponding boundary
            // condition for a natural spline, with zero second derivative on that boundary.
            var n = x.Count;
            if (n <= 0)
            {
                return;
            }

            if (_derivativesY2.Count > 0)
                _derivativesY2.Clear();

            try
            {
                // Temporary variable used in computation of spline coefficients.
                var splineCoefficients = new List<double>();
                _derivativesY2.Capacity = n;
                _derivativesY2.AddRange(Enumerable.Repeat(0d, n));
                if (yp1 > 0.99e30)
                {
                    _derivativesY2[0] = 0.0;
                    splineCoefficients.Add(0);
                }
                else
                {
                    _derivativesY2[0] = -0.5;
                    splineCoefficients.Add(3.0f / (x[1] - x[0]) * ((y[1] - y[0]) / (x[1] - x[0]) - yp1));
                }
                // generate second derivatives at internal points using recursive spline equations.
                for (var i = 1; i <= n - 2; i++)
                {
                    var sig = (x[i] - x[i - 1]) / (x[i + 1] - x[i - 1]);
                    var p = sig * _derivativesY2[i - 1] + 2.0;
                    _derivativesY2[i] = (sig - 1.0) / p;
                    var lastSplineVal = splineCoefficients[i - 1];
                    var splineVal = (y[i + 1] - y[i]) / (x[i + 1] - x[i]) - (y[i] - y[i - 1]) / (x[i] - x[i - 1]);
                    splineVal = (6.0 * splineVal / (x[i + 1] - x[i - 1]) - sig * lastSplineVal) / p;
                    splineCoefficients.Add(splineVal);
                }
                double qn;
                double un;
                if (ypn > 0.99e30)
                    qn = un = 0.0;
                else
                {
                    qn = 0.5;
                    un = 3.0 / (x[n - 1] - x[n - 2]) * (ypn - (y[n - 1] - y[n - 2]) / (x[n - 1] - x[n - 2]));
                }

                var y2LastLast = _derivativesY2[n - 2];
                var tempLastLast = splineCoefficients[n - 2];
                var y2Last = (un - qn * tempLastLast) / (qn * y2LastLast + 1.0);
                _derivativesY2[n - 1] = y2Last;
                for (var k = n - 2; k >= 0; k--) //[gord] this loop takes forever when summing spectra
                {
                    var temp = splineCoefficients[k];
                    var y2NextVal = _derivativesY2[k + 1];
                    var y2Val = _derivativesY2[k];
                    _derivativesY2[k] = y2Val * y2NextVal + temp;
                }
            }
            catch (NullReferenceException e)
            {
#if DEBUG
                throw e;
#endif
            }
        }

        /// <summary>
        ///     Zero filling imputation. This function takes in data which has missing values and adds in zero values
        /// </summary>
        /// <param name="x">List of x values.</param>
        /// <param name="y">List of y values.</param>
        /// <param name="maxPtsToAddForZero">Maximum number of point to add for zero.</param>
        /// <returns>number of points added.</returns>
        public static int ZeroFillMissing(ref List<float> x, ref List<float> y, int maxPtsToAddForZero)
        {
            // TODO: Somehow combine this and the other ZeroFillMissing (float vs. double)?
            var tempX = new List<float>();
            var tempY = new List<float>();
            if (x.Count != y.Count)
            {
                throw new Exception("x and y need to be of the same size in ZeroFillMissing");
            }

            var numPts = x.Count;
            if (numPts <= 1)
            {
                //throw new System.Exception("x is empty in ZeroFillMissing");
                return 0;
            }
            tempX.Capacity = numPts * 2;
            tempY.Capacity = numPts * 2;

            var minDistance = x[1] - x[0];
            for (var index = 2; index < numPts - 1; index++)
            {
                var currentDiff = x[index] - x[index - 1];
                if (minDistance > currentDiff && currentDiff > 0)
                {
                    minDistance = currentDiff;
                }
            }

            tempX.Add(x[0]);
            tempY.Add(y[0]);
            var lastDiff = minDistance;
            var lastDiffIndex = 0;
            var lastX = x[0];
            for (var index = 1; index < numPts - 1; index++)
            {
                var currentDiff = x[index] - lastX;
                double diffBetweenCurrentAndBase = x[index] - x[lastDiffIndex];
                var differenceFactor = 1.5;

                if (Math.Sqrt(diffBetweenCurrentAndBase) > differenceFactor)
                {
                    differenceFactor = Math.Sqrt(diffBetweenCurrentAndBase);
                }

                if (currentDiff > differenceFactor * lastDiff)
                {
                    // insert points.
                    var numPtsToAdd = (int) (currentDiff / lastDiff + 0.5) - 1;
                    if (numPtsToAdd > 2 * maxPtsToAddForZero)
                    {
                        for (var ptNum = 0; ptNum < maxPtsToAddForZero; ptNum++)
                        {
                            if (lastX >= x[index])
                                break;
                            lastX += lastDiff;
                            tempX.Add(lastX);
                            tempY.Add(0);
                        }
                        var nextLastX = x[index] - maxPtsToAddForZero * lastDiff;
                        if (nextLastX > lastX + lastDiff)
                        {
                            lastX = nextLastX;
                        }
                        for (var ptNum = 0; ptNum < maxPtsToAddForZero; ptNum++)
                        {
                            lastX += lastDiff;
                            if (lastX >= x[index])
                                break;
                            tempX.Add(lastX);
                            tempY.Add(0);
                        }
                    }
                    else
                    {
                        for (var ptNum = 0; ptNum < numPtsToAdd; ptNum++)
                        {
                            lastX += lastDiff;
                            if (lastX >= x[index])
                                break;
                            tempX.Add(lastX);
                            tempY.Add(0);
                        }
                    }
                    tempX.Add(x[index]);
                    tempY.Add(y[index]);
                    lastX = x[index];
                }
                else
                {
                    tempX.Add(x[index]);
                    tempY.Add(y[index]);
                    lastDiff = currentDiff;
                    lastDiffIndex = index;
                    lastX = x[index];
                }
            }
            var numPtsAdded = tempX.Count - x.Count;

            x.Clear();
            y.Clear();
            x.AddRange(tempX);
            y.AddRange(tempY);
            return numPtsAdded;
        }

        /// <summary>
        ///     Zero filling imputation. This function takes in data which has missing values and adds in zero values
        /// </summary>
        /// <param name="x">List of x values.</param>
        /// <param name="y">List of y values.</param>
        /// <param name="maxPtsToAddForZero">Maximum number of point to add for zero.</param>
        /// <returns>number of points added.</returns>
        public static int ZeroFillMissing(ref List<double> x, ref List<double> y, int maxPtsToAddForZero)
        {
            var tempX = new List<double>();
            var tempY = new List<double>();
            if (x.Count != y.Count)
            {
                //throw new System.Exception("x and y need to be of the same size in ZeroFillMissing");
                return 0;
            }

            var numPts = x.Count;
            if (numPts <= 1)
            {
                //throw new System.Exception("x size is 1 or less in ZeroFillMissing Overloaded");
                return 0;
            }

            try
            {
                tempX.Capacity = numPts * 2;
                tempY.Capacity = numPts * 2;

                var minDistance = x[1] - x[0];
                for (var index = 2; index < numPts - 1; index++)
                {
                    var currentDiff = (float) (x[index] - x[index - 1]);
                    if (minDistance > currentDiff && currentDiff > 0)
                    {
                        minDistance = currentDiff;
                    }
                }

                tempX.Add(x[0]);
                tempY.Add(y[0]);
                var lastDiff = minDistance;
                var lastDiffIndex = 0;
                var lastX = x[0];

                for (var index = 1; index < numPts - 1; index++)
                {
                    var currentDiff = x[index] - lastX;
                    var diffBetweenCurrentAndBase = x[index] - x[lastDiffIndex];
                    var differenceFactor = 1.5;

                    if (Math.Sqrt(diffBetweenCurrentAndBase) > differenceFactor)
                    {
                        differenceFactor = Math.Sqrt(diffBetweenCurrentAndBase);
                    }

                    if (currentDiff > differenceFactor * lastDiff)
                    {
                        // insert points.
                        var numPtsToAdd = (int) (currentDiff / lastDiff + 0.5) - 1;
                        if (numPtsToAdd > 2 * maxPtsToAddForZero)
                        {
                            for (var ptNum = 0; ptNum < maxPtsToAddForZero; ptNum++)
                            {
                                if (lastX >= x[index])
                                    break;
                                lastX += lastDiff;
                                tempX.Add(lastX);
                                tempY.Add(0);
                            }
                            var nextLastX = x[index] - maxPtsToAddForZero * lastDiff;
                            if (nextLastX > lastX + lastDiff)
                            {
                                lastX = nextLastX;
                            }

                            for (var ptNum = 0; ptNum < maxPtsToAddForZero; ptNum++)
                            {
                                if (lastX >= x[index])
                                    break;
                                tempX.Add(lastX);
                                tempY.Add(0);
                                lastX += lastDiff;
                            }
                        }
                        else
                        {
                            for (var ptNum = 0; ptNum < numPtsToAdd; ptNum++)
                            {
                                lastX += lastDiff;
                                if (lastX >= x[index])
                                    break;
                                tempX.Add(lastX);
                                tempY.Add(0);
                            }
                        }
                        tempX.Add(x[index]);
                        tempY.Add(y[index]);
                        lastX = x[index];
                    }
                    else
                    {
                        tempX.Add(x[index]);
                        tempY.Add(y[index]);
                        lastDiff = currentDiff;
                        lastDiffIndex = index;
                        lastX = x[index];
                    }
                }
                var numPtsAdded = tempX.Count - x.Count;

                x.Clear();
                y.Clear();
                x.AddRange(tempX);
                y.AddRange(tempY);
                return numPtsAdded;
            }
            catch (NullReferenceException e)
            {
#if DEBUG
                throw e;
#endif
                return 0;
            }
        }
    }
}