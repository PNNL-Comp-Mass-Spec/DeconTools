using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Utilities;


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
                string headerLine = reader.ReadLine();    //first line is the header line.   

                List<string> headers = processLine(headerLine);
                
                int lineCounter = 1;
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    lineCounter++;

                    List<string> lineData = processLine(line);

                    MassTag massTag;
                    try
                    {
                        massTag = convertTextToMassTag(lineData, headers);
                    }
                    catch (Exception ex)
                    {
                        string msg = "Importer failed. Error reading line: " + lineCounter.ToString() + "\nDetails: " + ex.Message;
                        throw new Exception(msg);
                    }

                    data.MassTagList.Add(massTag);
                    
                     



                }


            }
        }

        private MassTag convertTextToMassTag(List<string> lineData, List<string> headers)
        {

            MassTag mt = new MassTag();
            mt.ChargeState =(short) parseIntField(lineData[getIndexForTableHeader(headers, "z", true)]);

            Check.Assert(mt.ChargeState != 0, "Charge state cannot be 0");

            mt.ID = parseIntField(lineData[getIndexForTableHeader(headers, "id", true)]);
            mt.PeptideSequence = lineData[getIndexForTableHeader(headers, "sequence", true)];
            mt.NETVal = parseFloatField(lineData[getIndexForTableHeader(headers, "net", true)]);
            mt.ObsCount = parseIntField(lineData[getIndexForTableHeader(headers, "obscount", true)]);
            mt.MZ = parseDoubleField(lineData[getIndexForTableHeader(headers, "mz", true)]);

            mt.CreatePeptideObject(false);

            if (mt.MZ == 0 || mt.MZ == double.NaN)
            {
                mt.MZ = mt.Peptide.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

                
            }
            mt.MonoIsotopicMass = (mt.MZ - Globals.PROTON_MASS) * mt.ChargeState;
            mt.RefID = parseIntField(lineData[getIndexForTableHeader(headers, "ref_id", true)]);
            mt.ProteinDescription = lineData[getIndexForTableHeader(headers, "description", true)];
            
            return mt;
            
        }

        private List<string> getHeaders(string headerLine)
        {
            List<string> processedLine = headerLine.Split(new char[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return processedLine;
        }
    }
}
