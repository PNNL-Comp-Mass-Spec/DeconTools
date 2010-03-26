using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DeconTools.Backend.Data.Importers
{
    public class MassTagFromTextFileImporter:IMassTagImporter
    {
        #region Constructors

        string m_filename;

        public MassTagFromTextFileImporter(string filename)
        {
            if (!File.Exists(filename)) throw new System.IO.IOException("MassTagImporter failed. File doesn't exist.");
            this.m_filename = filename;
            this.delimiter = '\t';


        }




        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void Import(DeconTools.Backend.Core.MassTagCollection data)
        {
            using (StreamReader reader = new StreamReader(m_filename))
            {
                reader.ReadLine();    //first line is the header line.   

                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    //MSPeakResult peak = convertTextToPeakResult(line);


                }


            }
        }
    }
}
