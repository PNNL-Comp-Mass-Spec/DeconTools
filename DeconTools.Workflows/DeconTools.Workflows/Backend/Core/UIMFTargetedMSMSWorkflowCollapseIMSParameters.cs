namespace DeconTools.Workflows.Backend.Core
{
    public class UIMFTargetedMSMSWorkflowCollapseIMSParameters : TargetedWorkflowParameters
    {
        public UIMFTargetedMSMSWorkflowCollapseIMSParameters()
        {

            ResultType = DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;
        }

        public override Globals.TargetedWorkflowTypes WorkflowType => Globals.TargetedWorkflowTypes.UIMFTargetedMSMSWorkflowCollapseIMS;
    }
}
