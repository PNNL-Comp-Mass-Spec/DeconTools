using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public abstract class MassTagResultBase : IsosResult
    {
        #region Constructors
        #endregion

        #region Properties
        
        public IList<ChromPeak> ChromPeaks { get; set; }

        public ChromPeak ChromPeakSelected { get; set; }

        public MassTag MassTag { get; set; }

        public double Score { get; set; }   // TODO: Do I need this  (IsosResult already has a Score in IsotopicProfile)


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
            if (this.IsotopicProfile != null)
            {
                sb.Append("Observed MZ and intensity = " + this.IsotopicProfile.getMonoPeak().XValue + "\t" + this.IsotopicProfile.getMonoPeak().Height + "\n");
            }
            sb.Append("FitScore = " + this.Score.ToString("0.0000") + "\n");
            return sb.ToString();
        }



        #endregion

        #region Private Methods
        #endregion


        public double GetNET()
        {
            if (ChromPeakSelected == null) return -1;
            else
            {
                return ChromPeakSelected.NETValue;
            }

        }

        internal virtual void AddLabelledIso(IsotopicProfile labelledIso)
        {
            throw new NotImplementedException();
        }
    }
}
