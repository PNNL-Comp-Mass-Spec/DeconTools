
namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflowParameters:TargetedWorkflowParameters
    {

      
        public BasicTargetedWorkflowParameters()
        {
           
            this.ResultType = DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;
        }

        public override Globals.TargetedWorkflowTypes WorkflowType
        {
            get
            {
                return  Globals.TargetedWorkflowTypes.UnlabelledTargeted1;
            }
        }

    }
}
