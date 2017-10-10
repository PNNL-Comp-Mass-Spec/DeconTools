using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using Task = DeconTools.Backend.Core.Task;

namespace DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm
{

    /// <summary>
    /// This class is very similar to the PattersonChargeStateCalculator. Paul Kline (SULI intern) made some changes (Aug 2013) and wanted to
    /// temporarily store them here until they are tested and incorporated in the official version. 
    /// </summary>
    public class PattersonChargeStateCalculatorWithChanges
    {
        private int _maxCharge;
        #region Constructors

        public PattersonChargeStateCalculatorWithChanges()
        {
            _maxCharge = 25;
        }

        #endregion

        #region Properties
       
        #endregion

        #region Public Methods
        public int GetChargeState(XYData rawData, List<Peak> peakList, MSPeak peak)
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

            //paul edit
            var numPoints = rightIndex - leftIndex;
            var numL = numPoints;
            if (numPoints<5)
            {
                return -1;
            }
            if (numPoints<256)
            {
                numL = 10 * numPoints;
            }
            //double sumOfDiffsBetweenValues = 0;
            //double pointCounter = 0;
            
            //for (int i = 0; i < filteredXYData.Xvalues.Length-1; i++)
            //{
            //    var y1 = filteredXYData.Yvalues[i];
            //    var y2 = filteredXYData.Yvalues[i + 1];
                
            //    if ( y1>0 && y2>0)
            //    {
            //        var x1 = filteredXYData.Xvalues[i];
            //        var x2 = filteredXYData.Xvalues[i + 1];

            //        sumOfDiffsBetweenValues += (x2 - x1);
            //        pointCounter++;
            //    }
            //}


            

            //int numL;
            //if (pointCounter>5)
            //{
            //    double averageDiffBetweenPoints = sumOfDiffsBetweenValues / pointCounter;

            //    numL = (int) Math.Ceiling((maxMZ - minMZ)/averageDiffBetweenPoints);

            //    numL = (int) (numL +numL*0.1);
            //    //numL = 445;
            //}
            //else
            //{
            //    int numPoints = rightIndex - leftIndex + 1;

            //    int desiredNumPoints = 256;

            //    int pointMultiplier = (int)Math.Ceiling(desiredNumPoints / (double)numPoints);

            //    if (numPoints < 5)
            //        return -1;

                
            //    if (numPoints < desiredNumPoints)
            //    {
            //        pointMultiplier = Math.Max(5, pointMultiplier);
            //        numL = pointMultiplier * numPoints;
            //    }
            //    else
            //    {
            //        numL = numPoints;
            //    }


                
           // }

             //Console.WriteLine("Number of points in interpolated data= " + numL);


            

            var interpolant = new alglib.spline1d.spline1dinterpolant();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            alglib.spline1d.spline1dbuildcubic(filteredXYData.Xvalues, filteredXYData.Yvalues, filteredXYData.Xvalues.Length, 1, +1, 1, -1, interpolant);

            sw.Stop();
            //Console.WriteLine("spline time = " + sw.ElapsedMilliseconds);

            

           // DisplayXYVals(filteredXYData);

            var evenlySpacedXYData = new XYData();
            evenlySpacedXYData.Xvalues = new double[numL];
            evenlySpacedXYData.Yvalues = new double[numL];

            for (var i = 0; i < numL; i++)
            {
                var xval = (minMZ + ((maxMZ - minMZ) * i) / numL);
                var yval = alglib.spline1d.spline1dcalc(interpolant, xval);

                evenlySpacedXYData.Xvalues[i] = xval;
                evenlySpacedXYData.Yvalues[i] = yval;
            }


            

            //Console.WriteLine();
            //DisplayXYVals(evenlySpacedXYData);


            var autoCorrScores = ACss(evenlySpacedXYData.Yvalues);

            var tempXYdata = new XYData();
            tempXYdata.Xvalues = autoCorrScores;
            tempXYdata.Yvalues = autoCorrScores;

           // DisplayXYVals(tempXYdata);


            var startingIndex = 0;
            while (startingIndex < numL - 1 && autoCorrScores[startingIndex] > autoCorrScores[startingIndex + 1])
            {
                startingIndex++;
            }

            var bestAutoCorrScore = -1.0;
            var bestChargeState = -1;


            GetHighestChargeStatePeak(minMZ, maxMZ, startingIndex, autoCorrScores, _maxCharge, ref bestAutoCorrScore, ref bestChargeState);

            if (bestChargeState == -1) return -1;

            var returnChargeStateVal = -1;

            var chargeStateList = new List<int>();
            GenerateChargeStateData(minMZ, maxMZ, startingIndex, autoCorrScores, _maxCharge, bestAutoCorrScore, chargeStateList);

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
                    //CHANGE
                    var anotherPeak = peak.XValue + 1.0 / (double)tempChargeState;//(1.003d / tempChargeState);//paul edit

                    var foundPeak = PeakUtilities.GetPeaksWithinTolerance(peakList, anotherPeak, peak.Width).Count > 0;
                    if (foundPeak)
                    {
                        returnChargeStateVal = tempChargeState;                       
                        if (peak.XValue * tempChargeState < 3000)
                        {
                            break;
                        }
                        else
                        {
                            //CHANGE
                            //paul edit. this c# version was just "return tempChargeState;" inside else.
                            var peakA = peak.XValue - (1.03 / (double)tempChargeState);
                            foundPeak = PeakUtilities.GetPeaksWithinTolerance(peakList, peakA, peak.Width).Count > 0;
                            if (foundPeak)
                            {
                                return tempChargeState;
                            }
                            
                        }


                    }
                    else
                    {
                        var peakA = peak.XValue - (1.0 / (double)tempChargeState);
                        foundPeak = PeakUtilities.GetPeaksWithinTolerance(peakList, peakA, peak.Width).Count > 0;
                        if (foundPeak && PeakUtilities.GetPeaksWithinTolerance(peakList, peakA, peak.Width).First().XValue*((double)tempChargeState)<3000)
                        {
                            return tempChargeState;
                        }    


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

            //then extract Autocorrelation scores (ACss)

            //determine highest charge state peak (?)
            
        }

        #endregion

        #region Private Methods


        private double[] ACss(double[] inData)
        {
            

            var numPoints = inData.Length;
            var outData = new double[numPoints];


            double sum = 0;
            double average = 0;

            for (var i = 0; i < numPoints; i++)
            {
                sum += inData[i];

            }

            average = sum / numPoints;

            sum = 0;
            for (var i = 0; i < numPoints; i++)
            {
                sum = 0;

                //double currentValue = (inData[i] - average);

                int j;
                for (j = 0; j < (numPoints - i - 1); j++)
                {
                    //CHANGE POSSIBLY IMPORTANT CHANGE. cpp version uses inData[j] instead of inData[i]. 
                    //sum += (currentValue) * (inData[i + j] - average);
                    sum += (inData[j] - average) * (inData[i + j] - average);
                }

                if (j > 0)//I[Paul] don't see why this check needs to happen.
                {
                    outData[i] = (sum / numPoints);
                }
                


            }
            //for (int i = 0; i < outData.Length; i++)
            //{
            //    Console.WriteLine(outData[i]);
                
            //}
            return outData;

        }

    
        private void GenerateChargeStateData(double minMZ, double maxMZ, int startingIndex, double[] autoCorrScores, int _maxCharge, double bestAutoCorrScore, List<int> chargeStatesAndScores)
        {
            var goingUp = false;
            var wasGoingUp = false;

            var chargeState = -1;
            var numPoints = autoCorrScores.Length;

            for (var i = startingIndex; i < numPoints; i++)
            {
                if (i < 2) continue;

                goingUp = autoCorrScores[i] > autoCorrScores[i - 1];




                if (wasGoingUp && !goingUp)
                {
                    var currentChargeState = (numPoints / ((maxMZ - minMZ) * (i - 1)) );


                    var tempAutocorrScore = autoCorrScores[i];
                    var currentAutoCorrScore = autoCorrScores[i - 1];

                    //Console.WriteLine(i+ "\tCurrent charge state=\t" + currentChargeState + "\tcurrent corr score= \t" + 
                    //                  currentAutoCorrScore +"\tComparedCorrScore= \t"+tempAutocorrScore);


                    if ((currentAutoCorrScore > bestAutoCorrScore * 0.1) && (currentChargeState < 1.0*_maxCharge))
                    {
                        chargeState =(int)( .5 + currentChargeState);//(int) Math.Round(currentChargeState);
                        chargeStatesAndScores.Add(chargeState);
                       // Console.WriteLine("charge state added to list: " + chargeState);
                    }
                }
                wasGoingUp = goingUp;
                
            }

        }



        private void GetHighestChargeStatePeak(double minMZ, double maxMZ, int startingIndex, double[] autoCorrelationScores, int maxChargeState, ref double bestAutoCorrectionScore, ref int bestChargeState)
        {
            var goingUp = false;
            var wasGoingUp = false;

            var numPoints = autoCorrelationScores.Length;

            int chargeState;
            for (var i = startingIndex; i < numPoints; i++)
            {
                if (i < 2) continue;

                goingUp = (autoCorrelationScores[i] - autoCorrelationScores[i - 1]) > 0;
                if (wasGoingUp && !goingUp)
                {
                    chargeState = (int)(numPoints / ((maxMZ - minMZ) * (i - 1)) + 0.5);
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
        private void DisplayXYVals(XYData xydata)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < xydata.Xvalues.Length; i++)
            {
                sb.Append(xydata.Xvalues[i] + "\t" + xydata.Yvalues[i]);
                sb.Append(Environment.NewLine);

            }

            Console.WriteLine(sb.ToString());
        }

        private XYData getFilteredXYData(XYData inputXYData, int leftIndex, int rightIndex)
        {
            var numPoints = rightIndex - leftIndex + 1;

            var outputXYData = new XYData();


            outputXYData.Xvalues = new double[numPoints];
            outputXYData.Yvalues = new double[numPoints];

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
