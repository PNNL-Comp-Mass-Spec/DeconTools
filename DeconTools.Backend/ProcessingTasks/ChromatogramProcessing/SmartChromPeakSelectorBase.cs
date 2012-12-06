
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
                Console.WriteLine(peak.XValue.ToString("0.00") + "\t" + peak.NETValue.ToString("0.0000") + "\t" + abundance + "\t" + fitScore.ToString("0.0000") + "\t" + i_score.ToString("0.000") + "\t"+ isIsotopicProfileFlagged);
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
#if DEBUG
            int tempMinScanWithinTol = resultList.Run.GetScanValueForNET(normalizedElutionTime - Parameters.NETTolerance);
            int tempMaxScanWithinTol = resultList.Run.GetScanValueForNET(normalizedElutionTime + Parameters.NETTolerance);
            int tempCenterTol = resultList.Run.GetScanValueForNET(normalizedElutionTime);


            Console.WriteLine("SmartPeakSelector --> NETTolerance= "+ Parameters.NETTolerance + ";  chromMinCenterMax= " + tempMinScanWithinTol + "\t" + tempCenterTol + "" +
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
                    PeakQualityData pq = new PeakQualityData(chromPeak);
                    peakQualityList.Add(pq);

					// TODO: Currently hard-coded to sum only 1 scan
                    SetScansForMSGenerator(chromPeak, resultList.Run, 1);

                    //This resets the flags and the scores on a given result
                    currentResult.ResetResult();

                    //generate a mass spectrum
                    msgen.Execute(resultList);
    
                    //find isotopic profile
                    TargetedMSFeatureFinder.Execute(resultList);

                    //get fit score
                    fitScoreCalc.Execute(resultList);

                    //get i_score
                    resultValidator.Execute(resultList);

                    //collect the results together
                    addScoresToPeakQualityData(pq, currentResult);

#if DEBUG

                    pq.Display();
#endif
                }

                //run a algorithm that decides, based on fit score mostly. 
                bestChromPeak = determineBestChromPeak(peakQualityList, currentResult);
            }

            SetScansForMSGenerator(bestChromPeak, resultList.Run, Parameters.NumScansToSum);

            UpdateResultWithChromPeakAndLCScanInfo(currentResult, bestChromPeak);
            
           
            bool failedChromPeakSelection = (currentResult.ChromPeakSelected == null || currentResult.ChromPeakSelected.XValue == 0);
            if (failedChromPeakSelection)
            {
                currentResult.FailedResult = true;
                currentResult.FailureType = Globals.TargetedResultFailureType.ChrompeakNotFoundWithinTolerances;
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
                currentResult.FailureType = Globals.TargetedResultFailureType.ChrompeakNotFoundWithinTolerances;
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
                        currentResult.FailureType = Globals.TargetedResultFailureType.TooManyHighQualityChrompeaks;
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
