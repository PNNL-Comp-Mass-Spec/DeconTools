
namespace DeconTools.Workflows.Backend.Core
{
    /// <summary>
    /// Concrete parameter class for the basicTargetedWorkflowExecutor
    /// </summary>
    public class BasicTargetedWorkflowExecutorParameters : WorkflowExecutorBaseParameters
    {


        public override Globals.TargetedWorkflowTypes WorkflowType => Globals.TargetedWorkflowTypes.BasicTargetedWorkflowExecutor1;
    }
}
