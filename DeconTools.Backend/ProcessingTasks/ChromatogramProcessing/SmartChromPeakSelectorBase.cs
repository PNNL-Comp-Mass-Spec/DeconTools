
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

        #region Constructors
        #endregion

        #region Properties

        public DeconToolsPeakDetector MSPeakDetector { get; set; }
        //public DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders.BasicTFF TargetedMSFeatureFinder { get; set; }

        public TFFBase TargetedMSFeatureFinder { get; set; }
        
        protected SmartChromPeakSelectorParameters _parameters;
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

			List<ChromPeakQualityData> peakQualityList = new List<ChromPeakQualityData>();

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
					ChromPeakQualityData pq = new ChromPeakQualityData(chromPeak);
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
                    AddScoresToPeakQualityData(pq, currentResult);

#if DEBUG
                    pq.Display();
#endif
                }

                //run a algorithm that decides, based on fit score mostly. 
                bestChromPeak = determineBestChromPeak(peakQualityList, currentResult);
            }

			currentResult.ChromPeakQualityList = peakQualityList;

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

		protected void AddScoresToPeakQualityData(ChromPeakQualityData pq, TargetedResultBase currentResult)
        {
            if (currentResult.IsotopicProfile == null)
            {
                pq.IsotopicProfileFound = false;
                return;
            }
            else
            {
                pq.IsotopicProfileFound = true;
                pq.Abundance = currentResult.IsotopicProfile.IntensityAggregate;
                pq.FitScore = currentResult.Score;
                pq.InterferenceScore = currentResult.InterferenceScore;
            	pq.IsotopicProfile = currentResult.IsotopicProfile;
                bool resultHasFlags = (currentResult.Flags != null && currentResult.Flags.Count > 0);
                pq.IsIsotopicProfileFlagged = resultHasFlags;
            }
        }

        //TODO: delete this if unused
        protected void SetDefaultTargetedFeatureFinderSettings(double toleranceInPPM)
        {
            TargetedMSFeatureFinder.ToleranceInPPM = toleranceInPPM;
        }
        #endregion


		protected virtual ChromPeak determineBestChromPeak(List<ChromPeakQualityData> peakQualityList, TargetedResultBase currentResult)
        {
            var filteredList1 = (from n in peakQualityList
                                 where n.IsotopicProfileFound == true &&
                                 n.FitScore < 1 && n.InterferenceScore < 1 &&
                                 n.IsIsotopicProfileFlagged == false
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
                bestpeak = filteredList1[0].Peak;
            }
            else
            {
                filteredList1 = filteredList1.OrderBy(p => p.FitScore).ToList();

                double diffFirstAndSecondFitScores = Math.Abs(filteredList1[0].FitScore - filteredList1[1].FitScore);

                bool differenceIsSmall = (diffFirstAndSecondFitScores < 0.05);
                if (differenceIsSmall)
                {
                    if (_parameters.MultipleHighQualityMatchesAreAllowed)
                    {

                        if (filteredList1[0].Abundance >= filteredList1[1].Abundance)
                        {
                            bestpeak = filteredList1[0].Peak;
                        }
                        else
                        {
                            bestpeak = filteredList1[1].Peak;
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
                    bestpeak = filteredList1[0].Peak;
                }
            }

            return bestpeak;
        }
    }
}
