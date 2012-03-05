
namespace DeconTools.Workflows.Backend.Core
{
    public class LcmsFeatureTargetedWorkflowExecutorParameters : WorkflowExecutorBaseParameters
    {

        public string MassTagsForReference { get; set; }
       
        public override Globals.TargetedWorkflowTypes WorkflowType
        {
            get { return Globals.TargetedWorkflowTypes.LcmsFeatureTargetedWorkflowExecutor1; }
        }
    }
}
