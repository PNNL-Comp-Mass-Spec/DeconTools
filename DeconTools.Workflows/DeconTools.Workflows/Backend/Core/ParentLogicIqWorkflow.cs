using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    /// <summary>
    /// Gathers metrics on grandchildren and decides which peak to quantify
    /// </summary>
    public class ParentLogicIqWorkflow : BasicIqWorkflow
    {
        #region Constructors

        public ParentLogicIqWorkflow(Run run, TargetedWorkflowParameters parameters) : base(run, parameters)
        {
        }

        public ParentLogicIqWorkflow(TargetedWorkflowParameters parameters) : base(parameters)
        {
        }

        #endregion

        #region Properties

        #endregion

        protected override void ExecuteWorkflow(IqResult result)
        {
            //Executes the ChargeState level children workflows
            var children = result.Target.ChildTargets();
            foreach (var child in children)
            {
                child.DoWorkflow();
            }
        }
    }
}
