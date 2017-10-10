using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DeconTools.Backend.Runs
{
    public class MSScanFromTextFileRun : XYDataRun
    {

        private char m_delimiter;
        private readonly int m_xvalsColumnIndex = 0;
        private readonly int m_yvalsColumnIndex = 1;



        public MSScanFromTextFileRun(string fileName)
        {
            Filename = Path.GetFullPath(fileName);

            MSFileType = Globals.MSFileType.Ascii;
            var baseFilename = Path.GetFileName(Filename);
            DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            DataSetPath = Path.GetDirectoryName(Filename);
            XYData = new XYData();
            CurrentScanSet = new Core.ScanSet(0);    //


        }
        public MSScanFromTextFileRun(string fileName, char delimiter)
            : this(fileName)
        {

            m_delimiter = delimiter;

        }


        public MSScanFromTextFileRun(string fileName, char delimiter,
            int xvalsColumnIndex, int yvalsColumnIndex)
            : this(fileName)
        {
            m_delimiter = delimiter;
            m_xvalsColumnIndex = xvalsColumnIndex;
            m_yvalsColumnIndex = yvalsColumnIndex;
        }

        internal XYData loadDataFromFile(string filename)
        {
            if (m_delimiter == 0) m_delimiter = determineDelimiter(filename);

            var sr = new StreamReader(filename);

            var xvals = new List<double>();
            var yvals = new List<double>();

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

                if (m_yvalsColumnIndex >= vals.Count)
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
                    throw new InvalidOperationException("XY importer error. Cannot find y-values in column " + (m_yvalsColumnIndex + 1).ToString());
                }


                xvals.Add(parseDoubleField(vals[m_xvalsColumnIndex]));
                yvals.Add(parseDoubleField(vals[m_yvalsColumnIndex]));
            }

            var xydata = new XYData
            {
                Xvalues = xvals.ToArray(),
                Yvalues = yvals.ToArray()
            };

            return xydata;

        }
        private double parseDoubleField(string inputstring)
        {
            if (double.TryParse(inputstring, out var result))
                return result;

            return double.NaN;
        }

        private float parseFloatField(string inputstring)
        {
            if (float.TryParse(inputstring, out var result))
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
                    var linematch = Regex.Match(line, @"^[0-9\.-]+(?<delim>\D+)[0-9\.-]+");

                    if (linematch.Success)
                    {
                        var matchedString = linematch.Groups["delim"].Value;

                        if (matchedString.Length > 0)
                        {
                            var delim = matchedString[0];

                            if (!delimiterCount.ContainsKey(delim))
                            {
                                delimiterCount.Add(delim, 0);

                            }
                            delimiterCount[delim]++;   //adds one to the count of occurances for this delimiter.

                        }
                    }




                }

                var mostCommonDelimiter = '\t';
                var delimCounter = 0;

                foreach (var item in delimiterCount)
                {
                    if (item.Value > delimCounter)
                    {
                        delimCounter = item.Value;
                        mostCommonDelimiter = item.Key;
                    }


                }

                sr.Close();
                return mostCommonDelimiter;

            }

        }



        public override XYData GetMassSpectrum(Core.ScanSet scanset, double minMZ, double maxMZ)
        {
            XYData xydata;

            try
            {
                xydata = loadDataFromFile(Filename);
            }
            catch (Exception ex)
            {

                throw new IOException("There was an error reading file " + Utilities.DiagnosticUtilities.GetFullPathSafe(Filename) + "\n\n" + ex.Message);
            }

            xydata = xydata.TrimData(minMZ, maxMZ);

            return xydata;
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
