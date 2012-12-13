using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
	public class UIMFTargetedWorkflowExecutor : TargetedWorkflowExecutor
	{
		public UIMFTargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath) : base(parameters, datasetPath) { }
		public UIMFTargetedWorkflowExecutor(WorkflowExecutorBaseParameters workflowExecutorParameters, WorkflowParameters workflowParameters, string datasetPath) : base(workflowExecutorParameters, workflowParameters, datasetPath) { }

		public override void InitializeWorkflow()
		{
			_resultsFolder = string.IsNullOrEmpty(ExecutorParameters.ResultsFolder) ? RunUtilities.GetDatasetParentFolder(DatasetPath) : getResultsFolder(ExecutorParameters.ResultsFolder);

			MassTagsForTargetedAlignment = GetMassTagTargets(ExecutorParameters.TargetsUsedForAlignmentFilePath);

			bool targetsFilePathIsEmpty = (String.IsNullOrEmpty(ExecutorParameters.TargetsFilePath));

			string currentTargetsFilePath = targetsFilePathIsEmpty ? TryFindTargetsForCurrentDataset() : ExecutorParameters.TargetsFilePath;

			Targets = CreateTargets(ExecutorParameters.TargetType, currentTargetsFilePath);

			if (ExecutorParameters.TargetType == Globals.TargetType.LcmsFeature)
			{
				UpdateTargetMissingInfo();
			}

			if (_workflowParameters == null)
			{
				_workflowParameters = WorkflowParameters.CreateParameters(ExecutorParameters.WorkflowParameterFile);
				_workflowParameters.LoadParameters(ExecutorParameters.WorkflowParameterFile);
			}

			if (ExecutorParameters.TargetedAlignmentIsPerformed)
			{
				if (string.IsNullOrEmpty(ExecutorParameters.TargetedAlignmentWorkflowParameterFile))
				{
					throw new FileNotFoundException(
						"Cannot initialize workflow. TargetedAlignment is requested but TargetedAlignmentWorkflowParameter file is not found. Check path for the 'TargetedAlignmentWorkflowParameterFile' ");
				}


				TargetedAlignmentWorkflowParameters = new TargetedAlignerWorkflowParameters();
				TargetedAlignmentWorkflowParameters.LoadParameters(ExecutorParameters.TargetedAlignmentWorkflowParameterFile);
			}

			TargetedWorkflow = TargetedWorkflow.CreateWorkflow(_workflowParameters);
		}

		public override void Execute()
		{
			SetupLogging();

			SetupAlignment();

			ReportGeneralProgress("Started Processing....");
			ReportGeneralProgress("Dataset = " + DatasetPath);

			if (!RunIsInitialized)
			{
				InitializeRun(DatasetPath);
			}

			ExecutePreProcessingHook();

			ProcessDataset();

			ExecutePostProcessingHook();

			ExportData();

			HandleAlignmentInfoFiles();
		}
	}
}
