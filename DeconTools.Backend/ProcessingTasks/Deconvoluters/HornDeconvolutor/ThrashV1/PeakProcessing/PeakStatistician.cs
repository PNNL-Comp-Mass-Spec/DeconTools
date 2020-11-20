using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing
{
    /// <summary>
    ///     class used to compute FWHM and signal to noise for peaks.
    /// </summary>
    internal class PeakStatistician
    {
        /// <summary>
        ///     Find signal to noise value at position specified.
        /// </summary>
        /// <param name="yValue">is intensity at specified index.</param>
        /// <param name="intensities">is List of intensities.</param>
        /// <param name="index">is position of point at which we want to calculate signal to noise.</param>
        /// <returns>returns computed signal to noise value.</returns>
        /// <remarks>
        ///     Looks for local minima on the left and the right hand sides and calculates signal to noise of peak relative to
        ///     the minimum of these shoulders.
        /// </remarks>
        public double FindSignalToNoise(double yValue, List<double> intensities, int index)
        // The place in arrDerivative the derivative crossed 0
        {
            double minIntensityLeft = 0, minIntensityRight = 0;
            if (yValue.Equals(0))
                return 0;

            if (index <= 0 || index >= intensities.Count - 1)
                return 0;

            // Find the first local minimum as we go down the m/z range.
            var found = false;
            for (var i = index; i > 0; i--)
            {
                if (intensities[i + 1] >= intensities[i] && intensities[i - 1] > intensities[i])
                // Local minima here \/
                {
                    minIntensityLeft = intensities[i];
                    found = true;
                    break;
                }
            }
            if (!found)
                minIntensityLeft = intensities[0];

            found = false;
            //// Find the first local minimum as we go up the m/z range.
            for (var i = index; i < intensities.Count - 1; i++)
            {
                if (intensities[i + 1] >= intensities[i] && intensities[i - 1] > intensities[i])
                // Local minima here \/
                {
                    minIntensityRight = intensities[i];
                    found = true;
                    break;
                }
            }
            if (!found)
                minIntensityRight = intensities[intensities.Count - 1];
            if (minIntensityLeft.Equals(0))
            {
                if (minIntensityRight.Equals(0))
                    return 100;
                return 1.0 * yValue / minIntensityRight;
            }
            if (minIntensityRight < minIntensityLeft && !minIntensityRight.Equals(0))
                return 1.0 * yValue / minIntensityRight;

            return 1.0 * yValue / minIntensityLeft;
        }

        /// <summary>
        ///     Find full width at half maximum value at position specified.
        /// </summary>
        /// <param name="mzs">is List of mzs.</param>
        /// <param name="intensities">is List of intensities.</param>
        /// <param name="dataIndex">is position of point at which we want to calculate FWHM.</param>
        /// <param name="signalToNoise">is option parameter that specifies minimum signal to noise ratio to use.</param>
        /// <returns>returns computed FWHM.</returns>
        /// <remarks>
        ///     Looks for half height locations at left and right side, and uses twice of that value as the FWHM value. If half
        ///     height
        ///     locations cannot be found (because of say an overlapping neighboring peak), we perform interpolations.
        /// </remarks>
        public double FindFwhm(List<double> mzs, List<double> intensities, int dataIndex,
            double signalToNoise = 0.0)
        {
            if (intensities[dataIndex].Equals(0))
                return 0.0;

            var peakHalf = intensities[dataIndex] / 2.0;
            var mass = mzs[dataIndex];

            if (dataIndex <= 0 || dataIndex >= mzs.Count - 1)
                return 0;

            // internal variable to store temporary m/z values. It also guarantees a workspace that doesn't need to be reallocated all the time.
            var mzTempList = new List<double>();
            // internal variable to store temporary intensity values. It also guarantees a workspace that doesn't need to be reallocated all the time.
            var intensityTempList = new List<double>();

            var upper = mzs[0];
            for (var index = dataIndex; index >= 0; index--)
            {
                var currentMass = mzs[index];
                var y1 = intensities[index];
                if ((y1 < peakHalf) || (Math.Abs(mass - currentMass) > 5.0) ||
                    ((index < 1 || intensities[index - 1] > y1) && (index < 2 || intensities[index - 2] > y1) &&
                     (signalToNoise < 4.0)))
                {
                    var y2 = intensities[index + 1];
                    var x1 = mzs[index];
                    var x2 = mzs[index + 1];
                    if (!(y2 - y1).Equals(0) && (y1 < peakHalf))
                    {
                        upper = x1 - (x1 - x2) * (peakHalf - y1) / (y2 - y1);
                    }
                    else
                    {
                        upper = x1;
                        var points = dataIndex - index + 1;
                        if (points >= 3)
                        {
                            mzTempList.Clear();
                            intensityTempList.Clear();
                            if (mzTempList.Capacity < points)
                            {
                                mzTempList.Capacity = points;
                                intensityTempList.Capacity = points;
                            }

                            var j = points - 1;
                            for (; j >= 0; j--)
                            {
                                mzTempList.Add(mzs[dataIndex - j]);
                                intensityTempList.Add(intensities[dataIndex - j]);
                            }
                            for (j = 0; j < points && intensityTempList[0].Equals(intensityTempList[j]); j++)
                            {
                            }

                            if (j == points)
                                return 0.0;
                            // coe is coefficients found by curve regression.
                            var iStat = CurveReg(intensityTempList, mzTempList, points, out var coe, 1, out _);
                            // only if successful calculation of peak was done, should we change upper.
                            if (iStat != -1)
                                upper = coe[1] * peakHalf + coe[0];
                        }
                    }
                    break;
                }
            }

            var lower = mzs[mzs.Count - 1];
            for (var index = dataIndex; index < mzs.Count; index++)
            {
                var currentMass = mzs[index];
                var y1 = intensities[index];
                if ((y1 < peakHalf) || (Math.Abs(mass - currentMass) > 5.0) ||
                    ((index > mzs.Count - 2 || intensities[index + 1] > y1) &&
                     (index > mzs.Count - 3 || intensities[index + 2] > y1) && signalToNoise < 4.0))
                {
                    var y2 = intensities[index - 1];
                    var x1 = mzs[index];
                    var x2 = mzs[index - 1];

                    if (!(y2 - y1).Equals(0) && (y1 < peakHalf))
                    {
                        lower = x1 - (x1 - x2) * (peakHalf - y1) / (y2 - y1);
                    }
                    else
                    {
                        lower = x1;
                        var points = index - dataIndex + 1;
                        if (points >= 3)
                        {
                            mzTempList.Clear();
                            intensityTempList.Clear();
                            if (mzTempList.Capacity < points)
                            {
                                mzTempList.Capacity = points;
                                intensityTempList.Capacity = points;
                            }
                            for (var k = points - 1; k >= 0; k--)
                            {
                                mzTempList.Add(mzs[index - k]);
                                intensityTempList.Add(intensities[index - k]);
                            }
                            int j;
                            for (j = 0; j < points && intensityTempList[0].Equals(intensityTempList[j]); j++)
                            {
                            }

                            if (j == points)
                                return 0.0;
                            // coe is coefficients found by curve regression.
                            var iStat = CurveReg(intensityTempList, mzTempList, points, out var coe, 1, out _);
                            // only if successful calculation of peak was done, should we change lower.
                            if (iStat != -1)
                                lower = coe[1] * peakHalf + coe[0];
                        }
                    }
                    break;
                }
            }

            if (upper.Equals(0.0))
                return 2 * Math.Abs(mass - lower);
            if (lower.Equals(0.0))
                return 2 * Math.Abs(mass - upper);
            return Math.Abs(upper - lower);
        }

        /// <summary>
        /// Calculate Least Square error mapping y = f(x).  [GORD] This is linear regression - that's it!
        /// </summary>
        /// <param name="x">List of x values.</param>
        /// <param name="y">List of y values.</param>
        /// <param name="n">number of points in List.</param>
        /// <param name="terms">output coefficients of Least Square Error parameters. Coefficients are slope and intercept!</param>
        /// <param name="nTerms">order of the function y = f(x).</param>
        /// <param name="mse">minimum square error value.</param>
        /// <returns>returns 0 if successful an -1 if not.</returns>
        public int CurveReg(List<double> x, List<double> y, int n, out double[] terms, int nTerms, out double mse)
        {
            // weights
            var w = new double[n];
            for (var i = 0; i < n; i++)
            {
                w[i] = 1.0;
            }

            // weighted powers of x matrix transpose = At
            var aT = Matrix<double>.Build.Dense(nTerms + 1, n, 0);
            for (var i = 0; i < n; i++)
            {
                aT[0, 1] = w[i];
                for (var j = 1; j < nTerms + 1; j++)
                {
                    aT[j, i] = aT[j - 1, i] * x[i];
                }
            }

            // Z = weighted y List
            var z = Matrix<double>.Build.Dense(n, 1, 0);
            for (var i = 0; i < n; i++)
            {
                z[i, 0] = w[i] * y[i];
            }

            //var aT_T = aT.Transpose();
            //var At_At_T = aT.Multiply(aT_T);
            //var I_At_At_T = At_At_T.Inverse();
            //var At_Ai_At = I_At_At_T.Multiply(aT);
            //var b = At_Ai_At.Multiply(z);
            var b = aT.TransposeAndMultiply(aT).Inverse().Multiply(aT).Multiply(z);

            // make a matrix with the fit y and the difference
            var outA = Matrix<double>.Build.Dense(2, n, 0);

            // calculate the least squares y values
            terms = new double[nTerms + 1];
            mse = 0.0;
            for (var i = 0; i < n; i++)
            {
                terms[0] = b[0, 0];
                var yFit = b[0, 0];
                var xPow = x[i];
                for (var j = 1; j <= nTerms; j++)
                {
                    terms[j] = b[j, 0];
                    yFit += b[j, 0] * xPow;
                    xPow = xPow * x[i];
                }
                outA[0, i] = yFit;
                outA[1, i] = y[i] - yFit;
                mse += y[i] - yFit;
            }
            return 0;
        }
    }
}