using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ChargeStateDeciders
{
    public class XICCorrelatingChargeDecider : ChargeStateDecider
    {
        private Run _run;
        public XICCorrelatingChargeDecider(Run runp)
        {
            _run = runp;
        }
        public override IsotopicProfile DetermineCorrectIsotopicProfile(List<IsotopicProfile> potentialIsotopicProfiles)
        {
            if (null == potentialIsotopicProfiles || potentialIsotopicProfiles.Count()==0) return null;

            if (_run.ResultCollection.MSPeakResultList.Count() == 0) 
            {              
               _run = RunUtilities.CreateAndLoadPeaks(_run.Filename);                  
            }
            potentialIsotopicProfiles= potentialIsotopicProfiles.OrderByDescending(n => n.ChargeState).ToList();

            int[] chargeStates = getChargeStates(potentialIsotopicProfiles);
            double[] correlations = new double[chargeStates.Length];
            double[] correlationswithAltChargeState = new double[chargeStates.Length];
            int indexCurrentFeature = -1;
            double bestScore = -1;
            IsotopicProfile bestFeat = potentialIsotopicProfiles.First();
            foreach (var potentialfeature in potentialIsotopicProfiles)
            {
                indexCurrentFeature++;
                double correlation =0.0;
                double altCorr=0.0;
                getCorrelation(potentialfeature, out correlation, out altCorr);

                Console.WriteLine("chargeState: " + potentialfeature.ChargeState);
                Console.WriteLine("correlation value: " + correlation);
                Console.WriteLine("another Charge State present Corr? : " + altCorr);
                Console.WriteLine("Score: " + potentialfeature.Score);
                correlations[indexCurrentFeature] = correlation;
                correlationswithAltChargeState[indexCurrentFeature] = altCorr;
                if (bestScore<correlation)
                {
                    bestScore = correlation;
                    bestFeat = potentialfeature;
                }
            }
            return IsotopicProfileDecision1(chargeStates, correlations, correlationswithAltChargeState, potentialIsotopicProfiles, bestFeat,bestScore);

            
        }

        private IsotopicProfile IsotopicProfileDecision1(int[] chargeStates, double[] correlations, double[] correlationswithAltChargeState, List<IsotopicProfile> potentialIsotopicProfiles, IsotopicProfile bestFeat, double bestScore)
        {
            double[] standDevsOfEachSet = new double[correlations.Length];
            for (int i = 0; i < correlations.Length; i++)
            {

                HashSet<int> indexesWhoAreFactorsOfMe = getIndexesWhoAreAFactorOfMe(i, chargeStates);
                if (null == indexesWhoAreFactorsOfMe)
                {
                    break; //null means that we are at the end of the set. st dev is already defaulted at 0, which is what it 
                    //would be to take the st. dev of one item.
                }
                int length = indexesWhoAreFactorsOfMe.Count() + 1;
                double[] arrayofCorrelationsInSet = new double[length];
                arrayofCorrelationsInSet[0] = correlations[i];
                for (int i2 = 1; i2 < length; i2++)
                {
                    arrayofCorrelationsInSet[i2] = correlations[indexesWhoAreFactorsOfMe.ElementAt(i2 - 1)];
                }
                standDevsOfEachSet[i] = MathNet.Numerics.Statistics.Statistics.StandardDeviation(arrayofCorrelationsInSet);
                if (standDevsOfEachSet[i] < 0.05)//DANGER
                {
                    Console.WriteLine("BEST CORRELATION: " + correlations[i] + "\nBEST CHARGE STATE: " + potentialIsotopicProfiles.ElementAt(i).ChargeState);
                    return potentialIsotopicProfiles.ElementAt(i);
                }
            }

            //If none were really close, just return the highest correlation.
            Console.WriteLine("BEST CORRELATION: " + bestScore + "\nBEST CHARGE STATE: " + bestFeat.ChargeState);
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
            //    double[] arrayOfCorrelations = new double[indexesWhoAreFactorsOfMe.Count()];
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

        

        
        private int[] getChargeStates(List<IsotopicProfile> potentialIsotopicProfiles)
        {
            int[] chargestates = new int[potentialIsotopicProfiles.Count()];

            for (int i = 0; i < chargestates.Length; i++)
            {
                chargestates[i] = potentialIsotopicProfiles.ElementAt(i).ChargeState;
            }
            return chargestates;
        }

        private double getCorrelation(IsotopicProfile potentialfeature, out double correlation, out double altCorr)
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

            return getCorrelation(monoIndex, otherPeakToLookForIndex, potentialfeature, out correlation, out altCorr);

        }
        private double getCorrelation(int peak1Index, int peak2Index, IsotopicProfile potentialfeature, out double correlation, out double altCorr)
        {
            double widthPeak1 = potentialfeature.Peaklist[peak1Index].Width;
            double xValuePeak1 = potentialfeature.Peaklist[peak1Index].XValue;
            var ppmTolerancePeak1 = (widthPeak1 / 2.35) / xValuePeak1 * 1e6;    //   peak's sigma value / mz * 1e6 

            double widthPeak2 = potentialfeature.Peaklist[peak2Index].Width;
            double xValuePeak2 = potentialfeature.Peaklist[peak2Index].XValue;
            var ppmTolerancePeak2 = (widthPeak2 / 2.35) / xValuePeak2 * 1e6;    //   peak's sigma value / mz * 1e6

            altCorr= getCorrelation( xValuePeak1, getMZofLowerChargeState(potentialfeature, peak1Index), ppmTolerancePeak1, ppmTolerancePeak2);
            
            correlation= getCorrelation(xValuePeak1, xValuePeak2, ppmTolerancePeak1, ppmTolerancePeak2);
            return correlation;

        }
        private double getCorrelation(double MZpeak1, double MZpeak2, double tolerancePPMpeak1, double tolerancePPMpeak2)
        {
            var chromgenPeak1 = new PeakChromatogramGenerator();
            chromgenPeak1.ChromatogramGeneratorMode = Globals.ChromatogramGeneratorMode.TOP_N_PEAKS;
            chromgenPeak1.TopNPeaksLowerCutOff = 0.4;
            var chromxydataPeak1 = chromgenPeak1.GenerateChromatogram(_run, 1, _run.GetNumMSScans(), MZpeak1, tolerancePPMpeak1, Globals.ToleranceUnit.PPM);
            var chromxydataPeak2 = chromgenPeak1.GenerateChromatogram(_run, 1, _run.GetNumMSScans(), MZpeak2, tolerancePPMpeak2, Globals.ToleranceUnit.PPM);

            #region Align chroms for Correlation
            if (null == chromxydataPeak1 || null == chromxydataPeak2){return -1.0;}
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
                return -2.0;
            }
            int chromPeak1StartIndex = chromxydataPeak1.GetClosestXVal(minX);
            int chromPeak2StartIndex = chromxydataPeak2.GetClosestXVal(minX);
            int chromPeak1StopIndex = chromxydataPeak1.GetClosestXVal(maxX);
            int chromPeak2StopIndex = chromxydataPeak2.GetClosestXVal(maxX);

            
            double[] arrayToCorrelatePeak1 = new double[chromPeak1StopIndex - chromPeak1StartIndex + 1];
            double[] arrayToCorrelatePeak2 = new double[chromPeak2StopIndex - chromPeak2StartIndex + 1];
            #endregion

            for (int i = chromPeak1StartIndex, j = 0; i <= chromPeak1StopIndex; i++, j++)
            {
                arrayToCorrelatePeak1[j] = chromxydataPeak1.Yvalues[i];
            }
            for (int i = chromPeak2StartIndex, j = 0; i <= chromPeak2StopIndex; i++, j++)
            {
                arrayToCorrelatePeak2[j] = chromxydataPeak2.Yvalues[i];
            }
            return MathNet.Numerics.Statistics.Correlation.Pearson(arrayToCorrelatePeak1, arrayToCorrelatePeak2);
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
