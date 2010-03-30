using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProteinCalc;


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

        public IsotopicProfile IsotopicProfile { get; set; }    // the theoretical profile

        public Peptide Peptide { get; set; }

        public string EmpiricalFormula { get; set; }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        public int[] GetEmpiricalFormulaAsIntArray()
        {
            if (this.Peptide == null) return null;
            return this.Peptide.GetEmpiricalFormulaIntArray();
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
            this.Peptide = new Peptide(this.PeptideSequence);
            if (verifyMass)
            {
                if ((this.Peptide.MonoIsotopicMass - this.MonoIsotopicMass) > 0.001) this.Peptide = null;   // the MassTag.MonoIsotopicMass is directly from DMS and is the official mass. If the peptide's calculated MonoMW differs, then there must be a modification or something else going on; don't use it. 
            }


        }
    }
}
