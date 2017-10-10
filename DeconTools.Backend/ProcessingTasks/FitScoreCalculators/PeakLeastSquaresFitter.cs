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



        public double GetFit(List<Peak> theorPeakList, List<Peak> observedPeakList, double minIntensityForScore, double toleranceInPPM)
        {
            const int numPeaksToTheLeftForScoring = 0;
            return GetFit(theorPeakList, observedPeakList, minIntensityForScore, toleranceInPPM, numPeaksToTheLeftForScoring, out var _);
        }

        public double GetFit(
            List<Peak> theorPeakList,
            List<Peak> observedPeakList,
            double minIntensityForScore,
            double toleranceInPPM,
            int numPeaksToTheLeftForScoring,
            out int ionCountUsed)
        {
            Utilities.IqLogger.IqLogger.SamPayneLog("Min Intensity For Scoring: " + minIntensityForScore);
            Utilities.IqLogger.IqLogger.SamPayneLog("PPM Tolerance: " + toleranceInPPM);

            ionCountUsed = 0;
            var theorIntensitiesUsedInCalc = new List<double>();
            var observedIntensitiesUsedInCalc = new List<double>();

            //first gather all the intensities from theor and obs peaks

            var maxTheorIntensity = double.MinValue;
            foreach (var peak in theorPeakList)
            {
                if (peak.Height > maxTheorIntensity)
                {
                    maxTheorIntensity = peak.Height;
                }
            }

            for (var index = 0; index < theorPeakList.Count; index++)
            {
                var peak = theorPeakList[index];

                var overrideMinIntensityCutoff = index < numPeaksToTheLeftForScoring;

                if (peak.Height > minIntensityForScore || overrideMinIntensityCutoff)
                {
                    theorIntensitiesUsedInCalc.Add(peak.Height);

                    Utilities.IqLogger.IqLogger.SamPayneLog("Theoretical Peak Selected!	Peak Height: " + peak.Height + " Peak X-Value: " + peak.XValue);

                    //find peak in obs data
                    var mzTolerance = toleranceInPPM*peak.XValue/1e6;
                    var foundPeaks = PeakUtilities.GetPeaksWithinTolerance(observedPeakList, peak.XValue, mzTolerance);

                    double obsIntensity;
                    if (foundPeaks.Count == 0)
                    {
                        Utilities.IqLogger.IqLogger.SamPayneLog("No Observed Peaks Found Within Tolerance" + Environment.NewLine);
                        obsIntensity = 0;
                    }
                    else if (foundPeaks.Count == 1)
                    {
                        obsIntensity = foundPeaks.First().Height;
                        Utilities.IqLogger.IqLogger.SamPayneLog("Observed Peak Selected!	Peak Height: " + foundPeaks[0].Height + " Peak X-Value " + foundPeaks[0].XValue + Environment.NewLine);
                    }
                    else
                    {
                        obsIntensity = foundPeaks.OrderByDescending(p => p.Height).First().Height;
                        Utilities.IqLogger.IqLogger.SamPayneLog("Observed Peak Selected!	Peak Height: " + foundPeaks[0].Height + " Peak X-Value " + foundPeaks[0].XValue + Environment.NewLine);
                    }

                    observedIntensitiesUsedInCalc.Add(obsIntensity);
                }
                else
                {
                    Utilities.IqLogger.IqLogger.SamPayneLog("Theoretical Peak Not Selected!	Peak Height: " + peak.Height + " Peak X-Value: " + peak.XValue + Environment.NewLine);
                }
            }

            //the minIntensityForScore is too high and no theor peaks qualified. This is bad. But we don't
            //want to throw errors here
            if (theorIntensitiesUsedInCalc.Count == 0)
            {
                Utilities.IqLogger.IqLogger.SamPayneLog("No peaks meet minIntensityForScore." + Environment.NewLine);
                return 1.0;
            }

            var maxObs = observedIntensitiesUsedInCalc.Max();
            if (Math.Abs(maxObs) < float.Epsilon) maxObs = double.PositiveInfinity;
            Utilities.IqLogger.IqLogger.SamPayneLog("Max Observed Intensity: " + maxObs);

            var normalizedObs = observedIntensitiesUsedInCalc.Select(p => p / maxObs).ToList();

            var maxTheor = theorIntensitiesUsedInCalc.Max();
            var normalizedTheo = theorIntensitiesUsedInCalc.Select(p => p / maxTheor).ToList();
            Utilities.IqLogger.IqLogger.SamPayneLog("Max Theoretical Intensity: " + maxTheor + Environment.NewLine);


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
            for (var i = 0; i < normalizedTheo.Count; i++)
            {
                var diff = normalizedObs[i] - normalizedTheo[i];

                sumSquareOfDiffs += (diff * diff);
                sumSquareOfTheor += (normalizedTheo[i]*normalizedTheo[i]);
                Utilities.IqLogger.IqLogger.SamPayneLog("Normalized Observed: " + normalizedObs[i]);
                Utilities.IqLogger.IqLogger.SamPayneLog("Normalized Theoretical: " + normalizedTheo[i]);
                Utilities.IqLogger.IqLogger.SamPayneLog("Iterator: " + i + " Sum of Squares Differences: " + sumSquareOfDiffs + " Sum of Squares Theoretical: " + sumSquareOfTheor + Environment.NewLine);
            }

            ionCountUsed = normalizedTheo.Count;

            var fitScore = sumSquareOfDiffs / sumSquareOfTheor;
            if (double.IsNaN(fitScore) || fitScore > 1)
            {
                fitScore = 1;
            }
            else
            {
                // Future possibility (considered in January 2014):
                // Normalize the fit score by the number of theoretical ions
                // fitScore /= ionCountUsed;
            }

            Utilities.IqLogger.IqLogger.SamPayneLog("Fit Score: " + fitScore + Environment.NewLine);
            return fitScore;
        }


        #endregion

        #region Private Methods

        #endregion

    }
}
