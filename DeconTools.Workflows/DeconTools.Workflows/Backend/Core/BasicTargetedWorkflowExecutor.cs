using System.ComponentModel;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflowExecutor : TargetedWorkflowExecutor
    {

        #region Constructors
        public BasicTargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath, BackgroundWorker backgroundWorker = null) 
            : base(parameters, datasetPath, backgroundWorker) { }
		public BasicTargetedWorkflowExecutor(WorkflowExecutorBaseParameters workflowExecutorParameters, WorkflowParameters workflowParameters, string datasetPath) : base(workflowExecutorParameters, workflowParameters, datasetPath) { }

        #endregion


        #region Public Methods

        #endregion

    }
}
