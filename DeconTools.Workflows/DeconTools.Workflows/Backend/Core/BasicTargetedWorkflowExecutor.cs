using System.ComponentModel;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflowExecutor : TargetedWorkflowExecutor
    {

        #region Constructors
        public BasicTargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath, BackgroundWorker backgroundWorker = null)
            : base(parameters, datasetPath, backgroundWorker) { }

        public BasicTargetedWorkflowExecutor(WorkflowExecutorBaseParameters workflowExecutorParameters, WorkflowParameters workflowParameters, string datasetPath) : base(workflowExecutorParameters, workflowParameters, datasetPath) { }

        public BasicTargetedWorkflowExecutor(WorkflowExecutorBaseParameters workflowExecutorParameters,
            TargetedWorkflow workflow, string datasetPath, BackgroundWorker backgroundWorker =null) : base(workflowExecutorParameters, workflow, datasetPath,backgroundWorker) { }

        #endregion


        #region Public Methods

        #endregion

    }
}
