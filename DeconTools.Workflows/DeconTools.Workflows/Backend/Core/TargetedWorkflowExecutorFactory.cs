
namespace DeconTools.Workflows.Backend.Core
{
    public class TargetedWorkflowExecutorFactory
    {
        public static TargetedWorkflowExecutor CreateTargetedWorkflowExecutor(WorkflowExecutorBaseParameters workflowParameters, string datasetPath)
        {
            //TODO: add ResultReprocessingWorkflow

            switch (workflowParameters.WorkflowType)
            {
                case Globals.TargetedWorkflowTypes.BasicTargetedWorkflowExecutor1:
                    return new BasicTargetedWorkflowExecutor(workflowParameters, datasetPath);
                case Globals.TargetedWorkflowTypes.LcmsFeatureTargetedWorkflowExecutor1:
                    return new LcmsFeatureTargetedWorkflowExecutor(workflowParameters, datasetPath);
                case Globals.TargetedWorkflowTypes.SipperWorkflowExecutor1:
                    return new SipperWorkflowExecutor(workflowParameters, datasetPath);
                case Globals.TargetedWorkflowTypes.TopDownTargetedWorkflowExecutor1:
                    return new TopDownTargetedWorkflowExecutor(workflowParameters, datasetPath);
                default:
                    throw new System.ArgumentException("Workflow type: " + workflowParameters.WorkflowType +
                                                       " is not an executor type of workflow");
            }
        }
    }
}
