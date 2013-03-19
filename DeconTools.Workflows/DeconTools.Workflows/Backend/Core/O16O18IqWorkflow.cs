using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18IqWorkflow:IqWorkflow
    {

        #region Constructors
        public O16O18IqWorkflow(Run run, TargetedWorkflowParameters parameters)
            : base(run, parameters)
        {
        }

        public O16O18IqWorkflow(TargetedWorkflowParameters parameters)
            : base(parameters)
        {
        }
        #endregion

        public override TargetedWorkflowParameters WorkflowParameters { get; set; }
        public override ResultExporter CreateExporter()
        {
            throw new System.NotImplementedException();
        }
    }
}
