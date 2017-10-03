// Written by Navdeep Jaitly and Gordon Slysz.
// Copyright 2012, Battelle Memorial Institute for the Department of Energy (PNNL, Richland, WA)
// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
// Website: http://panomics.pnnl.gov/software/
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


        public virtual Peak CreatePeak(double xvalue, float height, float width = 0, float signalToNoise = 0)
        {
            return new MSPeak(xvalue, height, width, signalToNoise);
        }


        #endregion

        #region Public Methods

        public override List<Peak> FindPeaks(XYData xydata, double minX=0, double maxX=0)
        {
            if (xydata == null) return new List<Peak>();

            return FindPeaks(xydata.Xvalues, xydata.Yvalues, minX, maxX);
        }

        public List<Peak> FindPeaks(double[] xvalues, double[] yvalues, double minXValue = 0, double maxXValue = 0)
        {
            var peakList = new List<Peak>();

            if (xvalues == null || yvalues == null || xvalues.Length < 3)
            {
                return peakList;
            }

            if (minXValue == 0 && maxXValue == 0)
            {
                maxXValue = xvalues[xvalues.Length - 1];
            }

            //Get background intensity
            BackgroundIntensity = GetBackgroundIntensity(yvalues, xvalues);

            _intensityThreshold = BackgroundIntensity * PeakToBackgroundRatio;

            if (IsDataThresholded)
            {
                if (SignalToNoiseThreshold != 0)
                {
                    BackgroundIntensity = _intensityThreshold / SignalToNoiseThreshold;
                }
                else if (_intensityThreshold != 0)
                {
                    BackgroundIntensity = _intensityThreshold;
                }
                else
                {
                    BackgroundIntensity = 1;
                }
            }

            //Find start index in raw data
            var startIndex = MathUtils.GetClosest(xvalues, minXValue);

            //Find stop index in raw data
            var stopIndex = MathUtils.GetClosest(xvalues, maxXValue);

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
                        if (yvalues[currentIndex] > 0)
                        {							
                            while (nextIndex < stopIndex && yvalues[nextIndex] == 0)
                            {
                                nextIndex += 1;
                            }

                            if (yvalues[nextIndex] > 0)
                            {
                                double currentWidth = (float)(xvalues[nextIndex] - xvalues[currentIndex]);
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
                        var currentIntensity = yvalues[index];

                        double signalToNoise = -1;
                        if (IsDataThresholded)
                        {
                            if (BackgroundIntensity > 0)
                                signalToNoise = currentIntensity / BackgroundIntensity;
                            else
                                signalToNoise = currentIntensity;
                        }
                        else
                        {
                            signalToNoise = CalculateSignalToNoise(yvalues, index);
                        }

                        var peak = CreatePeak(xvalues[index], (float)yvalues[index], peakWidth, (float)signalToNoise);
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
                    if (stopIndex >= xvalues.Length - 2)
                    {
                        stopIndex = xvalues.Length - 2;
                    }

                    for (var index = startIndex; index <= stopIndex; index++)
                    {
                        // double fwhm = -1;
                        var currentIntensity = yvalues[index];
                        var lastIntensity = yvalues[index - 1];
                        var nextIntensity = yvalues[index + 1];

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
                                signalToNoise = CalculateSignalToNoise(yvalues, index);
                            }
                        }

                        if (signalToNoise > SignalToNoiseThreshold)
                        {
                            var calculatedXValue = CalculateFittedValue(xvalues, yvalues, index);
                            var width = CalculateFWHM(xvalues, yvalues, index, signalToNoise);

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

        protected override double GetBackgroundIntensity(double[] yvalues, double[]xvalues=null)
        {
            double thresholdMultiplier = 5;

            var firstAverage = GetAverageIntensity(yvalues);

            var secondAverage = GetAverageIntensity(yvalues, firstAverage * thresholdMultiplier);

            return secondAverage;
        }


        /// <summary>
        /// Calculate full width half maximum of a peak
        /// </summary>
        /// <param name="xvalues"></param>
        /// <param name="yvalues"></param>
        /// <param name="index">index of point in array that is the apex or max of the peak</param>
        /// <param name="signalToNoise"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        private double CalculateFWHM(double[] xvalues, double[] yvalues, int index, double signalToNoise)
        {

            var numPoints = xvalues.Length;
            if (index >= numPoints || index < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "Trying to calculate peak width, but index of peak apex was out of range.");
            }


            var peakApexIntensity = yvalues[index];
            var halfHeightIntensity = peakApexIntensity / 2;

            double x1, x2, y1, y2;




            double interpolatedX1 = 0;
            double interpolatedX2 = 0;

            //Moving to the left of the peak apex, 
            //will find the first point whose intensity is below the peakHalf
            for (var i = index; i >= 0; i--)
            {
                y1 = yvalues[i];

                if (y1 >= halfHeightIntensity) continue;

                x1 = xvalues[i];
                x2 = xvalues[i + 1];
                y2 = yvalues[i + 1];

                if ((y2 - y1) != 0)
                {
                    var slope = (y2 - y1) / (x2 - x1);
                    var yintercept = y1 - (slope * x1);

                    interpolatedX1 = (halfHeightIntensity - yintercept) / slope;
                    break;
                }
            }

            if(interpolatedX1 == 0)
            {
                if (index > 0) interpolatedX1 = xvalues[index - 1];
                else interpolatedX1 = xvalues[index];
            }

            //moving to the right of the peak apex
            for (var i = index; i < numPoints; i++)
            {
                y2 = yvalues[i];

                if (y2 >= halfHeightIntensity) continue;

                x1 = xvalues[i - 1];
                y1 = yvalues[i - 1];

                x2 = xvalues[i];

                if ((y2 - y1) != 0)
                {
                    var slope = (y2 - y1) / (x2 - x1);
                    var yintercept = y1 - (slope * x1);

                    interpolatedX2 = (halfHeightIntensity - yintercept) / slope;
                    break;
                }
            }

            if (interpolatedX2 == 0)
            {
                if (index < xvalues.Length - 1) interpolatedX2 = xvalues[index + 1];
                else interpolatedX2 = xvalues[index];
            }

            return interpolatedX2 - interpolatedX1;   //return the width
        }

        private double CalculateFittedValue(double[] xvalues, double[] yvalues, int index)
        {
            switch (PeakFitType)
            {
                case Globals.PeakFitType.Undefined:
                    throw new NotImplementedException();

                case Globals.PeakFitType.APEX:
                    return xvalues[index];

                case Globals.PeakFitType.LORENTZIAN:
                    throw new NotImplementedException();


                case Globals.PeakFitType.QUADRATIC:

                    return CalculateQuadraticFittedValue(xvalues, yvalues, index);


                default:
                    throw new ArgumentOutOfRangeException();
            }


        }

        /// <summary>
        /// Examine the 3 data points surrounding the given data point to compute an interpolated x value for where the peak apex most likely lies
        /// </summary>
        /// <param name="xvalues"></param>
        /// <param name="yvalues"></param>
        /// <param name="index"></param>
        /// <returns>The interpolated x value</returns>
        private double CalculateQuadraticFittedValue(double[] xvalues, double[] yvalues, int index)
        {
            if (index < 1)
                return xvalues[0];
            if (index >= xvalues.Length - 1)
                return xvalues.Last();

            var x1 = xvalues[index - 1];
            var x2 = xvalues[index];
            var x3 = xvalues[index + 1];
            var y1 = yvalues[index - 1];
            var y2 = yvalues[index];
            var y3 = yvalues[index + 1];

            var calculatedVal = (y2 - y1) * (x3 - x2) - (y3 - y2) * (x2 - x1);
            if (calculatedVal == 0)
            {
                return x2; // no good.  Just return the known peak
            }

            calculatedVal = ((x1 + x2) - ((y2 - y1) * (x3 - x2) * (x1 - x3)) / calculatedVal) / 2.0;
            return calculatedVal; // Calculated new peak.  Return it.
        }

        private double GetAverageIntensity(double[] intensities, double maxIntensity = double.MaxValue)
        {
            var numPoints = intensities.Length;
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
        /// <param name="yvalues"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private double CalculateSignalToNoise(double[] yvalues, int index)
        {
            double minIntensityLeft = 0;
            double minIntensityRight = 0;

            var numPoints = yvalues.Length;
            if (index <= 0 || index >= numPoints - 1) return 0;

            var currentIntensity = yvalues[index];
            if (Math.Abs(currentIntensity - 0) < double.Epsilon) return 0;

            //Find the local minimum as we move down the m/z range
            var found = false;
            for (var i = index; i > 0; i--)
            {
                var yValue = yvalues[i];

                if (yvalues[i + 1] >= yValue && yvalues[i - 1] > yValue)
                {
                    minIntensityLeft = yValue;

                    found = true;
                    break;
                }
            }

            if (!found) minIntensityLeft = yvalues[0];

            //Find the local minimum as we move up the m/z range
            found = false;
            for (var i = index; i < numPoints - 1; i++)
            {
                var yValue = yvalues[i];

                if (yvalues[i + 1] >= yValue && yvalues[i - 1] > yValue)
                {
                    minIntensityRight = yValue;
                    found = true;
                    break;
                }
            }

            if (!found) minIntensityRight = yvalues[numPoints - 1];

            if (minIntensityLeft == 0)
            {
                if (minIntensityRight == 0)
                {
                    return 100;
                }
                else
                {
                    return currentIntensity / minIntensityRight;
                }
            }

            if (minIntensityRight < minIntensityLeft && minIntensityRight != 0)
            {
                return currentIntensity / minIntensityRight;
            }

            return currentIntensity / minIntensityLeft;
        }

        #endregion
    }
}
