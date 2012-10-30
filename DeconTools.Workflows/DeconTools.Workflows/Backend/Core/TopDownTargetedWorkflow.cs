using System;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
	public class TopDownTargetedWorkflow : TargetedWorkflow
	{
		public Dictionary<int, TargetedResultBase> TargetResults { get; set; }

		
		public TopDownTargetedWorkflow(Run run, TargetedWorkflowParameters parameters)
		{
			Run = run;
            _workflowParameters = parameters;

            InitializeWorkflow();
        }

		public TopDownTargetedWorkflow(TargetedWorkflowParameters parameters) : this(null, parameters)
		{

		}

        protected override void DoPostInitialization()
        {
            base.DoPostInitialization();
            _iterativeTFFParameters = new IterativeTFFParameters
            {
                ToleranceInPPM = _workflowParameters.MSToleranceInPPM
            };

            _iterativeTFFParameters.MinimumRelIntensityForForPeakInclusion = 0.4;   //TODO: add comment why we need this
            _msfeatureFinder = new IterativeTFF(_iterativeTFFParameters);
        }
		

		public override void Execute()
		{
			Check.Require(Run != null, "Run has not been defined.");
			
			Run.ResultCollection.ResultType = DeconTools.Backend.Globals.ResultType.TOPDOWN_TARGETED_RESULT;

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

                if (((TopDownTargetedWorkflowParameters)_workflowParameters).SaveChromatogramData)
                {
                    Result.ChromValues = new XYData {Xvalues = ChromatogramXYData.Xvalues, Yvalues = ChromatogramXYData.Yvalues};    
                }
				
				//TargetResults.Add(Run.CurrentMassTag.ID, Result);
			}
			catch (Exception ex)
			{
				var result = Run.ResultCollection.GetTargetedResult(Run.CurrentMassTag);
				result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;
				result.FailedResult = true;
			}
		}
	}
}
