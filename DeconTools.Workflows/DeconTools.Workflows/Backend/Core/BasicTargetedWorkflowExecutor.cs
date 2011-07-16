using System;
using System.IO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using DeconTools.Workflows.Backend.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflowExecutor : TargetedWorkflowExecutor
    {

        #region Constructors
        public BasicTargetedWorkflowExecutor(BasicTargetedWorkflowExecutorParameters parameters) : base(parameters) { }

        #endregion

        #region Properties
        
        /// <summary>
        /// WorkflowExecutor paramters. Careful not to confuse it with the other workflow parameters used in processing in each massTag
        /// </summary>
        public override WorkflowParameters WorkflowParameters
        {
            get
            {
                return ExecutorParameters;
            }
            set
            {
                if (value is BasicTargetedWorkflowExecutorParameters)
                {
                    ExecutorParameters = value as BasicTargetedWorkflowExecutorParameters;
                }
            }
        }
        #endregion

        #region Public Methods
        public override void InitializeWorkflow()
        {
            _datasetPathList = getListDatasetPaths(ExecutorParameters.FileContainingDatasetPaths);
            _loggingFileName = getLogFileName(ExecutorParameters.LoggingFolder);
            _resultsFolder = getResultsFolder(ExecutorParameters.ResultsFolder);

            this.MassTagsForTargetedAlignment = getMassTagTargets(ExecutorParameters.MassTagsForAlignmentFilePath);
            this.MassTagsToBeTargeted = getMassTagTargets(ExecutorParameters.MassTagsToBeTargetedFilePath);
            this._workflowParameters = new BasicTargetedWorkflowParameters();
            this._workflowParameters.LoadParameters(ExecutorParameters.WorkflowParameterFile);

            this.TargetedAlignmentWorkflowParameters = new TargetedAlignerWorkflowParameters();
            this.TargetedAlignmentWorkflowParameters.LoadParameters(ExecutorParameters.TargetedAlignmentWorkflowParameterFile);

            this.targetedWorkflow = new BasicTargetedWorkflow(_workflowParameters);

        }

        #endregion


        protected override TargetedResultToTextExporter createExporter(string outputFileName)
        {
            TargetedResultToTextExporter exporter = new UnlabelledTargetedResultToTextExporter(outputFileName);
            return exporter;
        }


 



        
    }
}
