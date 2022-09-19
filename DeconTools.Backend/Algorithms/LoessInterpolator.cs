using System;
using System.Collections.Generic;

namespace DeconTools.Backend.Algorithms
{
    public class LoessInterpolator
    {
        public static double DEFAULT_BANDWIDTH = 0.3;
        public static int DEFAULT_ROBUSTNESS_ITERATIONS = 2;

        /**
         * The bandwidth parameter: when computing the loess fit at
         * a particular point, this fraction of source points closest
         * to the current point is taken into account for computing
         * a least-squares regression.
         *
         * A sensible value is usually 0.25 to 0.5.
         */
        private readonly double bandwidth;

        /**
         * The number of robustness iterations parameter: this many
         * robustness iterations are done.
         *
         * A sensible value is usually 0 (just the initial fit without any
         * robustness iterations) to 4.
         */
        private readonly int robustnessIterations;

        public LoessInterpolator()
        {
            bandwidth = DEFAULT_BANDWIDTH;
            robustnessIterations = DEFAULT_ROBUSTNESS_ITERATIONS;
        }

        public LoessInterpolator(double bandwidth, int robustnessIterations)
        {
            if (bandwidth < 0 || bandwidth > 1)
            {
                throw new ApplicationException(string.Format("bandwidth must be in the interval [0,1], but got {0}", bandwidth));
            }
            this.bandwidth = bandwidth;
            if (robustnessIterations < 0)
            {
                throw new ApplicationException(string.Format("the number of robustness iterations must be non-negative, but got {0}", robustnessIterations));
            }
            this.robustnessIterations = robustnessIterations;
        }

        /**
         * Compute a loess fit on the data at the original abscissas.
         *
         * @param xVal the arguments for the interpolation points
         * @param yVal the values for the interpolation points
         * @return values of the loess fit at corresponding original abscissas
         * @throws MathException if some of the following conditions are false:
         * <ul>
         * <li> Arguments and values are of the same size that is greater than zero</li>
         * <li> The arguments are in a strictly increasing order</li>
         * <li> All arguments and values are finite real numbers</li>
         * </ul>
         */
        public double[] Smooth(double[] xVal, double[] yVal)
        {
            if (xVal.Length != yVal.Length)
            {
                throw new ApplicationException(string.Format("Loess expects the abscissa and ordinate arrays to be of the same size, but got {0} abscissas and {1} ordinates", xVal.Length, yVal.Length));
            }
            var n = xVal.Length;
            if (n == 0)
            {
                throw new ApplicationException("Loess expects at least 1 point");
            }

            CheckAllFiniteReal(xVal, true);
            CheckAllFiniteReal(yVal, false);
            CheckStrictlyIncreasing(xVal);

            if (n == 1)
            {
                return new[] { yVal[0] };
            }

            if (n == 2)
            {
                return new[] { yVal[0], yVal[1] };
            }

            var bandwidthInPoints = (int)(bandwidth * n);

            if (bandwidthInPoints < 2)
            {
                throw new ApplicationException(string.Format("the bandwidth must be large enough to accomodate at least 2 points. There are {0} " +
                    " data points, and bandwidth must be at least {1} but it is only {2}",
                    n, 2.0 / n, bandwidth
                ));
            }

            var res = new double[n];

            var residuals = new double[n];
            var sortedResiduals = new double[n];

            var robustnessWeights = new double[n];

            // Do an initial fit and 'robustnessIterations' robustness iterations.
            // This is equivalent to doing 'robustnessIterations+1' robustness iterations
            // starting with all robustness weights set to 1.
            for (var i = 0; i < robustnessWeights.Length; i++)
            {
                robustnessWeights[i] = 1;
            }

            for (var iteration = 0; iteration <= robustnessIterations; ++iteration)
            {
                int[] bandwidthInterval = { 0, bandwidthInPoints - 1 };
                // At each x, compute a local weighted linear regression
                for (var i = 0; i < n; ++i)
                {
                    var x = xVal[i];

                    // Find out the interval of source points on which
                    // a regression is to be made.
                    if (i > 0)
                    {
                        updateBandwidthInterval(xVal, i, bandwidthInterval);
                    }

                    var iLeft = bandwidthInterval[0];
                    var iRight = bandwidthInterval[1];

                    // Compute the point of the bandwidth interval that is
                    // farthest from x
                    int edge;
                    if (xVal[i] - xVal[iLeft] > xVal[iRight] - xVal[i])
                    {
                        edge = iLeft;
                    }
                    else
                    {
                        edge = iRight;
                    }

                    // Compute a least-squares linear fit weighted by
                    // the product of robustness weights and the TriCube
                    // weight function.
                    // See http://en.wikipedia.org/wiki/Linear_regression
                    // (section "Univariate linear case")
                    // and http://en.wikipedia.org/wiki/Weighted_least_squares
                    // (section "Weighted least squares")
                    double sumWeights = 0;
                    double sumX = 0, sumXSquared = 0, sumY = 0, sumXY = 0;
                    var denominator = Math.Abs(1.0 / (xVal[edge] - x));
                    for (var k = iLeft; k <= iRight; ++k)
                    {
                        var xk = xVal[k];
                        var yk = yVal[k];
                        double dist;
                        if (k < i)
                        {
                            dist = (x - xk);
                        }
                        else
                        {
                            dist = (xk - x);
                        }
                        var w = TriCube(dist * denominator) * robustnessWeights[k];
                        var xkw = xk * w;
                        sumWeights += w;
                        sumX += xkw;
                        sumXSquared += xk * xkw;
                        sumY += yk * w;
                        sumXY += yk * xkw;
                    }

                    var meanX = sumX / sumWeights;
                    var meanY = sumY / sumWeights;
                    var meanXY = sumXY / sumWeights;
                    var meanXSquared = sumXSquared / sumWeights;

                    double beta;
                    if (Math.Abs(meanXSquared - meanX * meanX) < double.Epsilon)
                    {
                        beta = 0;
                    }
                    else
                    {
                        beta = (meanXY - meanX * meanY) / (meanXSquared - meanX * meanX);
                    }

                    var alpha = meanY - beta * meanX;

                    res[i] = beta * x + alpha;
                    residuals[i] = Math.Abs(yVal[i] - res[i]);
                }

                // No need to recompute the robustness weights at the last
                // iteration, they won't be needed anymore
                if (iteration == robustnessIterations)
                {
                    break;
                }

                // Recompute the robustness weights.

                // Find the median residual.
                // An Array.Copy and a sort are completely tractable here,
                // because the preceding loop is a lot more expensive
                Array.Copy(residuals, sortedResiduals, n);

                // Array.Copy(residuals, 0, sortedResiduals, 0, n);
                Array.Sort(sortedResiduals);
                var medianResidual = sortedResiduals[n / 2];

                if (Math.Abs(medianResidual) < double.Epsilon)
                {
                    break;
                }

                for (var i = 0; i < n; ++i)
                {
                    var arg = residuals[i] / (6 * medianResidual);
                    robustnessWeights[i] = (arg >= 1) ? 0 : Math.Pow(1 - arg * arg, 2);
                }
            }

            return res;
        }

        /**
         * Given an index interval into xVal that embraces a certain number of
         * points closest to xVal[i-1], update the interval so that it embraces
         * the same number of points closest to xVal[i]
         *
         * @param xVal arguments array
         * @param i the index around which the new interval should be computed
         * @param bandwidthInterval a two-element array {left, right} such that: <p/>
         * <tt>(left==0 or xVal[i] - xVal[left-1] > xVal[right] - xVal[i])</tt>
         * <p/> and also <p/>
         * <tt>(right==xVal.length-1 or xVal[right+1] - xVal[i] > xVal[i] - xVal[left])</tt>.
         * The array will be updated.
         */
        private static void updateBandwidthInterval(IReadOnlyList<double> xVal, int i, IList<int> bandwidthInterval)
        {
            var left = bandwidthInterval[0];
            var right = bandwidthInterval[1];
            // The right edge should be adjusted if the next point to the right
            // is closer to xVal[i] than the leftmost point of the current interval
            if (right < xVal.Count - 1 &&
               xVal[right + 1] - xVal[i] < xVal[i] - xVal[left])
            {
                bandwidthInterval[0]++;
                bandwidthInterval[1]++;
            }
        }

        /**
         * Compute the
         * <a href="http://en.wikipedia.org/wiki/Local_regression#Weight_function">TriCube</a>
         * weight function
         *
         * @param x the argument
         * @return (1-|x|^3)^3
         */
        private static double TriCube(double x)
        {
            var tmp = 1 - x * x * x;
            return tmp * tmp * tmp;
        }

        /**
         * Check that all elements of an array are finite real numbers.
         *
         * @param values the values array
         * @param areAbscissas if true, elements are abscissas otherwise they are ordinates
         * @throws MathException if one of the values is not
         *         a finite real number
         */
        private static void CheckAllFiniteReal(IReadOnlyList<double> values, bool areAbscissas)
        {
            for (var i = 0; i < values.Count; i++)
            {
                var x = values[i];
                if (double.IsInfinity(x) || double.IsNaN(x))
                {
                    var pattern = areAbscissas ?
                            "all abscissas must be finite real numbers, but {0}-th is {1}" :
                            "all ordinates must be finite real numbers, but {0}-th is {1}";
                    throw new ApplicationException(string.Format(pattern, i, x));
                }
            }
        }

        /**
         * Check that elements of the abscissas array are in a strictly
         * increasing order.
         *
         * @param xVal the abscissas array
         * @throws MathException if the abscissas array
         * is not in a strictly increasing order
         */
        private static void CheckStrictlyIncreasing(IReadOnlyList<double> xVal)
        {
            for (var i = 0; i < xVal.Count; ++i)
            {
                if (i >= 1 && xVal[i - 1] >= xVal[i])
                {
                    throw new ApplicationException(string.Format(
                            "the abscissas array must be sorted in a strictly " +
                            "increasing order, but the {0}-th element is {1} " +
                            "whereas {2}-th is {3}",
                            i - 1, xVal[i - 1], i, xVal[i]));
                }
            }
        }
    }
}
