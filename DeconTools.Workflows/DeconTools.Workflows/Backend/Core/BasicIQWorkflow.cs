using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicIqWorkflow:IqWorkflow
    {

        #region Constructors
        public BasicIqWorkflow(Run run, TargetedWorkflowParameters parameters)
            : base(run, parameters)
        {
        }

        public BasicIqWorkflow(TargetedWorkflowParameters parameters)
            : base(parameters)
        {
        }
        #endregion


      

        public override TargetedWorkflowParameters WorkflowParameters { get; set; }
    }
}
