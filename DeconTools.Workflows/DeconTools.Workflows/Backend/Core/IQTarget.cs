using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class IqTarget
    {
        private IqWorkflow _workflow;

        private List<IqTarget> _childTargets;

        public IqTarget()
        {
            _childTargets = new List<IqTarget>();
        }


        public IqTarget(IqWorkflow workflow)
            : this()
        {
            _workflow = workflow;
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

        #endregion

        public IEnumerable<IqTarget> ChildTargets()
        {
            return _childTargets;
        }


        public void SetWorkflow(IqWorkflow workflow)
        {
            _workflow = workflow;
        }



        public IqResult DoWorkflow()
        {
            return _workflow.Execute(this);
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
            if (_workflow != null)
            {
                return _workflow.Run;
            }

            return null;
        }



        public override string ToString()
        {
            return (ID + "; " + Code + "; " + EmpiricalFormula + "; " + MonoMassTheor.ToString("0.0000"));
        }

    }
}
