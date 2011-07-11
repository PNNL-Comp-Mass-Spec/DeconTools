using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MassAlignmentInfoFromTextImporter : ImporterBase<List<MassAlignmentDataItem>>
    {

        List<MassAlignmentDataItem> _massAndTimeCorrectionData = new List<MassAlignmentDataItem>();
        private string[] mzHeaders = { "mz" };
        private string[] mzPPMCorrectionHeaders = { "mzPPMCorrection" };
        private string[] scanHeaders= { "scan" };
        private string[] scanPPMCorrectionHeaders = { "scanPPMCorrection" };
        private string _filename;

     


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
                reader = new StreamReader(this._filename);
            }
            catch (Exception)
            {
                throw new System.IO.IOException("There was a problem importing from the file.");
            }

            using (StreamReader sr = reader)
            {
                if (sr.Peek() == -1)
                {
                    sr.Close();
                    throw new InvalidDataException("There is no data in the file we are trying to read.");

                }

                string headerLine = sr.ReadLine();
                CreateHeaderLookupTable(headerLine);

                bool areHeadersValid = ValidateHeaders();

                if (!areHeadersValid)
                {
                    throw new InvalidDataException("There is a problem with the column headers.");
                }


                string line;
                int lineCounter = 1;   //used for tracking which line is being processed. 

                //read and process each line of the file
                while (sr.Peek() > -1)
                {
                    line = sr.ReadLine();
                    List<string> processedData = ProcessLine(line);

                    //ensure that processed line is the same size as the header line
                    if (processedData.Count != m_columnHeaders.Count)
                    {
                        throw new InvalidDataException("Data in row #" + lineCounter.ToString() + "is invalid - \nThe number of columns does not match that of the header line");
                    }

                    MassAlignmentDataItem massAndTimePPMCorrItem = ConvertTextToDataObject(processedData);
                    _massAndTimeCorrectionData.Add(massAndTimePPMCorrItem);
                    lineCounter++;
                }
            }
        }

        private MassAlignmentDataItem ConvertTextToDataObject(List<string> processedData)
        {
            float mz = ParseFloatField(LookupData(processedData, mzHeaders));
            float mzPPMCorrection = ParseFloatField(LookupData(processedData, mzPPMCorrectionHeaders));
            float scan = ParseFloatField(LookupData(processedData, scanHeaders));
            float scanPPMCorrection = ParseFloatField(LookupData(processedData, scanPPMCorrectionHeaders));

            MassAlignmentDataItem item = new MassAlignmentDataItem(mz, mzPPMCorrection, scan, scanPPMCorrection);
            return item;
        }

        private bool ValidateHeaders()
        {
            return true;
        }
    }
}
