using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{



    public abstract class SmartChromPeakSelectorBase : ChromPeakSelectorBase
    {

        protected MSGenerator msgen;
        protected DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidatorTask resultValidator;
        protected MassTagFitScoreCalculator fitScoreCalc;

        

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

        
        private SmartChromPeakSelectorParameters _parameters;
        public override ChromPeakSelectorParameters Parameters
        {
            get { return _parameters; }
            set { _parameters = value as SmartChromPeakSelectorParameters; }
        }



        #endregion

        #region Public Methods
        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, this.Name + " failed. MassTag was not defined.");


            TargetedResultBase currentResult = resultList.GetTargetedResult(resultList.Run.CurrentMassTag);

            if (msgen == null)
            {
                msgen = MSGeneratorFactory.CreateMSGenerator(resultList.Run.MSFileType);
                msgen.IsTICRequested = false;
            }

            TargetBase mt = resultList.Run.CurrentMassTag;


            float normalizedElutionTime;

            if (currentResult.Run.CurrentMassTag.ElutionTimeUnit == Globals.ElutionTimeUnit.ScanNum)
            {
                normalizedElutionTime = resultList.Run.CurrentMassTag.ScanLCTarget / (float)currentResult.Run.GetNumMSScans();
            }
            else
            {
                normalizedElutionTime = resultList.Run.CurrentMassTag.NormalizedElutionTime;
            }



            //collect Chrom peaks that fall within the NET tolerance
            List<ChromPeak> peaksWithinTol = new List<ChromPeak>(); // 
            foreach (ChromPeak peak in resultList.Run.PeakList)
            {
                if (Math.Abs(peak.NETValue - normalizedElutionTime) <= Parameters.NETTolerance)     //peak.NETValue was determined by the ChromPeakDetector or a future ChromAligner Task
                {
                    peaksWithinTol.Add(peak);
                }
            }


            List<PeakQualityData> peakQualityList = new List<PeakQualityData>();

            

            //iterate over peaks within tolerance and score each peak according to MSFeature quality
            //Console.WriteLine("MT= " + currentResult.MassTag.ID + ";z= " + currentResult.MassTag.ChargeState + "; mz= " + currentResult.MassTag.MZ.ToString("0.000") + ";  ------------------------- PeaksWithinTol = " + peaksWithinTol.Count);

            currentResult.NumChromPeaksWithinTolerance = peaksWithinTol.Count;
            currentResult.NumQualityChromPeaks = -1;

            ChromPeak bestChromPeak;
            if (currentResult.NumChromPeaksWithinTolerance > _parameters.NumChromPeaksAllowed)
            {
                bestChromPeak = null;
            }
            else
            {
                foreach (var peak in peaksWithinTol)
                {
                    ScanSet scanset = createNonSummedScanSet(peak, resultList.Run);
                    PeakQualityData pq = new PeakQualityData(peak);
                    peakQualityList.Add(pq);

                    resultList.Run.CurrentScanSet = scanset;

                    //This resets the flags and the scores on a given result
                    currentResult.ResetResult();

                    //generate a mass spectrum
                    msgen.Execute(resultList);

                    //detect peaks
                    //MSPeakDetector.MinX = currentResult.Target.MZ - 10;
                    //MSPeakDetector.MaxX = currentResult.Target.MZ + 20;
                    //MSPeakDetector.Execute(resultList);

                    //find isotopic profile
                    TargetedMSFeatureFinder.Execute(resultList);

                    //get fit score
                    fitScoreCalc.Execute(resultList);

                    //get i_score
                    resultValidator.Execute(resultList);

                    //collect the results together
                    addScoresToPeakQualityData(pq, currentResult);

                    //pq.Display();

                }


                //run a algorithm that decides, based on fit score mostly. 
                bestChromPeak = determineBestChromPeak(peakQualityList, currentResult);
            }


            ScanSet bestScanset = CreateSummedScanSet(bestChromPeak, resultList.Run);
            resultList.Run.CurrentScanSet = bestScanset;   // maybe good to set this here so that the MSGenerator can operate on it...  

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

        

        private ScanSet createNonSummedScanSet(ChromPeak chromPeak, Run run)
        {
            if (chromPeak == null || chromPeak.XValue == 0) return null;

            int bestScan = (int)chromPeak.XValue;
            bestScan = run.GetClosestMSScan(bestScan, Globals.ScanSelectionMode.CLOSEST);

            int numScansToSum = 1;
            return new ScanSetFactory().CreateScanSet(run, bestScan, numScansToSum);

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
                    if (_parameters.MultipleHighQualityMatchesAreAllowed)
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
