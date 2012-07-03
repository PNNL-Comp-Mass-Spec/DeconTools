using System;
using System.Text;


namespace DeconTools.Backend.Core
{
    [Serializable]
    public class PeptideTarget : TargetBase
    {
        #region Constructors
        public PeptideTarget()
            : base()
        {

        }


        public PeptideTarget(PeptideTarget copiedTarget)
            : base(copiedTarget)
        {
            this.ProteinDescription = copiedTarget.ProteinDescription;
            this.GeneReference = copiedTarget.GeneReference;
            this.RefID = copiedTarget.RefID;
        }

        #endregion

        #region Properties






        /// <summary>
        /// the ID of the protein to which the peptide MassTag is linked
        /// </summary>
        public int RefID { get; set; }
        public string ProteinDescription { get; set; }
        public string GeneReference { get; set; }


        #endregion



        public override string GetEmpiricalFormulaFromTargetCode()
        {
            return PeptideUtils.GetEmpiricalFormulaForPeptideSequence(Code);
        }



        public void CalculateMassesForIsotopicProfile(int chargeState)
        {
            if (this.IsotopicProfile == null || this.IsotopicProfile.Peaklist == null) return;

            for (int i = 0; i < this.IsotopicProfile.Peaklist.Count; i++)
            {
                double calcMZ = this.MonoIsotopicMass / chargeState + Globals.PROTON_MASS + i * 1.00235 / chargeState;
                this.IsotopicProfile.Peaklist[i].XValue = calcMZ;
            }

        }





        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(ID);
            sb.Append("; ");
            sb.Append(MonoIsotopicMass.ToString("0.0000"));
            sb.Append("; ");
            sb.Append(MZ.ToString("0.0000"));
            sb.Append("; ");
            sb.Append(ChargeState);
            sb.Append("; ");
            sb.Append(NormalizedElutionTime);
            sb.Append("; ");
            sb.Append(Code);

            return sb.ToString();
        }
    }
}
