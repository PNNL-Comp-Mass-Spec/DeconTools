using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Utilities
{
    public class XYDataLoaderFromTextFile
    {

        public void loadDataFromFile(string filename, ref float[] xdata, ref float[] ydata)
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(filename);

            List<float> xvals = new List<float>();
            List<float> yvals = new List<float>();

            while (sr.Peek() != -1)
            {
                string line = sr.ReadLine();
                List<string> vals = processLine(line);

                xvals.Add(parseFloatField(vals[0]));
                yvals.Add(parseFloatField(vals[1]));
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



    }
}
