using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.Quantifiers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;

namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18Workflow : TargetedWorkflow
    {

        private O16O18TargetedIterativeFeatureFinder _o16o18FeatureFinder;
        private O16O18QuantifierTask _quant;

        #region Constructors

        public O16O18Workflow(Run run, TargetedWorkflowParameters parameters)
        {
            this.WorkflowParameters = parameters;

            this.Run = run;
            InitializeWorkflow();
        }

        public O16O18Workflow(TargetedWorkflowParameters parameters)
            : this(null, parameters)
        {

        }

        #endregion

        #region Properties

        #endregion

        #region IWorkflow Members

        public override void Execute()
        {

            ResetStoredData();

            this.Run.ResultCollection.ResultType = DeconTools.Backend.Globals.ResultType.O16O18_TARGETED_RESULT;



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

                ExecuteTask(_o16o18FeatureFinder);
                ExecuteTask(_fitScoreCalc);

                ExecuteTask(_quant);


            }
            catch (Exception ex)
            {
                TargetedResultBase result = this.Run.ResultCollection.GetTargetedResult(this.Run.CurrentMassTag);
                result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;

                return;
            }

        }


        protected override void DoPostInitialization()
        {
            base.DoPostInitialization();

            _o16o18FeatureFinder = new O16O18TargetedIterativeFeatureFinder(_iterativeTFFParameters);
            _quant = new O16O18QuantifierTask();

        }


       
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

        
  
    }
}
