using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicIqWorkflow:IqWorkflow
    {

        #region Constructors
        public BasicIqWorkflow(Run run, TargetedWorkflowParameters parameters)
            : base(run, parameters)
        {
        }

        public BasicIqWorkflow(TargetedWorkflowParameters parameters)
            : base(parameters)
        {
        }
        #endregion


      

        public override TargetedWorkflowParameters WorkflowParameters { get; set; }
        public override IqResultExporter CreateExporter()
        {
            return new IqLabelFreeResultExporter();
        }
    }
}
