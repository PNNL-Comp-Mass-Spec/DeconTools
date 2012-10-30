// Written by Navdeep Jaitly and Gordon Slysz.  
// Copyright 2012, Battelle Memorial Institute for the Department of Energy (PNNL, Richland, WA)
// E-mail: gordon.slysz@pnl.gov
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
    public class DeconToolsPeakDetectorV2:PeakDetector
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

        #endregion

        #region Properties



        public Globals.RawDataType RawDataType { get; set; }

        public bool IsDataThresholded { get; set; }

        public Globals.PeakFitType PeakFitType { get; set; }

        public double SignalToNoiseThreshold { get; set; }

        public double PeakToBackgroundRatio { get; set; }

       


        #endregion

        #region Public Methods
        public override List<Peak> FindPeaks(XYData xydata, double minX, double maxX)
        {
            if (xydata == null) return new List<Peak>();

            return  FindPeaks(xydata.Xvalues, xydata.Yvalues, minX, maxX);
        }

        public List<Peak> FindPeaks(double[] xvalues, double[] yvalues, double minXValue, double maxXValue)
        {
            List<Peak> peakList = new List<Peak>();

            if (xvalues == null || yvalues == null || xvalues.Length < 3)
            {
                return peakList;
            }


            //Get background intensity
            double backgroundIntensity = GetBackgroundIntensity(yvalues);

            _intensityThreshold = backgroundIntensity * PeakToBackgroundRatio;


            if (IsDataThresholded)
            {
                if (SignalToNoiseThreshold != 0)
                {
                    backgroundIntensity = _intensityThreshold / SignalToNoiseThreshold;
                }
                else if (_intensityThreshold != 0)
                {
                    backgroundIntensity = _intensityThreshold;
                }
                else
                {
                    backgroundIntensity = 1;
                }

            }


            //Find start index in raw data
            int startIndex = MathUtils.GetClosest(xvalues, minXValue);

            //Find stop index in raw data
            int stopIndex = MathUtils.GetClosest(xvalues, maxXValue);

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


            for (int index = startIndex; index <= stopIndex; index++)
            {
               // double fwhm = -1;
                double currentIntensity = yvalues[index];
                double lastIntensity = yvalues[index - 1];
                double nextIntensity = yvalues[index + 1];

                switch (RawDataType)
                {
                    case Globals.RawDataType.Profile:

                        bool peakApexFound = currentIntensity >= lastIntensity &&
                            currentIntensity >= nextIntensity;


                        double signalToNoise = -1;

                        if (peakApexFound && currentIntensity > _intensityThreshold)
                        {

                            if (IsDataThresholded)
                            {
                                signalToNoise = currentIntensity / backgroundIntensity;
                            }
                            else
                            {
                                signalToNoise = CalculateSignalToNoise(yvalues, index);
                            }

                        }

                        if (signalToNoise > SignalToNoiseThreshold)
                        {
                            var peak = new Peak();

                            double calculatedXValue = CalculateFittedValue(xvalues, yvalues, index);

                            double width = CalculateFWHM(xvalues, yvalues, index, signalToNoise);

                            peak.DataIndex = index;
                            peak.XValue = calculatedXValue;
                            peak.Width = (float)width;
                            peak.Height = (float)currentIntensity;

                            peakList.Add(peak);

                        }


                        break;
                    case Globals.RawDataType.Centroided:
                        // raw data is in centroided form. We simply convert each xy datapoint to a peak

                        if (currentIntensity >= _intensityThreshold)
                        {
                            Peak peak = new Peak();
                            peak.DataIndex = index;
                            peak.XValue = xvalues[index];
                            peak.Height = (float)yvalues[index];
                            peak.Width = 0;

                            peakList.Add(peak);

                        }


                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return peakList;



        }

        public double GetCurrentBackgroundIntensity()
        {
            return _intensityThreshold;
        }

        private double GetBackgroundIntensity(double[] yvalues)
        {
            double thresholdMultiplier = 5;

            double firstAverage = GetAverageIntensity(yvalues);

            double secondAverage = GetAverageIntensity(yvalues, firstAverage * thresholdMultiplier);

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

            int numPoints = xvalues.Length;
            if (index>=numPoints || index <0)
            {
                throw new ArgumentOutOfRangeException(
                    "Trying to calculate peak width, but index of peak apex was out of range.");
            }

            
            double peakApexIntensity = yvalues[index];
            double halfHeightIntensity = peakApexIntensity / 2;

            double x1, x2, y1, y2;

            


            double interpolatedX1 = 0;
            double interpolatedX2 = 0;

            //Moving to the left of the peak apex, 
            //will find the first point whose intensity is below the peakHalf
            for (int i = index; i >= 0; i--)
            {
                y1 = yvalues[i];

                if (y1 >= halfHeightIntensity) continue;

                x1 = xvalues[i];
                x2 = xvalues[i + 1];
                y2 = yvalues[i + 1];

                if ((y2 - y1) != 0)
                {
                    double slope = (y2 - y1) / (x2 - x1);
                    double yintercept = y1 - (slope * x1);

                    interpolatedX1 = (halfHeightIntensity - yintercept)/slope;
                    break;
                }
            }

            //moving to the right of the peak apex
            for (int i = index; i < numPoints; i++)
            {
                y2 = yvalues[i];

                if (y2 >= halfHeightIntensity) continue;

                x1 = xvalues[i -1];
                y1 = yvalues[i-1];

                x2 = xvalues[i];

                if ((y2-y1)!=0)
                {
                    double slope = (y2 - y1) / (x2 - x1);
                    double yintercept = y1 - (slope * x1);

                    interpolatedX2 = (halfHeightIntensity - yintercept) / slope;
                    break;
                }
                

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
                    throw new NotImplementedException();
                    
                case Globals.PeakFitType.LORENTZIAN:
                    throw new NotImplementedException();

                    
                case Globals.PeakFitType.QUADRATIC:

                    return CalculateQuadraticFittedValue(xvalues, yvalues, index);

                    
                default:
                    throw new ArgumentOutOfRangeException();
            }


        }

        private double CalculateQuadraticFittedValue(double[] xvalues, double[] yvalues, int index)
        {
            if (index < 1)
                return xvalues[0];
            if (index >= xvalues.Length - 1)
                return xvalues.Last();

            double x1 = xvalues[index - 1];
            double x2 = xvalues[index];
            double x3 = xvalues[index + 1];
            double y1 = yvalues[index - 1];
            double y2 = yvalues[index];
            double y3 = yvalues[index + 1];

            double calculatedVal = (y2 - y1) * (x3 - x2) - (y3 - y2) * (x2 - x1);
            if (calculatedVal == 0)
            {
                return x2; // no good.  Just return the known peak
            }

            calculatedVal = ((x1 + x2) - ((y2 - y1) * (x3 - x2) * (x1 - x3)) / calculatedVal) / 2.0;
            return calculatedVal; // Calculated new peak.  Return it.
        }

        private double GetAverageIntensity(double[] intensities, double maxIntensity = double.MaxValue)
        {
            int numPoints = intensities.Length;
            if (numPoints == 0)
                return 0;

            double backgroundIntensity = 0;
            int numPointsUsed = 0;
            for (int i = 0; i < numPoints; i++)
            {
                if (intensities[i] <= maxIntensity && intensities[i] != 0)
                {
                    backgroundIntensity += intensities[i];
                    numPointsUsed++;
                }
            }
            return backgroundIntensity / numPointsUsed;
        }

        private double CalculateSignalToNoise(double[] yvalues, int index)
        {
            throw new NotImplementedException();
        }

        #endregion

     

    }
}
