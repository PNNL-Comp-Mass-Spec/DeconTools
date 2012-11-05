using System.ComponentModel;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public class UIMFWorkflowExecutor1:TargetedWorkflowExecutor
    {

        #region Constructors

        public UIMFWorkflowExecutor1(WorkflowExecutorBaseParameters parameters, string datasetPath, BackgroundWorker backgroundWorker)
            : base(parameters, datasetPath, backgroundWorker)
        {
        }

        public UIMFWorkflowExecutor1(WorkflowExecutorBaseParameters parameters, Run run, BackgroundWorker backgroundWorker)
            : base(parameters, run, backgroundWorker)
        {
        }
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
