using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.FitScoreCalculators
{
    public class PeakLeastSquaresFitter:LeastSquaresFitter
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods


        
        public double GetFit(List<Peak>theorPeakList,List<Peak>observedPeakList, double minIntensityForScore, double toleranceInPPM)
        {
            

            List<double> theorIntensitiesUsedInCalc = new List<double>();
            var observedIntensitiesUsedInCalc = new List<double>();
           
            //first gather all the intensities from theor and obs peaks

            int indexMaxTheor = 0;
            double maxTheorIntensity = double.MinValue;
            for (int i = 0; i < theorPeakList.Count; i++)
            {
                if (theorPeakList[i].Height>maxTheorIntensity)
                {
                    maxTheorIntensity = theorPeakList[i].Height;
                    indexMaxTheor = i;

                }
            }

            for (int index = 0; index < theorPeakList.Count; index++)
            {
                var peak = theorPeakList[index];
                if (peak.Height < minIntensityForScore) continue;
                theorIntensitiesUsedInCalc.Add(peak.Height);

                //find peak in obs data
                double mzTolerance = toleranceInPPM*peak.XValue/1e6;
                var foundPeaks = PeakUtilities.GetPeaksWithinTolerance(observedPeakList, peak.XValue, mzTolerance);

                double obsIntensity;
                if (foundPeaks.Count == 0)
                {
                    obsIntensity = 0;
                }
                else if (foundPeaks.Count == 1)
                {
                    obsIntensity = foundPeaks.First().Height;
                }
                else
                {
                    obsIntensity = foundPeaks.OrderByDescending(p => p.Height).First().Height;
                }

                observedIntensitiesUsedInCalc.Add(obsIntensity);
            }

            //the minIntensityForScore is too high and no theor peaks qualified. This is bad. But we don't
            //want to throw errors here
            if (theorIntensitiesUsedInCalc.Count == 0) return 1.0;

            double maxObs = observedIntensitiesUsedInCalc.Max();
            if (Math.Abs(maxObs - 0) < float.Epsilon) maxObs = double.PositiveInfinity;

            List<double> normalizedObs = observedIntensitiesUsedInCalc.Select(p => p / maxObs).ToList();

            double maxTheor = theorIntensitiesUsedInCalc.Max();
            List<double> normalizedTheo = theorIntensitiesUsedInCalc.Select(p => p / maxTheor).ToList();


            //foreach (var val in normalizedObs)
            //{
            //    Console.WriteLine(val);
            //}

            //Console.WriteLine();
            //foreach (var val in normalizedTheo)
            //{
            //    Console.WriteLine(val);
            //}


            double sumSquareOfDiffs = 0;
            double sumSquareOfTheor = 0;
            for (int i = 0; i < normalizedTheo.Count; i++)
            {
                var diff = normalizedObs[i] - normalizedTheo[i];

                sumSquareOfDiffs += (diff*diff);
                sumSquareOfTheor += (normalizedTheo[i]*normalizedTheo[i]);

            }

            double fitScore = sumSquareOfDiffs/sumSquareOfTheor;

            return fitScore;
        }


        #endregion

        #region Private Methods

        #endregion

    }
}
