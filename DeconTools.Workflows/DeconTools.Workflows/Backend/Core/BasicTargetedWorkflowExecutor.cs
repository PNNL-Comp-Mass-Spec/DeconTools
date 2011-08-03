using DeconTools.Utilities;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflowExecutor : TargetedWorkflowExecutor
    {

        #region Constructors
        public BasicTargetedWorkflowExecutor(BasicTargetedWorkflowExecutorParameters parameters, string datasetPath) : base(parameters,datasetPath) { }

        #endregion

        #region Properties
        
        /// <summary>
        /// WorkflowExecutor parameters. Careful not to confuse it with the other workflow parameters used in processing in each massTag
        /// </summary>
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

            this.MassTagsForTargetedAlignment = getMassTagTargets(ExecutorParameters.MassTagsForAlignmentFilePath);
            this.MassTagsToBeTargeted = getMassTagTargets(ExecutorParameters.MassTagsToBeTargetedFilePath);

            Check.Ensure(this.MassTagsToBeTargeted != null && this.MassTagsToBeTargeted.MassTagList.Count > 0, "Target massTags is empty. Check the path to the massTag data file.");


            this._workflowParameters = WorkflowParameters.CreateParameters(ExecutorParameters.WorkflowParameterFile);
            this._workflowParameters.LoadParameters(ExecutorParameters.WorkflowParameterFile);

            this.TargetedAlignmentWorkflowParameters = new TargetedAlignerWorkflowParameters();
            this.TargetedAlignmentWorkflowParameters.LoadParameters(ExecutorParameters.TargetedAlignmentWorkflowParameterFile);

            this.targetedWorkflow = TargetedWorkflow.CreateWorkflow(this._workflowParameters);
           
            

        }

        #endregion


        protected override TargetedResultToTextExporter createExporter(string outputFileName)
        {
            TargetedResultToTextExporter exporter = new UnlabelledTargetedResultToTextExporter(outputFileName);
            return exporter;
        }


 



        
    }
}
