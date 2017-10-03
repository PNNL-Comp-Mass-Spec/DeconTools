using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class IqTarget
    {
        protected List<IqTarget> _childTargets;

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

        /// <summary>
        /// Copies the target parameter to a new IqTarget.
        /// </summary>
        /// <param name="target">target parameter</param>
        public IqTarget(IqTarget target)
            : this()
        {
            var util = new IqTargetUtilities();
            util.CopyTargetProperties(target, this);
        }


        #region Properties

        public int ID { get; set; }

        public string EmpiricalFormula { get; set; }

        public string DatabaseReference { get; set; }

        public string Code { get; set; }

        public double MonoMassTheor { get; set; }

        public int ChargeState { get; set; }

        public double MZTheor { get; set; }

        public IsotopicProfile TheorIsotopicProfile { get; set; }

        public double ElutionTimeTheor { get; set; }


        public int ScanLC { get; set; }


        public IqTarget ParentTarget { get; set; }

        public IqWorkflow Workflow { get; set; }

        public int NodeLevel
        {
            get
            {
                if (ParentTarget == null) return 0;

                return 1 + ParentTarget.NodeLevel;
            }
        }

        public IqTarget RootTarget
        {
            get
            {
                if (ParentTarget == null) return this;

                return ParentTarget.RootTarget;
            }
            //TODO: remove this set.  RootTarget should not have a Set. 
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
            Workflow.Execute(iqResult);
        }

        public IqResult CreateResult()
        {
            return CreateResult(this);
        }

        public IqResult CreateResult(IqTarget target)
        {
            var result = Workflow.CreateIQResult(target);

            if (target.HasParent)
            {
                if (ParentTarget.GetResult() == null)
                {
                    ParentTarget._result=  ParentTarget.CreateResult();
                }
                result.ParentResult = ParentTarget._result;
                ParentTarget._result.AddResult(result);
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

        public bool HasParent => ParentTarget != null;

        /// <summary>
        /// Indicates the quality of the target. E.g. the MSGF probability score for the target
        /// </summary>
        public double QualityScore { get; set; }




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

        public int GetChildCount()
        {
            return _childTargets.Count;
        }

        public override string ToString()
        {
            return (ID + "; " + Code + "; " + EmpiricalFormula + "; " + MonoMassTheor.ToString("0.0000"));
        }


        /// <summary>
        /// RefineChildTargets is meant to implement a method to properly add or remove child targets from a parent based on some type of parameters.
        /// EG: Top down uses RefineIqTarget to remove redundant charge state targets and add missing charge state targets as well.
        /// </summary>
        public virtual void RefineChildTargets()
        {
            throw new NotImplementedException("RefineChildTargets Method not implemented!");
        }
    }
}


