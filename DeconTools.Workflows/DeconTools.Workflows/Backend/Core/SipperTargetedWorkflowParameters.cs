

namespace DeconTools.Workflows.Backend.Core
{
    public class SipperTargetedWorkflowParameters : TargetedWorkflowParameters
    {

        public SipperTargetedWorkflowParameters()
        {
            ChromPeakSelectorMode = DeconTools.Backend.Globals.PeakSelectorMode.ClosestToTarget;
            
            ResultType = DeconTools.Backend.Globals.ResultType.SIPPER_TARGETED_RESULT;
        }


        public override Globals.TargetedWorkflowTypes WorkflowType
        {
            get { return Globals.TargetedWorkflowTypes.SipperTargeted1; }
        }
    }
}
