using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProteinCalc;
using System.Text.RegularExpressions;
using DeconTools.Utilities;


namespace DeconTools.Backend.Core
{
    public class MassTag
    {
        #region Constructors
        public MassTag()
        {

        }
        #endregion

        #region Properties
        public int ID { get; set; }
        public double MonoIsotopicMass { get; set; }
        public double MZ { get; set; }

        /// <summary>
        /// Number of times MassTag was observed at given ChargeState
        /// </summary>
        public int ObsCount { get; set; }
        public short ChargeState { get; set; }
        public float NETVal { get; set; }
        public string PeptideSequence { get; set; }    //the base peptide sequence (no mods shown)
        public bool ContainsMods { get; set; }  //indicates whether or not peptide contains modifications or abnormalities

        /// <summary>
        /// the ID of the protein to which the MassTag is linked
        /// </summary>
        public int RefID { get; set; }
        public string ProteinDescription { get; set; }

        public IsotopicProfile IsotopicProfile { get; set; }    // the theoretical profile

        public IsotopicProfile IsotopicProfileLabelled { get; set; }  // an optional labelled isotopic profile (i.e used in N15-labelling)


        public Peptide Peptide { get; set; }

        public string EmpiricalFormula { get; set; }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        /// <summary>
        /// Outputs the number of C,H,N,O,S atoms in that order.
        /// TODO:  this needs to be cleaned up!
        /// </summary>
        /// <returns></returns>
        public int[] GetEmpiricalFormulaAsIntArray()
        {
            if (this.Peptide == null)
            {
                if (this.EmpiricalFormula != null && this.EmpiricalFormula.Length > 0)
                {
                    Dictionary<string, int> atomCounts = parseEmpiricalFormulaString(this.EmpiricalFormula);
                    Check.Require(atomCounts.Count == 5, "Currently, formulas must be five elements in length and contain C, H, N, O, S, in that order. '0' must be used if there are no atoms of that element.");
                    Check.Require(atomCounts.ContainsKey("C"), "Currently, formulas must contain Carbon");
                    Check.Require(atomCounts.ContainsKey("H"), "Currently, formulas must contain Hydrogen");
                    Check.Require(atomCounts.ContainsKey("N"), "Currently, formulas must contain Nitrogen");
                    Check.Require(atomCounts.ContainsKey("O"), "Currently, formulas must contain Oxygen");
                    Check.Require(atomCounts.ContainsKey("S"), "Currently, formulas must contain Sulfur");

                    int[] formulaIntArray = new int[5];
                    formulaIntArray[0] = atomCounts["C"];
                    formulaIntArray[1] = atomCounts["H"];
                    formulaIntArray[2] = atomCounts["N"];
                    formulaIntArray[3] = atomCounts["O"];
                    formulaIntArray[4] = atomCounts["S"];
                    return formulaIntArray;

                }
                else
                {
                    return null;
                }
            }
            return this.Peptide.GetEmpiricalFormulaIntArray();
        }

        private Dictionary<string, int> parseEmpiricalFormulaString(string p)
        {
            Regex re = new Regex(@"([A-Z][a-z]*)(\d*)");    //got this from StackOverflow
            MatchCollection mc = re.Matches(p);

            Dictionary<string, int> parsedFormula = new Dictionary<string, int>();

            foreach (Match item in mc)
            {
                bool numAtomsAreIndicated = (item.Groups.Count == 3);

                int numAtoms = 0;
                string elementSymbol = String.Empty;

                elementSymbol = item.Groups[1].Value;
                if (numAtomsAreIndicated)
                {
                    numAtoms = Int32.Parse(item.Groups[2].Value);
                }
                else
                {
                    numAtoms = 1;
                }

                bool formulaContainsDuplicateElements = (parsedFormula.ContainsKey(elementSymbol));
                Check.Require(!formulaContainsDuplicateElements, "Cannot parse formula string. It contains multiple identical elements.");

                parsedFormula.Add(elementSymbol, numAtoms);
            
   
            }

            return parsedFormula;

        }

        public void CalculateMassesForIsotopicProfile(int chargeState)
        {
            if (this.IsotopicProfile == null || this.IsotopicProfile.Peaklist==null) return;

            for (int i = 0; i < this.IsotopicProfile.Peaklist.Count; i++)
            {
                double calcMZ = this.MonoIsotopicMass / chargeState + Globals.PROTON_MASS + i * 1.00235 / chargeState;
                this.IsotopicProfile.Peaklist[i].XValue = calcMZ;
            }

        }



        public void CreatePeptideObject()
        {
            this.CreatePeptideObject(true);

        }

        public void CreatePeptideObject(bool verifyMass)
        {
           
            if (this.PeptideSequence == null || this.PeptideSequence.Length == 0) return;

            try
            {
                this.Peptide = new Peptide(this.PeptideSequence);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating peptide object. details: " + ex.Message);
                this.Peptide = null;
                return;
            }
            
           
            if (verifyMass)
            {
                if ((this.Peptide.MonoIsotopicMass - this.MonoIsotopicMass) > 0.001) this.Peptide = null;   // the MassTag.MonoIsotopicMass is directly from DMS and is the official mass. If the peptide's calculated MonoMW differs, then there must be a modification or something else going on; don't use it. 
            }


        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.ID);
            sb.Append("; ");
            sb.Append(this.MZ.ToString("0.0000"));
            sb.Append("; ");
            sb.Append(this.ChargeState);
            sb.Append("; ");
            sb.Append(this.NETVal);
            sb.Append("; ");
            sb.Append(this.PeptideSequence);

            return sb.ToString();
        }
    }
}
