using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
    public abstract class IqTargetImporter : ImporterBase<List<IqTarget>>
    {
        private int _lineCounter;
        //note that case does not matter in the header
        //protected string[] DatasetHeaders = { "dataset" };
        //protected string[] EmpiricalFormulaHeaders = { "formula", "empirical_formula", "empiricalFormula", "molecular_formula", "molecularFormula" };
        //protected string[] CodeHeaders = { "code", "sequence" };
        //protected string[] TargetIDHeaders = { "id", "mass_tag_id", "massTagid", "targetid", "mtid" };
        //protected string[] MonomassHeaders = {"MonoMass", "MonoisotopicMass", "UMCMonoMW", "MonoMassIso1" };
        //protected string[] AlternateIDHeader = { "MatchedMassTagID", "AlternateID" };
        //protected string[] MzHeaders = { "MonoMZ","MonoisotopicMZ", "UMCMZForChargeBasis" };
        //protected string[] ScanHeaders = {"ScanLC","LCScan", "scan", "scanClassRep" };
        //protected string[] NETHeaders = { "net", "ElutionTime", "RT", "NETElutionTime" };


        #region Constructors

        protected IqTargetImporter()
        {
            DatasetHeaders = new [] { "dataset" };
            EmpiricalFormulaHeaders = new[] { "formula", "empirical_formula", "empiricalFormula", "molecular_formula", "molecularFormula" };
            CodeHeaders = new[] { "code", "sequence" , "peptide" };
            TargetIDHeaders = new[] { "id", "mass_tag_id", "massTagid", "targetid", "mtid" };
            MonomassHeaders = new[] { "MonoMass", "MonoisotopicMass", "UMCMonoMW", "MonoMassIso1" };
            AlternateIDHeader = new[] { "MatchedMassTagID", "AlternateID" };
            MzHeaders = new[] { "MonoMZ", "MonoisotopicMZ", "UMCMZForChargeBasis" };
            ScanHeaders = new[] { "ScanLC", "LCScan", "Scan", "scanClassRep" , "ScanNum" };
            NETHeaders = new[] { "net", "ElutionTime", "RT", "NETElutionTime" };
            QualityScoreHeaders = new[] {"QValue", "QualityScore"};
            ChargeStateHeaders = new[] {"ChargeState", "Z", "Charge"};
        }


        #endregion

        #region Properties

        protected string[] DatasetHeaders { get; set; }

        protected string[] EmpiricalFormulaHeaders { get; set; }

        protected string[] CodeHeaders { get; set; }

        protected string[] TargetIDHeaders { get; set; }

        protected string[] MonomassHeaders { get; set; }

        protected string[] AlternateIDHeader { get; set; }

        protected string[] MzHeaders { get; set; }

        protected string[] ScanHeaders { get; set; }

        protected string[] NETHeaders { get; set; }

        protected string[] QualityScoreHeaders { get; set; }

        protected string[] ChargeStateHeaders { get; set; }


        protected string Filename { get; set; }

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
                _lineCounter = 1;   //used for tracking which line is being processed. 

                //read and process each line of the file
                while (sr.Peek() > -1)
                {
                    line = sr.ReadLine();
                    List<string> processedData = ProcessLine(line);

                    //ensure that processed line is the same size as the header line
                    if (processedData.Count != m_columnHeaders.Count)
                    {
                        throw new InvalidDataException("In File: " + Path.GetFileName(Filename) + "; Data in row # " + _lineCounter + " is NOT valid - \nThe number of columns does not match that of the header line");
                    }

                    IqTarget target = ConvertTextToIqTarget(processedData);

                    allTargets.Add(target);
                    _lineCounter++;

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

        protected int GetAutoIncrementForTargetID()
        {
            return _lineCounter;
        }


        #endregion

        #region Private Methods

        #endregion


    }
}
