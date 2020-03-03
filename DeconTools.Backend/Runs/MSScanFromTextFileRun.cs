using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DeconTools.Backend.Runs
{
    public sealed class MSScanFromTextFileRun : XYDataRun
    {

        private char m_delimiter;
        private readonly int m_xValsColumnIndex = 0;
        private readonly int m_yValsColumnIndex = 1;

        public MSScanFromTextFileRun(string filePath)
        {
            DatasetFileOrDirectoryPath = Path.GetFullPath(filePath);

            MSFileType = Globals.MSFileType.Ascii;
            var baseFilename = Path.GetFileName(DatasetFileOrDirectoryPath);
            DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            DatasetDirectoryPath = Path.GetDirectoryName(DatasetFileOrDirectoryPath);
            XYData = new XYData();
            CurrentScanSet = new Core.ScanSet(0);    //


        }
        public MSScanFromTextFileRun(string fileName, char delimiter)
            : this(fileName)
        {

            m_delimiter = delimiter;

        }


        public MSScanFromTextFileRun(string fileName, char delimiter,
            int xValsColumnIndex, int yValsColumnIndex)
            : this(fileName)
        {
            m_delimiter = delimiter;
            m_xValsColumnIndex = xValsColumnIndex;
            m_yValsColumnIndex = yValsColumnIndex;
        }

        internal XYData loadDataFromFile(string filename)
        {
            if (m_delimiter == 0) m_delimiter = determineDelimiter(filename);

            var sr = new StreamReader(filename);

            var xVals = new List<double>();
            var yVals = new List<double>();

            var foundStartOfXYData = false;

            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (!foundStartOfXYData)     //contains header, but haven't found start of numerical data
                {
                    var match = Regex.Match(line, @"^\d+");
                    if (match.Success)
                    {
                        foundStartOfXYData = true;     //found a line that starts with numbers.

                    }
                    else
                    {
                        continue;
                    }

                }

                var vals = processLine(line);

                if (m_yValsColumnIndex >= vals.Count)
                {
                    using (var tempSr = sr)
                    {
                        try
                        {
                            tempSr.Close();
                        }
                        catch (Exception)
                        {
                            // Ignore errors here
                        }
                    }
                    throw new InvalidOperationException("XY importer error. Cannot find y-values in column " + (m_yValsColumnIndex + 1).ToString());
                }


                xVals.Add(ParseDoubleField(vals[m_xValsColumnIndex]));
                yVals.Add(ParseDoubleField(vals[m_yValsColumnIndex]));
            }

            var xyData = new XYData
            {
                Xvalues = xVals.ToArray(),
                Yvalues = yVals.ToArray()
            };

            return xyData;

        }
        private double ParseDoubleField(string inputString)
        {
            if (double.TryParse(inputString, out var result))
                return result;

            return double.NaN;
        }

        private float ParseFloatField(string inputString)
        {
            if (float.TryParse(inputString, out var result))
                return result;

            return float.NaN;
        }

        private List<string> processLine(string inputLine)
        {
            char[] splitter = { m_delimiter };
            var returnedList = new List<string>();

            var arr = inputLine.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            foreach (var str in arr)
            {
                returnedList.Add(str);
            }
            return returnedList;
        }

        private char determineDelimiter(string fileName)
        {

            using (var sr = new StreamReader(fileName))
            {

                var foundStartOfXYData = false;

                var counter = 0;

                var delimiterCount = new Dictionary<char, int>();

                while (!sr.EndOfStream && counter < 100)
                {
                    var line = sr.ReadLine();
                    counter++;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (!foundStartOfXYData)
                    {
                        var match = Regex.Match(line, @"^\d+");
                        if (match.Success)
                        {
                            foundStartOfXYData = true;     //found a line that starts with numbers.

                        }
                        else
                        {
                            continue;
                        }

                    }

                    //we found the start of the data, now figure out the delimiter
                    var lineMatch = Regex.Match(line, @"^[0-9\.-]+(?<delim>\D+)[0-9\.-]+");

                    if (lineMatch.Success)
                    {
                        var matchedString = lineMatch.Groups["delim"].Value;

                        if (matchedString.Length > 0)
                        {
                            var delimiter = matchedString[0];

                            if (!delimiterCount.ContainsKey(delimiter))
                            {
                                delimiterCount.Add(delimiter, 0);

                            }
                            delimiterCount[delimiter]++;   //adds one to the count of occurrences for this delimiter.

                        }
                    }
                }

                var mostCommonDelimiter = '\t';
                var delimiterCounter = 0;

                foreach (var item in delimiterCount)
                {
                    if (item.Value > delimiterCounter)
                    {
                        delimiterCounter = item.Value;
                        mostCommonDelimiter = item.Key;
                    }


                }

                sr.Close();
                return mostCommonDelimiter;

            }

        }



        public override XYData GetMassSpectrum(Core.ScanSet scanSet, double minMZ, double maxMZ)
        {
            XYData xyData;

            try
            {
                xyData = loadDataFromFile(DatasetFileOrDirectoryPath);
            }
            catch (Exception ex)
            {

                throw new IOException("There was an error reading file " + Utilities.DiagnosticUtilities.GetFullPathSafe(DatasetFileOrDirectoryPath) + "\n\n" + ex.Message);
            }

            xyData = xyData.TrimData(minMZ, maxMZ);

            return xyData;
        }

        public override double GetTime(int scanNum)
        {
            return 0;    // there is no time associated with this type of Run
        }

        public override int GetMSLevel(int scanNum)
        {
            return 1;      // simply assume that it is a regular MS
        }


    }
}
