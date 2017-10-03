using System;
using System.Collections.Generic;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public abstract class TargetBase
    {
        protected PeptideUtils PeptideUtils = new PeptideUtils();

        #region Constructors
        public TargetBase()
        {
            ElementLookupTable = new Dictionary<string, int>();
            ElutionTimeUnit = Globals.ElutionTimeUnit.NormalizedElutionTime;
            ChargeStateTargets = new List<int>();
            MsLevel = 1; // Default to MS1 Target
        }

        protected TargetBase(TargetBase copiedTarget)
        {
            ChargeState = copiedTarget.ChargeState;
            Code = copiedTarget.Code;
            ElutionTimeUnit = copiedTarget.ElutionTimeUnit;
            EmpiricalFormula = copiedTarget.EmpiricalFormula;
            ID = copiedTarget.ID;
            IsotopicProfile = copiedTarget.IsotopicProfile == null
                                  ? null
                                  : copiedTarget.IsotopicProfile.CloneIsotopicProfile();
            IsotopicProfileLabelled = copiedTarget.IsotopicProfileLabelled == null
                                          ? null
                                          : copiedTarget.IsotopicProfileLabelled.CloneIsotopicProfile();
            MZ = copiedTarget.MZ;
            ModCount = copiedTarget.ModCount;
            ModDescription = copiedTarget.ModDescription;
            MonoIsotopicMass = copiedTarget.MonoIsotopicMass;
            NormalizedElutionTime = copiedTarget.NormalizedElutionTime;
            ObsCount = copiedTarget.ObsCount;
            ScanLCTarget = copiedTarget.ScanLCTarget;
            MsLevel = copiedTarget.MsLevel;
        }
        #endregion

        #region Properties
        public int ID { get; set; }
        public double MonoIsotopicMass { get; set; }
        public short ChargeState { get; set; }
        public double MZ { get; set; }
        public int MsLevel { get; set; }
        public List<int> ChargeStateTargets { get; set; }

        /// <summary>
        /// Indicates if target contains modifications
        /// </summary>
        public bool ContainsMods => ModCount > 0;

        /// <summary>
        /// Number of modifications on target
        /// </summary>
        public short ModCount { get; set; }

        /// <summary>
        /// Description of modification
        /// </summary>
        public string ModDescription { get; set; }

        private string _empiricalFormula;
        public string EmpiricalFormula
        {
            get => _empiricalFormula;
            set
            {
                _empiricalFormula = value;
                updateElementLookupTable();
            }
        }

        public Globals.ElutionTimeUnit ElutionTimeUnit { get; set; }

        /// <summary>
        /// In some workflows, the target LC scan might be known
        /// </summary>
        public int ScanLCTarget { get; set; }

        public float NormalizedElutionTime { get; set; }

        /// <summary>
        /// the string representative for the Target. E.g. For peptides, this is the single letter amino acid sequence
        /// </summary>
        public string Code { get; set; }


        public IsotopicProfile IsotopicProfile { get; set; }    // the theoretical isotopic profile

        public IsotopicProfile IsotopicProfileLabelled { get; set; }  // an optional labelled isotopic profile (i.e used in N15-labelling)

        public Dictionary<string, int> ElementLookupTable { get; private set; }

        /// <summary>
        /// Number of times MassTag was observed at given ChargeState
        /// </summary>
        public int ObsCount { get; set; }

        #endregion

        #region Public Methods

        public abstract string GetEmpiricalFormulaFromTargetCode();

        public int GetAtomCountForElement(string elementSymbol)
        {
            if (EmpiricalFormula == null || EmpiricalFormula.Length == 0) return 0;

            if (this.ElementLookupTable.ContainsKey(elementSymbol))
            {
                return this.ElementLookupTable[elementSymbol];
            }
            else
            {
                return 0;
            }


        }


        

        public override string ToString()
        {
            return ID + "; " + MonoIsotopicMass.ToString("0.000") + "; " + ChargeState;
        }


        #endregion

        #region Private Methods

        private void updateElementLookupTable()
        {
            this.ElementLookupTable = this.PeptideUtils.ParseEmpiricalFormulaString(this.EmpiricalFormula);
        }







        #endregion

    }
}
