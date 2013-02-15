using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Workflows.Backend.Core
{
    public class IQMillionWorkflowParameters : TargetedWorkflowParameters
    {
        public IQMillionWorkflowParameters()
		{
			this.ResultType = DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;
		}

		public override Globals.TargetedWorkflowTypes WorkflowType
		{
			get
			{
				return Globals.TargetedWorkflowTypes.IQMillionWorkflow;
			}
		}
    }
}
