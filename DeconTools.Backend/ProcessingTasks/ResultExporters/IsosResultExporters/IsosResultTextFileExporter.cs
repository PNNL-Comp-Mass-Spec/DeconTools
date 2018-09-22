using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public abstract class IsosResultTextFileExporter : IsosResultExporter
    {

        #region Properties
        public virtual char Delimiter { get; set; }

        public string FileName { get; set; }

        public override int TriggerToExport {get;set;}

        #endregion

        #region Public Methods
        public override void ExportIsosResults(List<IsosResult> isosResultList)
        {
            Check.Assert(!string.IsNullOrEmpty(FileName), Name + " failed. Illegal filename.");
            using (var writer = File.AppendText(FileName))
            {
                foreach (var result in isosResultList)
                {
                    var isosResultOutput = buildIsosResultOutput(result);

                    if (!string.IsNullOrEmpty(isosResultOutput))
                    {
                        writer.WriteLine(isosResultOutput);
                    }


                }

                writer.Flush();
                writer.Close();
            }
        }

        protected virtual void initializeAndWriteHeader()
        {

            Check.Assert(!string.IsNullOrEmpty(FileName), string.Format("{0} failed. Export file's FileName wasn't declared.", Name));

            try
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry(string.Format("{0} failed. Details: " + ex.Message +
                    "; STACKTRACE = " + PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex), Name), true);
                throw;
            }


            using (var writer = File.AppendText(FileName))
            {
                var headerLine = buildHeaderLine();
                writer.WriteLine(headerLine);
                writer.Flush();
                writer.Close();
            }
        }

        #endregion


        protected abstract string buildIsosResultOutput(IsosResult result);
        protected abstract string buildHeaderLine();

    }
}
