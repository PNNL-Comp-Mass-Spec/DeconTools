using System.Text;


namespace DeconTools.Backend.Core
{
    public class MassTag : TargetBase
    {
        #region Constructors
        public MassTag()
        {

        }
        #endregion

        #region Properties


        /// <summary>
        /// Number of times MassTag was observed at given ChargeState
        /// </summary>
        public int ObsCount { get; set; }



        public bool ContainsMods { get; set; }  //indicates whether or not peptide contains modifications or abnormalities

        /// <summary>
        /// the ID of the protein to which the MassTag is linked
        /// </summary>
        public int RefID { get; set; }
        public string ProteinDescription { get; set; }

        
        #endregion



        public override string GetEmpiricalFormulaFromTargetCode()
        {
            return this.PeptideUtils.GetEmpiricalFormulaForPeptideSequence(this.Code);
        }



        /// <summary>
        /// Outputs the number of C,H,N,O,S atoms in that order.
        /// TODO:  this needs to be depreciated!
        /// </summary>
        /// <returns></returns>
        public int[] GetEmpiricalFormulaAsIntArray()
        {
                   
                    int[] formulaIntArray = new int[5];
                    formulaIntArray[0] = this.GetAtomCountForElement("C");
                    formulaIntArray[1] = this.GetAtomCountForElement("H");
                    formulaIntArray[2] = this.GetAtomCountForElement("N");
                    formulaIntArray[3] = this.GetAtomCountForElement("O");
                    formulaIntArray[4] = this.GetAtomCountForElement("S");
                    return formulaIntArray;
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
            StringBuilder sb = new StringBuilder();
            sb.Append(this.ID);
            sb.Append("; ");
            sb.Append(this.MZ.ToString("0.0000"));
            sb.Append("; ");
            sb.Append(this.ChargeState);
            sb.Append("; ");
            sb.Append(this.NormalizedElutionTime);
            sb.Append("; ");
            sb.Append(this.Code);

            return sb.ToString();
        }
    }
}
