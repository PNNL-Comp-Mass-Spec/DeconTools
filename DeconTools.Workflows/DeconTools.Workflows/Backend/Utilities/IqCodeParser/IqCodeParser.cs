using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;

namespace DeconTools.Workflows.Backend.Utilities.IqCodeParser
{
    /// <summary>
    /// Base class for parsing in sequences that do or do not contain PTMs
    /// </summary>
    public class IqCodeParser
    {

        #region Constructors
        
        public IqCodeParser()
        {
            PeptideUtils = new PeptideUtils();
            SequenceExpression = @"[\].)(\[+-123456789]";
            PtmExpression = @"([+-]?([0-9]*)\.?([0-9]+))";
        }

        #endregion

        #region Properties

        //Regular expression that finds PTMs in a specified file format
        protected string PtmExpression { get; set; }

        //Regular expression that finds the unmodified sequence in a specified file format
        protected string SequenceExpression { get; set; }

        protected PeptideUtils PeptideUtils { get; set; }

        #endregion

        #region Public Methods

        //Returns an empirical formula string with or without PTMs.
        //Accounts for the PTMs using the Averagine formula.
        //Parses out the PTMs and calculates the empirical formula for the known unmodified sequence.
        //Adds or subtracts the PTM formula from the sequence formula based on the overall mass of the PTMs
        public string GetEmpiricalFormulaFromSequence(string code, bool cysteinesAreModified = false)
        {
            var ptmFormula = "";
            var sequenceFormula = "";
            var empiricalFormula = "";

            var ptmMass = PtmMassFromCode(code);
            ptmFormula = IsotopicDistributionCalculator.Instance.GetAveragineFormulaAsString(Math.Abs(ptmMass), false);

            sequenceFormula = SequenceToEmpiricalFormula(code, cysteinesAreModified);

            if(ptmMass < 0)
            {
                empiricalFormula = EmpiricalFormulaUtilities.SubtractFormula(sequenceFormula, ptmFormula);
            }
            else
            {
                empiricalFormula = EmpiricalFormulaUtilities.AddFormula(sequenceFormula, ptmFormula);
            }

            //TEMPORARY HANDLING OF BAD TARGETS WITH PTM > SEQUENCE
            //CHECK THE UPDATEMISSINGTARGETINFO IN IQTARGETUTILITES WHEN CHANGING
            if (String.IsNullOrEmpty(empiricalFormula))
            {
                empiricalFormula = "C0H0N0O0S0";
            }
            return empiricalFormula;
        }

        //Extracts the PTM masses from the input seqence.
        //Uses the regex PTMExpression to parse for the PTM in a give input format.
        public double PtmMassFromCode(string code)
        {
            var ptmMass = 0.0;
            var masses = Regex.Matches(code, PtmExpression);
            foreach (Match match in masses)
            {
                ptmMass += Convert.ToDouble(match.Value);
            }
            return ptmMass;
        }

        //Extracts the unmodified sequence from the original input sequence. 
        //Uses the regex SequenceExpression to parse for the sequence in a specified format.
        public string SequenceToEmpiricalFormula(string code, bool cysteinesAreModified = false)
        {
            //TODO:  this doesn't work with:  '


            var sequence = "";

            //Removes the . start and stop notation
            var front = Regex.Replace(code, @"^[A-Z\-_]?[\.]", "");
            var rear = Regex.Replace(front, @"[\.]+[A-Z\-_]?$", "");
            var test = Regex.Split(rear, SequenceExpression);

            foreach (var s in test)
            {
                sequence += Regex.Match(s, "^[A-Z]*[A-Z]$").Value;
            }

            
            return PeptideUtils.GetEmpiricalFormulaForPeptideSequence(sequence, true, cysteinesAreModified);
        }

        public bool CheckSequenceIntegrity(string sequence)
        {
            var ptmMass = PtmMassFromCode(sequence);
            var sequenceFormula = SequenceToEmpiricalFormula(sequence);
            var sequenceMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(sequenceFormula);
            if ((ptmMass < 0) && (Math.Abs(ptmMass) > (sequenceMass / 2)))
            {
                return false;
            }
            return true;
        }

        public List<double> GetPTMList(string code)
        {
            var ptmMasses = new List<double>();
            var masses = Regex.Matches(code, PtmExpression);
            foreach (Match match in masses)
            {
                ptmMasses.Add(Convert.ToDouble(match.Value));
            }
            return ptmMasses;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
