using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public abstract class ScanResult_TextFileExporter : IScanResultExporter
    {
        protected string _filename;

        #region Constructors
        public ScanResult_TextFileExporter(string filename)
        {
            _filename = filename;

            try
            {
                if (File.Exists(_filename)) File.Delete(_filename);
            }
            catch (System.Exception)
            {
                
                throw;
            }
            


            Delimiter = ',';

            string header = buildHeaderLine();

            using (StreamWriter sw = new StreamWriter(new System.IO.FileStream(_filename, System.IO.FileMode.Append,
                        System.IO.FileAccess.Write, System.IO.FileShare.Read)))
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
            using (StreamWriter sw = new StreamWriter(new System.IO.FileStream(_filename, System.IO.FileMode.Append,
                        System.IO.FileAccess.Write, System.IO.FileShare.Read)))
            {
                sw.AutoFlush = true;

                foreach (var scanResult in resultList.ScanResultList)
                {
                    string output = buildScansResultOutput(scanResult);
                    sw.WriteLine(output);

                }

            }
        }

        public override void ExportScanResult(ScanResult scanResult)
        {
            using (StreamWriter sw = new StreamWriter(new System.IO.FileStream(_filename, System.IO.FileMode.Append,
                         System.IO.FileAccess.Write, System.IO.FileShare.Read)))
            {
                sw.AutoFlush = true;

                string output = buildScansResultOutput(scanResult);
                sw.WriteLine(output);
                
            }
        }


        protected override void CloseOutputFile()
        {

            base.CloseOutputFile();
        }

        protected abstract string buildScansResultOutput(ScanResult result);
        protected abstract string buildHeaderLine();
    }
}
