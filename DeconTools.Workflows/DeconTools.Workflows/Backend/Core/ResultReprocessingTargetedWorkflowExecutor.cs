using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.Core
{
    public class ResultReprocessingTargetedWorkflowExecutor : TargetedWorkflowExecutor
    {
        private TargetedResultDTO _currentResult;
        private string _currentDatasetName;


        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public ResultReprocessingTargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath, BackgroundWorker backgroundWorker) : base(parameters, datasetPath, backgroundWorker)
        {
        }

        public ResultReprocessingTargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, Run run, BackgroundWorker backgroundWorker) : base(parameters, run, backgroundWorker)
        {
        }



        public override void Execute()
        {
            if (string.IsNullOrEmpty(ExecutorParameters.TargetsFilePath))
            {
                throw new ApplicationException("Processing failed. TargetsFilePath is empty. Check your parameter file.");
            }

            if (!File.Exists(ExecutorParameters.TargetsFilePath))
            {
                throw new FileNotFoundException("File not found error for the TargetsFilePath. Check your parameter file for " + ExecutorParameters.TargetsFilePath);
            }


            TargetedResultFromTextImporter resultImporter = new UnlabeledTargetedResultFromTextImporter(ExecutorParameters.TargetsFilePath);
            var allResults = resultImporter.Import();

            var resultsSortedByDataset = allResults.Results.OrderBy(p => p.DatasetName).ToList();

            var totalResults = resultsSortedByDataset.Count();


            ResultRepository.Clear();

            //iterate over results

            var resultCounter = 0;
            foreach (var result in resultsSortedByDataset)
            {
                resultCounter++;
                _currentResult = result;


                if (result.DatasetName != _currentDatasetName)
                {
                    Run?.Close();


                    InitializeRun(result.DatasetName);

                    if (Run == null)
                    {
                        throw new ApplicationException("Error initializing dataset. (Run is null)");
                    }

                    _currentDatasetName = Run.DatasetName;

                }

                SetCurrentWorkflowTarget(result);

                try
                {
                    TargetedWorkflow.Execute();
                    ResultRepository.AddResult(TargetedWorkflow.Result);

                }
                catch (Exception ex)
                {
                    var errorString = "Error on MT\t" + result.TargetID + "\tchargeState\t" + result.ChargeState + "\t" + ex.Message + "\t" + ex.StackTrace;
                    ReportProcessingProgress(errorString, resultCounter);

                    throw;
                }

                var progressString = "Processed " + resultCounter + " of " + totalResults;

                if (_backgroundWorker != null)
                {
                    if (_backgroundWorker.CancellationPending)
                    {
                        return;
                    }
                }

                ReportProcessingProgress(progressString, resultCounter);

            }



        }
        private void SetCurrentWorkflowTarget(TargetedResultDTO result)
        {
            TargetBase target = new LcmsFeatureTarget();
            target.ChargeState = (short)result.ChargeState;
            target.ChargeStateTargets.Add(target.ChargeState);
            target.ElutionTimeUnit = DeconTools.Backend.Globals.ElutionTimeUnit.ScanNum;
            target.EmpiricalFormula = result.EmpiricalFormula;
            target.ID = (int)result.TargetID;


            target.IsotopicProfile = null;   //workflow will determine this

            target.MZ = result.MonoMZ;
            target.MonoIsotopicMass = result.MonoMass;
            target.ScanLCTarget = result.ScanLC;

            Run.CurrentMassTag = target;



        }

    }
}
