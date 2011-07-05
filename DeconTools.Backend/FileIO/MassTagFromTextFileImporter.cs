using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Utilities;


namespace DeconTools.Backend.FileIO
{
    public class MassTagFromTextFileImporter:IMassTagImporter
    {
        #region Constructors

        string m_filename;
        private List<string> _headers;

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
        public override DeconTools.Backend.Core.MassTagCollection Import()
        {
            DeconTools.Backend.Core.MassTagCollection data = new MassTagCollection();

            using (StreamReader reader = new StreamReader(m_filename))
            {
                string headerLine = reader.ReadLine();    //first line is the header line.   

                _headers = processLine(headerLine);
                
                int lineCounter = 1;
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    lineCounter++;

                    List<string> lineData = processLine(line);

                    MassTag massTag;
                    try
                    {
                        massTag = convertTextToMassTag(lineData);
                    }
                    catch (Exception ex)
                    {
                        string msg = "Importer failed. Error reading line: " + lineCounter.ToString() + "\nDetails: " + ex.Message;
                        throw new Exception(msg);
                    }

                    data.MassTagList.Add(massTag);
                    
                     



                }


            }
            return data;
        }

        private MassTag convertTextToMassTag(List<string> lineData)
        {

            MassTag mt = new MassTag();
            mt.ChargeState = (short)parseIntField(getValue(new string[] { "z", "charge_state"}, lineData, "0")); 

            mt.ID = parseIntField(getValue(new string[]{"id","mass_tag_id","massTagid"},lineData,"-1"));
            mt.PeptideSequence = getValue(new string[] { "peptide", "sequence" }, lineData, ""); 
            mt.NETVal = parseFloatField(getValue(new string[] { "net", "avg_ganet" }, lineData, "-1"));
            mt.ObsCount = parseIntField(getValue(new string[] { "obs", "obscount" }, lineData, "-1"));

            mt.MonoIsotopicMass = parseDoubleField(getValue(new string[] { "mass", "monoisotopicmass" }, lineData, "0"));

            mt.CreatePeptideObject(false);

            if (mt.ChargeState == 0)
            {

            }
            else
            {
                mt.MZ = parseDoubleField(getValue(new string[] { "mz" }, lineData, "0"));
                if (mt.MZ == 0 || mt.MZ == double.NaN)
                {
                    mt.MZ = mt.Peptide.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
                }

                mt.MonoIsotopicMass = (mt.MZ - Globals.PROTON_MASS) * mt.ChargeState;
            }
            
            mt.RefID = parseIntField(getValue(new string[] { "ref_id" }, lineData, "-1"));
            mt.ProteinDescription = getValue(new string[] { "description" }, lineData, "");
            
            return mt;
            
        }

        private string getValue(string[] possibleHeaders, List<string> lineData, string defaultVal)
        {
            foreach (var possibleHeader in possibleHeaders)
            {

                
                int indexOfHeader = getIndexForTableHeader(_headers, possibleHeader, true);
                bool foundHeader = (indexOfHeader != -1);

                if (foundHeader)
                {
                    return lineData[indexOfHeader];
                }
                
            }
            return defaultVal;

        }

        private List<string> getHeaders(string headerLine)
        {
            List<string> processedLine = headerLine.Split(new char[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return processedLine;
        }
    }
}
