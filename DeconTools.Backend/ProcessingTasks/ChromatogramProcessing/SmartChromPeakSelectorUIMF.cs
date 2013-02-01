using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public class SmartChromPeakSelectorUIMF : SmartChromPeakSelector
    {

        #region Constructors
        public SmartChromPeakSelectorUIMF(SmartChromPeakSelectorParameters parameters)
            : base(parameters)
        {

        }
        #endregion

        protected override void SetScansForMSGenerator(Core.ChromPeak chromPeak, Core.Run run, int numLCScansToSum)
        {

            if (chromPeak == null || chromPeak.XValue == 0)
            {
                return;
                //throw new NullReferenceException("Trying to use chromPeak to generate mass spectrum, but chrompeak is null");
            }

            var uimfrun = run as UIMFRun;

            var chromPeakScan = (int)Math.Round(chromPeak.XValue);
            var bestLCScan = uimfrun.GetClosestMS1Frame(chromPeakScan);

            if (numLCScansToSum > 1)
            {
                throw new NotSupportedException("SmartChrompeakSelector is trying to set which frames are summed. But summing across frames isn't supported yet. Someone needs to add the code");
            }
            else
            {
                if (run.CurrentMassTag.MsLevel == 1)
                {
                    var lcscanset = new ScanSet(bestLCScan);
                    uimfrun.CurrentScanSet = lcscanset;
                }
                else
                {
                    // TODO: This is hard-coded to work with the "sum all consecutive MS2 frames mode" but we should really look these up by going through the IMSScanCollection
                    var lcscanset = new ScanSet(bestLCScan + 1, bestLCScan + 1, bestLCScan + uimfrun.GetNumberOfConsecutiveMs2Frames());
                    uimfrun.CurrentScanSet = lcscanset;
                }

                // TODO: Hard coded to sum across all IMS Scans.
                int centerScan = (uimfrun.MinIMSScan + uimfrun.MaxIMSScan + 1) / 2;
                uimfrun.CurrentIMSScanSet = new IMSScanSet(centerScan, uimfrun.MinIMSScan, uimfrun.MaxIMSScan);
            }

        }

        protected override void UpdateResultWithChromPeakAndLCScanInfo(Core.TargetedResultBase result, Core.ChromPeak bestPeak)
        {
            var uimfRun = result.Run as UIMFRun;

            result.ChromPeakSelected = bestPeak;

            result.ScanSet = new ScanSet(uimfRun.CurrentScanSet.PrimaryScanNumber);
        }

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, this.Name + " failed. MassTag was not defined.");

            TargetedResultBase currentResult = resultList.GetTargetedResult(resultList.Run.CurrentMassTag);

            if (msgen == null)
            {
                msgen = MSGeneratorFactory.CreateMSGenerator(resultList.Run.MSFileType);
                msgen.IsTICRequested = false;
            }

            TargetBase currentTarget = resultList.Run.CurrentMassTag;

            // Set the MS Generator to use a window around the target so that we do not grab a lot of unecessary data from the UIMF file
            msgen.MinMZ = currentTarget.MZ - 10;
            msgen.MaxMZ = currentTarget.MZ + 10;

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


        }

		protected virtual ChromPeak determineBestChromPeak(List<ChromPeakQualityData> peakQualityList, TargetedResultBase currentResult)
		{
			var filteredList1 = (from n in peakQualityList
								 where n.IsotopicProfileFound == true &&
								 n.FitScore < 1 && n.InterferenceScore < 1
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
