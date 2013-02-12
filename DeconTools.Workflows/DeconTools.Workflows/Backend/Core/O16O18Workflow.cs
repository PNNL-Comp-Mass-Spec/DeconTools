using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.Quantifiers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;

namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18Workflow : TargetedWorkflow
    {
        private O16O18QuantifierTask _quant;

        #region Constructors

        public O16O18Workflow(Run run, TargetedWorkflowParameters parameters) : base(run,parameters)
        {
           
        }

        public O16O18Workflow(TargetedWorkflowParameters parameters):base (parameters)
        {
        }

        #endregion

   
        #region IWorkflow Members

       
        protected override void ExecutePostWorkflowHook()
        {
            base.ExecutePostWorkflowHook();
            ExecuteTask(_quant);
        }


        protected override void DoPostInitialization()
        {
            base.DoPostInitialization();

            _chromatogramCorrelator = new ChromatogramCorrelatorO16O18(_workflowParameters.ChromSmootherNumPointsInSmooth,
                                                                       _workflowParameters.ChromGenTolerance);


            _msfeatureFinder = new O16O18TargetedIterativeFeatureFinder(_iterativeTFFParameters);
            _quant = new O16O18QuantifierTask();

        }



        protected override DeconTools.Backend.Globals.ResultType GetResultType()
        {
            return DeconTools.Backend.Globals.ResultType.O16O18_TARGETED_RESULT;
        }

        #endregion

        
  
    }
}
