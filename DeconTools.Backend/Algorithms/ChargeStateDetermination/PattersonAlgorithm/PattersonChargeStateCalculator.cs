using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using Task = DeconTools.Backend.Core.Task;

namespace DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm
{
    public class PattersonChargeStateCalculator
    {
        private int _maxCharge;
        #region Constructors

        public PattersonChargeStateCalculator()
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
            double minus = 0.1;
            double plus = 1.1;

            double fwhm = peak.Width;


            int leftIndex = MathUtils.GetClosest(rawData.Xvalues, peak.XValue - fwhm - minus);
            int rightIndex = MathUtils.GetClosest(rawData.Xvalues, peak.XValue + fwhm + plus);
         
           

            XYData filteredXYData = getFilteredXYData(rawData, leftIndex, rightIndex);
            double minMZ = filteredXYData.Xvalues[0];
            double maxMZ = filteredXYData.Xvalues[filteredXYData.Xvalues.Length - 1];

            double sumOfDiffsBetweenValues = 0;
            double pointCounter = 0;
            
            for (int i = 0; i < filteredXYData.Xvalues.Length-1; i++)
            {
                var y1 = filteredXYData.Yvalues[i];
                var y2 = filteredXYData.Yvalues[i + 1];
                
                if ( y1>0 && y2>0)
                {
                    var x1 = filteredXYData.Xvalues[i];
                    var x2 = filteredXYData.Xvalues[i + 1];

                    sumOfDiffsBetweenValues += (x2 - x1);
                    pointCounter++;
                }
            }


            

            int numL;
            if (pointCounter>5)
            {
                double averageDiffBetweenPoints = sumOfDiffsBetweenValues / pointCounter;

                numL = (int) Math.Ceiling((maxMZ - minMZ)/averageDiffBetweenPoints);

                numL = (int) (numL +numL*0.1);
                //numL = 445;
            }
            else
            {
                int numPoints = rightIndex - leftIndex + 1;

                int desiredNumPoints = 256;

                int pointMultiplier = (int)Math.Ceiling(desiredNumPoints / (double)numPoints);

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


            

            alglib.spline1d.spline1dinterpolant interpolant = new alglib.spline1d.spline1dinterpolant();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            alglib.spline1d.spline1dbuildcubic(filteredXYData.Xvalues, filteredXYData.Yvalues, filteredXYData.Xvalues.Length, 1, +1, 1, -1, interpolant);

            sw.Stop();
            //Console.WriteLine("spline time = " + sw.ElapsedMilliseconds);

            

           // DisplayXYVals(filteredXYData);

            XYData evenlySpacedXYData = new XYData();
            evenlySpacedXYData.Xvalues = new double[numL];
            evenlySpacedXYData.Yvalues = new double[numL];

            for (int i = 0; i < numL; i++)
            {
                double xval = (minMZ + ((maxMZ - minMZ) * i) / numL);
                double yval = alglib.spline1d.spline1dcalc(interpolant, xval);

                evenlySpacedXYData.Xvalues[i] = xval;
                evenlySpacedXYData.Yvalues[i] = yval;
            }


            

            //Console.WriteLine();
            //DisplayXYVals(evenlySpacedXYData);


            double[] autoCorrScores = ACss(evenlySpacedXYData.Yvalues);

            XYData tempXYdata = new XYData();
            tempXYdata.Xvalues = autoCorrScores;
            tempXYdata.Yvalues = autoCorrScores;

           // DisplayXYVals(tempXYdata);


            int startingIndex = 0;
            while (startingIndex < numL - 1 && autoCorrScores[startingIndex] > autoCorrScores[startingIndex + 1])
            {
                startingIndex++;
            }

            double bestAutoCorrScore = -1;
            int bestChargeState = -1;


            GetHighestChargeStatePeak(minMZ, maxMZ, startingIndex, autoCorrScores, _maxCharge, ref bestAutoCorrScore, ref bestChargeState);

            if (bestChargeState == -1) return -1;

            int returnChargeStateVal = -1;

            List<int> chargeStateList = new List<int>();
            GenerateChargeStateData(minMZ, maxMZ, startingIndex, autoCorrScores, _maxCharge, bestAutoCorrScore, chargeStateList);

            for (int i = 0; i < chargeStateList.Count; i++)
            {
                int tempChargeState = chargeStateList[i];
                bool skip = false;
                for (int j = 0; j < i; j++)
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
                    double anotherPeak = peak.XValue + (1.003d / tempChargeState);

                    bool foundPeak = PeakUtilities.GetPeaksWithinTolerance(peakList, anotherPeak, peak.Width).Count > 0;
                    if (foundPeak)
                    {
                        returnChargeStateVal = tempChargeState;
                        if (peak.XValue * tempChargeState < 3000)
                        {
                            break;
                        }
                        else
                        {
                            return tempChargeState;
                        }


                    }
                    else
                    {


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
            

            int numPoints = inData.Length;
            double[] outData = new double[numPoints];


            double sum = 0;
            double average = 0;

            for (int i = 0; i < numPoints; i++)
            {
                sum += inData[i];

            }

            average = sum / numPoints;

            sum = 0;
            for (int i = 0; i < numPoints; i++)
            {
                sum = 0;

                double currentValue = (inData[i] - average);

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

    
        private void GenerateChargeStateData(double minMZ, double maxMZ, int startingIndex, double[] autoCorrScores, int _maxCharge, double bestAutoCorrScore, List<int> chargeStatesAndScores)
        {
            bool goingUp = false;
            bool wasGoingUp = false;

            int chargeState = -1;
            int numPoints = autoCorrScores.Length;

            for (int i = startingIndex; i < numPoints; i++)
            {
                if (i < 2) continue;

                goingUp = autoCorrScores[i] > autoCorrScores[i - 1];




                if (wasGoingUp && !goingUp)
                {
                    double currentChargeState = (numPoints / ((maxMZ - minMZ) * (i - 1)) );


                    double tempAutocorrScore = autoCorrScores[i];
                    double currentAutoCorrScore = autoCorrScores[i - 1];

                    //Console.WriteLine(i+ "\tCurrent charge state=\t" + currentChargeState + "\tcurrent corr score= \t" + 
                    //                  currentAutoCorrScore +"\tComparedCorrScore= \t"+tempAutocorrScore);


                    if ((currentAutoCorrScore > bestAutoCorrScore * 0.1) && (currentChargeState < _maxCharge))
                    {
                        chargeState = (int) Math.Round(currentChargeState);
                        chargeStatesAndScores.Add(chargeState);
                        
                    }
                }
                wasGoingUp = goingUp;
                
            }

        }



        private void GetHighestChargeStatePeak(double minMZ, double maxMZ, int startingIndex, double[] autoCorrelationScores, int maxChargeState, ref double bestAutoCorrectionScore, ref int bestChargeState)
        {
            bool goingUp = false;
            bool wasGoingUp = false;

            int numPoints = autoCorrelationScores.Length;

            int chargeState;
            for (int i = startingIndex; i < numPoints; i++)
            {
                if (i < 2) continue;

                goingUp = (autoCorrelationScores[i] - autoCorrelationScores[i - 1]) > 0;
                if (wasGoingUp && !goingUp)
                {
                    chargeState = (int)(numPoints / ((maxMZ - minMZ) * (i - 1)) + 0.5);
                    double currentAutoCorrScore = autoCorrelationScores[i - 1];
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

        private void DisplayXYVals(XYData xydata)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < xydata.Xvalues.Length; i++)
            {
                sb.Append(xydata.Xvalues[i] + "\t" + xydata.Yvalues[i]);
                sb.Append(Environment.NewLine);

            }

            Console.WriteLine(sb.ToString());
        }

        private XYData getFilteredXYData(XYData inputXYData, int leftIndex, int rightIndex)
        {
            int numPoints = rightIndex - leftIndex + 1;

            XYData outputXYData = new XYData();


            outputXYData.Xvalues = new double[numPoints];
            outputXYData.Yvalues = new double[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                outputXYData.Xvalues[i] = inputXYData.Xvalues[leftIndex + i];
                outputXYData.Yvalues[i] = inputXYData.Yvalues[leftIndex + i];

            }

            return outputXYData;
        }



        #endregion
    }
}
