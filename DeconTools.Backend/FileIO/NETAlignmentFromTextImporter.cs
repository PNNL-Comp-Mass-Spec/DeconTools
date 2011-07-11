using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class NETAlignmentFromTextImporter : ImporterBase<List<ScanNETPair>>
    {
  
        protected string[] scanHeaders = { "scan", "scanClassRep" };
        protected string[] netHeaders = { "net", "NETClassRep" };
        private string _filename;

        private List<ScanNETPair> _scanNETPairs = new List<ScanNETPair>();

        #region Constructors

        public NETAlignmentFromTextImporter(string filename)
        {
            this._filename = filename;
        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public override List<ScanNETPair> Import()
        {
          
            GetScanNETPairsFromFile();
            return _scanNETPairs;

        }

        #endregion

        #region Private Methods

        private void GetScanNETPairsFromFile()
        {
            _scanNETPairs.Clear();

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

                    ScanNETPair scanNETPair = ConvertTextToDataObject(processedData);
                    _scanNETPairs.Add(scanNETPair);
                    lineCounter++;

                }
            }
        }

        private ScanNETPair ConvertTextToDataObject(List<string> processedData)
        {
            float scan = ParseFloatField(LookupData(processedData, scanHeaders));
            float net = ParseFloatField(LookupData(processedData, netHeaders));

            return new ScanNETPair(scan, net);
        }

        private bool ValidateHeaders()
        {
            return true;
        }
        #endregion


    


      
    }
}
