using System;
using System.Text;
using System.Diagnostics;

namespace DeconTools.Backend.Utilities
{
    public class DiagnosticUtilities
    {
        public static string GetCurrentProcessInfo()
        {
            var sb = new StringBuilder();

            var currentProcess = Process.GetCurrentProcess();

            sb.Append("Process Name =\t" + currentProcess.ProcessName);
            sb.Append("; Private bytes =\t" + string.Format("{0:N0}", currentProcess.PrivateMemorySize64));

            return sb.ToString();
        }

        /// <summary>
        /// Returns the absolute path to the file or folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns>If the path is empty or invalid, then will return a message describing the problem</returns>
        public static string GetFullPathSafe(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "Error: Empty Path";
            }

            try
            {
                return System.IO.Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                return ex.Message + ": " + path;
            }
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
