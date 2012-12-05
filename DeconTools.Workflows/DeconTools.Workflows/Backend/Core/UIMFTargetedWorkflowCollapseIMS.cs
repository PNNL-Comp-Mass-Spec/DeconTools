using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
	public class UIMFTargetedWorkflowCollapseIMS : TargetedWorkflow
	{
		public UIMFTargetedWorkflowCollapseIMS(Run run, TargetedWorkflowParameters parameters)
        {
            this.WorkflowParameters = parameters;
            this.Run = run;

            InitializeWorkflow();
        }

		public UIMFTargetedWorkflowCollapseIMS(TargetedWorkflowParameters parameters)
			: this(null, parameters)
		{

		}

		public override void Execute()
		{
			Check.Require(this.Run != null, "Run has not been defined.");

			this.Run.ResultCollection.ResultType = DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;
		}
	}
}
