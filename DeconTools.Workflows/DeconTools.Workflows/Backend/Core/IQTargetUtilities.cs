using System;
using System.Collections.Generic;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;

namespace DeconTools.Workflows.Backend.Core
{
    public class IqTargetUtilities
    {
        private PeptideUtils _peptideUtils = new PeptideUtils();

        protected IsotopicDistributionCalculator IsotopicDistributionCalculator = IsotopicDistributionCalculator.Instance;


        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods


        public List<IqTarget> CreateChargeStateTargets(IqTarget iqTarget, double minMZObs = 400, double maxMZObserved = 1500)
        {
            int minCharge = 1;
            int maxCharge = 100;


            List<IqTarget> targetList = new List<IqTarget>();

            for (int charge = minCharge; charge <= maxCharge; charge++)
            {
                double mz = iqTarget.MonoMassTheor / charge + DeconTools.Backend.Globals.PROTON_MASS;

                if (mz < maxMZObserved)
                {

                    if (mz < minMZObs)
                    {
                        break;
                    }

                    IqTarget chargeStateTarget = new IqChargeStateTarget();

                    chargeStateTarget.ID = iqTarget.ID;
                    chargeStateTarget.MonoMassTheor = iqTarget.MonoMassTheor;
                    chargeStateTarget.ChargeState = charge;
                    chargeStateTarget.EmpiricalFormula = iqTarget.EmpiricalFormula;
                    chargeStateTarget.MZTheor = mz;
                    chargeStateTarget.ElutionTimeTheor = iqTarget.ElutionTimeTheor;
                    chargeStateTarget.Code = iqTarget.Code;

                    targetList.Add(chargeStateTarget);
                }
            }

            return targetList;
        }


        public List<IqTarget> CreateTargets(IEnumerable<string>empiricalFormulaList, double minMZObs =400, double maxMZObserved=1500 )
        {
            int targetIDCounter = 0;

            var targetList = new List<IqTarget>();


            foreach (string formula in empiricalFormulaList)
            {
                IqTarget parentTarget = new IqTargetBase();


                parentTarget.EmpiricalFormula = formula;
                parentTarget.ID = targetIDCounter++;

                parentTarget.MonoMassTheor =
                    EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(parentTarget.EmpiricalFormula);

                parentTarget.ElutionTimeTheor = 0.5;
                parentTarget.ChargeState = 0;     //this is the neutral mass


                var childTargets = CreateChargeStateTargets(parentTarget, minMZObs, maxMZObserved);
                targetList.AddRange(childTargets);
            }

            return targetList;




        }



        public virtual void UpdateTargetMissingInfo(IqTarget target, bool calcAveragineForMissingEmpiricalFormula=true)
        {

            
            bool isMissingMonoMass = target.MonoMassTheor <= 0;

            if (String.IsNullOrEmpty(target.EmpiricalFormula))
            {
                if (!String.IsNullOrEmpty(target.Code))
                {
                    //Create empirical formula based on code. Assume it is an unmodified peptide
                    target.EmpiricalFormula = _peptideUtils.GetEmpiricalFormulaForPeptideSequence(target.Code);

                }
                else
                {
                    if (isMissingMonoMass)
                    {
                        throw new ApplicationException(
                            "Trying to fill in missing data on target, but Target is missing both the 'Code' and the Monoisotopic Mass. One or the other is needed.");
                    }
                    target.Code = "AVERAGINE";
                    target.EmpiricalFormula =
                        IsotopicDistributionCalculator.GetAveragineFormulaAsString(target.MonoMassTheor);
                }
            }


            if (isMissingMonoMass)
            {
                target.MonoMassTheor =
                    EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(target.EmpiricalFormula);

                if (target.ChargeState!=0)
                {
                    target.MZTheor = target.MonoMassTheor / target.ChargeState + DeconTools.Backend.Globals.PROTON_MASS;    
                }

                
            }




        }


        #endregion

        #region Private Methods

        #endregion

       
    }
}
