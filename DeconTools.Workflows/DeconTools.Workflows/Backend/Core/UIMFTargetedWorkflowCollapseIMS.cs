using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public class UIMFTargetedWorkflowCollapseIMS : TargetedWorkflow
    {
        public UIMFTargetedWorkflowCollapseIMS(Run run, TargetedWorkflowParameters parameters) : base(run, parameters)
        {
            
        }

        public UIMFTargetedWorkflowCollapseIMS(TargetedWorkflowParameters parameters)
            : this(null, parameters)
        {

        }

        protected override DeconTools.Backend.Globals.ResultType GetResultType()
        {
            return  DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;
        }

    }
}
