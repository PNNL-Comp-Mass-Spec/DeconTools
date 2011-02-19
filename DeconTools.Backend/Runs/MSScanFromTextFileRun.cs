using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Utilities;
using System.Text.RegularExpressions;
using System.IO;

namespace DeconTools.Backend.Runs
{
    public class MSScanFromTextFileRun : XYDataRun
    {

        private double[] xdata;
        private double[] ydata;

        private char m_delimiter;
        private int m_xvalsColumnIndex = 0;
        private int m_yvalsColumnIndex = 1;



        public MSScanFromTextFileRun(string fileName)
        {
            this.Filename = Path.GetFullPath(fileName);

            this.MSFileType = Globals.MSFileType.Ascii;
            string baseFilename = Path.GetFileName(this.Filename);
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

        internal void loadDataFromFile(string filename)
        {
            if (m_delimiter == 0) m_delimiter = determineDelimiter(filename);

            System.IO.StreamReader sr = new System.IO.StreamReader(filename);

            List<double> xvals = new List<double>();
            List<double> yvals = new List<double>();

            bool foundStartOfXYData = false;

            while (sr.Peek() != -1)
            {
                string line = sr.ReadLine();





                if (foundStartOfXYData)
                {

                }

                if (!foundStartOfXYData)     //contains header, but haven't found start of numerical data
                {
                    Match match = Regex.Match(line, @"^\d+");
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

                List<string> vals = processLine(line);

                
                if (m_yvalsColumnIndex >= vals.Count)
                {
                    using (StreamReader tempSr = sr)
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

            xdata = xvals.ToArray();
            ydata = yvals.ToArray();

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
            List<string> returnedList = new List<string>();

            string[] arr = inputLine.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in arr)
            {
                returnedList.Add(str);
            }
            return returnedList;
        }

        private char determineDelimiter(string fileName)
        {

            using (System.IO.StreamReader sr = new System.IO.StreamReader(fileName))
            {

                List<double> xvals = new List<double>();
                List<double> yvals = new List<double>();

                bool foundStartOfXYData = false;

                int counter = 0;

                Dictionary<char, int> delimiterCount = new Dictionary<char, int>();

                while (sr.Peek() != -1 && counter < 100)
                {
                    string line = sr.ReadLine();
                    counter++;

                    if (!foundStartOfXYData)
                    {
                        Match match = Regex.Match(line, @"^\d+");
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
                    Match linematch = Regex.Match(line, @"^[0-9\.-]+(?<delim>\D+)[0-9\.-]+");

                    if (linematch.Success)
                    {
                        string matchedString = linematch.Groups["delim"].Value;

                        if (matchedString.Length > 0)
                        {
                            char delim = matchedString[0];

                            if (!delimiterCount.ContainsKey(delim))
                            {
                                delimiterCount.Add(delim, 0);

                            }
                            delimiterCount[delim]++;   //adds one to the count of occurances for this delimiter. 

                        }
                    }




                }

                char mostCommonDelimiter = '\t';
                int delimCounter = 0;

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



        public override void GetMassSpectrum(DeconTools.Backend.Core.ScanSet scanset, double minMZ, double maxMZ)
        {
            try
            {
                loadDataFromFile(this.Filename);
            }
            catch (Exception ex)
            {

                throw new System.IO.IOException("There was an error reading the file. Details below:\n\n" + ex.Message);
            }

            Check.Assert(this.xdata != null || this.ydata != null, "Error: no xy data read from file");

            this.XYData.Xvalues = this.xdata;
            this.XYData.Yvalues = this.ydata;
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
