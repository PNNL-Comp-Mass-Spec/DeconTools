using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using System.IO;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public abstract class IsosResultTextFileExporter : IIsosResultExporter
    {

        protected StreamWriter sw;

        #region Properties
        public abstract char Delimiter { get; set; }

        #endregion

        #region Public Methods
        public override void ExportIsosResults(DeconTools.Backend.Core.ResultCollection resultList)
        {
            foreach (IsosResult result in resultList.ResultList)
            {
                string isosResultOutput = buildIsosResultOutput(result);

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
        #endregion

        public override void CloseOutputFile()
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

        protected abstract string buildIsosResultOutput(IsosResult result);
        protected abstract string buildHeaderLine();

        #region Private Methods
        #endregion
    }
}
