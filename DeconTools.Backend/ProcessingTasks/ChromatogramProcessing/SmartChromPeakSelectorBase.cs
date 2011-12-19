﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{



    public abstract class SmartChromPeakSelectorBase : ChromPeakSelectorBase
    {

        protected DeconTools.Backend.ProcessingTasks.MSGenerator msgen;
        protected DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidatorTask resultValidator;
        protected MassTagFitScoreCalculator fitScoreCalc;

        ScanSetFactory scansetFactory = new ScanSetFactory();

        protected class PeakQualityData
        {

            internal PeakQualityData(ChromPeak peak)
            {
                this.i_score = 1;     // worst possible
                this.fitScore = 1;   // worst possible
                this.abundance = 0;
                this.peak = peak;
                this.isotopicProfileFound = false;
            }

            internal ChromPeak peak;
            internal bool isotopicProfileFound;
            internal double fitScore;
            internal double i_score;
            internal double abundance;

            internal bool isIsotopicProfileFlagged;


            internal void Display()
            {
                Console.WriteLine(peak.XValue.ToString("0.00") + "\t" + peak.NETValue.ToString("0.0000") + "\t" + abundance + "\t" + fitScore.ToString("0.0000") + "\t" + i_score.ToString("0.000"));
            }
        }

        #region Constructors
        #endregion

        #region Properties

        public DeconToolsPeakDetector MSPeakDetector { get; set; }
        //public DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders.BasicTFF TargetedMSFeatureFinder { get; set; }

        public TFFBase TargetedMSFeatureFinder { get; set; }

        public SmartChromPeakSelectorParameters Parameters { get; set; }



        #endregion

        #region Public Methods
        public override void Execute(ResultCollection resultColl)
        {
            Check.Require(resultColl.Run.CurrentMassTag != null, this.Name + " failed. MassTag was not defined.");

            if (msgen == null)
            {
                msgen = MSGeneratorFactory.CreateMSGenerator(resultColl.Run.MSFileType);
                msgen.IsTICRequested = false;
            }

            TargetBase mt = resultColl.Run.CurrentMassTag;

            //collect Chrom peaks that fall within the NET tolerance
            List<ChromPeak> peaksWithinTol = new List<ChromPeak>(); // 
            foreach (ChromPeak peak in resultColl.Run.PeakList)
            {
                if (Math.Abs(peak.NETValue - mt.NormalizedElutionTime) <= Parameters.NETTolerance)     //peak.NETValue was determined by the ChromPeakDetector or a future ChromAligner Task
                {
                    peaksWithinTol.Add(peak);
                }
            }


            List<PeakQualityData> peakQualityList = new List<PeakQualityData>();

            TargetedResultBase currentResult = resultColl.GetTargetedResult(resultColl.Run.CurrentMassTag);

            //iterate over peaks within tolerance and score each peak according to MSFeature quality
            //Console.WriteLine("MT= " + currentResult.MassTag.ID + ";z= " + currentResult.MassTag.ChargeState + "; mz= " + currentResult.MassTag.MZ.ToString("0.000") + ";  ------------------------- PeaksWithinTol = " + peaksWithinTol.Count);

            currentResult.NumChromPeaksWithinTolerance = peaksWithinTol.Count;
            currentResult.NumQualityChromPeaks = -1;

            ChromPeak bestChromPeak;
            if (currentResult.NumChromPeaksWithinTolerance > this.Parameters.NumChromPeaksAllowed)
            {
                bestChromPeak = null;
            }
            else
            {
                foreach (var peak in peaksWithinTol)
                {
                    ScanSet scanset = createNonSummedScanSet(peak, resultColl.Run);
                    PeakQualityData pq = new PeakQualityData(peak);
                    peakQualityList.Add(pq);

                    resultColl.Run.CurrentScanSet = scanset;

                    //This resets the flags and the scores on a given result
                    currentResult.ResetResult();

                    //generate a mass spectrum
                    msgen.Execute(resultColl);

                    //detect peaks
                    MSPeakDetector.Execute(resultColl);

                    //find isotopic profile
                    TargetedMSFeatureFinder.Execute(resultColl);

                    //get fit score
                    fitScoreCalc.Execute(resultColl);

                    //get i_score
                    resultValidator.Execute(resultColl);

                    //collect the results together
                    addScoresToPeakQualityData(pq, currentResult);

                    //pq.Display();

                }


                //run a algorithm that decides, based on fit score mostly. 
                bestChromPeak = determineBestChromPeak(peakQualityList, currentResult);
            }


            ScanSet bestScanset = createSummedScanSet(bestChromPeak, resultColl.Run);
            resultColl.Run.CurrentScanSet = bestScanset;   // maybe good to set this here so that the MSGenerator can operate on it...  

            currentResult.AddSelectedChromPeakAndScanSet(bestChromPeak, bestScanset);

            bool failedChromPeakSelection = (currentResult.ChromPeakSelected == null || currentResult.ChromPeakSelected.XValue == 0);
            if (failedChromPeakSelection)
            {
                currentResult.FailedResult = true;
                currentResult.FailureType = Globals.TargetedResultFailureType.CHROMPEAK_NOT_FOUND_WITHIN_TOLERANCES;
            }

        }

        #endregion

        #region Private Methods


        private void addScoresToPeakQualityData(PeakQualityData pq, TargetedResultBase currentResult)
        {
            if (currentResult.IsotopicProfile == null)
            {
                pq.isotopicProfileFound = false;
                return;
            }
            else
            {
                pq.isotopicProfileFound = true;
                pq.abundance = currentResult.IsotopicProfile.IntensityAggregate;
                pq.fitScore = currentResult.Score;
                pq.i_score = currentResult.InterferenceScore;

                bool resultHasFlags = (currentResult.Flags != null && currentResult.Flags.Count > 0);
                pq.isIsotopicProfileFlagged = resultHasFlags;
            }
        }

        private ScanSet createSummedScanSet(ChromPeak chromPeak, Run run)
        {
            if (chromPeak == null || chromPeak.XValue == 0) return null;

            int bestScan = (int)chromPeak.XValue;
            bestScan = run.GetClosestMSScan(bestScan, Globals.ScanSelectionMode.CLOSEST);

            switch (this.Parameters.SummingMode)
            {
                case SummingModeEnum.SUMMINGMODE_STATIC:
                    return scansetFactory.CreateScanSet(run, bestScan, this.Parameters.NumScansToSum);

                case SummingModeEnum.SUMMINGMODE_DYNAMIC:
                    double sigma = chromPeak.Width / 2.35;

                    int lowerScan = (int)Math.Round(chromPeak.XValue - (this.Parameters.AreaOfPeakToSumInDynamicSumming * sigma));
                    int closestLowerScan = run.GetClosestMSScan(lowerScan, Globals.ScanSelectionMode.CLOSEST);

                    int upperScan = (int)Math.Round(chromPeak.XValue + (this.Parameters.AreaOfPeakToSumInDynamicSumming * sigma));
                    int closestUpperScan = run.GetClosestMSScan(upperScan, Globals.ScanSelectionMode.CLOSEST);

                    ScanSet scanset = scansetFactory.CreateScanSet(run, bestScan, closestLowerScan, closestUpperScan);
                    scansetFactory.TrimScans(scanset, this.Parameters.MaxScansSummedInDynamicSumming);

                    return scanset;

                default:
                    return scansetFactory.CreateScanSet(run, bestScan, this.Parameters.NumScansToSum);
            }



        }

        private ScanSet createNonSummedScanSet(ChromPeak chromPeak, Run run)
        {
            if (chromPeak == null || chromPeak.XValue == 0) return null;

            int bestScan = (int)chromPeak.XValue;
            bestScan = run.GetClosestMSScan(bestScan, Globals.ScanSelectionMode.CLOSEST);

            int numScansToSum = 1;
            return new ScanSetFactory().CreateScanSet(run, bestScan, numScansToSum);

        }


        //TODO: delete this if unused
        protected void SetDefaultMSPeakDetectorSettings(double peakBR, double signoiseRatio, Globals.PeakFitType peakFitType, bool isThresholded)
        {
            MSPeakDetector.PeakBackgroundRatio = peakBR;
            MSPeakDetector.SigNoiseThreshold = signoiseRatio;
            MSPeakDetector.PeakFitType = peakFitType;
            MSPeakDetector.IsDataThresholded = isThresholded;
        }

        //TODO: delete this if unused
        protected void SetDefaultTargetedFeatureFinderSettings(double toleranceInPPM)
        {
            TargetedMSFeatureFinder.ToleranceInPPM = toleranceInPPM;
        }
        #endregion


        protected virtual ChromPeak determineBestChromPeak(List<PeakQualityData> peakQualityList, TargetedResultBase currentResult)
        {
            var filteredList1 = (from n in peakQualityList
                                 where n.isotopicProfileFound == true &&
                                 n.fitScore < 1 && n.i_score < 1 &&
                                 n.isIsotopicProfileFlagged == false
                                 select n).ToList();

            ChromPeak bestpeak;

            currentResult.NumQualityChromPeaks = filteredList1.Count;

            if (filteredList1.Count == 0)
            {
                bestpeak = null;
                currentResult.FailedResult = true;
                currentResult.FailureType = Globals.TargetedResultFailureType.CHROMPEAK_NOT_FOUND_WITHIN_TOLERANCES;
            }
            else if (filteredList1.Count == 1)
            {
                bestpeak = filteredList1[0].peak;
            }
            else
            {

                filteredList1 = filteredList1.OrderBy(p => p.fitScore).ToList();



                double diffFirstAndSecondFitScores = Math.Abs(filteredList1[0].fitScore - filteredList1[1].fitScore);

                bool differenceIsSmall = (diffFirstAndSecondFitScores < 0.05);
                if (differenceIsSmall)
                {
                    if (this.Parameters.MultipleHighQualityMatchesAreAllowed)
                    {

                        if (filteredList1[0].abundance >= filteredList1[1].abundance)
                        {
                            bestpeak = filteredList1[0].peak;
                        }
                        else
                        {
                            bestpeak = filteredList1[1].peak;
                        }


                    }
                    else
                    {
                        bestpeak = null;
                        currentResult.FailedResult = true;
                        currentResult.FailureType = Globals.TargetedResultFailureType.TOO_MANY_HIGH_QUALITY_CHROMPEAKS;
                    }
                }
                else
                {
                    bestpeak = filteredList1[0].peak;
                }



            }

            return bestpeak;
        }



    }
}
