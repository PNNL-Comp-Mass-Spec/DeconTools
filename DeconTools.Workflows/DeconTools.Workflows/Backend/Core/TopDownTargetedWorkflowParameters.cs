namespace DeconTools.Workflows.Backend.Core
{
    public class TopDownTargetedWorkflowParameters : TargetedWorkflowParameters
    {
        public TopDownTargetedWorkflowParameters()
        {
            ResultType = DeconTools.Backend.Globals.ResultType.TOPDOWN_TARGETED_RESULT;
        }

        public bool SaveChromatogramData { get; set; }


        public override Globals.TargetedWorkflowTypes WorkflowType
        {
            get
            {
                return Globals.TargetedWorkflowTypes.TopDownTargeted1;
            }
        }
    }
}
