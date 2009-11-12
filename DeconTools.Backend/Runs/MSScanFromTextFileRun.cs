using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    public class MSScanFromTextFileRun : XYDataRun
    {

        private double[] xdata;
        private double[] ydata;

        public MSScanFromTextFileRun(string fileName, Globals.XYDataFileType fileType)
        {
            this.FileType = fileType;
            this.Filename = fileName;
            this.XYData = new XYData();
            this.CurrentScanSet = new DeconTools.Backend.Core.ScanSet(0);    //
            
        }


       
        internal void loadDataFromFile(string filename)
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(filename);

            List<double> xvals = new List<double>();
            List<double> yvals = new List<double>();

            while (sr.Peek() != -1)
            {
                string line = sr.ReadLine();
                List<string> vals = processLine(line);

                xvals.Add(parseDoubleField(vals[0]));
                yvals.Add(parseDoubleField(vals[1]));
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
            char[] splitter = { '\t' };
            List<string> returnedList = new List<string>();

            string[] arr = inputLine.Split(splitter);
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
