using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Data.Importers
{
    /// <summary>
    /// Purpose of this importer is to retrieve a list of Mass Tag IDs from a delimited text file
    /// </summary>
    public class MassTagIDGenericImporter : Importer<MassTagCollection>
    {
        private string filename;
        private StreamReader reader;

        #region Constructors
        public MassTagIDGenericImporter(string filename)
            : this(filename, '\t')
        {

        }

        public MassTagIDGenericImporter(string filename, char delimiter)
        {
            Check.Require(File.Exists(filename), "MassTagID Importer failed. File does not exist.");
            this.filename = filename;

            try
            {
                reader = new StreamReader(filename);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            this.delimiter = delimiter;
        }


        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void Import(MassTagCollection massTagCollection)
        {
            massTagCollection.MassTagIDList = new List<long>();

            using (StreamReader reader = this.reader)
            {
                string header = reader.ReadLine();
                List<string> processedHeader = processLine(header);

                int indexOfMTIDCol = getIndexForTableHeader(processedHeader, "Mass_Tag_ID", true);
                if (indexOfMTIDCol == -1)
                {
                    indexOfMTIDCol = getIndexForTableHeader(processedHeader, "MassTagID", true);
                    if (indexOfMTIDCol == -1)
                    {
                        throw new Exception("Importer failed. Can't find column header 'Mass_Tag_ID'");
                    }
                }

                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    List<string> processedLine = processLine(line);


                    long massTagID = -1;
                    bool parsedOK = long.TryParse(processedLine[indexOfMTIDCol], out massTagID);
                    if (!parsedOK) massTagID = -1;
                    massTagCollection.MassTagIDList.Add(massTagID);
                }





            }


        }
    }
}
