using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public abstract class IMassTagResult : IsosResult
    {
        #region Constructors
        #endregion

        #region Properties
        public abstract List<ChromPeak> ChromPeaks { get; set; }

        public abstract ChromPeak ChromPeakSelected { get; set; }

        public abstract MassTag MassTag { get; set; }

        public abstract XYData ChromValues { get; set; }
        #endregion

        #region Public Methods
        public void DisplayToConsole()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("MassTag:  ID = \t" + MassTag.ID.ToString() + "; NET = \t"+ MassTag.NETVal.ToString("0.000")+"\n");
            sb.Append("ChromPeak ScanNum = " + ChromPeakSelected.XValue.ToString() + "\n");
            sb.Append("ChromPeak NETVal = " + ChromPeakSelected.NETValue.ToString("0.000") + "\n");
            sb.Append("ScanSet = { ");
            foreach (int scanNum in ScanSet.IndexValues)
            {
                sb.Append(scanNum);
                sb.Append(", ");
                
            }
            sb.Append("} \n");
            Console.Write(sb.ToString());
        }

        #endregion

        #region Private Methods
        #endregion

    }
}
