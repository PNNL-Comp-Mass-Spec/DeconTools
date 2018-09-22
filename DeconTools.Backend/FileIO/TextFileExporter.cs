using System;
using System.Collections.Generic;
using DeconTools.Utilities;
using System.IO;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.FileIO
{
    public abstract class TextFileExporter<T> : ExporterBase<T>
    {
        #region Constructors

        protected TextFileExporter(string filename)
            : this(filename, '\t')
        {

        }

        protected TextFileExporter(string filename, char delimiter)
        {
            FileName = filename;
            Delimiter = delimiter;

            initializeAndWriteHeader();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name of the Exporter - e.g. 'ScanResultExporter'; to be used in error reporting, etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full file path to which data is written
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Delimiter to be used in between columns of data
        /// </summary>
        public char Delimiter { get; set; }


        #endregion

        #region Public Methods

        protected void initializeAndWriteHeader()
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

            PossiblyCreateFolder(FileName);

            using (var writer = File.AppendText(FileName))
            {
                var headerLine = buildHeaderLine();
                writer.WriteLine(headerLine);
                writer.Flush();
                writer.Close();
            }
        }


        public override void ExportResults(IEnumerable<T> resultList)
        {
            Check.Assert(!string.IsNullOrEmpty(FileName), Name + " failed. Illegal filename.");
            using (var writer = File.AppendText(FileName))
            {
                foreach (var result in resultList)
                {
                    var resultOutput = buildResultOutput(result);
                    writer.WriteLine(resultOutput);
                }

                writer.Flush();
                writer.Close();
            }

        }

        protected abstract string buildResultOutput(T result);
        protected abstract string buildHeaderLine();


        #endregion

        #region Private Methods


        private void PossiblyCreateFolder(string filePath)
        {
            var fiFile = new FileInfo(filePath);
            var diDirectory = fiFile.Directory;

            if (diDirectory != null && !diDirectory.Exists)
            {
                diDirectory.Create();
            }

        }


        #endregion
    }
}
