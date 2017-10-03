using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18WorkflowParameters : TargetedWorkflowParameters
    {

        #region Constructors
        public O16O18WorkflowParameters()
        {
            ChromGeneratorMode = DeconTools.Backend.Globals.ChromatogramGeneratorMode.O16O18_THREE_MONOPEAKS;

            ResultType = DeconTools.Backend.Globals.ResultType.O16O18_TARGETED_RESULT;

        }
        #endregion


        public override Globals.TargetedWorkflowTypes WorkflowType => Globals.TargetedWorkflowTypes.O16O18Targeted1;
    }
}
