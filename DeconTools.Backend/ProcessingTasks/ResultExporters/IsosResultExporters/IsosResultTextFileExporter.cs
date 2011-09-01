using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using System.IO;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public abstract class IsosResultTextFileExporter : IIsosResultExporter
    {


        #region Properties
        public virtual char Delimiter { get; set; }

        public string FileName { get; set; }

        public override int TriggerToExport {get;set;}
       

        #endregion

        #region Public Methods
        public override void ExportIsosResults(List<IsosResult> isosResultList)
        {
            Check.Assert(this.FileName != null && this.FileName.Length > 0, this.Name + " failed. Illegal filename.");
            using (StreamWriter writer = File.AppendText(this.FileName))
            {
                foreach (IsosResult result in isosResultList)
                {
                    string isosResultOutput = buildIsosResultOutput(result);
                    writer.WriteLine(isosResultOutput);
                }

                writer.Flush();
                writer.Close();
            }
        }

        protected virtual void initializeAndWriteHeader()
        {

            Check.Assert(this.FileName != null && this.FileName.Length > 0, String.Format("{0} failed. Export file's FileName wasn't declared.", this.Name));

            try
            {
                if (File.Exists(this.FileName))
                {
                    File.Delete(this.FileName);
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry(String.Format("{0} failed. Details: " + ex.Message + "; STACKTRACE = " + ex.StackTrace, this.Name), Logger.Instance.OutputFilename);
                throw ex;
            }


            using (StreamWriter writer = File.AppendText(this.FileName))
            {
                string headerLine = buildHeaderLine();
                writer.Write(headerLine);
                writer.Flush();
                writer.Close();
            }
        }

        #endregion


        protected abstract string buildIsosResultOutput(IsosResult result);
        protected abstract string buildHeaderLine();

    }
}
