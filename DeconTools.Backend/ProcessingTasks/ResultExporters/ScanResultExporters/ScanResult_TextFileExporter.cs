using System;
using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public abstract class ScanResult_TextFileExporter : ScanResultExporter
    {
        protected string _filename;

        #region Constructors

        protected ScanResult_TextFileExporter(string filename)
        {
            _filename = filename;

            try
            {
                if (File.Exists(_filename)) File.Delete(_filename);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating file " + _filename + ": " + ex.Message);
            }

            if (!string.Equals(PNNLOmics.Utilities.StringUtilities.DblToString(3.14159, 4, false, 0.001, false), DblToString(3.14159, 4)))
            {
                Console.WriteLine("Note: using a period for the decimal point because the result files are CSV files");
            }

            Delimiter = ',';

            var header = buildHeaderLine();

            using (var sw = new StreamWriter(new FileStream(_filename, FileMode.Append,
                        FileAccess.Write, FileShare.Read)))
            {
                sw.AutoFlush = true;
                sw.WriteLine(header);
            }
        }

        #endregion

        #region Properties
        public virtual char Delimiter { get; set; }
        #endregion


        #region Public Methods
        #endregion

        #region Private Methods
        #endregion


        public override void ExportScanResults(DeconTools.Backend.Core.ResultCollection resultList)
        {
            using (var sw = new StreamWriter(new FileStream(_filename, FileMode.Append,
                        FileAccess.Write, FileShare.Read)))
            {
                sw.AutoFlush = true;

                foreach (var scanResult in resultList.ScanResultList)
                {
                    var output = buildScansResultOutput(scanResult);
                    sw.WriteLine(output);

                }

            }
        }

        public override void ExportScanResult(ScanResult scanResult)
        {
            using (var sw = new StreamWriter(new FileStream(_filename, FileMode.Append,
                         FileAccess.Write, FileShare.Read)))
            {
                sw.AutoFlush = true;

                var output = buildScansResultOutput(scanResult);
                sw.WriteLine(output);
                
            }
        }

        protected abstract string buildScansResultOutput(ScanResult result);
        protected abstract string buildHeaderLine();
    }
}
