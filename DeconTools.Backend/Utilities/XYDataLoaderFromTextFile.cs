using System.Collections.Generic;

namespace DeconTools.Utilities
{
    public class XYDataLoaderFromTextFile
    {

        public void loadDataFromFile(string filename, out float[] xdata, out float[] ydata)
        {
            var sr = new System.IO.StreamReader(filename);

            var xvals = new List<float>();
            var yvals = new List<float>();

            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var vals = processLine(line);

                xvals.Add(parseFloatField(vals[0]));
                yvals.Add(parseFloatField(vals[1]));
            }

            xdata = xvals.ToArray();
            ydata = yvals.ToArray();

        }

        private float parseFloatField(string inputstring)
        {
            if (float.TryParse(inputstring, out var result))
                return result;
            return float.NaN;
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
