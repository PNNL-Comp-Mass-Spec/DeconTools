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
                if (File.Exists(_filename))
                    File.Delete(_filename);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error creating file {0}: {1}", _filename, ex.Message), ex);
            }

            if (!string.Equals(PNNLOmics.Utilities.StringUtilities.DblToString(3.14159, 4, false, 0.001, false), DblToString(3.14159, 4)))
            {
                Console.WriteLine("Note: using a period for the decimal point because the result files are CSV files");
            }

            Delimiter = ',';

            // ReSharper disable once VirtualMemberCallInConstructor
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

        public char Delimiter { get; set; }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        public override void ExportScanResults(ResultCollection resultList)
        {
            const int MAX_ATTEMPTS = 4;

            var iteration = 1;
            while (iteration <= MAX_ATTEMPTS)
            {
                try
                {
                    using (var sw = new StreamWriter(new FileStream(_filename, FileMode.Append, FileAccess.Write, FileShare.Read)))
                    {
                        sw.AutoFlush = true;

                        foreach (var scanResult in resultList.ScanResultList)
                        {
                            var output = buildScansResultOutput(scanResult);
                            sw.WriteLine(output);
                        }
                    }

                    break;
                }
                catch (IOException ex)
                {
                    if (iteration < MAX_ATTEMPTS)
                    {
                        System.Threading.Thread.Sleep(iteration * 1000);
                        iteration++;
                    }
                    else
                    {
                        throw new IOException(string.Format(
                                                  "Unable to append to {0} after {1} attempts: {2}",
                                                  _filename, MAX_ATTEMPTS, ex.Message), ex);
                    }
                }
            }
        }

        public override void ExportScanResult(ScanResult scanResult)
        {
            const int MAX_ATTEMPTS = 4;

            var iteration = 1;
            while (iteration <= MAX_ATTEMPTS)
            {
                try
                {
                    using (var sw = new StreamWriter(new FileStream(_filename, FileMode.Append, FileAccess.Write, FileShare.Read)))
                    {
                        sw.AutoFlush = true;

                        var output = buildScansResultOutput(scanResult);
                        sw.WriteLine(output);
                    }

                    break;
                }
                catch (IOException ex)
                {
                    if (iteration < MAX_ATTEMPTS)
                    {
                        System.Threading.Thread.Sleep(iteration * 1000);
                        iteration++;
                    }
                    else
                    {
                        throw new IOException(string.Format(
                                                  "Unable to append to {0} after {1} attempts: {2}",
                                                  _filename, MAX_ATTEMPTS, ex.Message), ex);
                    }
                }
            }
        }

        protected abstract string buildScansResultOutput(ScanResult result);
        protected abstract string buildHeaderLine();
    }
}
