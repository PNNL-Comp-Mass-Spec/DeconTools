using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IqLogger;

namespace DeconTools.Backend.ProcessingTasks.ChargeStateDeciders
{
    public class ChromCorrelatingChargeDecider : ChargeStateDecider
    {
        private Run _run;
        public ChromCorrelatingChargeDecider(Run run)
        {
            _run = run;
        }
        public override IsotopicProfile DetermineCorrectIsotopicProfile(List<IsotopicProfile> potentialIsotopicProfiles)
        {
            if (null == potentialIsotopicProfiles || potentialIsotopicProfiles.Count == 0) return null;
            if (_run.ResultCollection.MSPeakResultList.Count == 0)
            {
                _run = RunUtilities.CreateAndLoadPeaks(_run.Filename);
            }
            potentialIsotopicProfiles = potentialIsotopicProfiles.OrderByDescending(n => n.ChargeState).ToList();

            int[] chargeStates = (from prof in potentialIsotopicProfiles select prof.ChargeState).ToArray();                              
            double[] correlations = new double[chargeStates.Length];
            double[,] correlationswithAltChargeState = new double[chargeStates.Length, 3];
            int indexCurrentFeature = -1;

            double bestScore = -1;
            IsotopicProfile bestFeature = potentialIsotopicProfiles.First();
            foreach (var potentialfeature in potentialIsotopicProfiles)
            {
                indexCurrentFeature++;
                            
                double correlation= GetCorrelation(potentialfeature);

                int[] charges;
                switch (potentialfeature.ChargeState)
                {
                    case 1:
                        charges = new int[] { 2, 3, 4 };
                        break;
                    case 2:
                        charges = new int[] { 1, 3, 4 };
                        break;
                    case 3:
                        charges = new int[] { 2, 1, 4 };
                        break;
                    default:
                        charges = new int[3];
                        charges[0] = potentialfeature.ChargeState - 1;
                        charges[1] = potentialfeature.ChargeState - 2;
                        charges[2] = potentialfeature.ChargeState - 3;
                        break;
                }


                //int newChargeState = (potentialfeature.ChargeState == 1) ? potentialfeature.ChargeState + 1 : potentialfeature.ChargeState - 1;
                double altCorr = GetCorrelationWithAnotherChargeState(potentialfeature, charges[0]);
                
                double altCorr2 = GetCorrelationWithAnotherChargeState(potentialfeature, charges[1]);
                
                double altCorr3 = GetCorrelationWithAnotherChargeState(potentialfeature, charges[2]);
                correlationswithAltChargeState[indexCurrentFeature,0] = altCorr;
                correlationswithAltChargeState[indexCurrentFeature,1] = altCorr2;
                correlationswithAltChargeState[indexCurrentFeature,2] = altCorr3;
                string reportString= "\nchargeState: " + potentialfeature.ChargeState + "\n" +
                    "correlation value: " + correlation + "\n" +
                    "ALTERNATE CORRELATION 1 ? : " + altCorr + "\n" +
                    "ALTERNATE CORRELATION 2 ? : " + altCorr2 + "\n" +
                    "ALTERNATE CORRELATION 3 ? : " + altCorr3 + "\n" +
                    "Score: " + potentialfeature.Score;
                IqLogger.Log.Debug(reportString);

                correlations[indexCurrentFeature] = correlation;
                
                if (bestScore < correlation)
                {
                    bestScore = correlation;
                    bestFeature = potentialfeature;
                }
            }
            return GetIsotopicProfileMethod1(chargeStates, correlations, correlationswithAltChargeState, potentialIsotopicProfiles, bestFeature, bestScore);

        }

        private double GetCorrelationWithAnotherChargeState(IsotopicProfile potentialfeature, int chargeState)
        {
            double monoPeakMZ= potentialfeature.Peaklist[potentialfeature.MonoIsotopicPeakIndex].XValue;
            double pretendMonoPeakMZ= getMZOfAnotherChargeState(potentialfeature, potentialfeature.MonoIsotopicPeakIndex,chargeState);

            double widthPeak1 = potentialfeature.Peaklist[potentialfeature.MonoIsotopicPeakIndex].Width;
            double xValuePeak1 = potentialfeature.Peaklist[potentialfeature.MonoIsotopicPeakIndex].XValue;
            var ppmTolerancePeak1 = (widthPeak1 / 2.35) / xValuePeak1 * 1e6;    //   peak's sigma value / mz * 1e6 

            return getCorrelation(monoPeakMZ, pretendMonoPeakMZ, ppmTolerancePeak1, ppmTolerancePeak1);
            //IsotopicProfile pretendProfile = new IsotopicProfile();
            //pretendProfile = potentialfeature.Clone();
            //pretendProfile.Peaklist = potentialfeature.Peaklist.Clone();
            //ConvertToNewChargeState(pretendProfile, chargeState);
            //pretendProfile.ChargeState = chargeState;

            //return GetCorrelation(potentialfeature);
            
        }

        private void ConvertToNewChargeState(IsotopicProfile pretendProfile, int chargeState)
        {
            for (int i = 0; i < pretendProfile.Peaklist.Count; i++)
            {
                pretendProfile.Peaklist[i].XValue = getMZOfAnotherChargeState(pretendProfile, i, chargeState);    
            }
            pretendProfile.MonoIsotopicMass = pretendProfile.Peaklist[pretendProfile.MonoIsotopicPeakIndex].XValue * chargeState;
        }

        private double getMZOfAnotherChargeState(IsotopicProfile potentialfeature,int peakIndex, int chargeState)
        {

            return (potentialfeature.Peaklist[peakIndex].XValue * (double)potentialfeature.ChargeState) / (double)(chargeState);
        }

        private IsotopicProfile GetIsotopicProfileMethod1(int[] chargeStates, double[] correlations, double[,] correlationswithAltChargeState, List<IsotopicProfile> potentialIsotopicProfiles, IsotopicProfile bestFeat, double bestScore)
        {
            double[] standDevsOfEachSet = new double[correlations.Length];
            double[] averageCorrOfEachSet = new double[correlations.Length];
            List<int>[] chargeStateSets= new List<int>[correlations.Length];

            List<int> contenders = new List<int>();
            for (int i = 0; i < correlations.Length; i++)
            {

                HashSet<int> indexesWhoAreFactorsOfMe = getIndexesWhoAreAFactorOfMe(i, chargeStates);
                if (null == indexesWhoAreFactorsOfMe)
                {
                    break; //null means that we are at the end of the set. st dev is already defaulted at 0, which is what it 
                    //would be to take the st. dev of one item.
                }
                int length = indexesWhoAreFactorsOfMe.Count + 1;
                double[] arrayofCorrelationsInSet = new double[length];

                arrayofCorrelationsInSet[0] = correlations[i];
                for (int i2 = 1; i2 < length; i2++)
                {
                    arrayofCorrelationsInSet[i2] = correlations[indexesWhoAreFactorsOfMe.ElementAt(i2 - 1)];
                }
                chargeStateSets[i]=GetSet(i,indexesWhoAreFactorsOfMe, chargeStates);
                standDevsOfEachSet[i] = MathNet.Numerics.Statistics.Statistics.StandardDeviation(arrayofCorrelationsInSet);
                averageCorrOfEachSet[i] = MathNet.Numerics.Statistics.Statistics.Mean(arrayofCorrelationsInSet);
                if (standDevsOfEachSet[i] < 0.05 && correlations[i] > .7)//DANGERous 0.05 and .7 
                {
                    string reportString = "BEST CORRELATION: " + correlations[i] + "\nBEST CHARGE STATE: " + potentialIsotopicProfiles.ElementAt(i).ChargeState;
                    IqLogger.Log.Debug(reportString);
                    contenders.Add(i);
                    
                    //return potentialIsotopicProfiles.ElementAt(i);
                }
                if (contenders.Count==1)
                {
                    IqLogger.Log.Debug("\nWas only one contender\n");
                    return potentialIsotopicProfiles.ElementAt(contenders.ElementAt(0));
                }


                foreach (int contender in contenders)
                {
                    if (anotherChargeStateExists(contender,correlationswithAltChargeState))
                    {
                        //TODO: and no correlations of other non-factor charge states.
                        return potentialIsotopicProfiles.ElementAt(contender);
                    }
                    
                }
                    
                

            }

            //If none were really close, just return the highest correlation.            
            string reportString2 = "BEST CORRELATION: " + bestScore + "\nBEST CHARGE STATE: " + bestFeat.ChargeState;
            IqLogger.Log.Debug(reportString2);

            return bestFeat;

            //TODO:
            //ideas:
            //1. determine how many 'sets' there are.
            //2. determine who is 'right' in each set. this could entail looking at the st devs of the correlations 
            //3. determine which 'set' is 'right'.

            //also, look for that feature present in another charge state. 

            //for (int i = 0; i < chargeStates.Length; i++)
            //{
            //    HashSet<int> indexesWhoAreFactorsOfMe = getIndexesWhoAreAFactorOfMe(i, chargeStates);
            //    double[] arrayOfCorrelations = new double[indexesWhoAreFactorsOfMe.Count];
            //    for (int i2 = 0; i2 < arrayOfCorrelations.Length; i2++)
            //    {
            //        arrayOfCorrelations[i2] = indexesWhoAreFactorsOfMe.ElementAt(i2);
            //    }
            //    if (MathNet.Numerics.Statistics.Statistics.StandardDeviation(arrayOfCorrelations) < .05) return potentialIsotopicProfiles[i];

            //}

            //take the first one, see how its correlation compares with ones that are factors of itself. 
            //if it's about the same... then that confirms that the higher charge state is right.
            //if it's higher, that also confirms it is that first one.
            //if it's lower, exceedingly, then it's not that one and you should move on to try out the next one.

        }

        private bool anotherChargeStateExists(int contender, double[,] correlationswithAltChargeState)
        {
            for (int i = 0; i < correlationswithAltChargeState.GetLength(1); i++)
            {
                if (correlationswithAltChargeState[contender,i]!=-3 && correlationswithAltChargeState[contender,i]<=1.1)
                {
                    return true;
                }
            }
            return false;
        }

        private List<int> GetSet(int i, HashSet<int> indexesWhoAreFactorsOfMe, int[] chargeStates)
        {
            List<int> list = new List<int>();
            list.Add(chargeStates[i]);
            foreach (var index in indexesWhoAreFactorsOfMe)
            {
                list.Add(chargeStates[index]);
            }
            return list;
        }


        private double GetCorrelation(IsotopicProfile potentialfeature)
        {
            //TODO: change how many correlations are done here. right now only correlates 2 peaks on each features and looks at that correlation
            int monoIndex = potentialfeature.MonoIsotopicPeakIndex;
            int otherPeakToLookForIndex = 0;
            if (monoIndex == 0) { otherPeakToLookForIndex = 1; }
            else if (monoIndex < 0)
            {
                //HELP not sure how monoIndex becomes less than 0...
                monoIndex = 0;
                otherPeakToLookForIndex = 1;
            }
            else { otherPeakToLookForIndex = monoIndex - 1; }
            //HELP should it use theoprofile instead of the .XValue?
            double otherPeakToLookFor = potentialfeature.Peaklist[otherPeakToLookForIndex].XValue;

            return getCorrelation(monoIndex, otherPeakToLookForIndex, potentialfeature);

        }
        private double getCorrelation(int peak1Index, int peak2Index, IsotopicProfile potentialfeature)
        {
            double widthPeak1 = potentialfeature.Peaklist[peak1Index].Width;
            double xValuePeak1 = potentialfeature.Peaklist[peak1Index].XValue;
            var ppmTolerancePeak1 = (widthPeak1 / 2.35) / xValuePeak1 * 1e6;    //   peak's sigma value / mz * 1e6 

            double widthPeak2 = potentialfeature.Peaklist[peak2Index].Width;
            double xValuePeak2 = potentialfeature.Peaklist[peak2Index].XValue;
            var ppmTolerancePeak2 = (widthPeak2 / 2.35) / xValuePeak2 * 1e6;    //   peak's sigma value / mz * 1e6

            //altCorr = getCorrelation(xValuePeak1, getMZofLowerChargeState(potentialfeature, peak1Index), ppmTolerancePeak1, ppmTolerancePeak2);

            return getCorrelation(xValuePeak1, xValuePeak2, ppmTolerancePeak1, ppmTolerancePeak2);            
        }
        private double getCorrelation(double mzPeak1, double mzPeak2, double tolerancePPMpeak1, double tolerancePPMpeak2)
        {
            var chromgenPeak1 = new PeakChromatogramGenerator();
            chromgenPeak1.ChromatogramGeneratorMode = Globals.ChromatogramGeneratorMode.TOP_N_PEAKS;
            chromgenPeak1.TopNPeaksLowerCutOff = 0.4;
            var chromxydataPeak1 = chromgenPeak1.GenerateChromatogram(_run, 1, _run.GetNumMSScans(), mzPeak1, tolerancePPMpeak1, Globals.ToleranceUnit.PPM);
            var chromxydataPeak2 = chromgenPeak1.GenerateChromatogram(_run, 1, _run.GetNumMSScans(), mzPeak2, tolerancePPMpeak2, Globals.ToleranceUnit.PPM);
            if (null == chromxydataPeak1 || null == chromxydataPeak2) { return -3.0; }
         
            double[] arrayToCorrelatePeak1;
            double[] arrayToCorrelatePeak2;
            bool noOverlap= AlignAndFillArraysToCorrelate(chromxydataPeak1,chromxydataPeak2, out arrayToCorrelatePeak1, out arrayToCorrelatePeak2);
            if (noOverlap)
            {
                 return MathNet.Numerics.Statistics.Correlation.Pearson(arrayToCorrelatePeak1, arrayToCorrelatePeak2);
            }
            return -2;
        }

        private bool AlignAndFillArraysToCorrelate(XYData chromxydataPeak1, XYData chromxydataPeak2, out double[] arrayToCorrelatePeak1, out double[] arrayToCorrelatePeak2)
        {
            arrayToCorrelatePeak1 = null;
            arrayToCorrelatePeak2 = null;


            chromxydataPeak1.NormalizeYData();
            chromxydataPeak2.NormalizeYData();
            //Console.WriteLine("chromxydataPeak1");
            //chromxydataPeak1.Display();
            //Console.WriteLine("chromxydataPeak2");
            //chromxydataPeak2.Display();
            double lowestFramePeak1 = chromxydataPeak1.Xvalues.Min();
            double lowestFramePeak2 = chromxydataPeak2.Xvalues.Min();
            double highestFramePeak1 = chromxydataPeak1.Xvalues.Max();
            double highestFramePeak2 = chromxydataPeak2.Xvalues.Max();
            double minX = Math.Max(lowestFramePeak1, lowestFramePeak2);
            double maxX = Math.Min(highestFramePeak1, highestFramePeak2);
            bool noOverlap = (minX > maxX);
            if (noOverlap)
            {
                return false;
            }
            int chromPeak1StartIndex = chromxydataPeak1.GetClosestXVal(minX);
            int chromPeak2StartIndex = chromxydataPeak2.GetClosestXVal(minX);
            int chromPeak1StopIndex = chromxydataPeak1.GetClosestXVal(maxX);
            int chromPeak2StopIndex = chromxydataPeak2.GetClosestXVal(maxX);

            arrayToCorrelatePeak1 = new double[chromPeak1StopIndex - chromPeak1StartIndex + 1];
            arrayToCorrelatePeak2 = new double[chromPeak2StopIndex - chromPeak2StartIndex + 1];

            for (int i = chromPeak1StartIndex, j = 0; i <= chromPeak1StopIndex; i++, j++)
            {
                arrayToCorrelatePeak1[j] = chromxydataPeak1.Yvalues[i];
            }
            for (int i = chromPeak2StartIndex, j = 0; i <= chromPeak2StopIndex; i++, j++)
            {
                arrayToCorrelatePeak2[j] = chromxydataPeak2.Yvalues[i];
            }
            //for (int i = 0; i < arrayToCorrelatePeak1.Length; i++)
            //{
            //    Console.WriteLine(i + "\t" + arrayToCorrelatePeak1[i] + "\t" + arrayToCorrelatePeak2[i]);

            //}

            return true;
        }

        private double getMZofLowerChargeState(IsotopicProfile feature, int index)
        {
            if (feature.ChargeState == 1) return feature.Peaklist[index].XValue / 2;
            return (feature.Peaklist[index].XValue * (double)feature.ChargeState) / (double)(feature.ChargeState - 1);
        }
        private HashSet<int> getIndexesWhoAreAFactorOfMe(int index, int[] chargestates)
        {
            if (index == chargestates.Length - 1) return null;

            int number = chargestates[index];
            HashSet<int> indexesWhoAreFactorsOfMe = new HashSet<int>();
            for (int i = index + 1; i < chargestates.Length; i++)
            {
                if (number % chargestates[i] == 0) indexesWhoAreFactorsOfMe.Add(i);
            }
            return indexesWhoAreFactorsOfMe;

        }
        private bool[] getIsAFactorInfo(int[] chargeStates)
        {
            bool[] isAFactorOfAnother = new bool[chargeStates.Length];
            for (int i = 0; i < isAFactorOfAnother.Length; i++)
            {
                isAFactorOfAnother[i] = getIsAFactorHelper(chargeStates, i);
            }
            return isAFactorOfAnother;
        }
        private bool getIsAFactorHelper(int[] chargeStates, int index)
        {
            int number = chargeStates[index];
            for (int i = chargeStates.Length - 1; i > index; i--)
            {
                if (chargeStates[i] % number == 0)
                {
                    return true;
                }
            }
            return false;
        }




    }
}
