using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Data
{
    public abstract class Importer<T>
    {
        public abstract T Import();

        protected char delimiter = '\t';

        protected string lookup(List<string> data, List<string> headers, string targetHeader)
        {
            int columnIndex = getIndexForTableHeader(headers, targetHeader,false);
            if (columnIndex == -1) return "";

            return data[columnIndex];

        }

        protected bool parseBoolField(string inputstring)
        {
            bool result = false;
            if (bool.TryParse(inputstring, out result))
                return result;
            else return false;     //TODO:  need to figure out the default value

        }

        protected short parseShortField(string inputstring)
        {
            short result = 0;
            if (Int16.TryParse(inputstring, out result))
                return result;
            else return 0;
        }

        protected double parseDoubleField(string inputstring)
        {
        
            double result = 0;
            if (double.TryParse(inputstring, out result))
                return result;
            else
            {
                return double.NaN;

            }

             
        }

        protected float parseFloatField(string inputstring)
        {
            float result = 0;
            if (float.TryParse(inputstring, out result))
                return result;
            else return float.NaN;

        }


        protected int parseIntField(string inputstring)
        {
            int result = 0;
            if (Int32.TryParse(inputstring, out result))
                return result;
            else
            {
                double secondAttempt = parseDoubleField(inputstring);
                if (secondAttempt != double.NaN)
                {
                    return Convert.ToInt32(secondAttempt);
                }
                else
                {
                    return -1;
                }

            }
                
                

        }

        protected List<string> processLine(string inputLine)
        {
            char[] splitter = { delimiter };
            List<string> returnedList = new List<string>();

            string[] arr = inputLine.Split(splitter);
            foreach (string str in arr)
            {
                returnedList.Add(str);
            }
            return returnedList;
        }
        protected int getIndexForTableHeader(List<string> tableHeaders, string target, bool ignoreCase)
        {
            for (int i = 0; i < tableHeaders.Count; i++)
            {
                string columnHeader;

                if (ignoreCase)
                {
                    columnHeader = tableHeaders[i].ToLower();
                    target = target.ToLower();
                }
                else
                {
                    columnHeader = tableHeaders[i];
                }

                
                if (columnHeader == target)
                {
                    return i;
                }
            }
            return -1;     //didn't find header!
        }


    }
}
