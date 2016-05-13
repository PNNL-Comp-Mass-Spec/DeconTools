using System;
using System.Collections.Generic;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ChargeDetermination
{
    /// <summary>
    /// Determine the charge state for a given peak, using context
    /// </summary>
    /// <seealso cref="DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm.PattersonChargeStateCalculator"/>
    public static class AutoCorrelationChargeDetermination
    {
        // might be too high a value.
        private const short MaxCharge = 25;

        /// <summary>
        ///     Calculate the autocorrelation values for the an input.
        /// </summary>
        /// <param name="iv">function for which we want to calculate autocorrelation.</param>
        /// <returns>autocorrelation values. ov[i] is the autocorrelation at distance i.</returns>
        /// <remarks>It is assumed that the x values for this function are equally spaced.</remarks>
        public static List<double> ACss(List<double> iv)
        {
            int i, j;
            var ivN = iv.Count;
            var ov = new List<double>(ivN);

            var ave = 0.0;
            for (j = 0; j < ivN; j++)
                ave += iv[j];
            ave = ave / ivN;
            //  for(i=0;i<IvN/2;i++)  GAA 09/27/03
            for (i = 0; i < ivN; i++)
            {
                var sum = 0.0;
                var topIndex = ivN - i - 1;
                for (j = 0; j < topIndex; j++)
                    sum += (iv[j] - ave) * (iv[j + i] - ave);

                if (j > 0)
                {
                    // too much weight given to high charges this way. DJ Jan 07 2007
                    ov.Add((float) (sum / ivN));
                    //Ov.Add(sum/j);
                }
                else
                    ov.Add(0);
            }
            return ov;
        }

        /// <summary>
        ///     Gets the charge state (determined by AutoCorrelation algorithm) for a peak in some data.
        /// </summary>
        /// <param name="peak">is the peak whose charge we want to detect.</param>
        /// <param name="peakData">is the PeakData object containing raw data, peaks, etc which are used in the process.</param>
        /// <returns>Returns the charge of the feature.</returns>
        public static short GetChargeState(ThrashV1Peak peak, PeakData peakData, bool debug)
        {
            var minus = 0.1;
            var plus = 1.1; // right direction to look
            var startIndex = PeakIndex.GetNearest(peakData.MzList, peak.Mz - peak.FWHM - minus, peak.DataIndex);
            var stopIndex = PeakIndex.GetNearest(peakData.MzList, peak.Mz + peak.FWHM + plus, peak.DataIndex);
            var numPts = stopIndex - startIndex;
            var numL = numPts;

            if (numPts < 5)
                return -1;

            if (numPts < 256)
                numL = 10 * numPts;

            // TODO: PattersonChargeStateCalculator does a lot of funny stuff around here.
            // List to temporarily store part of the raw data for which we want to do calculate the autocorrelation.
            // When performing autocorrelation to determine the charge we extract the points around the peak of interest
            // and calculate the autocorrelations for that stretch only. This variable stores the m/z values of these
            // points of interest.
            var xList = new List<double>(numPts);

            // List to temporarily store part of the raw data for which we want to do calculate the autocorrelation.
            // When performing autocorrelation to determine the charge we extract the points around the peak of interest
            // and calculate the autocorrelations for that stretch only. This variable stores the intensity values of these
            // points of interest.
            var yList = new List<double>(numPts);

            // odd behaviour / bug in vb code here .. should have started at start_index.
            for (var i = startIndex + 1; i <= stopIndex; i++)
            {
                var mz = peakData.MzList[i];
                var intensity = peakData.IntensityList[i];
                xList.Add(mz);
                yList.Add(intensity);
            }

            // variable to help us perform spline interpolation.
            var interpolation = new Interpolation();
            interpolation.Spline(xList, yList, 0, 0);

            var minMz = xList[0];
            var maxMz = xList[numPts - 1];

            // List to store the interpolated intensities of the region on which we performed the cubic spline interpolation.
            var iv = new List<double>(numL);
            for (var i = 0; i < numL; i++)
            {
                var xVal = minMz + (maxMz - minMz) * i / numL;
                var fVal = interpolation.Splint(xList, yList, xVal);
                iv.Add(fVal);
            }

            if (debug)
            {
                Console.Error.WriteLine("mz,intensity");
                for (var i = 0; i < numL; i++)
                {
                    var xVal = minMz + (maxMz - minMz) * i / numL;
                    Console.Error.WriteLine(xVal + "," + iv[i]);
                }
            }

            // List to store the autocorrelation values at the points in the region.
            var autocorrelationScores = ACss(iv);
            if (debug)
            {
                Console.Error.WriteLine("AutoCorrelation values");
                for (var i = 0; i < autocorrelationScores.Count; i++)
                {
                    var score = autocorrelationScores[i];
                    Console.Error.WriteLine((maxMz - minMz) * i / numL + "," + score);
                }
            }

            var minN = 0;
            while (minN < numL - 1 && autocorrelationScores[minN] > autocorrelationScores[minN + 1])
                minN++;

            // Determine the highest CS peak
            double bestAcScore;
            short bestChargeState;
            var success = HighestChargeStatePeak(minMz, maxMz, minN, autocorrelationScores, MaxCharge, out bestAcScore,
                out bestChargeState);

            if (!success)
                return -1; // Didn't find anything

            // List to temporarily store charge list. These charges are calculated at peak values of autocorrelation.
            // Now go back through the CS peaks and make a list of all CS that are at least 10% of the highest
            var charges = GenerateChargeStates(minMz, maxMz, minN, autocorrelationScores, MaxCharge, bestAcScore);

            // Get the final CS value to be returned
            short returnChargeStateVal = -1;
            // TODO: PattersonChargeStateCalculator really doesn't match the following code.
            var fwhm = peak.FWHM; // Store a copy of the FWHM to avoid modifying the actual value
            if (fwhm > 0.1)
                fwhm = 0.1;

            for (var i = 0; i < charges.Count; i++)
            {
                // no point retesting previous charge.
                var tempChargeState = charges[i];
                var skip = false;
                for (var j = 0; j < i; j++)
                {
                    if (charges[j] == tempChargeState)
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip)
                    continue;
                if (tempChargeState > 0)
                {
                    var peakA = peak.Mz + 1.0 / tempChargeState;
                    var found = true;
                    ThrashV1Peak isoPeak;
                    found = peakData.GetPeakFromAllOriginalIntensity(peakA - fwhm, peakA + fwhm, out isoPeak);
                    if (found)
                    {
                        returnChargeStateVal = tempChargeState;
                        if (isoPeak.Mz * tempChargeState < 3000)
                            break;
                        // if the mass is greater than 3000, lets make sure that multiple isotopes exist.
                        peakA = peak.Mz - 1.03 / tempChargeState;
                        found = peakData.GetPeakFromAllOriginalIntensity(peakA - fwhm, peakA + fwhm, out isoPeak);
                        if (found)
                        {
                            return tempChargeState;
                        }
                    }
                    else
                    {
                        peakA = peak.Mz - 1.0 / tempChargeState;
                        found = peakData.GetPeakFromAllOriginalIntensity(peakA - fwhm, peakA + fwhm, out isoPeak);
                        if (found && isoPeak.Mz * tempChargeState < 3000)
                        {
                            return tempChargeState;
                        }
                    }
                }
            }
            return returnChargeStateVal;
        }

        /// <summary>
        ///     Generates the list of possible charge states from the autocorrelation scores and the best score.
        /// </summary>
        /// <param name="minMz">minimum mz.</param>
        /// <param name="maxMz">maximum mz.</param>
        /// <param name="minN">the number of points at the start that we will skip in looking for high autocorrelation scores.</param>
        /// <param name="autocorrelationScores">List of autocorrelation scores.</param>
        /// <param name="maxChargeState">maximum charge state to look for.</param>
        /// <param name="bestAcScore">best autocorrelation score.</param>
        /// <returns>List of charge states generated.</returns>
        /// <remarks>
        ///     The list of charge states is generated by finding out what are the locally maximum autocorrelation scores and using
        ///     the corresponding
        ///     distance to figure out the charge. Obviously, a high autocorrleation score at 0.48 implies a charge of 2, at at
        ///     0.31 implies charge 3 etc.
        ///     Keep in mind that the autocorrelation score is not going to be perfectly at 0.5, or 0.33 etc, but approximately at
        ///     those points.
        /// </remarks>
        private static List<short> GenerateChargeStates(double minMz, double maxMz, int minN, List<double> autocorrelationScores,
            double maxChargeState, double bestAcScore)
        {
            var chargeStates = new List<short>();
            // Preparation...
            var wasGoingUp = false;

            var numPts = autocorrelationScores.Count;
            // First determine the highest CS peak...
            for (var i = minN; i < numPts; i++)
            {
                if (i < 2)
                    continue;
                bool goingUp = autocorrelationScores[i] > autocorrelationScores[i - 1];

                if (wasGoingUp && !goingUp)
                {
                    var chargeState = numPts / ((maxMz - minMz) * (i - 1));
                    var currentAutoCorScore = autocorrelationScores[i - 1];
                    // TODO: PattersonChargeStateCalculator uses chargeState < maxChargeState
                    if ((currentAutoCorScore > bestAcScore * 0.1) && (chargeState <= maxChargeState))
                    {
                        // Forcing round of midpoint values away from zero (at least for positive values)...
                        // The default for Math.Round is MidpointRounding.ToEven, which will round 2.5 exactly to 2
                        // We should instead use MidpointRounding.AwayFromZero, which will round 2.5 to 3.
                        // TODO: This is a difference compared to PattersonChargeStateCalculator
                        chargeStates.Add((short) (0.5 + chargeState));
                    }
                }
                wasGoingUp = goingUp;
            }
            return chargeStates;
        }

        /// <summary>
        ///     Gets the best autocorrelation score, and the charge state corresponding to the distance.
        /// </summary>
        /// <param name="minMz">minimum m/z value of the chosen range</param>
        /// <param name="maxMz">maximum m/z value of the chose range</param>
        /// <param name="minN">the number of points at the start that we will skip in looking for high autocorrelation scores.</param>
        /// <param name="autocorrelationScores">List of autocorrelation scores.</param>
        /// <param name="maxChargeState">maximum charge state that we are looking for.</param>
        /// <param name="bestAutoCorrelationScore">returns the best autocorrelation score, or -1 if none found.</param>
        /// <param name="bestChargeState">returns the charge state at the best autocorrelation score distance, or -1 if none found.</param>
        /// <returns>true is successful, false otherwise</returns>
        private static bool HighestChargeStatePeak(double minMz, double maxMz, int minN, List<double> autocorrelationScores,
            short maxChargeState, out double bestAutoCorrelationScore, out short bestChargeState)
        {
            bestAutoCorrelationScore = -1;
            bestChargeState = -1;
            var numPts = autocorrelationScores.Count;

            // Preparation...
            var wasGoingUp = false;

            // First determine the highest CS peak...
            for (var i = minN; i < numPts; i++)
            {
                if (i < 2)
                    continue;
                var goingUp = autocorrelationScores[i] - autocorrelationScores[i - 1] > 0;
                if (wasGoingUp && !goingUp)
                {
                    var chargeState = (short) (0.5 + numPts / ((maxMz - minMz) * (i - 1)));
                    var currentAutoCorScore = autocorrelationScores[i - 1];
                    if ((Math.Abs(currentAutoCorScore / autocorrelationScores[0]) > 0.05) && chargeState <= maxChargeState)
                    {
                        if (Math.Abs(currentAutoCorScore) > bestAutoCorrelationScore)
                        {
                            bestAutoCorrelationScore = Math.Abs(currentAutoCorScore);
                            bestChargeState = chargeState;
                        }
                    }
                }
                wasGoingUp = goingUp;
            }
            return !bestAutoCorrelationScore.Equals(-1.0);
        }
    }
}