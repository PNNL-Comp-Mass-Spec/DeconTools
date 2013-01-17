using System;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

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

    }
}
