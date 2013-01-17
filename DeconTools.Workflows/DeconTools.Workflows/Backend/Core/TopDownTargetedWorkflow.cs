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

		
		public TopDownTargetedWorkflow(Run run, TargetedWorkflowParameters parameters) : base(run,parameters)
		{
		    
        }

		public TopDownTargetedWorkflow(TargetedWorkflowParameters parameters) : base(parameters)
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
		

		
        protected override DeconTools.Backend.Globals.ResultType GetResultType()
        {
            return DeconTools.Backend.Globals.ResultType.TOPDOWN_TARGETED_RESULT;
        }

        protected override void ExecutePostWorkflowHook()
        {
            base.ExecutePostWorkflowHook();
            if (((TopDownTargetedWorkflowParameters)_workflowParameters).SaveChromatogramData)
            {
                Result.ChromValues = new XYData { Xvalues = ChromatogramXYData.Xvalues, Yvalues = ChromatogramXYData.Yvalues };
            }
        }
	}
}
