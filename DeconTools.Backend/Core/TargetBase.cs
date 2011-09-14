using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DeconTools.Utilities;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Core
{
    public abstract class TargetBase
    {

        protected PeptideUtils PeptideUtils = new PeptideUtils();


        #region Constructors
        public TargetBase()
        {
            this.ElementLookupTable = new Dictionary<string, int>();
        }
        #endregion

        #region Properties
        public int ID { get; set; }
        public double MonoIsotopicMass { get; set; }
        public short ChargeState { get; set; }
        public double MZ { get; set; }

        private string _empiricalFormula;
        public string EmpiricalFormula
        {
            get
            {
                return _empiricalFormula;
            }
            set
            {
                _empiricalFormula = value;
                updateElementLookupTable();
            }

        }





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


        #endregion

        #region Private Methods

        private void updateElementLookupTable()
        {
            this.ElementLookupTable = this.PeptideUtils.parseEmpiricalFormulaString(this.EmpiricalFormula);
        }




       


        #endregion

    }
}
