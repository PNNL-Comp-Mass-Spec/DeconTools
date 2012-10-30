using System;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflow : TargetedWorkflow
    {
       

        #region Constructors

        public BasicTargetedWorkflow(Run run, TargetedWorkflowParameters parameters)
        {
            this.WorkflowParameters = parameters;
            this.Run = run;

            InitializeWorkflow();
        }

        public BasicTargetedWorkflow(TargetedWorkflowParameters parameters)
            : this(null, parameters)
        {

        }

        #endregion

        #region Properties
        string _name;
        public string Name
        {
            get
            { return this.ToString(); }
            set
            {
                _name = value;
            }

        }
        #endregion

  
    
        public override void Execute()
        {
            Check.Require(this.Run != null, "Run has not been defined.");

            //TODO: remove this later:
            //this.Run.CreateDefaultScanToNETAlignmentData();

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



                //updateMassAndNETCalibrationValues

            }
            catch (Exception ex)
            {
                TargetedResultBase result = (TargetedResultBase)this.Run.ResultCollection.GetTargetedResult(this.Run.CurrentMassTag);
                result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;
                result.FailedResult = true;
                return;
            }
        }

     
    }
}
