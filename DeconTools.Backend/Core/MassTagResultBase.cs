﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Core
{
    public abstract class MassTagResultBase : IsosResult
    {
        #region Constructors
        #endregion

        #region Properties
        
        public IList<ChromPeak> ChromPeaks { get; set; }

        public int NumChromPeaksWithinTolerance { get; set; }


        public bool WasPreviouslyProcessed { get; set; }

        public ChromPeak ChromPeakSelected { get; set; }

        public MassTag MassTag { get; set; }

        public double Score { get; set; }   // TODO: Do I need this  (IsosResult already has a Score in IsotopicProfile)

        public string ErrorDescription { get; set; }

        public XYData ChromValues { get; set; }
        #endregion

        #region Public Methods
        public virtual void DisplayToConsole()
        {
            string info = buildBasicConsoleInfo();
            Console.Write(info);
        }

        protected string buildBasicConsoleInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("****** Match ******\n");
            sb.Append("NET = \t" + MassTag.NETVal.ToString("0.000") + "\n");
            sb.Append("ChromPeak ScanNum = " + ChromPeakSelected.XValue.ToString() + "\n");
            sb.Append("ChromPeak NETVal = " + ChromPeakSelected.NETValue.ToString("0.000") + "\n");
            sb.Append("ScanSet = { ");
            foreach (int scanNum in ScanSet.IndexValues)
            {
                sb.Append(scanNum);
                sb.Append(", ");

            }
            sb.Append("} \n");
            if (this.IsotopicProfile != null && this.IsotopicProfile.Peaklist!=null && this.IsotopicProfile.Peaklist.Count>0)
            {
                sb.Append("Observed MZ and intensity = " + this.IsotopicProfile.getMonoPeak().XValue + "\t" + this.IsotopicProfile.getMonoPeak().Height + "\n");
            }
            sb.Append("FitScore = " + this.Score.ToString("0.0000") + "\n");
            return sb.ToString();
        }



        #endregion

        #region Private Methods
        #endregion


        public int GetScanNum()
        {
            if (ScanSet == null) return -1;
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

        public double GetMZOfMostIntenseTheorIsotopicPeak()
        {
            if (this.MassTag == null || this.MassTag.IsotopicProfile == null)
            {
                return 0;
            }
            else
            {
                return this.MassTag.IsotopicProfile.getMostIntensePeak().XValue;
            }

        }


        public double GetMZOfObservedPeakClosestToTargetVal(double targetMZ)
        {
            if (this.IsotopicProfile == null || this.IsotopicProfile.Peaklist == null)
            {
                return 0;
            }
            else
            {
                int indexOfTargetPeak = PeakUtilities.getIndexOfClosestValue(this.IsotopicProfile.Peaklist, targetMZ, 0, this.IsotopicProfile.Peaklist.Count - 1, 0.1);
                if (indexOfTargetPeak != -1)
                {
                    return this.IsotopicProfile.Peaklist[indexOfTargetPeak].XValue;
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



      

        public virtual void AddSelectedChromPeakAndScanSet(ChromPeak bestPeak, ScanSet scanset)
        {
            this.ChromPeakSelected = bestPeak;
            this.ScanSet = scanset;
            WasPreviouslyProcessed = true;    //indicate that this result has been added to...  use this to help control the addition of labelled (N15) data
        }

        public virtual void AddNumChromPeaksWithinTolerance(int numChromPeaksWithinTolerance)
        {
            this.NumChromPeaksWithinTolerance = numChromPeaksWithinTolerance;
        }


        public virtual void ResetResult()
        {
            this.Flags.Clear();
            this.ErrorDescription = "";
            this.Score = 1;
            this.InterferenceScore = 1;
            this.IsotopicProfile = null;
        }

               
    }
}
