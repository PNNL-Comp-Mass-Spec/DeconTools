using System;
using System.Collections.Generic;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class PeptideTarget : TargetBase
    {
        #region Constructors
        public PeptideTarget()
        {
        }

        public PeptideTarget(PeptideTarget copiedTarget)
            : base(copiedTarget)
        {
            ProteinDescription = copiedTarget.ProteinDescription;
            GeneReference = copiedTarget.GeneReference;
            RefID = copiedTarget.RefID;
        }

        #endregion

        #region Properties

        /// <summary>
        /// the ID of the protein to which the peptide MassTag is linked
        /// </summary>
        public int RefID { get; set; }
        public string ProteinDescription { get; set; }
        public string GeneReference { get; set; }

        /// <summary>
        /// Number of multiple proteins matching peptide. '0' means it is a unique peptide matching only one protein.
        /// This is awkward, but designed to match how this is done in the DMS database.
        /// </summary>
        public int MultipleProteinCount { get; set; }

        #endregion

        public override string GetEmpiricalFormulaFromTargetCode()
        {
            return PeptideUtils.GetEmpiricalFormulaForPeptideSequence(Code);
        }

        public void CalculateMassesForIsotopicProfile(int chargeState)
        {
            if (IsotopicProfile?.Peaklist == null)
            {
                return;
            }

            for (var i = 0; i < IsotopicProfile.Peaklist.Count; i++)
            {
                var calcMZ = MonoIsotopicMass / chargeState + Globals.PROTON_MASS + i * 1.00235 / chargeState;
                IsotopicProfile.Peaklist[i].XValue = calcMZ;
            }
        }

        public override string ToString()
        {
            var data = new List<string> {
                ID.ToString(),
                MonoIsotopicMass.ToString("0.0000"),
                MZ.ToString("0.0000"),
                ChargeState.ToString(),
                NormalizedElutionTime.ToString("0.000"),
                Code
            };

            return string.Join("; ", data);
        }
    }
}
