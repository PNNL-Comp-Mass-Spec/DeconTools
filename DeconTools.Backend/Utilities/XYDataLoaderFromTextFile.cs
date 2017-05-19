using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Utilities
{
    public class XYDataLoaderFromTextFile
    {

        public void loadDataFromFile(string filename, ref float[] xdata, ref float[] ydata)
        {
            var sr = new System.IO.StreamReader(filename);

            var xvals = new List<float>();
            var yvals = new List<float>();

            while (sr.Peek() != -1)
            {
                var line = sr.ReadLine();
                var vals = processLine(line);

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
            var returnedList = new List<string>();

            var arr = inputLine.Split(splitter);
            foreach (var str in arr)
            {
                returnedList.Add(str);
            }
            return returnedList;
        }



    }
}
