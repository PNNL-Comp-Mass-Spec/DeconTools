using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflowExecutor : TargetedWorkflowExecutor
    {

        #region Constructors
        public BasicTargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath) : base(parameters,datasetPath) { }

        #endregion

        #region Properties
        
      
        //public override WorkflowParameters WorkflowParameters
        //{
        //    get
        //    {
        //        return ExecutorParameters;
        //    }
        //    set
        //    {
        //        if (value is BasicTargetedWorkflowExecutorParameters)
        //        {
        //            ExecutorParameters = value as BasicTargetedWorkflowExecutorParameters;
        //        }
        //    }
        //}
        #endregion

        #region Public Methods
        public override void InitializeWorkflow()
        {
            //_loggingFileName = getLogFileName(ExecutorParameters.LoggingFolder);
            _resultsFolder = getResultsFolder(ExecutorParameters.ResultsFolder);

            MassTagsForTargetedAlignment = getMassTagTargets(ExecutorParameters.MassTagsForAlignmentFilePath);
            Targets = getMassTagTargets(ExecutorParameters.TargetsFilePath);

            Check.Ensure(Targets != null && Targets.TargetList.Count > 0,
                         "Target massTags is empty. Check the path to the massTag data file.");


            _workflowParameters = WorkflowParameters.CreateParameters(ExecutorParameters.WorkflowParameterFile);
            _workflowParameters.LoadParameters(ExecutorParameters.WorkflowParameterFile);

            TargetedAlignmentWorkflowParameters = new TargetedAlignerWorkflowParameters();
            TargetedAlignmentWorkflowParameters.LoadParameters(ExecutorParameters.TargetedAlignmentWorkflowParameterFile);

            targetedWorkflow = TargetedWorkflow.CreateWorkflow(_workflowParameters);
           
            

        }

        #endregion
        
    }
}
