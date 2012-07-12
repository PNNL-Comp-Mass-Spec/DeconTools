namespace DeconTools.Workflows.Backend.Core
{
	public class TopDownTargetedWorkflowParameters : TargetedWorkflowParameters
	{
		public TopDownTargetedWorkflowParameters()
		{
			ResultType = DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;
		}

		public override Globals.TargetedWorkflowTypes WorkflowType
		{
			get
			{
				return Globals.TargetedWorkflowTypes.TopDownTargetedWorkflowExecutor1;
			}
		}
	}
}
