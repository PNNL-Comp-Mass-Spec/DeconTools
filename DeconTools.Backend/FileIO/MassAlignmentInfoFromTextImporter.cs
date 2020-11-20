using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MassAlignmentInfoFromTextImporter : ImporterBase<List<MassAlignmentDataItem>>
    {
        readonly List<MassAlignmentDataItem> _massAndTimeCorrectionData = new List<MassAlignmentDataItem>();
        private readonly string[] mzHeaders = { "mz" };
        private readonly string[] mzPPMCorrectionHeaders = { "mzPPMCorrection" };
        private readonly string[] scanHeaders = { "scan" };
        private readonly string[] scanPPMCorrectionHeaders = { "scanPPMCorrection" };
        private readonly string _filename;

        #region Constructors
        public MassAlignmentInfoFromTextImporter(string filename)
        {
            _filename = filename;
        }
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public override List<MassAlignmentDataItem> Import()
        {
            GetMassAndTimePPMCorrectionsFromFile();
            return _massAndTimeCorrectionData;
        }

        private void GetMassAndTimePPMCorrectionsFromFile()
        {
            StreamReader reader;

            try
            {
                reader = new StreamReader(_filename);
            }
            catch (Exception ex)
            {
                throw new IOException("There was a problem importing from file " + _filename + ": " + ex.Message);
            }

            using (var sr = reader)
            {
                if (sr.EndOfStream)
                {
                    sr.Close();
                    throw new InvalidDataException("There is no data in file " + _filename);
                }

                var headerLine = sr.ReadLine();
                CreateHeaderLookupTable(headerLine);

                var areHeadersValid = ValidateHeaders();

                if (!areHeadersValid)
                {
                    throw new InvalidDataException("There is a problem with the column headers in file " + _filename);
                }

                var lineCounter = 1;   //used for tracking which line is being processed.

                //read and process each line of the file
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var processedData = ProcessLine(line);

                    //ensure that processed line is the same size as the header line
                    if (processedData.Count != m_columnHeaders.Count)
                    {
                        throw new InvalidDataException("Data in row #" + lineCounter.ToString() + "is invalid - \nThe number of columns does not match that of the header line");
                    }

                    var massAndTimePPMCorrItem = ConvertTextToDataObject(processedData);
                    _massAndTimeCorrectionData.Add(massAndTimePPMCorrItem);
                    lineCounter++;
                }

                sr.Close();
            }
        }

        private MassAlignmentDataItem ConvertTextToDataObject(List<string> processedData)
        {
            var mz = ParseFloatField(LookupData(processedData, mzHeaders));
            var mzPPMCorrection = ParseFloatField(LookupData(processedData, mzPPMCorrectionHeaders));
            var scan = ParseFloatField(LookupData(processedData, scanHeaders));
            var scanPPMCorrection = ParseFloatField(LookupData(processedData, scanPPMCorrectionHeaders));

            var item = new MassAlignmentDataItem(mz, mzPPMCorrection, scan, scanPPMCorrection);
            return item;
        }

        private bool ValidateHeaders()
        {
            return true;
        }
    }
}
