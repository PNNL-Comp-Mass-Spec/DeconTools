// Written by Navdeep Jaitly and Gordon Slysz.
// Copyright 2012, Battelle Memorial Institute for the Department of Energy (PNNL, Richland, WA)
// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
// Website: https://panomics.pnnl.gov/software/
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0


using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakDetectors
{
    public class DeconToolsPeakDetectorV2 : PeakDetector
    {
        private double _intensityThreshold;

        #region Constructors

        public DeconToolsPeakDetectorV2()
        {
            IsDataThresholded = false;
            SignalToNoiseThreshold = 2.0;
            PeakToBackgroundRatio = 2.0;
            PeakFitType = Globals.PeakFitType.QUADRATIC;
            PeaksAreStored = false;
            _intensityThreshold = 0;
        }

        public DeconToolsPeakDetectorV2(double peakToBackgroundRatio,
                                        double signalToNoiseThreshold,
                                        Globals.PeakFitType peakFitType = Globals.PeakFitType.QUADRATIC,
                                        bool isDataThresholded = false)
        {
            PeakToBackgroundRatio = peakToBackgroundRatio;
            SignalToNoiseThreshold = signalToNoiseThreshold;
            PeakFitType = peakFitType;
            IsDataThresholded = isDataThresholded;
        }


        #endregion

        #region Properties



        public Globals.RawDataType RawDataType { get; set; }

        public bool IsDataThresholded { get; set; }

        public Globals.PeakFitType PeakFitType { get; set; }

        public double SignalToNoiseThreshold { get; set; }

        public double PeakToBackgroundRatio { get; set; }


        public virtual Peak CreatePeak(double xValue, float height, float width = 0, float signalToNoise = 0)
        {
            return new MSPeak(xValue, height, width, signalToNoise);
        }


        #endregion

        #region Public Methods

        public override List<Peak> FindPeaks(XYData xyData, double minX = 0, double maxX = 0)
        {
            if (xyData == null) return new List<Peak>();

            return FindPeaks(xyData.Xvalues, xyData.Yvalues, minX, maxX);
        }

        public List<Peak> FindPeaks(double[] xValues, double[] yValues, double minXValue = 0, double maxXValue = 0)
        {
            var peakList = new List<Peak>();

            if (xValues == null || yValues == null || xValues.Length < 3)
            {
                return peakList;
            }

            if (Math.Abs(minXValue) < float.Epsilon && Math.Abs(maxXValue) < float.Epsilon)
            {
                maxXValue = xValues[xValues.Length - 1];
            }

            //Get background intensity
            BackgroundIntensity = GetBackgroundIntensity(yValues, xValues);

            _intensityThreshold = BackgroundIntensity * PeakToBackgroundRatio;

            if (IsDataThresholded)
            {
                if (Math.Abs(SignalToNoiseThreshold) > float.Epsilon)
                {
                    BackgroundIntensity = _intensityThreshold / SignalToNoiseThreshold;
                }
                else if (Math.Abs(_intensityThreshold) > float.Epsilon)
                {
                    BackgroundIntensity = _intensityThreshold;
                }
                else
                {
                    BackgroundIntensity = 1;
                }
            }

            //Find start index in raw data
            var startIndex = MathUtils.GetClosest(xValues, minXValue);

            //Find stop index in raw data
            var stopIndex = MathUtils.GetClosest(xValues, maxXValue);

            switch (RawDataType)
            {
                case Globals.RawDataType.Centroided:
                    // Find the median width between any two adjacent x values that are less than 0.2 Th apart and have positive abundances

                    var peakWidths = new List<double>();
                    var peakWidth = 0.01F;

                    var currentIndex = startIndex;
                    while (currentIndex < stopIndex)
                    {
                        var nextIndex = currentIndex + 1;
                        if (yValues[currentIndex] > 0)
                        {
                            while (nextIndex < stopIndex && Math.Abs(yValues[nextIndex]) < float.Epsilon)
                            {
                                nextIndex += 1;
                            }

                            if (yValues[nextIndex] > 0)
                            {
                                double currentWidth = (float)(xValues[nextIndex] - xValues[currentIndex]);
                                if (currentWidth < 0.2)
                                    peakWidths.Add(currentWidth);
                            }
                        }
                        currentIndex = nextIndex;
                    }

                    if (peakWidths.Count > 0)
                        peakWidth = (float)(MathUtils.GetMedian(peakWidths) / 2);

                    if (peakWidth < 0.01)
                        peakWidth = 0.01F;

                    for (var index = startIndex; index <= stopIndex; index++)
                    {
                        var currentIntensity = yValues[index];

                        double signalToNoise;
                        if (IsDataThresholded)
                        {
                            if (BackgroundIntensity > 0)
                                signalToNoise = currentIntensity / BackgroundIntensity;
                            else
                                signalToNoise = currentIntensity;
                        }
                        else
                        {
                            signalToNoise = CalculateSignalToNoise(yValues, index);
                        }

                        var peak = CreatePeak(xValues[index], (float)yValues[index], peakWidth, (float)signalToNoise);
                        peak.DataIndex = index;
                        peakList.Add(peak);

                    }

                    break;

                case Globals.RawDataType.Profile:
                    //Adjust start index if necessary
                    if (startIndex <= 0)
                    {
                        startIndex = 1;
                    }

                    //adjust stop index if necessary
                    if (stopIndex >= xValues.Length - 2)
                    {
                        stopIndex = xValues.Length - 2;
                    }

                    for (var index = startIndex; index <= stopIndex; index++)
                    {
                        // double fwhm = -1;
                        var currentIntensity = yValues[index];
                        var lastIntensity = yValues[index - 1];
                        var nextIntensity = yValues[index + 1];

                        var peakApexFound = currentIntensity >= lastIntensity && currentIntensity >= nextIntensity;

                        double signalToNoise = -1;

                        if (peakApexFound && currentIntensity > _intensityThreshold)
                        {

                            if (IsDataThresholded)
                            {
                                signalToNoise = currentIntensity / BackgroundIntensity;
                            }
                            else
                            {
                                signalToNoise = CalculateSignalToNoise(yValues, index);
                            }
                        }

                        if (signalToNoise > SignalToNoiseThreshold)
                        {
                            var calculatedXValue = CalculateFittedValue(xValues, yValues, index);
                            var width = CalculateFWHM(xValues, yValues, index);

                            var peak = CreatePeak(calculatedXValue, (float)currentIntensity, (float)width, (float)signalToNoise);
                            peak.DataIndex = index;

                            peakList.Add(peak);
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return peakList;
        }

        protected override void ExecutePostProcessingHook(Run run)
        {
            base.ExecutePostProcessingHook(run);

            if (PeaksAreStored)
            {
                run.ResultCollection.FillMSPeakResults();
            }

            GatherPeakStatistics(run);

        }


        public double GetCurrentBackgroundIntensity()
        {
            return _intensityThreshold;
        }

        protected override double GetBackgroundIntensity(double[] yValues, double[] xValues = null)
        {
            double thresholdMultiplier = 5;

            var firstAverage = GetAverageIntensity(yValues);

            var secondAverage = GetAverageIntensity(yValues, firstAverage * thresholdMultiplier);

            return secondAverage;
        }


        /// <summary>
        /// Calculate full width half maximum of a peak
        /// </summary>
        /// <param name="xValues"></param>
        /// <param name="yValues"></param>
        /// <param name="index">index of point in array that is the apex or max of the peak</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        private double CalculateFWHM(IReadOnlyList<double> xValues, IReadOnlyList<double> yValues, int index)
        {

            var numPoints = xValues.Count;
            if (index >= numPoints || index < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    "Trying to calculate peak width, but index of peak apex was out of range.");
            }


            var peakApexIntensity = yValues[index];
            var halfHeightIntensity = peakApexIntensity / 2;

            double x1, x2, y1, y2;




            double interpolatedX1 = 0;
            double interpolatedX2 = 0;

            //Moving to the left of the peak apex,
            //will find the first point whose intensity is below the peakHalf
            for (var i = index; i >= 0; i--)
            {
                y1 = yValues[i];

                if (y1 >= halfHeightIntensity) continue;

                x1 = xValues[i];
                x2 = xValues[i + 1];
                y2 = yValues[i + 1];

                if (Math.Abs(y2 - y1) > float.Epsilon)
                {
                    var slope = (y2 - y1) / (x2 - x1);
                    var yIntercept = y1 - (slope * x1);

                    interpolatedX1 = (halfHeightIntensity - yIntercept) / slope;
                    break;
                }
            }

            if (Math.Abs(interpolatedX1) < float.Epsilon)
            {
                if (index > 0) interpolatedX1 = xValues[index - 1];
                else interpolatedX1 = xValues[index];
            }

            //moving to the right of the peak apex
            for (var i = index; i < numPoints; i++)
            {
                y2 = yValues[i];

                if (y2 >= halfHeightIntensity) continue;

                x1 = xValues[i - 1];
                y1 = yValues[i - 1];

                x2 = xValues[i];

                if (Math.Abs(y2 - y1) > float.Epsilon)
                {
                    var slope = (y2 - y1) / (x2 - x1);
                    var yIntercept = y1 - (slope * x1);

                    interpolatedX2 = (halfHeightIntensity - yIntercept) / slope;
                    break;
                }
            }

            if (Math.Abs(interpolatedX2) < float.Epsilon)
            {
                if (index < xValues.Count - 1) interpolatedX2 = xValues[index + 1];
                else interpolatedX2 = xValues[index];
            }

            return interpolatedX2 - interpolatedX1;   //return the width
        }

        private double CalculateFittedValue(IReadOnlyList<double> xValues, IReadOnlyList<double> yValues, int index)
        {
            switch (PeakFitType)
            {
                case Globals.PeakFitType.Undefined:
                    throw new NotImplementedException();

                case Globals.PeakFitType.APEX:
                    return xValues[index];

                case Globals.PeakFitType.LORENTZIAN:
                    throw new NotImplementedException();


                case Globals.PeakFitType.QUADRATIC:

                    return CalculateQuadraticFittedValue(xValues, yValues, index);


                default:
                    throw new ArgumentOutOfRangeException();
            }


        }

        /// <summary>
        /// Examine the 3 data points surrounding the given data point to compute an interpolated x value for where the peak apex most likely lies
        /// </summary>
        /// <param name="xValues"></param>
        /// <param name="yValues"></param>
        /// <param name="index"></param>
        /// <returns>The interpolated x value</returns>
        private double CalculateQuadraticFittedValue(IReadOnlyList<double> xValues, IReadOnlyList<double> yValues, int index)
        {
            if (index < 1)
                return xValues[0];
            if (index >= xValues.Count - 1)
                return xValues.Last();

            var x1 = xValues[index - 1];
            var x2 = xValues[index];
            var x3 = xValues[index + 1];
            var y1 = yValues[index - 1];
            var y2 = yValues[index];
            var y3 = yValues[index + 1];

            var calculatedVal = (y2 - y1) * (x3 - x2) - (y3 - y2) * (x2 - x1);
            if (Math.Abs(calculatedVal) < float.Epsilon)
            {
                return x2; // no good.  Just return the known peak
            }

            calculatedVal = ((x1 + x2) - ((y2 - y1) * (x3 - x2) * (x1 - x3)) / calculatedVal) / 2.0;
            return calculatedVal; // Calculated new peak.  Return it.
        }

        private double GetAverageIntensity(IReadOnlyList<double> intensities, double maxIntensity = double.MaxValue)
        {
            var numPoints = intensities.Count;
            if (numPoints == 0) return 0;

            double backgroundIntensity = 0;
            var numPointsUsed = 0;
            for (var i = 0; i < numPoints; i++)
            {
                var intensity = intensities[i];

                if (intensity <= maxIntensity && intensity > 0)
                {
                    backgroundIntensity += intensity;
                    numPointsUsed++;
                }
            }
            return backgroundIntensity / numPointsUsed;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="yValues"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private double CalculateSignalToNoise(IReadOnlyList<double> yValues, int index)
        {
            double minIntensityLeft = 0;
            double minIntensityRight = 0;

            var numPoints = yValues.Count;
            if (index <= 0 || index >= numPoints - 1) return 0;

            var currentIntensity = yValues[index];
            if (Math.Abs(currentIntensity - 0) < double.Epsilon) return 0;

            //Find the local minimum as we move down the m/z range
            var found = false;
            for (var i = index; i > 0; i--)
            {
                var yValue = yValues[i];

                if (yValues[i + 1] >= yValue && yValues[i - 1] > yValue)
                {
                    minIntensityLeft = yValue;

                    found = true;
                    break;
                }
            }

            if (!found) minIntensityLeft = yValues[0];

            //Find the local minimum as we move up the m/z range
            found = false;
            for (var i = index; i < numPoints - 1; i++)
            {
                var yValue = yValues[i];

                if (yValues[i + 1] >= yValue && yValues[i - 1] > yValue)
                {
                    minIntensityRight = yValue;
                    found = true;
                    break;
                }
            }

            if (!found) minIntensityRight = yValues[numPoints - 1];

            if (Math.Abs(minIntensityLeft) < float.Epsilon)
            {
                if (Math.Abs(minIntensityRight) < float.Epsilon)
                {
                    return 100;
                }
                return currentIntensity / minIntensityRight;
            }

            if (minIntensityRight < minIntensityLeft && Math.Abs(minIntensityRight) > float.Epsilon)
            {
                return currentIntensity / minIntensityRight;
            }

            return currentIntensity / minIntensityLeft;
        }

        #endregion
    }
}
