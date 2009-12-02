using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;
using System.IO;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public abstract class ScanResult_TextFileExporter:IScanResultExporter
    {
        protected StreamWriter sw;

        #region Constructors
        #endregion

        #region Properties
        public abstract char Delimiter { get; set; }
        #endregion


        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void ExportScanResults(DeconTools.Backend.Core.ResultCollection resultList)
        {
            foreach (ScanResult result in resultList.ScanResultList)
            {
                string isosResultOutput = buildScansResultOutput(result);

                try
                {
                    sw.WriteLine(isosResultOutput);
                }
                catch (Exception ex)
                {
                    Logger.Instance.AddEntry("IsosResultExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                    throw new Exception("Result exporter failed.  Check to see if it is already open or not.");
                }
            }
        }

        protected override void CloseOutputFile()
        {
            if (sw == null) return;
            try
            {
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("IsosResultExporter failed to close the output file properly. Details: " + ex.Message, Logger.Instance.OutputFilename);
            }
            base.CloseOutputFile();
        }

        protected abstract string buildScansResultOutput(ScanResult result);
        protected abstract string buildHeaderLine();
    }
}
