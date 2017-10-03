using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    public class MSScanFromTextFileRun : XYDataRun
    {

        private char m_delimiter;
        private int m_xvalsColumnIndex = 0;
        private int m_yvalsColumnIndex = 1;



        public MSScanFromTextFileRun(string fileName)
        {
            this.Filename = Path.GetFullPath(fileName);

            this.MSFileType = Globals.MSFileType.Ascii;
            var baseFilename = Path.GetFileName(this.Filename);
            this.DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            this.DataSetPath = Path.GetDirectoryName(this.Filename);
            this.XYData = new XYData();
            this.CurrentScanSet = new DeconTools.Backend.Core.ScanSet(0);    //


        }
        public MSScanFromTextFileRun(string fileName, char delimiter)
            : this(fileName)
        {

            this.m_delimiter = delimiter;

        }


        public MSScanFromTextFileRun(string fileName, char delimiter,
            int xvalsColumnIndex, int yvalsColumnIndex)
            : this(fileName)
        {
            this.m_delimiter = delimiter;
            this.m_xvalsColumnIndex = xvalsColumnIndex;
            this.m_yvalsColumnIndex = yvalsColumnIndex;
        }

        internal XYData loadDataFromFile(string filename)
        {
            if (m_delimiter == 0) m_delimiter = determineDelimiter(filename);

            var sr = new System.IO.StreamReader(filename);

            var xvals = new List<double>();
            var yvals = new List<double>();

            var foundStartOfXYData = false;

            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();



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



                if (line == "")
                {
                    break;
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

                        }
                    }
                    throw new InvalidOperationException("XY importer error. Cannot find y-values in column " + (m_yvalsColumnIndex + 1).ToString());
                }


                xvals.Add(parseDoubleField(vals[m_xvalsColumnIndex]));
                yvals.Add(parseDoubleField(vals[m_yvalsColumnIndex]));
            }

            var xydata = new XYData();
            xydata.Xvalues = xvals.ToArray();
            xydata.Yvalues = yvals.ToArray();

            return xydata;

        }
        private double parseDoubleField(string inputstring)
        {
            double result = 0;
            if (double.TryParse(inputstring, out result))
                return result;
            else return double.NaN;

        }

        private float parseFloatField(string inputstring)
        {
            float result = 0;
            if (float.TryParse(inputstring, out result))
                return result;
            else return float.NaN;

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

            using (var sr = new System.IO.StreamReader(fileName))
            {

                var xvals = new List<double>();
                var yvals = new List<double>();

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




            throw new NotImplementedException();
        }



        public override XYData GetMassSpectrum(DeconTools.Backend.Core.ScanSet scanset, double minMZ, double maxMZ)
        {
            var xydata = new XYData();

            try
            {
                xydata = loadDataFromFile(this.Filename);
            }
            catch (Exception ex)
            {

                throw new System.IO.IOException("There was an error reading file " + Utilities.DiagnosticUtilities.GetFullPathSafe(this.Filename) + "\n\n" + ex.Message);
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
