using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;


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
            
            CreatePeaksIfNeeded();//note: does not actually create peaks. Only loads them. An exception is thrown if it's not there.
            potentialIsotopicProfiles = potentialIsotopicProfiles.OrderByDescending(n => n.ChargeState).ToList();

            int[] chargeStates = (from prof in potentialIsotopicProfiles select prof.ChargeState).ToArray();
            double[] correlations = new double[chargeStates.Length];
            double[,] correlationswithAltChargeState = new double[chargeStates.Length, 2];//[INDEX,HOWMANYOTHERSTATESTOTRY]
            int indexCurrentFeature = -1;

            double bestScore = -1;
            IsotopicProfile bestFeature = potentialIsotopicProfiles.First();

            int index = potentialIsotopicProfiles.First().MonoIsotopicPeakIndex;
            if (index == -1)
            {
                index = 0;
            }
            //string reportString1 = "\tM/Z : \t" + potentialIsotopicProfiles.First().Peaklist[index].XValue + "\t";
            //IqLogger.Log.Debug(reportString1);

            //with line spaces
            string reportString1 = "\nM/Z : " + potentialIsotopicProfiles.First().Peaklist[index].XValue + "\n";
            IqLogger.Log.Debug(reportString1);
            foreach (var potentialfeature in potentialIsotopicProfiles)
            {
                indexCurrentFeature++;

                double correlation = GetCorrelation(potentialfeature);
                int[] chargesToTry = GetChargesToTry(potentialfeature);

                for (int i = 0; i < chargesToTry.Length; i++)
                {
                    correlationswithAltChargeState[indexCurrentFeature, i] = GetCorrelationWithAnotherChargeState(potentialfeature, chargesToTry[i]);
                }

                //lines output
                string reportString = "\nCHARGE: " + potentialfeature.ChargeState + "\n" +
                   "CORRELATION: " + correlation + "\n";
                for (int i = 0; i < chargesToTry.Length; i++)
                {
                    reportString += "charge " + chargesToTry[i] + " (M/Z =" + GetMZOfAnotherChargeState(potentialfeature, index, chargesToTry[i]) + ") correlation: " +
                    correlationswithAltChargeState[indexCurrentFeature, i] + "\n";
                }
                reportString += "Score: " + potentialfeature.Score;
                IqLogger.Log.Debug(reportString);


                //tabular output
                //string reportString = "\tCHARGE: " + potentialfeature.ChargeState + "\t" +
                //   "CORRELATION: \t" + correlation + "\t";
                //for (int i = 0; i < chargesToTry.Length; i++)
                //{
                //    reportString += "charge \t" + chargesToTry[i] + "\t (M/Z =\t" + GetMZOfAnotherChargeState(potentialfeature, index, chargesToTry[i]) + "\t) correlation: \t" +
                //    correlationswithAltChargeState[indexCurrentFeature, i] + "\t";
                //}
                //reportString += "Score: \t" + potentialfeature.Score + "\t";
                //IqLogger.Log.Debug(reportString);


                correlations[indexCurrentFeature] = correlation;

                if (bestScore < correlation)
                {
                    bestScore = correlation;
                    bestFeature = potentialfeature;
                }
            }
            return GetIsotopicProfileMethod1(chargeStates, correlations, correlationswithAltChargeState, potentialIsotopicProfiles, bestFeature, bestScore);
        }
  
        //note: does not actually create peaks. Only loads them. An exception is thrown if it's not there.
        private void CreatePeaksIfNeeded()
        {
            if (_run.ResultCollection.MSPeakResultList.Count == 0)
            {
                   string expectedPeaksfile = _run.DataSetPath + "\\" + _run.DatasetName + "_peaks.txt";
                   if (!File.Exists(expectedPeaksfile))
                   {
                       Exception ex = new Exception("No Peaks file found.");
                       throw (ex);
                    //    CreatePeaksFile();
                    //ExportPeaks_copied(expectedPeaksfile);   
                   
                   }
                 RunUtilities.GetPeaks(_run, expectedPeaksfile);
            }
        }

        private int[] GetChargesToTry(IsotopicProfile potentialfeature)
        {
            switch (potentialfeature.ChargeState)
            {
                case 1:
                    return new int[] { 2, 3 };
                default:
                    return new int[2] { potentialfeature.ChargeState - 1, potentialfeature.ChargeState + 1 };
            }
        }

        private double GetCorrelationWithAnotherChargeState(IsotopicProfile potentialfeature, int chargeState)
        {

            if (potentialfeature.MonoIsotopicPeakIndex == -1)
            {
                return -3;
            }
            double monoPeakMZ = potentialfeature.Peaklist[potentialfeature.MonoIsotopicPeakIndex].XValue;
            double pretendMonoPeakMZ = GetMZOfAnotherChargeState(potentialfeature, potentialfeature.MonoIsotopicPeakIndex, chargeState);

            double widthPeak1 = potentialfeature.Peaklist[potentialfeature.MonoIsotopicPeakIndex].Width;
            double xValuePeak1 = potentialfeature.Peaklist[potentialfeature.MonoIsotopicPeakIndex].XValue;
            var ppmTolerancePeak1 = (widthPeak1 / 2.35) / xValuePeak1 * 1e6;    //   peak's sigma value / mz * 1e6 

            return getCorrelation(monoPeakMZ, pretendMonoPeakMZ, ppmTolerancePeak1, ppmTolerancePeak1);         

        }

        private void ConvertToNewChargeState(IsotopicProfile pretendProfile, int chargeState)
        {
            for (int i = 0; i < pretendProfile.Peaklist.Count; i++)
            {
                pretendProfile.Peaklist[i].XValue = GetMZOfAnotherChargeState(pretendProfile, i, chargeState);
            }
            pretendProfile.MonoIsotopicMass = pretendProfile.Peaklist[pretendProfile.MonoIsotopicPeakIndex].XValue * chargeState;
        }

        private double GetMZOfAnotherChargeState(IsotopicProfile potentialfeature, int peakIndex, int chargeState)
        {

            return (potentialfeature.Peaklist[peakIndex].XValue * (double)potentialfeature.ChargeState) / (double)(chargeState);
        }

        private IsotopicProfile GetIsotopicProfileMethod1(int[] chargeStates, double[] correlations, double[,] correlationswithAltChargeState, List<IsotopicProfile> potentialIsotopicProfiles, IsotopicProfile bestFeat, double bestScore)
        {
            double[] standDevsOfEachSet = new double[correlations.Length];
            double[] averageCorrOfEachSet = new double[correlations.Length];
            List<int>[] chargeStateSets = new List<int>[correlations.Length];
            List<int> contendingCharges = chargeStates.ToList();//new List<int>();
            #region Metric 1, altCharge present
            Dictionary<int, double> favorites = new Dictionary<int, double>();

            bool anotherChargeStateExists = false;
            foreach (int contender in contendingCharges)
            {
                int contenderIndex = Array.IndexOf(chargeStates, contender);
                anotherChargeStateExists = AnotherChargeStateExists(contenderIndex, correlationswithAltChargeState);
                if (anotherChargeStateExists)
                {
                    //TODO: and no correlations of other non-factor charge states.
                    double[] corrs = new double[correlationswithAltChargeState.GetLength(1)];
                    for (int i = 0; i < corrs.Length; i++)
                    {
                        corrs[i] = correlationswithAltChargeState[contenderIndex, i];

                    }
                    double bestAltCorr = corrs.Max();
                    favorites.Add(contenderIndex, bestAltCorr);
                }

            }
            if (favorites.Count>0)
            {
                return potentialIsotopicProfiles.ElementAt((favorites.OrderByDescending(x => x.Value).First().Key));
            }

            #endregion
            #region Metric 2, stand dev
            for (int i = 0; i < correlations.Length; i++)
            {
                HashSet<int> indexesWhoAreFactorsOfMe = GetIndexesWhoAreAFactorOfMe(i, chargeStates);
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
                chargeStateSets[i] = GetSet(i, indexesWhoAreFactorsOfMe, chargeStates);
                standDevsOfEachSet[i] = MathNet.Numerics.Statistics.Statistics.StandardDeviation(arrayofCorrelationsInSet);
                averageCorrOfEachSet[i] = MathNet.Numerics.Statistics.Statistics.Mean(arrayofCorrelationsInSet);

                double correlationThreshold = 0.49;
                double standartDeviationThreshold = .3;

                if (standDevsOfEachSet[i] < standartDeviationThreshold && correlations[i] > correlationThreshold)//DANGERous 0.05 and .7 
                {
                    foreach (int index in indexesWhoAreFactorsOfMe)
                    {
                        contendingCharges.Remove(chargeStates[index]);
                    }
                }
                if (contendingCharges.Count == 1)//if there is only one left after it's own factors are removed, it's that one.
                {
                    // IqLogger.Log.Debug("\nWas only one contender\n");
                    return potentialIsotopicProfiles.ElementAt(Array.IndexOf(chargeStates, contendingCharges.First()));
                }
            }
            #endregion
            #region Metric 3, ask Patterson
            
            var chargeStateCalculator = new PattersonChargeStateCalculatorWithChanges();
            int chargeState = chargeStateCalculator.GetChargeState(_run.XYData, _run.PeakList, potentialIsotopicProfiles.First().getMonoPeak());            
            IqLogger.Log.Debug("had to use the patterson calculator and this is what it gave me: " + chargeState);
            for (int i = 0; i < chargeStates.Length; i++)
            {
                IqLogger.Log.Debug(chargeStates[i] + "\t");
            }
            IqLogger.Log.Debug("Charge state length: " + chargeStates.Length);
            if (chargeStates.Contains(chargeState))
            {
                IqLogger.Log.Debug(Array.IndexOf(chargeStates, chargeState));
                return potentialIsotopicProfiles.ElementAt(Array.IndexOf(chargeStates, chargeState));
            }
            else
            {
                int bestChargeIndex = -1;
                double bestCorrelation = -6.0;//arbitrary negative
                foreach (int charge in contendingCharges)
                {
                    int index = Array.IndexOf(chargeStates, charge);
                    if (bestCorrelation < correlations[index])
                    {
                        bestCorrelation = correlations[index];
                        bestChargeIndex = index;
                    }
                }
                if (bestCorrelation<=0)
                {
                    return null;
                }
                return potentialIsotopicProfiles.ElementAt(bestChargeIndex);
            }
            #endregion
        }

        private bool AnotherChargeStateExists(int contenderIndex, double[,] correlationswithAltChargeState)
        {
            for (int i = 0; i < correlationswithAltChargeState.GetLength(1); i++)
            {
                if (correlationswithAltChargeState[contenderIndex, i] != -3 &&
                    correlationswithAltChargeState[contenderIndex, i] <= 1.1 &&
                    correlationswithAltChargeState[contenderIndex, i] > 0)
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
            //TODO: correlate narrow range. -100 +100 
            //TODO: for gord.
            int scanWindow=100;
            int lowerScan = _run.CurrentScanSet.PrimaryScanNumber - scanWindow;
            int upperScan = _run.CurrentScanSet.PrimaryScanNumber + scanWindow;

            var chromxydataPeak1 = chromgenPeak1.GenerateChromatogram(_run, lowerScan, upperScan, mzPeak1, tolerancePPMpeak1, Globals.ToleranceUnit.PPM);
            var chromxydataPeak2 = chromgenPeak1.GenerateChromatogram(_run, lowerScan, upperScan, mzPeak2, tolerancePPMpeak2, Globals.ToleranceUnit.PPM);

            //var chromxydataPeak1 = chromgenPeak1.GenerateChromatogram(_run, 1, _run.GetNumMSScans(), mzPeak1, tolerancePPMpeak1, Globals.ToleranceUnit.PPM);
            //var chromxydataPeak2 = chromgenPeak1.GenerateChromatogram(_run, 1, _run.GetNumMSScans(), mzPeak2, tolerancePPMpeak2, Globals.ToleranceUnit.PPM);
            if (null == chromxydataPeak1 || null == chromxydataPeak2) { return -3.0; }

            double[] arrayToCorrelatePeak1;
            double[] arrayToCorrelatePeak2;
            bool overlap = AlignAndFillArraysToCorrelate(chromxydataPeak1, chromxydataPeak2, out arrayToCorrelatePeak1, out arrayToCorrelatePeak2);
            if (overlap)
            {
                double corr = MathNet.Numerics.Statistics.Correlation.Pearson(arrayToCorrelatePeak1, arrayToCorrelatePeak2);
                if (double.IsNaN(corr))
                {
                    return -2;   //it's present, but they don't overlap any. same as other -2 value.                 
                }
                //TODO: implement iq standard of getting linear regression.
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
            bool Overlap = (minX < maxX);
            if (!Overlap)
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

            if (arrayToCorrelatePeak1.Length< 5 || arrayToCorrelatePeak2.Length<5)
            {
                return false; 
            }

            return true;
        }

        private HashSet<int> GetIndexesWhoAreAFactorOfMe(int index, int[] chargestates)
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

        internal HashSet<int> GetPotentialChargeState(int indexOfCurrentPeak, List<Peak> mspeakList, double ppmTolerance)
        {
            //List<IsotopicProfile> potentialProfiles = new List<IsotopicProfile>();
            CreatePeaksIfNeeded();//note: does not actually create peaks. Only loads them. An exception is thrown if it's not there.
            

            HashSet<int> chargeStates= new HashSet<int>();
            int charge=1;
            for (; charge < 10; charge++)
            {
                double mzPeak1=mspeakList.ElementAt(indexOfCurrentPeak).XValue;
                double distanceToFindNextPeak = 1.0 / (double)charge;
                double xValueToLookFor = mzPeak1 + distanceToFindNextPeak;
                double lowerMZ = xValueToLookFor - ppmTolerance * xValueToLookFor / 1e6;
                double upperMZ = xValueToLookFor + ppmTolerance * xValueToLookFor / 1e6; 


                
                var peak2 = mspeakList.Find(peak => peak.XValue<= upperMZ && peak.XValue>=lowerMZ);                
                if (peak2==null)
                {
                    continue;
                }
                double mzPeak2 = peak2.XValue;
                double correlation = getCorrelation(mzPeak1, mzPeak2, ppmTolerance, ppmTolerance);
                if (correlation>0.2)
                {
                    chargeStates.Add(charge);
                }
            }

            string reportString521= "Potential Charges using correlation: ";
            foreach (var curcharge in chargeStates)
            {
                reportString521+=curcharge + "\t";
            }
            IqLogger.Log.Debug(reportString521 + "\n");
            return chargeStates;
        }
  
        private void CreatePeaksFile()
        {
            _run.ScanSetCollection.Create(_run, 1, 1, false);
            //PeakDetectAndExportWorkflowParameters parameters = new PeakDetectAndExportWorkflowParameters();
            //parameters.LCScarnMin = 5500;
            //parameters.LCScanMax = 6500;
            //parameters.OutputFolder = outputFolder;

            //PeakDetectAndExportWorkflow workflow = new PeakDetectAndExportWorkflow(run, parameters);
            //workflow.Execute();
        }
        private void ExportPeaks_copied(string peaksfile)
        {
            DeconToolsPeakDetectorV2 _ms1PeakDetector;
            DeconToolsPeakDetectorV2 _ms2PeakDetectorForCentroidedData;
            DeconToolsPeakDetectorV2 _ms2PeakDetectorForProfileData;
            MSGenerator msGen = MSGeneratorFactory.CreateMSGenerator(_run.MSFileType);

            _ms1PeakDetector = new DeconToolsPeakDetectorV2(2.0, 2.0,
                DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);

            _ms2PeakDetectorForProfileData = new DeconToolsPeakDetectorV2(2.0,
                                                                          2.0,
                                                                          DeconTools.Backend.Globals.PeakFitType.QUADRATIC,
                                                                          false);
            _ms2PeakDetectorForCentroidedData = new DeconToolsPeakDetectorV2(0, 0, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            _ms2PeakDetectorForCentroidedData.RawDataType = DeconTools.Backend.Globals.RawDataType.Centroided;

            _ms2PeakDetectorForProfileData.PeaksAreStored = true;
            _ms2PeakDetectorForCentroidedData.PeaksAreStored = true;
            _ms1PeakDetector.PeaksAreStored = true;


            var peakExporter = new PeakListTextExporter(_run.MSFileType, peaksfile);

            int numTotalScans = _run.ScanSetCollection.ScanSetList.Count;
            int scanCounter = 0;

            if (_run.MSFileType == DeconTools.Backend.Globals.MSFileType.PNNL_UIMF)
            {
                var uimfrun = _run as UIMFRun;

                int numTotalFrames = uimfrun.ScanSetCollection.ScanSetList.Count;
                int frameCounter = 0;

                foreach (var frameSet in uimfrun.ScanSetCollection.ScanSetList)
                {
                    frameCounter++;
                    uimfrun.CurrentScanSet = frameSet;
                    uimfrun.ResultCollection.MSPeakResultList.Clear();

                    foreach (var scanSet in uimfrun.IMSScanSetCollection.ScanSetList)
                    {
                        uimfrun.CurrentIMSScanSet = (IMSScanSet)scanSet;
                        msGen.Execute(uimfrun.ResultCollection);
                        _ms1PeakDetector.Execute(uimfrun.ResultCollection);

                    }
                    peakExporter.WriteOutPeaks(uimfrun.ResultCollection.MSPeakResultList);

                    if (frameCounter % 5 == 0 || scanCounter == numTotalFrames)
                    {
                        double percentProgress = frameCounter * 100 / numTotalFrames;
                       // reportProgress(percentProgress);
                    }

                }

            }
            else
            {
                foreach (var scan in _run.ScanSetCollection.ScanSetList)
                {
                    scanCounter++;

                    _run.CurrentScanSet = scan;

                    _run.ResultCollection.MSPeakResultList.Clear();

                    msGen.Execute(_run.ResultCollection);
                    if (_run.GetMSLevel(scan.PrimaryScanNumber) == 1)
                    {
                        _ms1PeakDetector.Execute(_run.ResultCollection);
                    }
                    else
                    {
                        var dataIsCentroided = _run.IsDataCentroided(scan.PrimaryScanNumber);
                        if (dataIsCentroided)
                        {
                            _ms2PeakDetectorForCentroidedData.Execute(_run.ResultCollection);
                        }
                        else
                        {
                            _ms2PeakDetectorForProfileData.Execute(_run.ResultCollection);
                        }
                    }

                    peakExporter.WriteOutPeaks(_run.ResultCollection.MSPeakResultList);


                }
            }



        }
    }
}
