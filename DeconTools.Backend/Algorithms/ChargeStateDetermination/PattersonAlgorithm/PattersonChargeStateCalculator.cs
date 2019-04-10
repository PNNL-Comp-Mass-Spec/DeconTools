using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm
{
    public static class PattersonChargeStateCalculator
    {
        private const int MaxCharge = 25;

        #region Public Methods
        public static int GetChargeState(XYData rawData, List<Peak> peakList, MSPeak peak)
        {

            //look in rawData to the left (-0.1) and right (+1.1) of peak
            var minus = 0.1;
            var plus = 1.1;

            double fwhm = peak.Width;


            var leftIndex = MathUtils.GetClosest(rawData.Xvalues, peak.XValue - fwhm - minus);
            var rightIndex = MathUtils.GetClosest(rawData.Xvalues, peak.XValue + fwhm + plus);



            var filteredXYData = getFilteredXYData(rawData, leftIndex, rightIndex);
            var minMZ = filteredXYData.Xvalues[0];
            var maxMZ = filteredXYData.Xvalues[filteredXYData.Xvalues.Length - 1];

            double sumOfDiffsBetweenValues = 0;
            double pointCounter = 0;

            for (var i = 0; i < filteredXYData.Xvalues.Length - 1; i++)
            {
                var y1 = filteredXYData.Yvalues[i];
                var y2 = filteredXYData.Yvalues[i + 1];

                if (y1 > 0 && y2 > 0)
                {
                    var x1 = filteredXYData.Xvalues[i];
                    var x2 = filteredXYData.Xvalues[i + 1];

                    sumOfDiffsBetweenValues += (x2 - x1);
                    pointCounter++;
                }
            }




            int numL;
            if (pointCounter > 5)
            {
                var averageDiffBetweenPoints = sumOfDiffsBetweenValues / pointCounter;

                numL = (int)Math.Ceiling((maxMZ - minMZ) / averageDiffBetweenPoints);

                numL = (int)(numL + numL * 0.1);
                //numL = 445;
            }
            else
            {
                var numPoints = rightIndex - leftIndex + 1;

                var desiredNumPoints = 256;

                var pointMultiplier = (int)Math.Ceiling(desiredNumPoints / (double)numPoints);

                if (numPoints < 5)
                    return -1;


                if (numPoints < desiredNumPoints)
                {
                    pointMultiplier = Math.Max(5, pointMultiplier);
                    numL = pointMultiplier * numPoints;
                }
                else
                {
                    numL = numPoints;
                }



            }

            //Console.WriteLine("Number of points in interpolated data= " + numL);


            var interpolationFunction = new alglib.spline1d.spline1dinterpolant();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            alglib.spline1d.spline1dbuildcubic(
                filteredXYData.Xvalues, filteredXYData.Yvalues, filteredXYData.Xvalues.Length,
                1, +1, 1, -1, interpolationFunction);

            sw.Stop();
            //Console.WriteLine("spline time = " + sw.ElapsedMilliseconds);

            // DisplayXYVals(filteredXYData);

            var evenlySpacedXYData = new XYData
            {
                Xvalues = new double[numL],
                Yvalues = new double[numL]
            };

            for (var i = 0; i < numL; i++)
            {
                var xVal = (minMZ + ((maxMZ - minMZ) * i) / numL);
                var yVal = alglib.spline1d.spline1dcalc(interpolationFunction, xVal);

                evenlySpacedXYData.Xvalues[i] = xVal;
                evenlySpacedXYData.Yvalues[i] = yVal;
            }


            //Console.WriteLine();
            //DisplayXYVals(evenlySpacedXYData);


            var autoCorrScores = ACss(evenlySpacedXYData.Yvalues);

            //var tempXYData = new XYData
            //{
            //    Xvalues = autoCorrScores,
            //    Yvalues = autoCorrScores
            //};

            // DisplayXYVals(tempXYData);


            var startingIndex = 0;
            while (startingIndex < numL - 1 && autoCorrScores[startingIndex] > autoCorrScores[startingIndex + 1])
            {
                startingIndex++;
            }

            double bestAutoCorrScore = -1;
            var bestChargeState = -1;


            GetHighestChargeStatePeak(minMZ, maxMZ, startingIndex, autoCorrScores, MaxCharge, ref bestAutoCorrScore, ref bestChargeState);

            if (bestChargeState == -1) return -1;

            var returnChargeStateVal = -1;

            var chargeStateList = new List<int>();
            GenerateChargeStateData(minMZ, maxMZ, startingIndex, autoCorrScores, MaxCharge, bestAutoCorrScore, chargeStateList);

            for (var i = 0; i < chargeStateList.Count; i++)
            {
                var tempChargeState = chargeStateList[i];
                var skip = false;
                for (var j = 0; j < i; j++)
                {
                    if (chargeStateList[j] == tempChargeState)
                    {
                        skip = true;
                        break;
                    }

                }

                if (skip) continue;

                if (tempChargeState > 0)
                {
                    var anotherPeak = peak.XValue + (1.003d / tempChargeState);

                    var foundPeak = PeakUtilities.GetPeaksWithinTolerance(peakList, anotherPeak, peak.Width).Count > 0;
                    if (foundPeak)
                    {
                        returnChargeStateVal = tempChargeState;
                        if (peak.XValue * tempChargeState < 3000)
                        {
                            break;
                        }

                        return tempChargeState;
                    }

                }

            }

            return returnChargeStateVal;




            //StringBuilder sb = new StringBuilder();
            //foreach (var item in chargeStatesAndScores)
            //{
            //    sb.Append(item.Key + "\t" + item.Value + "\n");


            //}
            //Console.WriteLine(sb.ToString());




            //StringBuilder sb = new StringBuilder();
            //foreach (var item in autoCorrScores)
            //{
            //    sb.Append(item);
            //    sb.Append(Environment.NewLine);

            //}
            //Console.WriteLine(sb.ToString());

            //then extract AutoCorrelation scores (ACss)

            //determine highest charge state peak (?)

        }

        #endregion

        #region Private Methods


        private static double[] ACss(IReadOnlyList<double> inData)
        {


            var numPoints = inData.Count;
            var outData = new double[numPoints];


            double sum = 0;

            for (var i = 0; i < numPoints; i++)
            {
                sum += inData[i];

            }

            var average = sum / numPoints;

            for (var i = 0; i < numPoints; i++)
            {
                sum = 0;

                var currentValue = (inData[i] - average);

                int j;
                for (j = 0; j < (numPoints - i - 1); j++)
                {
                    sum += (currentValue) * (inData[i + j] - average);
                    //sum += (inData[i] - average) * (inData[i + j] - average);
                }

                if (j > 0)
                {
                    outData[i] = (sum / numPoints);
                }



            }

            return outData;

        }

        private static void GenerateChargeStateData(
            double minMZ, double maxMZ,
            int startingIndex,
            IReadOnlyList<double> autoCorrScores,
            int maxCharge,
            double bestAutoCorrScore,
            ICollection<int> chargeStatesAndScores)
        {
            var wasGoingUp = false;

            var numPoints = autoCorrScores.Count;

            for (var i = startingIndex; i < numPoints; i++)
            {
                if (i < 2) continue;

                var goingUp = autoCorrScores[i] > autoCorrScores[i - 1];

                if (wasGoingUp && !goingUp)
                {
                    var currentChargeState = (numPoints / ((maxMZ - minMZ) * (i - 1)));


                    // var tempAutoCorrScore = autoCorrScores[i];
                    var currentAutoCorrScore = autoCorrScores[i - 1];

                    //Console.WriteLine(i+ "\tCurrent charge state=\t" + currentChargeState + "\tcurrent corr score= \t" +
                    //                  currentAutoCorrScore +"\tComparedCorrScore= \t"+tempAutoCorrScore);


                    if ((currentAutoCorrScore > bestAutoCorrScore * 0.1) && (currentChargeState < maxCharge))
                    {
                        var chargeState = (int)Math.Round(currentChargeState);
                        chargeStatesAndScores.Add(chargeState);
                    }
                }
                wasGoingUp = goingUp;

            }

        }



        private static void GetHighestChargeStatePeak(
            double minMZ, double maxMZ,
            int startingIndex,
            IReadOnlyList<double> autoCorrelationScores,
            int maxChargeState,
            ref double bestAutoCorrectionScore,
            ref int bestChargeState)
        {
            var wasGoingUp = false;

            var numPoints = autoCorrelationScores.Count;

            for (var i = startingIndex; i < numPoints; i++)
            {
                if (i < 2) continue;

                var goingUp = (autoCorrelationScores[i] - autoCorrelationScores[i - 1]) > 0;
                if (wasGoingUp && !goingUp)
                {
                    var chargeState = (int)(numPoints / ((maxMZ - minMZ) * (i - 1)) + 0.5);
                    var currentAutoCorrScore = autoCorrelationScores[i - 1];
                    if (Math.Abs(currentAutoCorrScore / autoCorrelationScores[0]) > 0.05 && chargeState <= maxChargeState)
                    {

                        if (Math.Abs(currentAutoCorrScore) > bestAutoCorrectionScore)
                        {
                            bestAutoCorrectionScore = Math.Abs(currentAutoCorrScore);
                            bestChargeState = chargeState;
                        }
                    }
                }

                wasGoingUp = goingUp;
            }
        }

        [Obsolete("Unused")]
        private static void DisplayXYVals(XYData xyData)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                sb.Append(xyData.Xvalues[i] + "\t" + xyData.Yvalues[i]);
                sb.Append(Environment.NewLine);

            }

            Console.WriteLine(sb.ToString());
        }

        private static XYData getFilteredXYData(XYData inputXYData, int leftIndex, int rightIndex)
        {
            var numPoints = rightIndex - leftIndex + 1;

            var outputXYData = new XYData
            {
                Xvalues = new double[numPoints],
                Yvalues = new double[numPoints]
            };

            for (var i = 0; i < numPoints; i++)
            {
                outputXYData.Xvalues[i] = inputXYData.Xvalues[leftIndex + i];
                outputXYData.Yvalues[i] = inputXYData.Yvalues[leftIndex + i];

            }

            return outputXYData;
        }

        #endregion
    }
}
