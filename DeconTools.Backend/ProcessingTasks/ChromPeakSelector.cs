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
        public ChromPeakSelector()
            : this(0.05)
        {

        }

        public ChromPeakSelector(double tolerance)
            : this(tolerance, Globals.PeakSelectorMode.MOST_INTENSE)
        {

        }

        public ChromPeakSelector(double tolerance, Globals.PeakSelectorMode peakSelectorMode)
        {
            this.Tolerance = tolerance;
            this.PeakSelectionMode = peakSelectorMode;
            this.numScansToSum = 1;
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
            Check.Require(resultList.Run.PeakList != null, "ChromPeakSelector failed. Peak list has not been established. You need to run a peak detector.");
            Check.Require(resultList.Run.CurrentMassTag != null, "ChromPeakSelector failed. Mass Tag must be defined but it isn't.");

            List<ChromPeak> peaksWithinTol = new List<ChromPeak>(); // will collect Chrom peaks that fall within the NET tolerance

            foreach (ChromPeak peak in resultList.Run.PeakList)
            {
                if (Math.Abs(peak.NETValue - resultList.Run.CurrentMassTag.NETVal) <= tolerance)     //peak.NETValue was determined by the ChromPeakDetector or a future ChromAligner Task
                {
                    peaksWithinTol.Add(peak);
                }
            }

            MassTagResultBase result = resultList.MassTagResultList[resultList.Run.CurrentMassTag];

            if (peaksWithinTol.Count == 0)
            {
                result.ScanSet = null;
                result.Flags.Add(new ChromPeakNotFoundResultFlag("ChromPeakSelectorFailed. No LC peaks found with tolerance for specified mass tag."));
            }
            else if (peaksWithinTol.Count == 1)
            {
                result.ChromPeakSelected = peaksWithinTol[0];
                result.ScanSet = createSummedScanSet(result.ChromPeakSelected, resultList.Run);
                
             
            }
            else
            {
                result.ChromPeakSelected = selectBestPeak(this.peakSelectionMode, peaksWithinTol, resultList.Run.CurrentMassTag.NETVal);
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

        private ChromPeak selectBestPeak(Globals.PeakSelectorMode peakSelectorMode, List<ChromPeak> peaksWithinTol, float targetNET)
        {
            ChromPeak bestPeak = new ChromPeak();

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
