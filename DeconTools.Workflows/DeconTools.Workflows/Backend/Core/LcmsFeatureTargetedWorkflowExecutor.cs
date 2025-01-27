﻿using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public class LcmsFeatureTargetedWorkflowExecutor : TargetedWorkflowExecutor
    {
        #region Constructors
        public LcmsFeatureTargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath) : base(parameters, datasetPath)
        {
        }
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public new void InitializeWorkflow()
        {
            var executorParams = (LcmsFeatureTargetedWorkflowExecutorParameters) ExecutorParameters;

            //_loggingFileName = GetLogFileName(ExecutorParameters.LoggingDirectory);
            _resultsDirectory = GetResultsDirectory(ExecutorParameters.OutputDirectoryBase);

            Targets = GetLcmsFeatureTargets(ExecutorParameters.TargetsFilePath);

            MassTagsForReference = GetMassTagTargets(executorParams.MassTagsForReference);

            UpdateTargetsWithMassTagInfo(Targets, MassTagsForReference);

            Check.Ensure(Targets?.TargetList.Count > 0,
                         "Target massTags is empty. Check the path to the massTag data file.");

            _workflowParameters = WorkflowParameters.CreateParameters(ExecutorParameters.WorkflowParameterFile);
            _workflowParameters.LoadParameters(ExecutorParameters.WorkflowParameterFile);

            //TargetedAlignmentWorkflowParameters = new TargetedAlignerWorkflowParameters();
            //TargetedAlignmentWorkflowParameters.LoadParameters(ExecutorParameters.TargetedAlignmentWorkflowParameterFile);

            TargetedWorkflow = TargetedWorkflow.CreateWorkflow(_workflowParameters);
        }

        private void UpdateTargetsWithMassTagInfo(TargetCollection targets, TargetCollection massTagsForReference)
        {
            var massTagIDList = (from n in massTagsForReference.TargetList select n.ID).ToList();

            foreach (LcmsFeatureTarget target in targets.TargetList)
            {
                if (massTagIDList.Contains(target.FeatureToMassTagID))
                {
                    var mt = massTagsForReference.TargetList.First(p => p.ID == target.FeatureToMassTagID);
                    target.Code = mt.Code;
                    target.EmpiricalFormula = mt.EmpiricalFormula;
                }
                else
                {
                    //TODO: use averagine
                }
            }
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
