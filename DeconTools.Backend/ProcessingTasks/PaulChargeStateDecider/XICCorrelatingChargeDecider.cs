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
            if (_run.ResultCollection.MSPeakResultList.Count() == 0) 
            { 
                _run = RunUtilities.CreateAndLoadPeaks(_run.Filename); 
            }
            potentialIsotopicProfiles.OrderByDescending(n => n.ChargeState);

            int[] chargeStates = getChargeStates(potentialIsotopicProfiles);
            double[] correlations = new double[chargeStates.Length];
            int indexCurrentFeature = -1;
            double bestScore = -1;
            IsotopicProfile bestFeat = potentialIsotopicProfiles.First();
            foreach (var potentialfeature in potentialIsotopicProfiles)
            {
                indexCurrentFeature++;
                double correlation = getCorrelation(potentialfeature);
                Console.WriteLine("chargeState: " + potentialfeature.ChargeState);
                Console.WriteLine("correlation value: " + correlation);
                correlations[indexCurrentFeature] = correlation;
                if (bestScore<correlation)
                {
                    bestScore = correlation;
                    bestFeat = potentialfeature;
                }
            }
            Console.Write("Best correlation: " + bestScore + "\n charge state: " + bestFeat.ChargeState);
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

        private double getCorrelation(IsotopicProfile potentialfeature)
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

            var alternatechargestatecorr = getCorrelation(getMZofLowerChargeState(potentialfeature, peak1Index), getMZofLowerChargeState(potentialfeature, peak1Index), ppmTolerancePeak1, ppmTolerancePeak2);
            Console.WriteLine("ALTERNATE CHARGE STATE CORRELATION: " + alternatechargestatecorr);

            return getCorrelation(xValuePeak1, xValuePeak2, ppmTolerancePeak1, ppmTolerancePeak2);

        }
        private double getCorrelation(double MZpeak1, double MZpeak2, double tolerancePPMpeak1, double tolerancePPMpeak2)
        {
            var chromgenPeak1 = new PeakChromatogramGenerator();
            chromgenPeak1.ChromatogramGeneratorMode = Globals.ChromatogramGeneratorMode.TOP_N_PEAKS;
            chromgenPeak1.TopNPeaksLowerCutOff = 0.4;
            var chromxydataPeak1 = chromgenPeak1.GenerateChromatogram(_run, 1, _run.GetNumMSScans(), MZpeak1, tolerancePPMpeak1, Globals.ToleranceUnit.PPM);
            var chromxydataPeak2 = chromgenPeak1.GenerateChromatogram(_run, 1, _run.GetNumMSScans(), MZpeak2, tolerancePPMpeak2, Globals.ToleranceUnit.PPM);

            #region Align chroms for Correlation
            if (null == chromxydataPeak1 || null == chromxydataPeak2){return 0.0;}
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
            for (int i = index + 1; i < chargestates.Length - 1; i++)
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
