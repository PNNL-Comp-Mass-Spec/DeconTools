using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
	public class UIMFTargetedMSMSWorkflowCollapseIMS : TargetedWorkflow
	{
		public UIMFTargetedMSMSWorkflowCollapseIMS(Run run, TargetedWorkflowParameters parameters)
        {
            this.WorkflowParameters = parameters;
            this.Run = run;

            InitializeWorkflow();
        }

		public UIMFTargetedMSMSWorkflowCollapseIMS(TargetedWorkflowParameters parameters)
			: this(null, parameters)
		{

		}

		public override void Execute()
		{
			Check.Require(this.Run != null, "Run has not been defined.");

			this.Run.ResultCollection.ResultType = DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;

			ResetStoredData();

			try
            {
				Result = Run.ResultCollection.GetTargetedResult(Run.CurrentMassTag);
				Result.ResetResult();

				ExecuteTask(_theorFeatureGen);
				ExecuteTask(_chromGen);
				ExecuteTask(_chromSmoother);
				updateChromDataXYValues(Run.XYData);

				ExecuteTask(_chromPeakDetector);
				UpdateChromDetectedPeaks(Run.PeakList);

				ExecuteTask(_chromPeakSelector);
				ChromPeakSelected = Result.ChromPeakSelected;

				Result.ResetMassSpectrumRelatedInfo();

				ExecuteTask(MSGenerator);
				updateMassSpectrumXYValues(Run.XYData);

				ExecuteTask(_msfeatureFinder);

				ExecuteTask(_fitScoreCalc);
				ExecuteTask(_resultValidator);

				if (_workflowParameters.ChromatogramCorrelationIsPerformed)
				{
					ExecuteTask(_chromatogramCorrelatorTask);
				}
			}
			catch (Exception ex)
			{
				TargetedResultBase result = this.Run.ResultCollection.GetTargetedResult(this.Run.CurrentMassTag);
				result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;
				result.FailedResult = true;
				return;
			}
		}
	}
}
