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
        private bool m_containsHeader = false;
        private int m_xvalsColumnIndex = 0;
        private int m_yvalsColumnIndex = 0;

        public MSScanFromTextFileRun(string fileName, Globals.XYDataFileType fileType, char delimiter)
            : this(fileName, fileType, delimiter, false)
        {


        }

        public MSScanFromTextFileRun(string fileName, Globals.XYDataFileType fileType, char delimiter, bool containsHeader)
            : this(fileName, fileType, delimiter, containsHeader, 0, 1)
        {


        }

        public MSScanFromTextFileRun(string fileName, Globals.XYDataFileType fileType, char delimiter, bool containsHeader, 
            int xvalsColumnIndex, int yvalsColumnIndex)
        {



  


            this.FileType = fileType;
            this.Filename = Path.GetFullPath(fileName);
            
            string baseFilename = Path.GetFileName(this.Filename);
            this.DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            this.DataSetPath = Path.GetDirectoryName(this.Filename);
            
            
            this.XYData = new XYData();
            this.CurrentScanSet = new DeconTools.Backend.Core.ScanSet(0);    //
            this.m_delimiter = delimiter;
            this.m_containsHeader = containsHeader;
            this.m_xvalsColumnIndex = xvalsColumnIndex;
            this.m_yvalsColumnIndex = yvalsColumnIndex;
        }


        

        internal void loadDataFromFile(string filename)
        {
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

                if (m_containsHeader && !foundStartOfXYData)     //contains header, but haven't found start of numerical data
                {
                    Match match = Regex.Match(line,@"^\d+");
                    if (match.Success)
                    {
                        foundStartOfXYData = true;     //found a line that starts with numbers. 

                    }
                    else
                    {
                        continue;
                    }
 
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
