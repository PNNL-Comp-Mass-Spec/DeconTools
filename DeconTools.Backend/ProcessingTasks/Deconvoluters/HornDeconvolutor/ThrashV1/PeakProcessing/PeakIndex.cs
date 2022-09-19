using System;
using System.Collections.Generic;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing
{
    /// <summary>
    ///     Used to search sorted list of points for closest point.
    /// </summary>
    /// <remarks>
    ///     Can look for the closest point either
    ///     - By looking in a binary search between specified indices
    ///     - linear search around a given index.
    /// </remarks>
    public static class PeakIndex
    {
        /// <summary>
        ///     does a search for the given value by doing a linear scan to the left of the given index
        /// </summary>
        /// <param name="vec">is the List of the points.</param>
        /// <param name="mzVal">is the value we are looking for.</param>
        /// <param name="startIndex">index of the peak to the left of which we are scanning.</param>
        /// <returns>returns the index of the point that is closest to the specified value.</returns>
        public static int LookLeft(List<double> vec, double mzVal, int startIndex)
        {
            // mzVal <= vec[start_index] so start moving index further left.
            var nearestIndex = startIndex;
            var nextIndex = startIndex;

            if (nextIndex == 0)
            {
                return 0;
            }

            var nextVal = vec[nextIndex];
            var bestDistance = Math.Abs(mzVal - nextVal);

            while (nextVal > mzVal)
            {
                nextIndex--;
                nextVal = vec[nextIndex];
                var dist = Math.Abs(nextVal - mzVal);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    nearestIndex = nextIndex;
                }
                if (nextIndex == 0)
                {
                    break;
                }
            }
            return nearestIndex;
        }

        /// <summary>
        ///     does a search for the given value by doing a linear scan to the right of the given index
        /// </summary>
        /// <param name="vec">is the List of the points.</param>
        /// <param name="mzVal">is the value we are looking for.</param>
        /// <param name="startIndex">index of the peak to the right of which we are scanning.</param>
        /// <returns>returns the index of the point that is closest to the specified value.</returns>
        public static int LookRight(List<double> vec, double mzVal, int startIndex)
        {
            // mzVal >= vec[start_index] so start moving index further right.
            var nearestIndex = startIndex;
            var nextIndex = startIndex;
            var numPts = vec.Count;

            if (nextIndex >= numPts - 1)
            {
                return numPts - 1;
            }

            var nextVal = vec[nextIndex];
            var bestDistance = Math.Abs(mzVal - nextVal);

            // we've gone back too far, possibly. Move past the mz_val and return that value.
            while (nextVal < mzVal)
            {
                nextIndex++;
                nextVal = vec[nextIndex];
                var dist = Math.Abs(nextVal - mzVal);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    nearestIndex = nextIndex;
                }

                if (nextIndex == numPts - 1)
                {
                    break;
                }
            }
            return nearestIndex;
        }

        /// <summary>
        ///     Gets the index of the point nearest to the specified point, between the specified indices.
        /// </summary>
        /// <param name="vec">is the List of the points.</param>
        /// <param name="mzVal">is the value we are looking for.</param>
        /// <param name="startIndex">minimum index of the point.</param>
        /// <param name="stopIndex">maximum index of the point.</param>
        /// <returns>returns the index of the point that is closest to the specified value.</returns>
        public static int GetNearestBinary(List<double> vec, double mzVal, int startIndex, int stopIndex)
        {
            // TODO: Use built-in binary search?
            if (vec[startIndex] > mzVal)
            {
                return startIndex;
            }

            if (vec[stopIndex] < mzVal)
            {
                return stopIndex;
            }

            while (true)
            {
                var minVal = vec[startIndex];
                var maxVal = vec[stopIndex];
                if (Math.Abs(stopIndex - startIndex) <= 1 && mzVal >= minVal && mzVal <= maxVal)
                {
                    //return closer value.
                    if (Math.Abs(minVal - mzVal) < Math.Abs(maxVal - mzVal))
                    {
                        return startIndex;
                    }

                    return stopIndex;
                }

                var ratio = (maxVal - mzVal) * 1.0 / (maxVal - minVal);
                var midIndex = (int)(startIndex * ratio + stopIndex * (1 - ratio) + 0.5);
                if (midIndex == startIndex)
                {
                    midIndex = startIndex + 1;
                }
                else if (midIndex == stopIndex)
                {
                    midIndex = stopIndex - 1;
                }

                var midVal = vec[midIndex];
                if (midVal >= mzVal)
                {
                    stopIndex = midIndex;
                }
                else if (midIndex + 1 == stopIndex)
                {
                    if (Math.Abs(midVal - mzVal) < Math.Abs(maxVal - mzVal))
                    {
                        return midIndex;
                    }

                    return stopIndex;
                }
                else
                {
                    var midNextVal = vec[midIndex + 1];
                    if (mzVal >= midVal && mzVal <= midNextVal)
                    {
                        if (mzVal - midVal < midNextVal - midVal)
                        {
                            return midIndex;
                        }

                        return midIndex + 1;
                    }
                    startIndex = midIndex + 1;
                }
            }
        }

        /// <summary>
        ///     Gets the index of the point nearest to the specified point.
        /// </summary>
        /// <param name="vec">is the List of the points.</param>
        /// <param name="mzVal">is the value we are looking for.</param>
        /// <param name="startIndex">index around which we are looking for the specified point.</param>
        /// <returns>returns the index of the point that is closest to the specified value.</returns>
        public static int GetNearest(List<double> vec, double mzVal, int startIndex)
        {
            // we're going to use continuity here, look at the difference
            // between consecutive points and estimate how much further we have to
            // go and start there.
            var numPts = vec.Count - 1;
            if (mzVal >= vec[numPts])
            {
                return numPts;
            }

            if (mzVal < vec[0])
            {
                return 0;
            }

            var distanceToGo = mzVal - vec[startIndex];
            double step;
            if (startIndex < numPts)
            {
                step = vec[startIndex + 1] - vec[startIndex];
            }
            else
            {
                step = vec[startIndex] - vec[startIndex - 1];
            }

            var moveBy = (int)(distanceToGo / step);
            var nextIndex = startIndex + moveBy;

            if (nextIndex < 0)
            {
                nextIndex = 0;
            }

            if (nextIndex > numPts)
            {
                nextIndex = numPts - 1;
            }

            if (mzVal >= vec[nextIndex])
            {
                return LookRight(vec, mzVal, nextIndex);
            }

            return LookLeft(vec, mzVal, nextIndex);
        }
    }
}