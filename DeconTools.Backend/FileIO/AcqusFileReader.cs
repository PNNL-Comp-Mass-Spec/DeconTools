using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DeconTools.Backend.FileIO
{
    public class AcqusFileReader
    {
        string m_filename;


        #region Constructors
        public AcqusFileReader(string acqusFileName)
        {
            if (File.Exists(acqusFileName))
            {
                m_filename = acqusFileName;
            }
            else
            {
                throw new FileNotFoundException("Could not read file. File not found.");
            }

            DataLookupTable = new Dictionary<string, double>();

            using (StreamReader sr=new StreamReader(acqusFileName))
            {
                while (sr.Peek()!=-1)
                {
                    string currentLine = sr.ReadLine();
                    Match match= Regex.Match(currentLine, @"^##\$(?<name>.*)=\s(?<value>[0-9-\.]+)");

                    if (match.Success)
                    {
                        string variableName = match.Groups["name"].Value;

                        double parsedResult = -1;
                        bool canParseValue = double.TryParse(match.Groups["value"].Value, out parsedResult);
                        if (!canParseValue)
                        {
                            parsedResult = -1;
                        }

                        DataLookupTable.Add(variableName, parsedResult);


                    }

                }

            }


        }
        #endregion

        #region Properties
        public Dictionary<string, double> DataLookupTable { get; set; }
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion


    }
}
