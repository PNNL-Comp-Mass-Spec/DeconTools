using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Workflows.Backend.Utilities.IqCodeParser;

namespace DeconTools.Workflows.Backend.Core
{
    public class IqTargetUtilities
    {
        private PeptideUtils _peptideUtils = new PeptideUtils();
        protected IqCodeParser IqCodeParser = new IqCodeParser();
        protected IsotopicDistributionCalculator IsotopicDistributionCalculator = IsotopicDistributionCalculator.Instance;

        #region Public Methods

        public int GetTotalNodelLevels(IqTarget inputTarget)
        {
            var levels = 0;

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
            var iqtargets = new List<IqTarget>(inputTargets);

            var currentlevel = 0;

            while (currentlevel!=level)
            {
                currentlevel++;

                iqtargets = GetAllTargetsOnNextLevel(iqtargets);
            }

            return iqtargets;
        }

        public List<IqTarget>GetAllTargetsOnNextLevel(List<IqTarget>inputTargets)
        {
            var iqtargets = new List<IqTarget>();

            foreach (var target in inputTargets)
            {
                if (target.HasChildren())
                {
                    iqtargets.AddRange(target.ChildTargets());
                }
            }

            return iqtargets;
        }

        public void CreateChildTargets(List<IqTarget> targets, double minMZObs = 400, double maxMZObserved = 1500, int maxChargeStatesToCreate = 100, bool cysteinesAreModified = false)
        {
            foreach (var iqTarget in targets)
            {
                UpdateTargetMissingInfo(iqTarget,true, cysteinesAreModified);

                var childTargets = CreateChargeStateTargets(iqTarget, minMZObs, maxMZObserved, maxChargeStatesToCreate);
                iqTarget.AddTargetRange(childTargets);
            }
        }

        public List<IqTarget> CreateChargeStateTargets(IqTarget iqTarget, double minMZObs = 400, double maxMZObserved = 1500, int maxChargeStatesToCreate = 100)
        {
            var minCharge = 1;
            var maxCharge = 100;

            UpdateTargetMissingInfo(iqTarget);

            var targetList = new List<IqTarget>();

            for (var charge = minCharge; charge <= maxCharge; charge++)
            {
                var mz = iqTarget.MonoMassTheor / charge + DeconTools.Backend.Globals.PROTON_MASS;

                if (mz < maxMZObserved)
                {
                    if (mz < minMZObs)
                    {
                        break;
                    }

                    IqTarget chargeStateTarget = new IqChargeStateTarget();

                    CopyTargetProperties(iqTarget, chargeStateTarget);

                    //Note - make sure this step is done after the 'CopyTargetProperties'
                    chargeStateTarget.ChargeState = charge;

                    //adjust isotope profile to reflect new charge state
                    if (chargeStateTarget.TheorIsotopicProfile != null && iqTarget.TheorIsotopicProfile !=null)
                    {
                        chargeStateTarget.TheorIsotopicProfile = AdjustIsotopicProfileMassesFromChargeState(chargeStateTarget.TheorIsotopicProfile, iqTarget.TheorIsotopicProfile.ChargeState, charge);
                    }

                    chargeStateTarget.MZTheor = chargeStateTarget.MonoMassTheor/chargeStateTarget.ChargeState +
                                                DeconTools.Backend.Globals.PROTON_MASS;

                    targetList.Add(chargeStateTarget);
                }
            }

            //sort by charge state (descending). Then take the top N charge states. Then reorder by charge state (ascending).
            targetList = targetList.OrderByDescending(p => p.ChargeState).Take(maxChargeStatesToCreate).OrderBy(p => p.ChargeState).ToList();

            return targetList;
        }

        private IsotopicProfile AdjustIsotopicProfileMassesFromChargeState(IsotopicProfile iso, int existingCharge, int chargeNew)
        {
            if (iso != null)
            {
                //step 1, scale origional iso from mz to mono incease the input target is allready charged
                var massProton = DeconTools.Backend.Globals.PROTON_MASS;

                if (existingCharge > 0)
                {
                    foreach (var peak in iso.Peaklist)
                    {
                        peak.XValue = (peak.XValue * existingCharge) - massProton * existingCharge; //gives us a monoisotopic mass
                    }
                }

                //step 2, scale to mono back to mz using new charge 
                iso.ChargeState = chargeNew;
                foreach (var peak in iso.Peaklist)
                {
                    peak.XValue = (peak.XValue + chargeNew * massProton) / chargeNew;//gives us m/z
                }

                return iso;
            }
            return null;
        }

        public List<IqTarget> CreateTargets(IEnumerable<string> empiricalFormulaList, double minMZObs = 400, double maxMZObserved = 1500)
        {
            var targetIDCounter = 0;

            var targetList = new List<IqTarget>();

            foreach (var formula in empiricalFormulaList)
            {
                IqTarget parentTarget = new IqTargetBasic();

                parentTarget.EmpiricalFormula = formula;
                parentTarget.ID = targetIDCounter++;

                parentTarget.MonoMassTheor =
                    EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(parentTarget.EmpiricalFormula);

                parentTarget.ElutionTimeTheor = 0.5;
                parentTarget.ChargeState = 0;     //this is the neutral mass

                var childTargets = CreateChargeStateTargets(parentTarget, minMZObs, maxMZObserved);
                parentTarget.AddTargetRange(childTargets);

                targetList.Add(parentTarget);
            }

            return targetList;
        }

        /// <summary>
        /// clones simple properties and deeper trees
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public IqTarget DeepClone(IqTarget target)
        {
            var util = new IqTargetUtilities();

            //basic copy

            //TODO: the problem is here is we are creating an IqTargetBasic, which might not fit everyone's liking
            IqTarget copiedTarget = new IqTargetBasic();
            CopyTargetProperties(target, copiedTarget);

            //this returnes the copied tree
            var deepCopy = CloneIqTrees(target);

            //set root via private set
            copiedTarget.RootTarget = deepCopy.RootTarget;

            //set parent target
            copiedTarget.ParentTarget = deepCopy.ParentTarget;

            //set the child targets
            var childTargets = deepCopy.ChildTargets().ToList();
            if (deepCopy.HasChildren() && childTargets.Count > 0)
            {
                foreach (var subtarget in childTargets)
                {
                    copiedTarget.AddTarget(subtarget);
                }
            }

            return copiedTarget;
        }

        /// <summary>
        /// for cloning a IqTarget. Recursive
        /// </summary>
        /// <param name="target">Input the root target so all children will be cloned</param>
        /// <returns></returns>
        public IqTarget Clone(IqTarget target)
        {
            IqTarget tempTarget = new IqTargetBasic();

            CopyTargetProperties(target,tempTarget);

            //child targets
            var childTargets = target.ChildTargets().ToList();
            if (target.HasChildren() && childTargets.Count > 0)
            {
                foreach (var child in childTargets)
                {
                    var clone = Clone(child);
                    clone.ParentTarget = target;
                    tempTarget.AddTarget(clone);
                }
            }
            return tempTarget;
        }

        public virtual void UpdateTargetMissingInfo(IqTarget target, bool calcAveragineForMissingEmpiricalFormula = true, bool cysteinesAreModified = false)
        {
            var isMissingMonoMass = target.MonoMassTheor <= 0;

            if (string.IsNullOrEmpty(target.EmpiricalFormula))
            {
                if (!string.IsNullOrEmpty(target.Code))
                {
                    //Create empirical formula based on code. Assume it is an unmodified peptide
                    //target.EmpiricalFormula = _peptideUtils.GetEmpiricalFormulaForPeptideSequence(target.Code);
                    target.EmpiricalFormula = IqCodeParser.GetEmpiricalFormulaFromSequence(target.Code, cysteinesAreModified);
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
            }

            if (target.ChargeState != 0)
            {
                target.MZTheor = target.MonoMassTheor / target.ChargeState + DeconTools.Backend.Globals.PROTON_MASS;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// clones trees for root, parents and children and sets the correct node
        /// </summary>
        /// <param name="target">target we want to clone</param>
        /// <returns></returns>
        private static IqTarget CloneIqTrees(IqTarget target)
        {
            //initialize utilities
            var util = new IqTargetUtilities();

            //find root
            var rootNode = target.RootTarget;

            //this returnes the copied tree
            var tempTarget = util.Clone(rootNode);

            //select current node along the tree.  we need to mangle the charge and the ID to ensure uniqueness
            var selectID = target.ID;
            var selectChargeState = target.ChargeState;

            var targetList = new List<IqTarget>();
            targetList.Add(tempTarget);

            var nodeLevelTargets = util.GetTargetsFromNodelLevel(targetList, target.NodeLevel);

            var s = (from n in nodeLevelTargets where n.ID == selectID && n.ChargeState == selectChargeState select n).Take(1).ToList();

            return s[0];
        }

        public void CopyTargetProperties(IqTarget sourceTarget, IqTarget targetForUpdate, bool updateWorkflow = true)
        {
            targetForUpdate.ID = sourceTarget.ID;
            targetForUpdate.EmpiricalFormula = sourceTarget.EmpiricalFormula;
            targetForUpdate.Code = sourceTarget.Code;
            targetForUpdate.MonoMassTheor = sourceTarget.MonoMassTheor;
            targetForUpdate.ChargeState = sourceTarget.ChargeState;
            targetForUpdate.MZTheor = sourceTarget.MZTheor;
            targetForUpdate.ElutionTimeTheor = sourceTarget.ElutionTimeTheor;
            targetForUpdate.ScanLC = sourceTarget.ScanLC;
            targetForUpdate.QualityScore = sourceTarget.QualityScore;

            targetForUpdate.TheorIsotopicProfile = sourceTarget.TheorIsotopicProfile?.CloneIsotopicProfile();

            if (updateWorkflow)
            {
                targetForUpdate.Workflow = sourceTarget.Workflow;
            }
        }

        #endregion

    }
}
