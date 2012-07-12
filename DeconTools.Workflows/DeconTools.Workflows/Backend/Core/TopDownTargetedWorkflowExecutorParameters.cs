namespace DeconTools.Workflows.Backend.Core
{
	/// <summary>
	/// Concrete parameter class for the TopDownTargetedWorkflowExecutor
	/// </summary>
	public class TopDownTargetedWorkflowExecutorParameters : WorkflowExecutorBaseParameters
	{
		public override Globals.TargetedWorkflowTypes WorkflowType
		{
			get { return Globals.TargetedWorkflowTypes.TopDownTargetedWorkflowExecutor1; }
		}
	}
}
