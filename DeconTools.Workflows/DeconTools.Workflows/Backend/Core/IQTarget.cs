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
        }

     
        

        public IqTarget RootTarget
        {
            get
            {
                if (ParentTarget == null) return this;

                return ParentTarget.RootTarget;
            }
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
