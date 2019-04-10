using System;
using System.Collections.Generic;
using System.Linq;


namespace DeconTools.Backend.Utilities
{
    [Serializable]
    public class PeptideUtils
    {

        const double HYDROGEN_MASS = 1.00782503196;  // IUPAC 2002
        const double OXYGEN16_MASS = 15.994914622325; // IUPAC 2002

        const double FREE_ACID_MONOISOTOPIC_MASS = HYDROGEN_MASS + OXYGEN16_MASS;

        /// <summary>
        /// Amino acids
        /// </summary>
        /// <remarks>Keys are one-letter amino acid symbols; values are empirical formula and mass</remarks>
        private readonly Dictionary<char, KeyValuePair<string, double>> _aminoAcidList;

        #region Constructors

        public PeptideUtils()
        {
            _aminoAcidList = new Dictionary<char, KeyValuePair<string, double>> {
                { 'A', new KeyValuePair<string, double>("C3H5NO", 71.037114)},
                { 'R', new KeyValuePair<string, double>("C6H12N4O", 156.10111)},
                { 'N', new KeyValuePair<string, double>("C4H6N2O2", 114.04293)},
                { 'D', new KeyValuePair<string, double>("C4H5NO3", 115.02694)},
                { 'C', new KeyValuePair<string, double>("C3H5NOS", 103.00919)},
                { 'E', new KeyValuePair<string, double>("C5H7NO3", 129.04259)},
                { 'Q', new KeyValuePair<string, double>("C5H8N2O2", 128.05858)},
                { 'G', new KeyValuePair<string, double>("C2H3NO", 57.021464)},
                { 'H', new KeyValuePair<string, double>("C6H7N3O", 137.05891)},
                { 'I', new KeyValuePair<string, double>("C6H11NO", 113.08406)},
                { 'L', new KeyValuePair<string, double>("C6H11NO", 113.08406)},
                { 'K', new KeyValuePair<string, double>("C6H12N2O", 128.09496)},
                { 'M', new KeyValuePair<string, double>("C5H9NOS", 131.04048)},
                { 'F', new KeyValuePair<string, double>("C9H9NO", 147.06841)},
                { 'P', new KeyValuePair<string, double>("C5H7NO", 97.052764)},
                { 'S', new KeyValuePair<string, double>("C3H5NO2", 87.032029)},
                { 'T', new KeyValuePair<string, double>("C4H7NO2", 101.04768)},
                { 'W', new KeyValuePair<string, double>("C11H10N2O", 186.07931)},
                { 'Y', new KeyValuePair<string, double>("C9H9NO2", 163.06333)},
                { 'V', new KeyValuePair<string, double>("C5H9NO", 99.068414)}
            };

        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public double GetMonoIsotopicMassForPeptideSequence(string peptideSequence, bool includeAmineHydrogenAndFreeAcid = true, bool cysteinesAreModified=false)
        {

            peptideSequence = CleanUpPeptideSequence(peptideSequence);

            var sequenceIsValid = ValidateSequence(peptideSequence);
            if (!sequenceIsValid)
            {
                throw new ArgumentException("Inputted peptide sequence contains illegal characters.");
            }


            double outputMonoisotopicMass = 0;

            foreach (var aminoAcid in peptideSequence)
            {
                var aminoAcidResideMass = GetMonoisotopicMassForAminoAcidResidue(aminoAcid);

                outputMonoisotopicMass += aminoAcidResideMass;

                if (cysteinesAreModified &&  aminoAcid=='C')
                {
                    var iodoacetamideMass = 57.021463725623;

                    outputMonoisotopicMass += iodoacetamideMass;

                }
            }

            if (includeAmineHydrogenAndFreeAcid)
            {
                outputMonoisotopicMass += HYDROGEN_MASS;
                outputMonoisotopicMass += FREE_ACID_MONOISOTOPIC_MASS;
            }

            return outputMonoisotopicMass;
        }

        public string GetEmpiricalFormulaForPeptideSequence(string peptideSequence, bool includeAmineHydrogenAndFreeAcid = true, bool cysteinesAreModified = false)
        {
            peptideSequence = CleanUpPeptideSequence(peptideSequence);

            var sequenceIsValid = ValidateSequence(peptideSequence);
            if (!sequenceIsValid) return string.Empty;

            var outputEmpiricalFormula = "";

            foreach (var aminoAcid in peptideSequence)
            {
                var aminoAcidFormula = GetEmpiricalFormulaForAminoAcidResidue(aminoAcid);

                outputEmpiricalFormula = EmpiricalFormulaUtilities.AddFormula(outputEmpiricalFormula, aminoAcidFormula);

                if (cysteinesAreModified && aminoAcid=='C')
                {
                    outputEmpiricalFormula = EmpiricalFormulaUtilities.AddFormula(outputEmpiricalFormula, "H3C2NO");
                }
            }

            if (includeAmineHydrogenAndFreeAcid)
            {

                var nTerminalHydrogen = "H";
                var cTerminalFreeAcid = "OH";

                outputEmpiricalFormula = EmpiricalFormulaUtilities.AddFormula(outputEmpiricalFormula, nTerminalHydrogen);
                outputEmpiricalFormula = EmpiricalFormulaUtilities.AddFormula(outputEmpiricalFormula, cTerminalFreeAcid);
            }
            return outputEmpiricalFormula;
        }

        public string CleanUpPeptideSequence(string peptideSequence)
        {
            if (peptideSequence.Contains("."))
            {
                var numberDotChars = peptideSequence.Count(p => p == '.');

                if (numberDotChars == 1)
                {
                    throw new FormatException("There is a problem with peptide sequence " + peptideSequence +
                                              ";  it contains only one '.'. It should contain two.");
                }

                if (numberDotChars == 2)
                {
                    var indexFirstDot = peptideSequence.IndexOf('.');
                    var indexSecondDot = peptideSequence.LastIndexOf('.');

                    peptideSequence = peptideSequence.Substring(indexFirstDot+1, indexSecondDot - indexFirstDot-1);
                }
                else
                {
                    throw new FormatException("There is a problem with peptide sequence " + peptideSequence +
                                              ";  it contains more than two '.' characters");
                }
            }

            if (peptideSequence.Contains('*'))
            {
                peptideSequence = peptideSequence.Replace("*", string.Empty);
            }
            return peptideSequence;
        }

        private double GetMonoisotopicMassForAminoAcidResidue(char aminoAcidCode)
        {
            if (!_aminoAcidList.TryGetValue(aminoAcidCode, out var aminoAcidInfo))
            {
                throw new ArgumentException("Amino acid code is invalid: " + aminoAcidCode);
            }

            // Return the monoisotopic mass
            return aminoAcidInfo.Value;
        }

        public string GetEmpiricalFormulaForAminoAcidResidue(char aminoAcidCode)
        {
            if (!_aminoAcidList.TryGetValue(aminoAcidCode, out var aminoAcidInfo))
            {
                throw new ArgumentException("Amino acid code is invalid: " + aminoAcidCode);
            }

            // Return the empirical formula
            return aminoAcidInfo.Key;

        }

        public int GetNumAtomsForElement(string element, string empiricalFormula)
        {
            var parsedFormula = ParseEmpiricalFormulaString(empiricalFormula);

            if (parsedFormula.ContainsKey(element))
            {
                return parsedFormula[element];
            }

            return 0;
        }

        public Dictionary<string, int> ParseEmpiricalFormulaString(string empiricalFormula)
        {

            return EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(empiricalFormula);

        }

        public bool ValidateSequence(string peptideSequence)
        {
            if (string.IsNullOrEmpty(peptideSequence)) return false;

            foreach (var aminoAcid in peptideSequence)
            {
                if (!_aminoAcidList.ContainsKey(aminoAcid))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

    }
}
