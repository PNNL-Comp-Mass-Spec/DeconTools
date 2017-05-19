using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

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

        public void Execute(Run run)
        {
            Check.Require(run != null, "NETAlignmentInfoImporter failed. Run is not defined.");

            GetScanNETPairsFromFile();
            run.NetAlignmentInfo = new NetAlignmentInfoBasic(run.MinLCScan, run.MaxLCScan);
            run.NetAlignmentInfo.SetScanToNETAlignmentData(_scanNETPairs);

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

            using (var sr = reader)
            {
                if (sr.Peek() == -1)
                {
                    sr.Close();
                    throw new InvalidDataException("There is no data in the file we are trying to read.");

                }

                var headerLine = sr.ReadLine();
                CreateHeaderLookupTable(headerLine);

                var areHeadersValid = ValidateHeaders();

                if (!areHeadersValid)
                {
                    throw new InvalidDataException("There is a problem with the column headers.");
                }


                string line;
                var lineCounter = 1;   //used for tracking which line is being processed. 

                //read and process each line of the file
                while (sr.Peek() > -1)
                {
                    line = sr.ReadLine();
                    var processedData = ProcessLine(line);

                    //ensure that processed line is the same size as the header line
                    if (processedData.Count != m_columnHeaders.Count)
                    {
                        throw new InvalidDataException("Data in row #" + lineCounter.ToString() + "is invalid - \nThe number of columns does not match that of the header line");
                    }

                    var scanNETPair = ConvertTextToDataObject(processedData);
                    _scanNETPairs.Add(scanNETPair);
                    lineCounter++;

                }

                sr.Close();
            }
        }

        private ScanNETPair ConvertTextToDataObject(List<string> processedData)
        {
            var scan = ParseFloatField(LookupData(processedData, scanHeaders));
            var net = ParseFloatField(LookupData(processedData, netHeaders));

            return new ScanNETPair(scan, net);
        }

        private bool ValidateHeaders()
        {
            return true;
        }
        #endregion


    


      
    }
}
