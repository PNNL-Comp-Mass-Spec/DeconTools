using System;
using System.Collections.Generic;
using System.Linq;
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


        public int GetTotalNodelLevels(IqTarget inputTarget)
        {
            int levels = 0;

            var target = inputTarget.RootTarget;


            while (target.HasChildren())
            {
                target = target.ChildTargets().First();
                levels++;
            }

            return 1+ levels;

        }


        public List<IqTarget> GetTargetsFromNodelLevel(IqTarget target, int level)
        {
            var iqTargetList = new List<IqTarget>();
            iqTargetList.Add(target);

            return GetTargetsFromNodelLevel(iqTargetList, level);

        }



        public List<IqTarget> GetTargetsFromNodelLevel(List<IqTarget> inputTargets, int level)
        {

            List<IqTarget> iqtargets = new List<IqTarget>(inputTargets);

            int currentlevel = 0;

            while (currentlevel!=level)
            {
                currentlevel++;

                iqtargets = GetAllTargetsOnNextLevel(iqtargets);


            }

            return iqtargets;



        }


        public List<IqTarget>GetAllTargetsOnNextLevel(List<IqTarget>inputTargets)
        {
            List<IqTarget> iqtargets = new List<IqTarget>();

            foreach (var target in inputTargets)
            {
                if (target.HasChildren())
                {
                    iqtargets.AddRange(target.ChildTargets());
                }
            }

            return iqtargets;
        }





        public void CreateChildTargets(List<IqTarget> targets)
        {
            foreach (IqTarget iqTarget in targets)
            {
                UpdateTargetMissingInfo(iqTarget);

                var childTargets = CreateChargeStateTargets(iqTarget);
                iqTarget.AddTargetRange(childTargets);
            }
        }




        public List<IqTarget> CreateChargeStateTargets(IqTarget iqTarget, double minMZObs = 400, double maxMZObserved = 1500)
        {
            int minCharge = 1;
            int maxCharge = 100;

            UpdateTargetMissingInfo(iqTarget);


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


        public List<IqTarget> CreateTargets(IEnumerable<string> empiricalFormulaList, double minMZObs = 400, double maxMZObserved = 1500)
        {
            int targetIDCounter = 0;

            var targetList = new List<IqTarget>();


            foreach (string formula in empiricalFormulaList)
            {
                IqTarget parentTarget = new IqTargetBasic();


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



        public virtual void UpdateTargetMissingInfo(IqTarget target, bool calcAveragineForMissingEmpiricalFormula = true)
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

                if (target.ChargeState != 0)
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
