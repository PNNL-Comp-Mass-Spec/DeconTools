using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class IqTarget
    {  
        private List<IqTarget> _childTargets;

        IqResult _result;

        public IqTarget()
        {
            _childTargets = new List<IqTarget>();
        }

        public IqTarget(IqWorkflow workflow)
            : this()
        {
            Workflow = workflow;
        }

        public IqTarget(IqTarget copiedTarget)
        {
            ID = copiedTarget.ID;
            EmpiricalFormula = copiedTarget.EmpiricalFormula;
            Code = copiedTarget.Code;
            MonoMassTheor = copiedTarget.MonoMassTheor;
            ChargeState = copiedTarget.ChargeState;
            MZTheor = copiedTarget.MZTheor;

            TheorIsotopicProfile = copiedTarget.TheorIsotopicProfile == null
                                       ? null
                                       : copiedTarget.TheorIsotopicProfile.CloneIsotopicProfile();

            ElutionTimeTheor = copiedTarget.ElutionTimeTheor;

            if (ParentTarget != null)
            {
                ParentTarget = Clone(copiedTarget.ParentTarget);
            }

            if (copiedTarget.ParentTarget != null)
            {
                NodeLevel = copiedTarget.NodeLevel;
            }

            if (copiedTarget.ParentTarget != null && ParentTarget!=null)
            {
                ParentTarget.RootTarget = copiedTarget.ParentTarget.RootTarget;
            }

            if (copiedTarget._childTargets != null && copiedTarget._childTargets.Count > 0)
            {
                foreach (IqTarget target in copiedTarget._childTargets)
                {
                    _childTargets.Add(Clone(target));
                }
            }
        }

        private static IqTarget Clone(IqTarget copiedTarget) 
        {
            IqTarget tempTarget =  new IqTargetBasic();

            tempTarget.ID = copiedTarget.ID;
            tempTarget.EmpiricalFormula = copiedTarget.EmpiricalFormula;
            tempTarget.Code = copiedTarget.Code;
            tempTarget.MonoMassTheor = copiedTarget.MonoMassTheor;
            tempTarget.ChargeState = copiedTarget.ChargeState;
            tempTarget.MZTheor = copiedTarget.MZTheor;

            tempTarget.TheorIsotopicProfile = copiedTarget.TheorIsotopicProfile == null
                                       ? null
                                       : copiedTarget.TheorIsotopicProfile.CloneIsotopicProfile();

            tempTarget.ElutionTimeTheor = copiedTarget.ElutionTimeTheor;

            if (tempTarget.ParentTarget != null)
            {
                tempTarget.ParentTarget = Clone(copiedTarget.ParentTarget);
            }

            if (copiedTarget.ParentTarget != null)
            {
                tempTarget.NodeLevel = copiedTarget.NodeLevel;
            }

            if (copiedTarget.ParentTarget != null && tempTarget.ParentTarget != null)
            {
                tempTarget.ParentTarget.RootTarget = copiedTarget.ParentTarget.RootTarget;
            }
            return tempTarget;
        }

        #region Properties

        public int ID { get; set; }
        
        public string EmpiricalFormula { get; set; }
        
        public string Code { get; set; }
        
        public double MonoMassTheor { get; set; }
        
        public int ChargeState { get; set; }
        
        public double MZTheor { get; set; }

        public IsotopicProfile TheorIsotopicProfile { get; set; }
        
        public double ElutionTimeTheor { get; set; }

        public IqTarget ParentTarget { get; set; }

        public IqWorkflow Workflow { get; set; }

        public int NodeLevel
        {
            get
            {
                if (ParentTarget == null) return 0;

                return 1 + ParentTarget.NodeLevel;
            }
            set { }
        }

        public IqTarget RootTarget
        {
            get
            {
                if (ParentTarget == null) return this;

                return ParentTarget.RootTarget;
            }
            set { }
        }


        #endregion

        public IEnumerable<IqTarget> ChildTargets()
        {
            return _childTargets;
        }

        public void SetWorkflow(IqWorkflow workflow)
        {
            Workflow = workflow;
        }

        public void DoWorkflow()
        {
            _result = CreateResult(this);
            DoWorkflow(_result);
        }

        protected void DoWorkflow(IqResult iqResult)
        {
            if (iqResult.HasChildren())
            {
                var childresults = iqResult.ChildResults();
                foreach (var childResult in childresults)
                {
                    childResult.Target.DoWorkflow(childResult);
                }
            }
            Workflow.Execute(iqResult);
        }

        protected IqResult CreateResult()
        {
            return CreateResult(this);
        }

        protected IqResult CreateResult(IqTarget target)
        {
            var result = Workflow.CreateIQResult(target);

            if (target.HasChildren())
            {
                var childTargets = ChildTargets();
                foreach (var childTarget in childTargets)
                {
                    var childResult = CreateResult(childTarget);
                    result.AddResult(childResult);
                }
            }

            return result;


        }

        public IqResult GetResult()
        {
            return _result;
        }

        public bool HasChildren()
        {
            return _childTargets.Any();
        }

        public bool HasParent
        {
            get
            {
                return ParentTarget != null;
            }
        }

        public void AddTarget(IqTarget target)
        {
            target.ParentTarget = this;
            _childTargets.Add(target);
        }

        public void AddTargetRange(IEnumerable<IqTarget> targetsToBeAdded)
        {
            foreach (var iqTarget in targetsToBeAdded)
            {
                AddTarget(iqTarget);
            }
        }

        public void RemoveTarget(IqTarget target)
        {
            _childTargets.Remove(target);
        }

        public Run GetRun()
        {
            if (Workflow != null)
            {
                return Workflow.Run;
            }

            return null;
        }

        public override string ToString()
        {
            return (ID + "; " + Code + "; " + EmpiricalFormula + "; " + MonoMassTheor.ToString("0.0000"));
        }
    }
}
