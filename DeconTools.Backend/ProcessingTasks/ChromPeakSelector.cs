using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public ChromPeakSelector(int numLCScansToSum, double tolerance)
            : this(1,tolerance, Globals.PeakSelectorMode.MOST_INTENSE)
        {

        }

        public ChromPeakSelector(int numLCScansToSum, double tolerance, Globals.PeakSelectorMode peakSelectorMode)
        {
            this.Tolerance = tolerance;
            this.PeakSelectionMode = peakSelectorMode;
            this.numScansToSum = numLCScansToSum;
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
        public double Tolerance
        {
            get { return tolerance; }
            set { tolerance = value; }
        }


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

            ChromPeak bestPeak =(ChromPeak) selectBestPeak(this.PeakSelectionMode, resultList.Run.PeakList, resultList.Run.CurrentMassTag.NETVal, this.Tolerance);

            if (bestPeak == null)
            {
                result.ScanSet = null;
                result.Flags.Add(new ChromPeakNotFoundResultFlag("ChromPeakSelectorFailed. No LC peaks found with tolerance for specified mass tag."));
            }
            else 
            {
                result.ChromPeakSelected = bestPeak;
                result.ScanSet = createSummedScanSet(result.ChromPeakSelected, resultList.Run);
            }

            Check.Ensure(result.ChromPeakSelected.XValue != 0, "ChromPeakSelector failed. No chromatographic peak found within tolerances.");
            resultList.Run.CurrentScanSet = result.ScanSet;   // maybe good to set this here so that the MSGenerator can operate on it...  

        }

        private ScanSet createSummedScanSet(ChromPeak chromPeak, Run run)
        {
            int bestScan = (int)chromPeak.XValue;
            bestScan= run.GetClosestMSScan(bestScan, Globals.ScanSelectionMode.CLOSEST);
            return new ScanSetFactory().CreateScanSet(run, bestScan, this.numScansToSum);
        }

        public IPeak selectBestPeak(Globals.PeakSelectorMode peakSelectorMode, List<IPeak> chromPeakList, float targetNET, double netTolerance)
        {
            List<ChromPeak> peaksWithinTol = new List<ChromPeak>(); // will collect Chrom peaks that fall within the NET tolerance


            foreach (ChromPeak peak in chromPeakList)
            {
                if (Math.Abs(peak.NETValue - targetNET) <= netTolerance)     //peak.NETValue was determined by the ChromPeakDetector or a future ChromAligner Task
                {
                    peaksWithinTol.Add(peak);
                }
            }


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
                    throw new NotImplementedException();
                    break;
                default:
                    break;


            }
            
            return bestPeak;
        }


    }
}
