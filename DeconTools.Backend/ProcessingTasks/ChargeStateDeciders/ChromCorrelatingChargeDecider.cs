using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;

namespace DeconTools.Backend.ProcessingTasks.ChargeStateDeciders
{
    public class ChromCorrelatingChargeDecider : ChargeStateDecider
    {
        private readonly Run _run;

        public ChromCorrelatingChargeDecider(Run run)
        {
            _run = run;
        }

        public override IsotopicProfile DetermineCorrectIsotopicProfile(List<IsotopicProfile> potentialIsotopicProfiles)
        {
            if (null == potentialIsotopicProfiles || potentialIsotopicProfiles.Count == 0) return null;

            CreatePeaksIfNeeded();//note: does not actually create peaks. Only loads them. An exception is thrown if it's not there.
            potentialIsotopicProfiles = potentialIsotopicProfiles.OrderByDescending(n => n.ChargeState).ToList();

            var chargeStates = (from prof in potentialIsotopicProfiles select prof.ChargeState).ToArray();
            var correlations = new double[chargeStates.Length];
            var correlationsWithAltChargeState = new double[chargeStates.Length, 2];   // [Index, How many other states to try]
            var indexCurrentFeature = -1;

            double bestScore = -1;

            var index = potentialIsotopicProfiles[0].MonoIsotopicPeakIndex;
            if (index == -1)
            {
                index = 0;
            }
            //string reportString1 = "\tM/Z : \t" + potentialIsotopicProfiles.First().Peaklist[index].XValue + "\t";
            //IqLogger.Log.Debug(reportString1);

            //with line spaces
            var reportString1 = "\nM/Z : " + potentialIsotopicProfiles[0].Peaklist[index].XValue + "\n";
            IqLogger.LogTrace(reportString1);
            foreach (var potentialFeature in potentialIsotopicProfiles)
            {
                indexCurrentFeature++;

                var correlation = GetCorrelation(potentialFeature);
                var chargesToTry = GetChargesToTry(potentialFeature);

                for (var i = 0; i < chargesToTry.Length; i++)
                {
                    correlationsWithAltChargeState[indexCurrentFeature, i] = GetCorrelationWithAnotherChargeState(potentialFeature, chargesToTry[i]);
                }

                //lines output
                var reportString = "\nCHARGE: " + potentialFeature.ChargeState + "\n" +
                   "CORRELATION: " + correlation + "\n";
                for (var i = 0; i < chargesToTry.Length; i++)
                {
                    reportString += "charge " + chargesToTry[i] + " (M/Z =" + GetMZOfAnotherChargeState(potentialFeature, index, chargesToTry[i]) + ") correlation: " +
                    correlationsWithAltChargeState[indexCurrentFeature, i] + "\n";
                }
                reportString += "Score: " + potentialFeature.Score;
                IqLogger.LogTrace(reportString);

                //tabular output
                //string reportString = "\tCHARGE: " + potentialFeature.ChargeState + "\t" +
                //   "CORRELATION: \t" + correlation + "\t";
                //for (int i = 0; i < chargesToTry.Length; i++)
                //{
                //    reportString += "charge \t" + chargesToTry[i] + "\t (M/Z =\t" + GetMZOfAnotherChargeState(potentialFeature, index, chargesToTry[i]) + "\t) correlation: \t" +
                //    correlationsWithAltChargeState[indexCurrentFeature, i] + "\t";
                //}
                //reportString += "Score: \t" + potentialFeature.Score + "\t";
                //IqLogger.Log.Debug(reportString);

                correlations[indexCurrentFeature] = correlation;

                if (bestScore < correlation)
                {
                    bestScore = correlation;
                }
            }
            return GetIsotopicProfileMethod1(chargeStates, correlations, correlationsWithAltChargeState, potentialIsotopicProfiles);
        }

        //note: does not actually create peaks. Only loads them. An exception is thrown if it's not there.
        private void CreatePeaksIfNeeded()
        {
            if (_run.ResultCollection.MSPeakResultList.Count == 0)
            {
                var expectedPeaksFile = Path.Combine(_run.DatasetDirectoryPath, _run.DatasetName + "_peaks.txt");
                if (!File.Exists(expectedPeaksFile))
                {
                    var ex = new Exception("No Peaks file found.");
                    throw ex;
                    //    CreatePeaksFile();
                    //ExportPeaks_copied(expectedPeaksFile);

                }
                RunUtilities.GetPeaks(_run, expectedPeaksFile);
            }
        }

        private int[] GetChargesToTry(IsotopicProfile potentialFeature)
        {
            switch (potentialFeature.ChargeState)
            {
                case 1:
                    return new[] { 2, 3 };
                default:
                    return new[] { potentialFeature.ChargeState - 1, potentialFeature.ChargeState + 1 };
            }
        }

        private double GetCorrelationWithAnotherChargeState(IsotopicProfile potentialFeature, int chargeState)
        {
            if (potentialFeature.MonoIsotopicPeakIndex == -1)
            {
                return -3;
            }
            var monoPeakMZ = potentialFeature.Peaklist[potentialFeature.MonoIsotopicPeakIndex].XValue;
            var pretendMonoPeakMZ = GetMZOfAnotherChargeState(potentialFeature, potentialFeature.MonoIsotopicPeakIndex, chargeState);

            double widthPeak1 = potentialFeature.Peaklist[potentialFeature.MonoIsotopicPeakIndex].Width;
            var xValuePeak1 = potentialFeature.Peaklist[potentialFeature.MonoIsotopicPeakIndex].XValue;
            var ppmTolerancePeak1 = (widthPeak1 / 2.35) / xValuePeak1 * 1e6;    //   peak's sigma value / mz * 1e6

            return getCorrelation(monoPeakMZ, pretendMonoPeakMZ, ppmTolerancePeak1, ppmTolerancePeak1);
        }

        [Obsolete("Unused")]
        private void ConvertToNewChargeState(IsotopicProfile pretendProfile, int chargeState)
        {
            for (var i = 0; i < pretendProfile.Peaklist.Count; i++)
            {
                pretendProfile.Peaklist[i].XValue = GetMZOfAnotherChargeState(pretendProfile, i, chargeState);
            }
            pretendProfile.MonoIsotopicMass = pretendProfile.Peaklist[pretendProfile.MonoIsotopicPeakIndex].XValue * chargeState;
        }

        private double GetMZOfAnotherChargeState(IsotopicProfile potentialFeature, int peakIndex, int chargeState)
        {
            return (potentialFeature.Peaklist[peakIndex].XValue * potentialFeature.ChargeState) / chargeState;
        }

        private IsotopicProfile GetIsotopicProfileMethod1(int[] chargeStates, IReadOnlyList<double> correlations, double[,] correlationsWithAltChargeState, IReadOnlyCollection<IsotopicProfile> potentialIsotopicProfiles)
        {
            var standardDeviationOfEachSet = new double[correlations.Count];
            var averageCorrOfEachSet = new double[correlations.Count];
            var chargeStateSets = new List<int>[correlations.Count];
            var contendingCharges = chargeStates.ToList();//new List<int>();

            #region Metric 1, altCharge present
            var favorites = new Dictionary<int, double>();

            foreach (var contender in contendingCharges)
            {
                var contenderIndex = Array.IndexOf(chargeStates, contender);
                var anotherChargeStateExists = AnotherChargeStateExists(contenderIndex, correlationsWithAltChargeState);
                if (anotherChargeStateExists)
                {
                    //TODO: and no correlations of other non-factor charge states.
                    var correlationList = new double[correlationsWithAltChargeState.GetLength(1)];
                    for (var i = 0; i < correlationList.Length; i++)
                    {
                        correlationList[i] = correlationsWithAltChargeState[contenderIndex, i];
                    }
                    var bestAltCorr = correlationList.Max();
                    favorites.Add(contenderIndex, bestAltCorr);
                }
            }
            if (favorites.Count > 0)
            {
                return potentialIsotopicProfiles.ElementAt((favorites.OrderByDescending(x => x.Value).First().Key));
            }

            #endregion

            #region Metric 2, stand dev
            for (var i = 0; i < correlations.Count; i++)
            {
                var indexesWhoAreFactorsOfMe = GetIndexesWhoAreAFactorOfMe(i, chargeStates);
                if (null == indexesWhoAreFactorsOfMe)
                {
                    break; //null means that we are at the end of the set. st dev is already defaulted at 0, which is what it
                    //would be to take the st. dev of one item.
                }
                var length = indexesWhoAreFactorsOfMe.Count + 1;
                var arrayOfCorrelationsInSet = new double[length];

                arrayOfCorrelationsInSet[0] = correlations[i];
                for (var i2 = 1; i2 < length; i2++)
                {
                    arrayOfCorrelationsInSet[i2] = correlations[indexesWhoAreFactorsOfMe.ElementAt(i2 - 1)];
                }
                chargeStateSets[i] = GetSet(i, indexesWhoAreFactorsOfMe, chargeStates);
                standardDeviationOfEachSet[i] = MathNet.Numerics.Statistics.Statistics.StandardDeviation(arrayOfCorrelationsInSet);
                averageCorrOfEachSet[i] = MathNet.Numerics.Statistics.Statistics.Mean(arrayOfCorrelationsInSet);

                var correlationThreshold = 0.49;
                var standardDeviationThreshold = .3;

                if (standardDeviationOfEachSet[i] < standardDeviationThreshold && correlations[i] > correlationThreshold)   // Dangerous 0.05 and .7
                {
                    foreach (var index in indexesWhoAreFactorsOfMe)
                    {
                        contendingCharges.Remove(chargeStates[index]);
                    }
                }
                if (contendingCharges.Count == 1)//if there is only one left after it's own factors are removed, it's that one.
                {
                    // IqLogger.Log.Debug("\nWas only one contender\n");
                    return potentialIsotopicProfiles.ElementAt(Array.IndexOf(chargeStates, contendingCharges[0]));
                }
            }
            #endregion

            #region Metric 3, ask Patterson

            var chargeStateCalculator = new PattersonChargeStateCalculatorWithChanges();
            var chargeState = chargeStateCalculator.GetChargeState(_run.XYData, _run.PeakList, potentialIsotopicProfiles.First().getMonoPeak());
            IqLogger.LogDebug("had to use the patterson calculator and this is what it gave me: " + chargeState);
            foreach (var charge in chargeStates)
            {
                IqLogger.LogDebug(charge + "\t");
            }
            IqLogger.LogDebug("Charge state length: " + chargeStates.Length);
            if (chargeStates.Contains(chargeState))
            {
                IqLogger.LogDebug(Array.IndexOf(chargeStates, chargeState).ToString());
                return potentialIsotopicProfiles.ElementAt(Array.IndexOf(chargeStates, chargeState));
            }

            var bestChargeIndex = -1;
            var bestCorrelation = -6.0;//arbitrary negative
            foreach (var charge in contendingCharges)
            {
                var index = Array.IndexOf(chargeStates, charge);
                if (bestCorrelation < correlations[index])
                {
                    bestCorrelation = correlations[index];
                    bestChargeIndex = index;
                }
            }
            if (bestCorrelation <= 0)
            {
                return null;
            }
            return potentialIsotopicProfiles.ElementAt(bestChargeIndex);

            #endregion
        }

        private bool AnotherChargeStateExists(int contenderIndex, double[,] correlationsWithAltChargeState)
        {
            for (var i = 0; i < correlationsWithAltChargeState.GetLength(1); i++)
            {
                if (Math.Abs(correlationsWithAltChargeState[contenderIndex, i] + 3) > float.Epsilon &&
                    correlationsWithAltChargeState[contenderIndex, i] <= 1.1 &&
                    correlationsWithAltChargeState[contenderIndex, i] > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private List<int> GetSet(int i, IEnumerable<int> indexesWhoAreFactorsOfMe, IReadOnlyList<int> chargeStates)
        {
            var list = new List<int> { chargeStates[i] };

            foreach (var index in indexesWhoAreFactorsOfMe)
            {
                list.Add(chargeStates[index]);
            }
            return list;
        }

        private double GetCorrelation(IsotopicProfile potentialFeature)
        {
            //TODO: change how many correlations are done here. right now only correlates 2 peaks on each features and looks at that correlation
            var monoIndex = potentialFeature.MonoIsotopicPeakIndex;
            int otherPeakToLookForIndex;
            if (monoIndex == 0) { otherPeakToLookForIndex = 1; }
            else if (monoIndex < 0)
            {
                //HELP not sure how monoIndex becomes less than 0...
                monoIndex = 0;
                otherPeakToLookForIndex = 1;
            }
            else { otherPeakToLookForIndex = monoIndex - 1; }

            return getCorrelation(monoIndex, otherPeakToLookForIndex, potentialFeature);
        }
        private double getCorrelation(int peak1Index, int peak2Index, IsotopicProfile potentialFeature)
        {
            double widthPeak1 = potentialFeature.Peaklist[peak1Index].Width;
            var xValuePeak1 = potentialFeature.Peaklist[peak1Index].XValue;
            var ppmTolerancePeak1 = (widthPeak1 / 2.35) / xValuePeak1 * 1e6;    //   peak's sigma value / mz * 1e6

            double widthPeak2 = potentialFeature.Peaklist[peak2Index].Width;
            var xValuePeak2 = potentialFeature.Peaklist[peak2Index].XValue;
            var ppmTolerancePeak2 = (widthPeak2 / 2.35) / xValuePeak2 * 1e6;    //   peak's sigma value / mz * 1e6

            //altCorr = getCorrelation(xValuePeak1, getMZofLowerChargeState(potentialFeature, peak1Index), ppmTolerancePeak1, ppmTolerancePeak2);

            return getCorrelation(xValuePeak1, xValuePeak2, ppmTolerancePeak1, ppmTolerancePeak2);
        }

        private double getCorrelation(double mzPeak1, double mzPeak2, double tolerancePpmPeak1, double tolerancePpmPeak2)
        {
            var chromGenPeak1 = new PeakChromatogramGenerator
            {
                ChromatogramGeneratorMode = Globals.ChromatogramGeneratorMode.TOP_N_PEAKS,
                TopNPeaksLowerCutOff = 0.4
            };
            //TODO: correlate narrow range. -100 +100
            //TODO: for gord.
            var scanWindow = 100;
            var lowerScan = _run.CurrentScanSet.PrimaryScanNumber - scanWindow;
            var upperScan = _run.CurrentScanSet.PrimaryScanNumber + scanWindow;

            var chromXYDataPeak1 = chromGenPeak1.GenerateChromatogram(_run, lowerScan, upperScan, mzPeak1, tolerancePpmPeak1);
            var chromXYDataPeak2 = chromGenPeak1.GenerateChromatogram(_run, lowerScan, upperScan, mzPeak2, tolerancePpmPeak2);

            //var chromXYDataPeak1 = chromGenPeak1.GenerateChromatogram(_run, 1, _run.GetNumMSScans(), mzPeak1, tolerancePpmPeak1, Globals.ToleranceUnit.PPM);
            //var chromXYDataPeak2 = chromGenPeak1.GenerateChromatogram(_run, 1, _run.GetNumMSScans(), mzPeak2, tolerancePpmPeak2, Globals.ToleranceUnit.PPM);
            if (null == chromXYDataPeak1 || null == chromXYDataPeak2) { return -3.0; }
            var overlap = AlignAndFillArraysToCorrelate(chromXYDataPeak1, chromXYDataPeak2, out var arrayToCorrelatePeak1, out var arrayToCorrelatePeak2);
            if (overlap)
            {
                var corr = MathNet.Numerics.Statistics.Correlation.Pearson(arrayToCorrelatePeak1, arrayToCorrelatePeak2);
                if (double.IsNaN(corr))
                {
                    return -2;   //it's present, but they don't overlap any. same as other -2 value.
                }
                //TODO: implement iq standard of getting linear regression.
                return MathNet.Numerics.Statistics.Correlation.Pearson(arrayToCorrelatePeak1, arrayToCorrelatePeak2);
            }
            return -2;
        }

        private bool AlignAndFillArraysToCorrelate(XYData chromXYDataPeak1, XYData chromXYDataPeak2, out double[] arrayToCorrelatePeak1, out double[] arrayToCorrelatePeak2)
        {
            arrayToCorrelatePeak1 = null;
            arrayToCorrelatePeak2 = null;

            chromXYDataPeak1.NormalizeYData();
            chromXYDataPeak2.NormalizeYData();
            //Console.WriteLine("chromXYDataPeak1");
            //chromXYDataPeak1.Display();
            //Console.WriteLine("chromXYDataPeak2");
            //chromXYDataPeak2.Display();
            var lowestFramePeak1 = chromXYDataPeak1.Xvalues.Min();
            var lowestFramePeak2 = chromXYDataPeak2.Xvalues.Min();
            var highestFramePeak1 = chromXYDataPeak1.Xvalues.Max();
            var highestFramePeak2 = chromXYDataPeak2.Xvalues.Max();
            var minX = Math.Max(lowestFramePeak1, lowestFramePeak2);
            var maxX = Math.Min(highestFramePeak1, highestFramePeak2);
            var Overlap = (minX < maxX);
            if (!Overlap)
            {
                return false;
            }
            var chromPeak1StartIndex = chromXYDataPeak1.GetClosestXVal(minX);
            var chromPeak2StartIndex = chromXYDataPeak2.GetClosestXVal(minX);
            var chromPeak1StopIndex = chromXYDataPeak1.GetClosestXVal(maxX);
            var chromPeak2StopIndex = chromXYDataPeak2.GetClosestXVal(maxX);

            arrayToCorrelatePeak1 = new double[chromPeak1StopIndex - chromPeak1StartIndex + 1];
            arrayToCorrelatePeak2 = new double[chromPeak2StopIndex - chromPeak2StartIndex + 1];

            for (int i = chromPeak1StartIndex, j = 0; i <= chromPeak1StopIndex; i++, j++)
            {
                arrayToCorrelatePeak1[j] = chromXYDataPeak1.Yvalues[i];
            }
            for (int i = chromPeak2StartIndex, j = 0; i <= chromPeak2StopIndex; i++, j++)
            {
                arrayToCorrelatePeak2[j] = chromXYDataPeak2.Yvalues[i];
            }

            if (arrayToCorrelatePeak1.Length < 5 || arrayToCorrelatePeak2.Length < 5)
            {
                return false;
            }

            return true;
        }

        private HashSet<int> GetIndexesWhoAreAFactorOfMe(int index, IReadOnlyList<int> chargeStates)
        {
            if (index == chargeStates.Count - 1) return null;

            var number = chargeStates[index];
            var indexesWhoAreFactorsOfMe = new HashSet<int>();
            for (var i = index + 1; i < chargeStates.Count; i++)
            {
                if (number % chargeStates[i] == 0) indexesWhoAreFactorsOfMe.Add(i);
            }
            return indexesWhoAreFactorsOfMe;
        }

        internal HashSet<int> GetPotentialChargeState(int indexOfCurrentPeak, List<Peak> msPeakList, double ppmTolerance)
        {
            //List<IsotopicProfile> potentialProfiles = new List<IsotopicProfile>();
            CreatePeaksIfNeeded();//note: does not actually create peaks. Only loads them. An exception is thrown if it's not there.

            var chargeStates = new HashSet<int>();
            var charge = 1;
            for (; charge < 10; charge++)
            {
                var mzPeak1 = msPeakList.ElementAt(indexOfCurrentPeak).XValue;
                var distanceToFindNextPeak = 1.0 / charge;
                var xValueToLookFor = mzPeak1 + distanceToFindNextPeak;
                var lowerMZ = xValueToLookFor - ppmTolerance * xValueToLookFor / 1e6;
                var upperMZ = xValueToLookFor + ppmTolerance * xValueToLookFor / 1e6;

                var peak2 = msPeakList.Find(peak => peak.XValue <= upperMZ && peak.XValue >= lowerMZ);
                if (peak2 == null)
                {
                    continue;
                }
                var mzPeak2 = peak2.XValue;
                var correlation = getCorrelation(mzPeak1, mzPeak2, ppmTolerance, ppmTolerance);
                if (correlation > 0.2)
                {
                    chargeStates.Add(charge);
                }
            }

            var reportString521 = "Potential Charges using correlation: ";
            foreach (var currentCharge in chargeStates)
            {
                reportString521 += currentCharge + "\t";
            }
            IqLogger.LogTrace(reportString521 + "\n");
            return chargeStates;
        }

        [Obsolete("Unused")]
        private void CreatePeaksFile()
        {
            _run.ScanSetCollection.Create(_run, 1, 1, false);
            //PeakDetectAndExportWorkflowParameters parameters = new PeakDetectAndExportWorkflowParameters();
            //parameters.LCScanMin = 5500;
            //parameters.LCScanMax = 6500;
            //parameters.OutputDirectory = outputDirectory;

            //PeakDetectAndExportWorkflow workflow = new PeakDetectAndExportWorkflow(run, parameters);
            //workflow.Execute();
        }

        [Obsolete("Unused")]
        private void ExportPeaks_copied(string peaksFile)
        {
            var msGen = MSGeneratorFactory.CreateMSGenerator(_run.MSFileType);

            var _ms1PeakDetector = new DeconToolsPeakDetectorV2(2.0, 2.0);

            var _ms2PeakDetectorForProfileData = new DeconToolsPeakDetectorV2(2.0, 2.0);
            var _ms2PeakDetectorForCentroidedData = new DeconToolsPeakDetectorV2(0, 0, Globals.PeakFitType.QUADRATIC, true)
            {
                RawDataType = Globals.RawDataType.Centroided
            };

            _ms2PeakDetectorForProfileData.PeaksAreStored = true;
            _ms2PeakDetectorForCentroidedData.PeaksAreStored = true;
            _ms1PeakDetector.PeaksAreStored = true;

            var peakExporter = new PeakListTextExporter(_run.MSFileType, peaksFile);

            var scanCounter = 0;

            if (_run.MSFileType == Globals.MSFileType.PNNL_UIMF)
            {
                if (!(_run is UIMFRun uimfRun))
                    throw new InvalidCastException("_run is not of type UIMFRun");

                var numTotalFrames = uimfRun.ScanSetCollection.ScanSetList.Count;
                var frameCounter = 0;

                foreach (var frameSet in uimfRun.ScanSetCollection.ScanSetList)
                {
                    frameCounter++;
                    uimfRun.CurrentScanSet = frameSet;
                    uimfRun.ResultCollection.MSPeakResultList.Clear();

                    foreach (var scanSet in uimfRun.IMSScanSetCollection.ScanSetList)
                    {
                        uimfRun.CurrentIMSScanSet = (IMSScanSet)scanSet;
                        msGen.Execute(uimfRun.ResultCollection);
                        _ms1PeakDetector.Execute(uimfRun.ResultCollection);
                    }
                    peakExporter.WriteOutPeaks(uimfRun.ResultCollection.MSPeakResultList);

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
