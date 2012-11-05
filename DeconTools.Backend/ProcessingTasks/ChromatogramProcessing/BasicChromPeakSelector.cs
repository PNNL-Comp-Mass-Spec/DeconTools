using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public sealed class BasicChromPeakSelector : ChromPeakSelectorBase
    {
        #region Constructors


        public BasicChromPeakSelector(ChromPeakSelectorParameters parameters)
        {
            Parameters = parameters;
        }
       

        #endregion

        #region Properties

      
        //TODO:   figure out what uses this and why!   Default is 0 - that's all I know
        public int ScanOffSet { get; set; }

        public double ReferenceNETValueForReferenceMode { get; set; }

        public override ChromPeakSelectorParameters Parameters { get; set; }
       
        #endregion

  
        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, "ChromPeakSelector failed. Mass Tag must be defined but it isn't.");
            Check.Require(resultList.Run.PeakList != null, "ChromPeakSelector failed. Peak list has not been established. You need to run a peak detector.");
            Check.Require(resultList.Run.PeakList.Count > 0, "ChromPeakSelector failed. Peak list is empty.");
            Check.Require(resultList.Run.PeakList[0] is ChromPeak, "ChromPeakSelector failed. Input peaklist contains the wrong type of peak");

            TargetedResultBase result = resultList.GetTargetedResult(resultList.Run.CurrentMassTag);


            float normalizedElutionTime;

            if (result.Run.CurrentMassTag.ElutionTimeUnit == Globals.ElutionTimeUnit.ScanNum)
            {
                normalizedElutionTime = resultList.Run.CurrentMassTag.ScanLCTarget/(float)result.Run.GetNumMSScans();
            }
            else
            {
                normalizedElutionTime = resultList.Run.CurrentMassTag.NormalizedElutionTime;
            }


            int numPeaksWithinTolerance = 0;
            var bestPeak = (ChromPeak)selectBestPeak(Parameters.PeakSelectorMode, resultList.Run.PeakList, normalizedElutionTime, Parameters.NETTolerance, out numPeaksWithinTolerance);
            result.AddNumChromPeaksWithinTolerance(numPeaksWithinTolerance);



            //if (bestPeak == null)
            //{
            //    result.ScanSet = null;
            //    result.Flags.Add(new ChromPeakNotFoundResultFlag("ChromPeakSelectorFailed. No LC peaks found with tolerance for specified mass tag."));
            //}
            //else
            //{
            //    result.ChromPeakSelected = bestPeak;
            //    result.ScanSet = createSummedScanSet(result.ChromPeakSelected, resultList.Run, this.ScanOffSet);
            //}


            SetScansForMSGenerator(bestPeak, resultList.Run, true);

            UpdateResultWithChromPeakAndLCScanInfo(result, bestPeak);
            
            bool failedChromPeakSelection = (result.ChromPeakSelected == null || result.ChromPeakSelected.XValue == 0);
            if (failedChromPeakSelection)
            {
                result.FailedResult = true;
                result.FailureType = Globals.TargetedResultFailureType.ChrompeakNotFoundWithinTolerances;
            }

        }

        

        //private ScanSet createSummedScanSet(ChromPeak chromPeak, Run run, int scanOffset)
        //{
        //    if (chromPeak == null || chromPeak.XValue == 0) return null;

        //    int bestScan = (int)chromPeak.XValue;
        //    bestScan = run.GetClosestMSScan(bestScan, Globals.ScanSelectionMode.CLOSEST);
        //    bestScan = bestScan + scanOffset;
        //    return new ScanSetFactory().CreateScanSet(run, bestScan, this.NumScansToSum);
        //}


        public Peak selectBestPeak(Globals.PeakSelectorMode peakSelectorMode, List<Peak> chromPeakList, float targetNET, double netTolerance)
        {
            int numPeaksWithinTolerance = 0;
            return selectBestPeak(peakSelectorMode, chromPeakList, targetNET, netTolerance, out numPeaksWithinTolerance);
        }

        public Peak selectBestPeak(Globals.PeakSelectorMode peakSelectorMode, List<Peak> chromPeakList, float targetNET, double netTolerance, out int numPeaksWithinTolerance)
        {
            List<ChromPeak> peaksWithinTol = new List<ChromPeak>(); // will collect Chrom peaks that fall within the NET tolerance



            foreach (ChromPeak peak in chromPeakList)
            {
                if (Math.Abs(peak.NETValue - targetNET) <= netTolerance)     //peak.NETValue was determined by the ChromPeakDetector or a future ChromAligner Task
                {
                    peaksWithinTol.Add(peak);
                }
            }

            numPeaksWithinTolerance = peaksWithinTol.Count;


            ChromPeak bestPeak = null;

            switch (peakSelectorMode)
            {
                case Globals.PeakSelectorMode.ClosestToTarget:
                    double diff = double.MaxValue;

                    for (int i = 0; i < peaksWithinTol.Count; i++)
                    {
                        double currentDiff = Math.Abs(peaksWithinTol[i].NETValue - targetNET);

                        if (currentDiff < diff)
                        {
                            diff = currentDiff;
                            bestPeak = peaksWithinTol[i];
                        }
                    }
                    break;
                case Globals.PeakSelectorMode.MostIntense:
                    double max = -1;
                    for (int i = 0; i < peaksWithinTol.Count; i++)
                    {
                        double currentIntensity = peaksWithinTol[i].Height;

                        if (currentIntensity > max)
                        {
                            max = currentIntensity;
                            bestPeak = peaksWithinTol[i];
                        }
                    }
                    break;
                case Globals.PeakSelectorMode.RelativeToOtherChromPeak:
                    diff = double.MaxValue;


                    for (int i = 0; i < peaksWithinTol.Count; i++)
                    {
                        double currentDiff = Math.Abs(peaksWithinTol[i].NETValue - ReferenceNETValueForReferenceMode);

                        if (currentDiff < diff)
                        {
                            diff = currentDiff;
                            bestPeak = peaksWithinTol[i];
                        }
                    }

                    break;

                case Globals.PeakSelectorMode.N15IntelligentMode:
                    diff = double.MaxValue;

                    //want to only consider peaks that are less than the target NET.  (N15 peptides elutes at the same NET or earlier). 

                    peaksWithinTol.Clear();

                    foreach (ChromPeak peak in chromPeakList)
                    {

                        double currentDiff = ReferenceNETValueForReferenceMode - peak.NETValue;

                        if ((currentDiff) >= 0 && currentDiff <= netTolerance)     
                        {
                            peaksWithinTol.Add(peak);
                            if (currentDiff < diff)
                            {
                                diff = currentDiff;
                                bestPeak = peak;
                            }
                        }
                    }

                    numPeaksWithinTolerance = peaksWithinTol.Count;



                    break;
                default:
                    break;


            }

            return bestPeak;
        }


       
    }
}
