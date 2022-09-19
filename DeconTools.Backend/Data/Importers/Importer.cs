using System;
using System.Collections.Generic;

namespace DeconTools.Backend.Data
{
    public abstract class Importer<T>
    {
        public abstract T Import();

        protected char delimiter = '\t';

        protected string lookup(List<string> data, List<string> headers, string targetHeader)
        {
            var columnIndex = getIndexForTableHeader(headers, targetHeader, false);
            if (columnIndex == -1)
            {
                return "";
            }

            return data[columnIndex];
        }

        protected bool parseBoolField(string inputString)
        {
            if (bool.TryParse(inputString, out var result))
            {
                return result;
            }

            return false;     //TODO:  need to figure out the default value
        }

        protected short parseShortField(string inputString)
        {
            if (Int16.TryParse(inputString, out var result))
            {
                return result;
            }

            return 0;
        }

        protected double parseDoubleField(string inputString)
        {
            if (double.TryParse(inputString, out var result))
            {
                return result;
            }

            return double.NaN;
        }

        protected float parseFloatField(string inputString)
        {
            if (float.TryParse(inputString, out var result))
            {
                return result;
            }

            return float.NaN;
        }

        protected int parseIntField(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return -1;
            }

            if (int.TryParse(inputString, out var result))
            {
                return result;
            }

            var secondAttempt = parseDoubleField(inputString);
            if (!double.IsNaN(secondAttempt))
            {
                return (int)secondAttempt;
            }
            return -1;
        }

        protected List<string> processLine(string inputLine)
        {
            char[] splitter = { delimiter };
            var returnedList = new List<string>();

            var arr = inputLine.Split(splitter);
            foreach (var str in arr)
            {
                returnedList.Add(str);
            }
            return returnedList;
        }
        protected int getIndexForTableHeader(List<string> tableHeaders, string target, bool ignoreCase)
        {
            for (var i = 0; i < tableHeaders.Count; i++)
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
