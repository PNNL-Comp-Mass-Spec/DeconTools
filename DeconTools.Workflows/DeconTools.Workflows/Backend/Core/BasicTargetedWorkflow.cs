using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflow : TargetedWorkflow
    {


        #region Constructors

        public BasicTargetedWorkflow(Run run, TargetedWorkflowParameters parameters)
            : base(run, parameters)
        {
        }

        public BasicTargetedWorkflow(TargetedWorkflowParameters parameters):base (parameters)
        {
        }


        #endregion


        protected override DeconTools.Backend.Globals.ResultType GetResultType()
        {
            return DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;
        }


        protected override void ExecutePostWorkflowHook()
        {
            base.ExecutePostWorkflowHook();

            if (Result != null && Result.Target != null && Result.IsotopicProfile!=null && Success)
            {
               if (Run.IsMsAbundanceReportedAsAverage)
               {
                   Result.IntensityAggregate = Result.IntensityAggregate * Result.NumMSScansSummed;
               }

            }

        }

    }
}
