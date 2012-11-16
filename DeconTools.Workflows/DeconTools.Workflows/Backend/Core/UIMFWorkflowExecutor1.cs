using System.ComponentModel;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public class UIMFWorkflowExecutor1:TargetedWorkflowExecutor
    {

        #region Constructors

        public UIMFWorkflowExecutor1(WorkflowExecutorBaseParameters parameters, string datasetPath, BackgroundWorker backgroundWorker=null)
            : base(parameters, datasetPath, backgroundWorker)
        {
        }

        public UIMFWorkflowExecutor1(WorkflowExecutorBaseParameters parameters, Run run, BackgroundWorker backgroundWorker=null)
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
