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

        const double FREEACID_MONOISOTOPIC_MASS = HYDROGEN_MASS + OXYGEN16_MASS;

        const string AMINO_ACID_SINGLE_LETTER_CODES = "ARNDCEQGHILKMFPSTWYV";
        private List<Tuple<char, string, double>> _aminoAcidList;

        #region Constructors

        public PeptideUtils()
        {
            createAminoAcidDataTable();
        }

        private void createAminoAcidDataTable()
        {
            _aminoAcidList = new List<Tuple<char, string, double>>
            {
                Tuple.Create('A', "C3H5NO", 71.037114d),
                Tuple.Create('R', "C6H12N4O", 156.10111d),
                Tuple.Create('N', "C4H6N2O2", 114.04293d),
                Tuple.Create('D', "C4H5NO3", 115.02694d),
                Tuple.Create('C', "C3H5NOS", 103.00919d),
                Tuple.Create('E', "C5H7NO3", 129.04259d),
                Tuple.Create('Q', "C5H8N2O2", 128.05858d),
                Tuple.Create('G', "C2H3NO", 57.021464d),
                Tuple.Create('H', "C6H7N3O", 137.05891d),
                Tuple.Create('I', "C6H11NO", 113.08406d),
                Tuple.Create('L', "C6H11NO", 113.08406d),
                Tuple.Create('K', "C6H12N2O", 128.09496d),
                Tuple.Create('M', "C5H9NOS", 131.04048d),
                Tuple.Create('F', "C9H9NO", 147.06841d),
                Tuple.Create('P', "C5H7NO", 97.052764d),
                Tuple.Create('S', "C3H5NO2", 87.032029d),
                Tuple.Create('T', "C4H7NO2", 101.04768d),
                Tuple.Create('W', "C11H10N2O", 186.07931d),
                Tuple.Create('Y', "C9H9NO2", 163.06333d),
                Tuple.Create('V', "C5H9NO", 99.068414d)
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
                outputMonoisotopicMass += FREEACID_MONOISOTOPIC_MASS;
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

                var nterminalHydrogen = "H";
                var cterminalFreeAcid = "OH";

                outputEmpiricalFormula = EmpiricalFormulaUtilities.AddFormula(outputEmpiricalFormula, nterminalHydrogen);
                outputEmpiricalFormula = EmpiricalFormulaUtilities.AddFormula(outputEmpiricalFormula, cterminalFreeAcid);
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

            if (!AMINO_ACID_SINGLE_LETTER_CODES.Contains(aminoAcidCode))
            {
                throw new ArgumentException("Amino acid code is invalid. Inputted code= " + aminoAcidCode);
            }

            var aminoAcid = (from n in _aminoAcidList where n.Item1 == aminoAcidCode select n).First();

            return aminoAcid.Item3;
        }


        public string GetEmpiricalFormulaForAminoAcidResidue(char aminoAcidCode)
        {
            if (!AMINO_ACID_SINGLE_LETTER_CODES.Contains(aminoAcidCode))
            {
                throw new ArgumentException("Inputted amino acid code is not a proper single letter code. Inputted code = " + aminoAcidCode);
            }

            var aminoAcid = (from n in _aminoAcidList where n.Item1 == aminoAcidCode select n).First();

            return aminoAcid.Item2;

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
                if (!AMINO_ACID_SINGLE_LETTER_CODES.Contains(aminoAcid))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

    }
}
