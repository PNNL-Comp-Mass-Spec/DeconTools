using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class ChromPeakSelector : Task
    {
        #region Constructors
        public ChromPeakSelector(int numLCScansToSum)
            : this(1, 0.05)
        {

        }

        public ChromPeakSelector(int numLCScansToSum, double netTolerance)
            : this(1, netTolerance, Globals.PeakSelectorMode.MOST_INTENSE)
        {

        }

        public ChromPeakSelector(int numLCScansToSum, double netTolerance, Globals.PeakSelectorMode peakSelectorMode)
            : this(numLCScansToSum, netTolerance, peakSelectorMode, 0)
        {

        }

        public ChromPeakSelector(int numLCScansToSum, double netTolerance, Globals.PeakSelectorMode peakSelectorMode, int scanOffSet)
        {
            this.NETTolerance = netTolerance;
            this.PeakSelectionMode = peakSelectorMode;
            this.numScansToSum = numLCScansToSum;
            this.ScanOffSet = scanOffSet;
        }

        #endregion

        #region Properties

        private DeconTools.Backend.Globals.PeakSelectorMode peakSelectionMode;

        public DeconTools.Backend.Globals.PeakSelectorMode PeakSelectionMode
        {
            get { return peakSelectionMode; }
            set { peakSelectionMode = value; }
        }

        private double tolerance;
        public double NETTolerance
        {
            get { return tolerance; }
            set { tolerance = value; }
        }

        //TODO:   figure out what uses this and why!   Default is 0 - that's all I know
        public int ScanOffSet { get; set; }

        public double NETValueForIntelligentMode { get; set; }


        public int numScansToSum { get; set; }       // this might be better elsewhere, but for now put it here...
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, "ChromPeakSelector failed. Mass Tag must be defined but it isn't.");
            Check.Require(resultList.Run.PeakList != null, "ChromPeakSelector failed. Peak list has not been established. You need to run a peak detector.");
            Check.Require(resultList.Run.PeakList.Count > 0, "ChromPeakSelector failed. Peak list is empty.");
            Check.Require(resultList.Run.PeakList[0] is ChromPeak, "ChromPeakSelector failed. Input peaklist contains the wrong type of peak");

            MassTagResultBase result = resultList.GetMassTagResult(resultList.Run.CurrentMassTag);

            int numPeaksWithinTolerance = 0;
            ChromPeak bestPeak = (ChromPeak)selectBestPeak(this.PeakSelectionMode, resultList.Run.PeakList, resultList.Run.CurrentMassTag.NETVal, this.NETTolerance, out numPeaksWithinTolerance);
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

            ScanSet scanset = createSummedScanSet(bestPeak, resultList.Run, this.ScanOffSet);
            resultList.Run.CurrentScanSet = scanset;   // maybe good to set this here so that the MSGenerator can operate on it...  

            result.AddSelectedChromPeakAndScanSet(bestPeak, scanset);

            bool failedChromPeakSelection = (result.ChromPeakSelected == null || result.ChromPeakSelected.XValue == 0);
            if (failedChromPeakSelection)
            {
                result.FailedResult = true;
                result.FailureType = Globals.TargetedResultFailureType.CHROMPEAK_NOT_FOUND_WITHIN_TOLERANCES;
            }

        }

        private ScanSet createSummedScanSet(ChromPeak chromPeak, Run run, int scanOffset)
        {
            if (chromPeak == null || chromPeak.XValue == 0) return null;

            int bestScan = (int)chromPeak.XValue;
            bestScan = run.GetClosestMSScan(bestScan, Globals.ScanSelectionMode.CLOSEST);
            bestScan = bestScan + scanOffset;
            return new ScanSetFactory().CreateScanSet(run, bestScan, this.numScansToSum);
        }


        public IPeak selectBestPeak(Globals.PeakSelectorMode peakSelectorMode, List<IPeak> chromPeakList, float targetNET, double netTolerance)
        {
            int numPeaksWithinTolerance = 0;
            return selectBestPeak(peakSelectionMode, chromPeakList, targetNET, netTolerance, out numPeaksWithinTolerance);
        }

        public IPeak selectBestPeak(Globals.PeakSelectorMode peakSelectorMode, List<IPeak> chromPeakList, float targetNET, double netTolerance, out int numPeaksWithinTolerance)
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
                case Globals.PeakSelectorMode.CLOSEST_TO_TARGET:
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
                case Globals.PeakSelectorMode.MOST_INTENSE:
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
                case Globals.PeakSelectorMode.INTELLIGENT_MODE:
                    diff = double.MaxValue;

                   



                    for (int i = 0; i < peaksWithinTol.Count; i++)
                    {
                        double currentDiff = Math.Abs(peaksWithinTol[i].NETValue - NETValueForIntelligentMode);

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

                        double currentDiff = NETValueForIntelligentMode - peak.NETValue;

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
