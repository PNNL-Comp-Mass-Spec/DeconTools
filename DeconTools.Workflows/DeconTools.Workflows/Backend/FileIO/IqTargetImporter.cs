using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
    public abstract class IqTargetImporter:ImporterBase<List<IqTarget>> 
    {
        //note that case does not matter in the header
        protected string[] DatasetHeaders = { "dataset" };
        protected string[] EmpiricalFormulaHeaders = { "formula", "empirical_formula", "empiricalFormula", "molecular_formula", "molecularFormula" };
        protected string[] CodeHeaders = { "code", "sequence" };
        protected string[] TargetIDHeaders = { "id", "mass_tag_id", "massTagid", "targetid", "mtid" };
        protected string[] MonomassHeaders = { "MonoisotopicMass", "UMCMonoMW", "MonoMassIso1" };
        protected string[] AlternateIDHeader = { "MatchedMassTagID", "AlternateID" };
        protected string[] MzHeaders = { "MonoMZ", "UMCMZForChargeBasis" };
        protected string[] ScanHeaders = { "scan", "scanClassRep" };
        protected string[] NETHeaders = { "net", "ElutionTime", "RT", "NETElutionTime" };
        protected string Filename { get; set; }

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public override List<IqTarget> Import()
        {
            List<IqTarget> allTargets = new List<IqTarget>();

            StreamReader reader;

            if (!File.Exists(Filename))
            {
                throw new System.IO.IOException("Cannot import. File does not exist.");
            }

            try
            {
                reader = new StreamReader(this.Filename);
            }
            catch (Exception)
            {
                throw new System.IO.IOException("There was a problem importing from the file.");
            }

            using (StreamReader sr = reader)
            {
                if (sr.Peek() == -1)
                {
                    sr.Close();
                    throw new InvalidDataException("There is no data in the file we are trying to read.");

                }

                string headerLine = sr.ReadLine();
                CreateHeaderLookupTable(headerLine);

                bool areHeadersValid = ValidateHeaders();

                if (!areHeadersValid)
                {
                    throw new InvalidDataException("There is a problem with the column headers.");
                }


                string line;
                int lineCounter = 1;   //used for tracking which line is being processed. 

                //read and process each line of the file
                while (sr.Peek() > -1)
                {
                    line = sr.ReadLine();
                    List<string> processedData = ProcessLine(line);

                    //ensure that processed line is the same size as the header line
                    if (processedData.Count != m_columnHeaders.Count)
                    {
                        throw new InvalidDataException("In File: " + Path.GetFileName(Filename) + "; Data in row # " + lineCounter.ToString() + " is NOT valid - \nThe number of columns does not match that of the header line");
                    }

                    IqTarget target = ConvertTextToIqTarget(processedData);
                    allTargets.Add(target);
                    lineCounter++;

                }
                sr.Close();

            }

            //remove duplicates:
            var filteredTargets = (from n in allTargets
                                   group n by new
                                                  {
                                                      n.ID
                                                  }
                                   into grp
                                   select grp.First()).ToList();
            
            return filteredTargets;
        }

        protected abstract IqTarget ConvertTextToIqTarget(List<string> processedRowOfText);

        protected virtual bool ValidateHeaders()
        {
            return true;    //TODO: actually do some validation
        }
        
        #endregion

        #region Private Methods

        #endregion

     
    }
}
