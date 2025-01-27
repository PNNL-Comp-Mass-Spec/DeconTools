﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public class SmartChromPeakSelectorUIMF : SmartChromPeakSelector
    {
        #region Constructors
        public SmartChromPeakSelectorUIMF(SmartChromPeakSelectorParameters parameters)
            : base(parameters)
        {
        }
        #endregion

        protected override void UpdateResultWithChromPeakAndLCScanInfo(TargetedResultBase result, ChromPeak bestPeak)
        {
            if (!(result.Run is UIMFRun uimfRun))
            {
                throw new InvalidCastException("result.Run is not of type UIMFRun in UpdateResultWithChromPeakAndLCScanInfo");
            }

            result.ChromPeakSelected = bestPeak;

            result.ScanSet = new ScanSet(uimfRun.CurrentScanSet.PrimaryScanNumber);
        }

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, Name + " failed. MassTag was not defined.");

            var currentResult = resultList.GetTargetedResult(resultList.Run.CurrentMassTag);

            if (msGen == null)
            {
                msGen = MSGeneratorFactory.CreateMSGenerator(resultList.Run.MSFileType);
                msGen.IsTICRequested = false;
            }

            var currentTarget = resultList.Run.CurrentMassTag;

            // Set the MS Generator to use a window around the target so that we do not grab a lot of unnecessary data from the UIMF file
            msGen.MinMZ = currentTarget.MZ - 10;
            msGen.MaxMZ = currentTarget.MZ + 10;

            float normalizedElutionTime;

            if (currentResult.Run.CurrentMassTag.ElutionTimeUnit == DeconTools.Backend.Globals.ElutionTimeUnit.ScanNum)
            {
                normalizedElutionTime = resultList.Run.CurrentMassTag.ScanLCTarget / (float)currentResult.Run.GetNumMSScans();
            }
            else
            {
                normalizedElutionTime = resultList.Run.CurrentMassTag.NormalizedElutionTime;
            }

            // Collect Chrom peaks that fall within the NET tolerance
            var peaksWithinTol = new List<ChromPeak>(); //

            foreach (var item in resultList.Run.PeakList)
            {
                var peak = (ChromPeak)item;
                if (Math.Abs(peak.NETValue - normalizedElutionTime) <= Parameters.NETTolerance)     // Peak.NETValue was determined by the ChromPeakDetector or a future ChromAligner Task
                {
                    peaksWithinTol.Add(peak);
                }
            }

            var peakQualityList = new List<ChromPeakQualityData>();

            // Iterate over peaks within tolerance and score each peak according to MSFeature quality
#if DEBUG
            var tempMinScanWithinTol = (int) resultList.Run.NetAlignmentInfo.GetScanForNet(normalizedElutionTime - Parameters.NETTolerance);
            var tempMaxScanWithinTol = (int)resultList.Run.NetAlignmentInfo.GetScanForNet(normalizedElutionTime + Parameters.NETTolerance);
            var tempCenterTol = (int) resultList.Run.NetAlignmentInfo.GetScanForNet(normalizedElutionTime);

            Console.WriteLine("SmartPeakSelector --> NETTolerance= " + Parameters.NETTolerance + ";  chromMinCenterMax= " + tempMinScanWithinTol + "\t" + tempCenterTol + "" +
                              "\t" + tempMaxScanWithinTol);
            Console.WriteLine("MT= " + currentResult.Target.ID + ";z= " + currentResult.Target.ChargeState + "; mz= " + currentResult.Target.MZ.ToString("0.000") + ";  ------------------------- PeaksWithinTol = " + peaksWithinTol.Count);
#endif

            currentResult.NumChromPeaksWithinTolerance = peaksWithinTol.Count;
            currentResult.NumQualityChromPeaks = -1;

            ChromPeak bestChromPeak;
            if (currentResult.NumChromPeaksWithinTolerance > _parameters.NumChromPeaksAllowed)
            {
                bestChromPeak = null;
            }
            else
            {
                foreach (var chromPeak in peaksWithinTol)
                {
                    var pq = new ChromPeakQualityData(chromPeak);
                    peakQualityList.Add(pq);

                    // TODO: Currently hard-coded to sum only 1 scan
                    resultList.Run.CurrentScanSet=  ChromPeakUtilities.GetLCScanSetForChromPeakUIMF(chromPeak, resultList.Run, 1);

                    // This resets the flags and the scores on a given result
                    currentResult.ResetResult();

                    // Generate a mass spectrum
                    msGen.Execute(resultList);

                    // Find isotopic profile
                    TargetedMSFeatureFinder.Execute(resultList);

                    // Get fit score
                    fitScoreCalc.Execute(resultList);

                    // Get i_score
                    resultValidator.Execute(resultList);

                    // Collect the results together
#pragma warning disable 618
                    AddScoresToPeakQualityData(pq, currentResult);
#pragma warning restore 618

#if DEBUG
                    pq.Display();
#endif
                }

                // Run a algorithm that decides, based on fit score mostly.
                bestChromPeak = determineBestChromPeak(peakQualityList, currentResult);
            }

            currentResult.ChromPeakQualityList = peakQualityList;

            resultList.Run.CurrentScanSet=ChromPeakUtilities.GetLCScanSetForChromPeakUIMF(bestChromPeak, resultList.Run, Parameters.NumScansToSum);

            UpdateResultWithChromPeakAndLCScanInfo(currentResult, bestChromPeak);
        }

        protected new virtual ChromPeak determineBestChromPeak(List<ChromPeakQualityData> peakQualityList, TargetedResultBase currentResult)
        {
            var filteredList1 = (from n in peakQualityList
                                 where n.IsotopicProfileFound &&
                                 n.FitScore < 1 && n.InterferenceScore < 1
                                 select n).ToList();

            ChromPeak bestPeak;

            currentResult.NumQualityChromPeaks = filteredList1.Count;

            if (filteredList1.Count == 0)
            {
                bestPeak = null;
                currentResult.FailedResult = true;
                currentResult.FailureType = DeconTools.Backend.Globals.TargetedResultFailureType.ChrompeakNotFoundWithinTolerances;
            }
            else if (filteredList1.Count == 1)
            {
                bestPeak = filteredList1[0].Peak;
            }
            else
            {
                filteredList1 = filteredList1.OrderBy(p => p.FitScore).ToList();

                var diffFirstAndSecondFitScores = Math.Abs(filteredList1[0].FitScore - filteredList1[1].FitScore);

                var differenceIsSmall = (diffFirstAndSecondFitScores < 0.05);
                if (differenceIsSmall)
                {
                    if (_parameters.MultipleHighQualityMatchesAreAllowed)
                    {
                        if (filteredList1[0].Abundance >= filteredList1[1].Abundance)
                        {
                            bestPeak = filteredList1[0].Peak;
                        }
                        else
                        {
                            bestPeak = filteredList1[1].Peak;
                        }
                    }
                    else
                    {
                        bestPeak = null;
                        currentResult.FailedResult = true;
                        currentResult.FailureType = DeconTools.Backend.Globals.TargetedResultFailureType.TooManyHighQualityChrompeaks;
                    }
                }
                else
                {
                    bestPeak = filteredList1[0].Peak;
                }
            }

            // If any of the peaks were good, then we want to make sure to not consider the result an error.
            // I added this because if the last peak checked had an error, the entire result was still flagged as having an error.
            if (bestPeak != null)
            {
                currentResult.FailedResult = false;
                currentResult.FailureType = DeconTools.Backend.Globals.TargetedResultFailureType.None;
            }

            return bestPeak;
        }
    }
}
