namespace DeconTools.Workflows.Backend.Core
{
	public class UIMFTargetedMSMSWorkflowCollapseIMSParameters : TargetedWorkflowParameters
	{
		public UIMFTargetedMSMSWorkflowCollapseIMSParameters()
		{

			this.ResultType = DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;
		}

		public override Globals.TargetedWorkflowTypes WorkflowType
		{
			get
			{
				return Globals.TargetedWorkflowTypes.UIMFTargetedMSMSWorkflowCollapseIMS;
			}
		}
	}
}
