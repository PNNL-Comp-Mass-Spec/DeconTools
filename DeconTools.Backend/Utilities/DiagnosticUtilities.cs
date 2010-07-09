using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DeconTools.Backend.Utilities
{
    public class DiagnosticUtilities
    {


        public static string GetCurrentProcessInfo()
        {
            StringBuilder sb = new StringBuilder();

            Process currentProcess = Process.GetCurrentProcess();

            sb.Append("Process Name =\t" + currentProcess.ProcessName);
            sb.Append("; Private bytes =\t" + String.Format("{0:N0}", currentProcess.PrivateMemorySize64));

            return sb.ToString();


        }


        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
