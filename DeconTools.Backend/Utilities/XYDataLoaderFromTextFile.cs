using System.Collections.Generic;

namespace DeconTools.Utilities
{
    public class XYDataLoaderFromTextFile
    {
        public void LoadDataFromFile(string filename, out float[] xdata, out float[] yData)
        {
            var sr = new System.IO.StreamReader(filename);

            var xVals = new List<float>();
            var yVals = new List<float>();

            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var vals = processLine(line);

                xVals.Add(parseFloatField(vals[0]));
                yVals.Add(parseFloatField(vals[1]));
            }

            xdata = xVals.ToArray();
            yData = yVals.ToArray();
        }

        private float parseFloatField(string inputString)
        {
            if (float.TryParse(inputString, out var result))
            {
                return result;
            }

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
