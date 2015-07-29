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
            this.FileName = filename;
            this.Delimiter = delimiter;

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

            Check.Assert(!string.IsNullOrEmpty(this.FileName), String.Format("{0} failed. Export file's FileName wasn't declared.", this.Name));

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
                throw;
            }

            PossiblyCreateFolder(this.FileName);

            using (var writer = File.AppendText(this.FileName))
            {
                var headerLine = buildHeaderLine();
                writer.WriteLine(headerLine);
                writer.Flush();
                writer.Close();
            }
        }

     
        public override void ExportResults(IEnumerable<T> resultList)
        {
            Check.Assert(!string.IsNullOrEmpty(this.FileName), this.Name + " failed. Illegal filename.");
            using (var writer = File.AppendText(this.FileName))
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
