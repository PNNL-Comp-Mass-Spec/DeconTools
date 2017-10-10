using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Utilities;

using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.FitScoreCalculators
{
    /// <summary>
    /// Does a least-squares fit.  
    /// </summary>
    public class AreaFitter:LeastSquaresFitter
    {


        public double GetFit(XYData theorXYData, XYData observedXYData, double minIntensityForScore, double offset = 0)
        {
            return GetFit(theorXYData, observedXYData, minIntensityForScore, out var ionCountUsed, offset);
        }

        public double GetFit(XYData theorXYData, XYData observedXYData, double minIntensityForScore, out int ionCountUsed, double offset = 0)
        {
            Check.Require(theorXYData != null && theorXYData.Xvalues != null && theorXYData.Yvalues != null,
                "AreaFitter failed. Theoretical XY data is null");

            Check.Require(observedXYData != null & observedXYData.Xvalues != null && observedXYData.Yvalues != null,
                "AreaFitter failed. Observed XY data is null");
            //Check.Require(minIntensityForScore >= 0 && minIntensityForScore <= 100, "MinIntensityForScore should be between 0 and 100");


            ionCountUsed = 0;

            double sumOfSquaredDiff = 0;
            double sumOfSquaredTheorIntens = 0;
            var xmin = theorXYData.Xvalues[0] + offset;
            var xmax = theorXYData.Xvalues.Max() + offset;


            var trimmedObservedXYData = observedXYData.TrimData(xmin, xmax);
            //XYData trimmedObservedXYData = observedXYData;
            
            //trimmedObservedXYData.Display();
            //theorXYData.Display();

            var interpolatedValues = new List<double>();
            var theoreticalValues = new List<double>();


            for (var i = 0; i < theorXYData.Xvalues.Length; i++)
            {

                if (theorXYData.Yvalues[i] >= minIntensityForScore)
                {
                    var currentTheorMZ = theorXYData.Xvalues[i] + offset;

                    var indexOfClosest = MathUtils.GetClosest(trimmedObservedXYData.Xvalues, currentTheorMZ, 0.1);

                    if (indexOfClosest == -1)
                    {
                        //Console.WriteLine(i + "\t" + currentTheorMZ);
                        return 1;
                    }

                    //findout if closest is above or below
                    var closestIsBelow = (trimmedObservedXYData.Xvalues[indexOfClosest] < currentTheorMZ);

                    double mz1 = 0;
                    double mz2 = 0;
                    double intensity1 = 0;
                    double intensity2 = 0;

                    if (closestIsBelow)
                    {
                        mz1 = trimmedObservedXYData.Xvalues[indexOfClosest];
                        intensity1 = trimmedObservedXYData.Yvalues[indexOfClosest];
                        if (indexOfClosest == trimmedObservedXYData.Xvalues.Length - 1)   //if at the end of the XY array; this should be very rare
                        {
                            mz2 = mz1;
                            intensity2 = intensity1;
                        }
                        else
                        {
                            mz2 = trimmedObservedXYData.Xvalues[indexOfClosest + 1];
                            intensity2 = trimmedObservedXYData.Yvalues[indexOfClosest + 1];
                        }


                    }
                    else  // closest point is above the targetMZ; so get the mz of the point below
                    {
                        mz2 = trimmedObservedXYData.Xvalues[indexOfClosest];
                        intensity2 = trimmedObservedXYData.Yvalues[indexOfClosest];
                        if (indexOfClosest == 0)        //if at the beginning of the XY array  (rare)
                        {
                            mz1 = mz2;
                            intensity1 = intensity2;
                        }
                        else
                        {
                            mz1 = trimmedObservedXYData.Xvalues[indexOfClosest - 1];
                            intensity1 = trimmedObservedXYData.Yvalues[indexOfClosest - 1];
                        }
                    }


                    var interopolatedIntensity = MathUtils.getInterpolatedValue(mz1, mz2, intensity1, intensity2, currentTheorMZ);

                    interpolatedValues.Add(interopolatedIntensity);
                    theoreticalValues.Add(theorXYData.Yvalues[i]);

                    //double normalizedObservedIntensity = interopolatedIntensity / maxIntensity;

                    //double normalizedTheorIntensity = theorXYData.Yvalues[i]/maxTheorIntensity;   

                    //double diff = normalizedTheorIntensity - normalizedObservedIntensity;

                    //sumOfSquaredDiff += diff * diff;

                    //sumOfSquaredTheorIntens += normalizedTheorIntensity * normalizedTheorIntensity;


                }


            }

            var maxTheoreticalIntens = getMax(theoreticalValues);
            var maxObservIntens = getMax(interpolatedValues);


            for (var i = 0; i < theoreticalValues.Count; i++)
            {
                var normalizedObservedIntensity = interpolatedValues[i] / maxObservIntens;

                var normalizedTheorIntensity = theoreticalValues[i] / maxTheoreticalIntens;

                var diff = normalizedTheorIntensity - normalizedObservedIntensity;

                sumOfSquaredDiff += diff * diff;

                sumOfSquaredTheorIntens += normalizedTheorIntensity * normalizedTheorIntensity;
            }

            if (theoreticalValues.Count == 0 || Math.Abs(sumOfSquaredTheorIntens) < double.Epsilon) 
                return -1;

            //StringBuilder sb = new StringBuilder();
            //TestUtilities.GetXYValuesToStringBuilder(sb, theoreticalValues.ToArray(), interpolatedValues.ToArray());
            //Console.Write(sb.ToString());

            var fitScore = sumOfSquaredDiff / sumOfSquaredTheorIntens;

            ionCountUsed = theoreticalValues.Count;

            // Future possibility (considered in January 2014):
            // Normalize the fit score by the number of theoretical ions
            // fitScore /= ionCountUsed;
            
            return fitScore;

        }

        private double getMax(List<double> values)
        {
            var max = double.MinValue;

            foreach (var d in values)
            {
                if (d > max) max = d;
            }

            return max;
        }








    }
}
