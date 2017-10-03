using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Core
{
    public abstract class TargetedResultBase : IsosResult
    {
        #region Constructors

        protected TargetedResultBase()
        {
            ChromPeakQualityList = new List<ChromPeakQualityData>();
        }

        protected TargetedResultBase(TargetBase target)
        {
            Target = target;
            IsotopicProfile = new IsotopicProfile();
            ChromPeakQualityList = new List<ChromPeakQualityData>();
        }

        #endregion

        #region Properties

        public IList<ChromPeakQualityData> ChromPeakQualityList { get; set; }

        public int NumChromPeaksWithinTolerance { get; set; }

        public int NumMSScansSummed { get; set; }

        public bool WasPreviouslyProcessed { get; set; }

        public ChromPeak ChromPeakSelected { get; set; }

        public TargetBase Target { get; set; }

        public double Score { get; set; }   // TODO: Do I need this  (IsosResult already has a Score in IsotopicProfile)

        public string ErrorDescription { get; set; }

        public XYData ChromValues { get; set; }

        /// <summary>
        /// True when processing encounters a critical problem. Normally happens when the target doesn't exist in data.
        /// </summary>
        public bool FailedResult { get; set; }

        public ChromCorrelationData ChromCorrelationData { get; set; }

        /// <summary>
        /// Type of failure during targeted processing
        /// </summary>
        public Globals.TargetedResultFailureType FailureType { get; set; }


        #endregion

        #region Public Methods
        public virtual void DisplayToConsole()
        {
            var info = buildBasicConsoleInfo();
            Console.Write(info);
        }

        protected string buildBasicConsoleInfo()
        {
            var sb = new StringBuilder();
            sb.Append("****** Match ******\n");
            sb.Append("NET = \t" + Target.NormalizedElutionTime.ToString("0.000") + "\n");
            sb.Append("ChromPeak ScanNum = " + ChromPeakSelected.XValue.ToString() + "\n");
            sb.Append("ChromPeak NETVal = " + ChromPeakSelected.NETValue.ToString("0.000") + "\n");
            sb.Append("ScanSet = { ");
            foreach (var scanNum in ScanSet.IndexValues)
            {
                sb.Append(scanNum);
                sb.Append(", ");

            }
            sb.Append("} \n");
            if (IsotopicProfile != null && IsotopicProfile.Peaklist != null && IsotopicProfile.Peaklist.Count > 0)
            {
                sb.Append("Observed MZ and intensity = " + IsotopicProfile.getMonoPeak().XValue + "\t" + IsotopicProfile.getMonoPeak().Height + "\n");
            }
            sb.Append("FitScore = " + Score.ToString("0.0000") + "\n");
            return sb.ToString();
        }



        #endregion

        #region Private Methods
        #endregion


        public int GetScanNum()
        {
            if (ScanSet == null)
            {
                return Target.ScanLCTarget;
            }
            else
            {
                return ScanSet.PrimaryScanNumber;
            }
        }


        public double GetNET()
        {
            if (ChromPeakSelected == null) return -1;
            else
            {
                return ChromPeakSelected.NETValue;
            }

        }

       
        public virtual double GetNETAlignmentError()
        {
            double theorNET = Target.NormalizedElutionTime;
            var obsNET = Run.NetAlignmentInfo.GetNETValueForScan(GetScanNum());

            var netError = obsNET - theorNET;
            return netError;

        }

        public double GetMassErrorBeforeAlignmentInPPM()
        {
            var theorMZ = GetMZOfMostIntenseTheorIsotopicPeak();
            var observedMZ = GetMZOfObservedPeakClosestToTargetVal(theorMZ);
            
            var massErrorInPPM = (observedMZ - theorMZ) / theorMZ * 1e6;
            return massErrorInPPM;
            
        }


        public double GetMassErrorAfterAlignmentInPPM()
        {
            double massErrorInPPM = 0;
            var theorMZ = GetMZOfMostIntenseTheorIsotopicPeak();
            var observedMZ = GetMZOfObservedPeakClosestToTargetVal(theorMZ);

            var scan = GetScanNum();
            
            var alignedMZ = Run.GetAlignedMZ(observedMZ, scan);
            massErrorInPPM = (alignedMZ - theorMZ) / theorMZ * 1e6;
            
            return massErrorInPPM;
        }

        /// <summary>
        /// Gets the monoisotopic mass after calibration
        /// </summary>
        /// <returns></returns>
        public virtual double GetCalibratedMonoisotopicMass()
        {
           var monoMass = IsotopicProfile == null ? 0 : IsotopicProfile.MonoIsotopicMass;

            if (Run.MassIsAligned)
            {
                var theorMZ = GetMZOfMostIntenseTheorIsotopicPeak();
                var scan = GetScanNum();
                var ppmShift = Run.MassAlignmentInfo.GetPpmShift(theorMZ, scan);
                
                var alignedMono = monoMass - (ppmShift * monoMass / 1e6);
                return alignedMono;
            }

            return monoMass;




        }

        public double GetMZOfMostIntenseTheorIsotopicPeak()
        {
            if (Target == null || Target.IsotopicProfile == null)
            {
                return 0;
            }
            else
            {
                return Target.IsotopicProfile.getMostIntensePeak().XValue;
            }

        }


        public double GetMZOfObservedPeakClosestToTargetVal(double targetMZ)
        {
            if (IsotopicProfile == null || IsotopicProfile.Peaklist == null)
            {
                return 0;
            }
            else
            {
                var indexOfTargetPeak = PeakUtilities.getIndexOfClosestValue(IsotopicProfile.Peaklist, targetMZ, 0, IsotopicProfile.Peaklist.Count - 1, 0.1);
                if (indexOfTargetPeak != -1)
                {
                    return IsotopicProfile.Peaklist[indexOfTargetPeak].XValue;
                }
                else
                {
                    return 0;
                }
            }
        }


        internal virtual void AddLabelledIso(IsotopicProfile labelledIso)
        {
            throw new NotImplementedException();
        }

        internal virtual void AddTheoreticalLabelledIsotopicProfile(IsotopicProfile theorLabelledIso)
        {
            throw new NotImplementedException();
        }





        public virtual void AddSelectedChromPeakAndScanSet(ChromPeak bestPeak, ScanSet scanset, Globals.IsotopicProfileType isotopicProfileType = Globals.IsotopicProfileType.UNLABELLED)
        {
            
            if (isotopicProfileType== Globals.IsotopicProfileType.UNLABELLED)
            {

                ChromPeakSelected = bestPeak;
                ScanSet = scanset;

                if (ScanSet!=null)
                {
                    NumMSScansSummed = ScanSet.IndexValues.Count;    
                }

                
                

                var failedChromPeakSelection = (ChromPeakSelected == null || ChromPeakSelected.XValue == 0);
                if (failedChromPeakSelection)
                {
                    FailedResult = true;
                    FailureType = Globals.TargetedResultFailureType.ChrompeakNotFoundWithinTolerances;
                }
                else
                {
                    FailedResult = false;
                    FailureType = Globals.TargetedResultFailureType.None;
                }

            }
            else
            {
                throw new NotSupportedException("Cannot add data for a labeled result in this base class");
            }
            


        }



        public virtual void AddNumChromPeaksWithinTolerance(int numChromPeaksWithinTolerance)
        {
            NumChromPeaksWithinTolerance = numChromPeaksWithinTolerance;
        }


        public virtual void ResetResult()
        {
            Flags.Clear();
            ErrorDescription = "";
            Score = 1;
            InterferenceScore = 1;
            IsotopicProfile = null;
            ChromPeakSelected = null;
            FailedResult = false;
            FailureType = Globals.TargetedResultFailureType.None;
        }

        public virtual void ResetMassSpectrumRelatedInfo()
        {
            Score = 1;
            InterferenceScore = 1;
            IsotopicProfile = null;
        }



        public int NumQualityChromPeaks { get; set; }


        public double MonoIsotopicMassCalibrated { get; set; }

        public double MassErrorBeforeAlignment { get; set; }

        public double MassErrorAfterAlignment { get; set; }
    }
}
