using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
    public abstract class IqResultImporter 
    {
        //Note that uppercase/lowercase is ignored
        protected string[] DatasetHeaders = { "dataset" };
        protected string[] TargetIDHeaders = { "targetid" };
        protected string[] CodeHeaders = { "code", "sequence" };
        protected string[] EmpiricalFormulaHeaders = { "empiricalFormula" };
        protected string[] ChargeStateHeaders = { "chargestate", "z" };
        protected string[] MonoMassTheorHeaders = { "MonomassTheor" };
        protected string[] MzTheorHeaders = { "MzTheor" };
        
        
        protected string[] ElutionTimeTheorHeaders = { "ElutionTimeTheor" };
        protected string[] TargetScanHeaders = { "targetScan" };
        protected string[] MonomassObsHeaders = { "MonoMassObs" };
        protected string[] MZObsHeaders = { "MZObs" };
        protected string[] MonomassObsCalibratedHeaders = { "MonoMassObsCalibrated" };
        protected string[] MZObsCalibratedHeaders = {"MZObsCalibrated"};
        protected string[] ElutionTimeObsHeaders = { "ElutionTimeObs" };
        protected string[] ChromPeaksWithinTolHeaders = { "ChromPeaksWithinTolerance" };
        protected string[] LcScanObsHeaders = { "scan" };
        protected string[] AbundanceHeaders = { "abundance"};
        protected string[] FitScoreHeaders = { "IsoFitScore" };
        protected string[] InterferenceScoreHeaders = { "iscore", "interferenceScore" };

        protected char Delimiter = '\t';
        private List<string> _columnHeaders ;
        private Dictionary<string, int> _columnIndexTable;

        #region Constructors

        protected IqResultImporter (string filename)
        {
            Filename = filename;
        }
        #endregion

        #region Properties
        protected string Filename { get; set; }

        #endregion

        #region Public Methods
        public virtual List<IqResult> Import()
        {
            var resultList = new List<IqResult>();

            StreamReader reader;
            if (!File.Exists(Filename))
            {
                throw new IOException("Cannot import. File does not exist.");
            }

            try
            {
                reader = new StreamReader(Filename);
            }
            catch (Exception)
            {
                throw new IOException("There was a problem importing from the file.");
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


                var lineCounter = 1;   //used for tracking which line is being processed. 

                //read and process each line of the file
                while (sr.Peek() > -1)
                {
                    var line = sr.ReadLine();
                    var processedData = ProcessLine(line);

                    //ensure that processed line is the same size as the header line
                    if (processedData.Count != _columnHeaders.Count)
                    {
                        throw new InvalidDataException("In File: " + Path.GetFileName(Filename) + "; Data in row # " +
                                                       lineCounter.ToString("0") +
                                                       " is NOT valid - \nThe number of columns does not match that of the header line");
                    }

                    var iqResult = CreateIqResult();

                    ConvertTextToDataObject(ref iqResult, processedData);
                    resultList.Add(iqResult);
                    lineCounter++;

                }
                sr.Close();

            }
            return resultList;
        }

        protected abstract IqResult CreateIqResult();
        protected abstract void ConvertTextToDataObject(ref IqResult iqResult, List<string> rowData);

        protected void GetBasicIqResultData(ref IqResult result, List<string> rowData)
        {

            result.Target.ID = ParseIntField(rowData, TargetIDHeaders, 0);
            result.Target.ChargeState = ParseIntField(rowData, ChargeStateHeaders, 0);
            result.Target.MonoMassTheor = ParseDoubleField(rowData, MonoMassTheorHeaders, 0);
            result.Target.MZTheor = ParseDoubleField(rowData, MzTheorHeaders, 0);
            result.Target.ScanLC = ParseIntField(rowData, TargetScanHeaders);
            result.Target.ElutionTimeTheor = ParseDoubleField(rowData, ElutionTimeTheorHeaders, 0);
            result.Target.Code = ParseStringField(rowData, CodeHeaders);
            result.Target.EmpiricalFormula = ParseStringField(rowData, EmpiricalFormulaHeaders);

            result.Abundance = ParseFloatField(rowData, AbundanceHeaders, 0);
            result.ElutionTimeObs = ParseDoubleField(rowData, ElutionTimeObsHeaders, 0);
            result.FitScore = ParseDoubleField(rowData, FitScoreHeaders, 1);
            result.InterferenceScore = ParseDoubleField(rowData, InterferenceScoreHeaders, 1);
            result.LcScanObs = ParseIntField(rowData, LcScanObsHeaders);
            result.MZObs = ParseDoubleField(rowData, MZObsHeaders, 0);
            result.MonoMassObs = ParseDoubleField(rowData, MonomassObsHeaders, 0);
            result.MZObsCalibrated = ParseDoubleField(rowData, MZObsCalibratedHeaders, 0);
            result.MonoMassObsCalibrated = ParseDoubleField(rowData, MonomassObsCalibratedHeaders, 0);


            result.NumChromPeaksWithinTolerance = ParseIntField(rowData, ChromPeaksWithinTolHeaders);

            result.MassErrorBefore = (result.MonoMassObs -result.Target.MonoMassTheor)/result.Target.MonoMassTheor*1e6;
            result.MassErrorAfter= (result.MonoMassObsCalibrated - result.Target.MonoMassTheor) / result.Target.MonoMassTheor * 1e6;

        }
        
        protected virtual bool ValidateHeaders()
        {
            return true;
        }
        
        protected virtual string TryGetDatasetNameFromFileName()
        {
            var fileName = Path.GetFileName(Filename);
            if (fileName != null)
            {
                var datasetName = fileName.Replace("_iqResults.txt", String.Empty);

                return datasetName;
            }

            return string.Empty;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Parses a single line of data into a List of strings
        /// </summary>
        /// <param name="inputLine"></param>
        /// <returns></returns>
        private List<string> ProcessLine(string inputLine)
        {
            char[] splitter = { Delimiter };
            var arr = inputLine.Split(splitter);
            return arr.ToList();
        }

        private void CreateHeaderLookupTable(string headerLine)
        {
            _columnHeaders = ProcessLine(headerLine);
            _columnIndexTable = new Dictionary<string, int>();


            for (var i = 0; i < _columnHeaders.Count; i++)
            {
                var header = _columnHeaders[i].ToLower();

                if (!_columnIndexTable.ContainsKey(header))
                {
                    _columnIndexTable.Add(header, i);
                }

            }

        }

        private string ParseStringField(List<string> rowData, IEnumerable<string> headers, string defaultVal = "")
        {
            var rowValueString = LookupData(rowData, headers, string.Empty);

            if (string.IsNullOrEmpty(rowValueString))
            {
                return defaultVal;
            }

            return rowValueString;
        }

        private int ParseIntField(List<string> rowData, IEnumerable<string> headers, int defaultVal = -1)
        {
            var rowValueString = LookupData(rowData, headers, string.Empty).ToLower();

            if (string.IsNullOrEmpty(rowValueString))
            {
                return defaultVal;
            }

            int result;
            if (Int32.TryParse(rowValueString, out result))
            {
                return result;
            }

            return defaultVal;
        }

        private long ParseLongField(List<string> rowData, IEnumerable<string> headers, long defaultVal = -1)
        {
            var rowValueString = LookupData(rowData, headers, string.Empty).ToLower();

            if (string.IsNullOrEmpty(rowValueString))
            {
                return defaultVal;
            }

            long result;
            if (Int64.TryParse(rowValueString, out result))
            {
                return result;
            }

            return defaultVal;
        }

        private float ParseFloatField(List<string> rowData, IEnumerable<string> headers, float defaultVal = float.NaN)
        {
            var rowValueString = LookupData(rowData, headers, string.Empty).ToLower();

            if (string.IsNullOrEmpty(rowValueString))
            {
                return defaultVal;
            }

            float result;
            if (float.TryParse(rowValueString, out result))
            {
                return result;
            }

            return defaultVal;
        }

        private double ParseDoubleField(List<string> rowData, IEnumerable<string> headers, double defaultVal = double.NaN)
        {
            var rowValueString = LookupData(rowData, headers, string.Empty).ToLower();

            if (string.IsNullOrEmpty(rowValueString))
            {
                return defaultVal;
            }

            double result;
            if (double.TryParse(rowValueString, out result))
            {
                return result;
            }

            return defaultVal;
        }

        private short ParseShortField(List<string> rowData, IEnumerable<string> headers, short defaultVal = -1)
        {
            var rowValueString = LookupData(rowData, headers, string.Empty).ToLower();

            if (string.IsNullOrEmpty(rowValueString))
            {
                return defaultVal;
            }


            short result;
            if (Int16.TryParse(rowValueString, out result))
            {
                return result;
            }

            return defaultVal;


        }

        private bool ParseBoolField(List<string> rowData, IEnumerable<string> headers, bool defaultVal = false)
        {
            var rowValueString = LookupData(rowData, headers, string.Empty).ToLower();

            if (string.IsNullOrEmpty(rowValueString))
            {
                return defaultVal;
            }

            if (rowValueString == "1" || rowValueString == "true")
            {
                return true;

            }

            if (rowValueString == "0" || rowValueString == "false")
            {
                return false;
            }

            return defaultVal;


        }

        private string LookupData(List<string> row, IEnumerable<string> possibleColumnHeaders, string defaultValue)
        {

            foreach (var possibleHeader in possibleColumnHeaders)
            {
                var columnIndex = GetColumnIndexForHeader(possibleHeader);
                if (columnIndex != -1)
                {
                    return row[columnIndex];
                }
            }

            return defaultValue;

        }

        private int GetColumnIndexForHeader(string columnHeader)
        {

            columnHeader = columnHeader.ToLower();


            if (_columnIndexTable.ContainsKey(columnHeader))
            {
                return _columnIndexTable[columnHeader];
            }

            return -1;
        }

        #endregion

  
        


        
       


 

   

        
    }
}
