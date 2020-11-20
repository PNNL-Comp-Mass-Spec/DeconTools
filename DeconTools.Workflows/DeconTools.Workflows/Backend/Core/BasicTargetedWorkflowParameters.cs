
namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflowParameters:TargetedWorkflowParameters
    {
        public BasicTargetedWorkflowParameters()
        {
            ResultType = DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;
        }

        public override Globals.TargetedWorkflowTypes WorkflowType => Globals.TargetedWorkflowTypes.UnlabeledTargeted1;
    }
}
