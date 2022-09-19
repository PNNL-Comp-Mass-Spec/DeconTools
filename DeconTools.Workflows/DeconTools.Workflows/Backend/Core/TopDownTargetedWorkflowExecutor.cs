using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Data;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.Core
{
    public class TopDownTargetedWorkflowExecutor : TargetedWorkflowExecutor
    {
        private Dictionary<int, PrsmData> _prsmData;

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="datasetPath"></param>
        /// <param name="backgroundWorker"></param>
        public TopDownTargetedWorkflowExecutor(WorkflowParameters parameters, string datasetPath, BackgroundWorker backgroundWorker = null)
            : base(parameters, datasetPath, backgroundWorker)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="workflowParameters"></param>
        /// <param name="datasetPath"></param>
        /// <param name="backgroundWorker"></param>
        public TopDownTargetedWorkflowExecutor(WorkflowParameters parameters, WorkflowParameters workflowParameters, string datasetPath, BackgroundWorker backgroundWorker = null)
            : base(parameters, workflowParameters, datasetPath, backgroundWorker)
        {
        }

        #endregion

        #region Public Methods

        protected override void ExecutePreProcessingHook()
        {
            if (!(ExecutorParameters is TopDownTargetedWorkflowExecutorParameters executor)) return;

            //If user wants to save to Excel, we need to make sure that the underlying parameters supports saving chromatogram data
            if (!executor.ExportChromatogramData) return;

            if (TargetedWorkflow is TopDownTargetedWorkflow workflow &&
                workflow.WorkflowParameters is TopDownTargetedWorkflowParameters workflowParams)
            {
                workflowParams.SaveChromatogramData = true;
            }
        }

        protected override void ExecutePostProcessingHook()
        {
            if (ExecutorParameters is TopDownTargetedWorkflowExecutorParameters executor)
            {
                if (executor.ExportChromatogramData)
                {
                    ExportChromatogramDataForEachProtein();
                }
            }

            // Collapse + process results
            PostProcessResults(ResultRepository.Results);
        }

        private void ExportChromatogramDataForEachProtein()
        {
            var allResults = TargetedWorkflow.Run.ResultCollection.GetMassTagResults();

            var proteoformList = allResults.Select(p => p.Target.Code).Distinct();

            var counter = 0;
            foreach (var proteoform in proteoformList)
            {
                counter++;

                var resultsForProtein = allResults.Where(p => p.Target.Code == proteoform).ToList();

                var sb = new StringBuilder();
                var delimiter = '\t';

                sb.Append("Unique identifier= \t" + proteoform);
                sb.Append(Environment.NewLine);
                sb.Append("MSAlign row= " + counter);
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);

                var tableHeader = "z\tmz\tscan\tscore\tintensity";
                sb.Append(tableHeader);
                sb.Append(Environment.NewLine);

                foreach (var targetedResultBase in resultsForProtein)
                {
                    var result = (TopDownTargetedResult)targetedResultBase;

                    sb.Append(result.Target.ChargeState);
                    sb.Append(delimiter);
                    sb.Append(result.Target.MZ);
                    sb.Append(delimiter);

                    sb.Append(result.ChromPeakSelected?.XValue ?? 0);
                    sb.Append(delimiter);
                    sb.Append(result.Score);
                    sb.Append(delimiter);
                    sb.Append(result.IsotopicProfile == null ? 0 : result.IntensityAggregate);

                    sb.Append(Environment.NewLine);
                }

                sb.Append(Environment.NewLine);

                var topDownChromData = new TopdownChromData();

                foreach (var targetedResultBase in resultsForProtein)
                {
                    var result = (TopDownTargetedResult)targetedResultBase;

                    if (result.ChromValues != null)
                    {
                        topDownChromData.AddChromDataItem(result.ChromValues);
                    }
                }

                var allChromVals = topDownChromData.GetChromData();

                var lengthOfScanArray = allChromVals[0].Xvalues.Length;

                //add headers
                for (var i = 0; i < resultsForProtein.Count; i++)
                {
                    if (i == 0)
                    {
                        sb.Append("Scan");
                        sb.Append(delimiter);
                    }

                    sb.Append(resultsForProtein[i].Target.ChargeState.ToString("0") + "+");
                    sb.Append(delimiter);
                }

                sb.Append(Environment.NewLine);

                //add data from multiple chrom data arrays
                for (var i = 0; i < lengthOfScanArray; i++)
                {
                    var isFirstIteration = true;
                    foreach (var val in allChromVals)
                    {
                        if (isFirstIteration)
                        {
                            sb.Append(val.Xvalues[i]);
                            sb.Append(delimiter);

                            isFirstIteration = false;
                        }
                        sb.Append(val.Yvalues[i]);
                        sb.Append(delimiter);
                    }
                    sb.Append(Environment.NewLine);
                }

                var outputDebugFolder = Path.Combine(ExecutorParameters.OutputDirectoryBase, "Testing");
                if (!Directory.Exists(outputDebugFolder)) Directory.CreateDirectory(outputDebugFolder);

                var chromDataFilename = Path.Combine(outputDebugFolder, "chromData_" + counter.ToString("0").PadLeft(4, '0') + ".txt");

                using (var writer = new StreamWriter(chromDataFilename))
                {
                    writer.Write(sb.ToString());
                    writer.Close();
                }

                //output graph image

            }
        }

        protected override TargetCollection GetLcmsFeatureTargets(string targetsFilePath)
        {
            return GetMSAlignTargets(targetsFilePath);
        }

        protected override string GetOutputFilePath()
        {
            string resultsDirectoryPath;

            if (string.IsNullOrEmpty(ExecutorParameters.OutputDirectoryBase))
            {
                resultsDirectoryPath = RunUtilities.GetDatasetParentDirectory(DatasetPath);
            }
            else
            {
                resultsDirectoryPath = Path.Combine(ExecutorParameters.OutputDirectoryBase, "IqResults");
            }

            if (!Directory.Exists(resultsDirectoryPath))
                Directory.CreateDirectory(resultsDirectoryPath);

            return Path.Combine(resultsDirectoryPath, Run.DatasetName + "_quant.txt");
        }

        #endregion

        #region Private Methods

        private TargetCollection GetMSAlignTargets(string massTagFileName)
        {
            if (string.IsNullOrEmpty(massTagFileName))
            {
                ReportGeneralProgress("Warning: massTagFileName is empty; nothing to load");
                return new TargetCollection();
            }

            if (!File.Exists(massTagFileName))
            {
                ReportGeneralProgress("Warning: mass tags file does not exist; nothing to load: " + massTagFileName);
                return new TargetCollection();
            }

            var importer = new MassTagFromMSAlignFileImporter(massTagFileName);

            var targets = importer.Import(out _prsmData);

            if (importer.DataRowsProcessed == 0)
            {
                ReportGeneralProgress("Error: mass tags file was empty; cannot continue; see " + massTagFileName);
            }
            else
            {
                if (importer.DataRowsProcessed == importer.DataRowsSkippedUnknownMods)
                {
                    ReportGeneralProgress("Error: every peptide in the mass tags file had an unknown modification; cannot continue; see " + massTagFileName);
                    ReportGeneralProgress("   ... can only process peptides that are unmodified or have Acetylation (C2H2O, 42.01 Da), Phosphorylation (HPO3, 79.97 Da), or Pyroglutamate (H3N1, 17.03 Da) mods");
                }
            }

            return targets;
        }

        private void PostProcessResults(IList<TargetedResultDTO> results)
        {
            for (var i = 0; i < results.Count; i++)
            {
                var result = (TopDownTargetedResultDTO)results[i];
                result.PrsmList = new HashSet<int>();
                if (result.MatchedMassTagID > 0) result.PrsmList.Add(result.MatchedMassTagID);

                result.ChargeStateList = new List<int>();
                if (result.ChromPeakSelectedHeight > 0) result.ChargeStateList.Add(result.ChargeState);

                var havePrsmData = false;
                if (_prsmData.ContainsKey(result.MatchedMassTagID))
                {
                    havePrsmData = true;
                    result.ProteinName = _prsmData[result.MatchedMassTagID].ProteinName;
                    result.ProteinMass = _prsmData[result.MatchedMassTagID].ProteinMass;
                }

                // Find other results with same target code
                for (var j = i + 1; j < results.Count; j++)
                {
                    var otherResult = (TopDownTargetedResultDTO)results[j];

                    if (!result.PeptideSequence.Equals(otherResult.PeptideSequence)) continue;

                    // Add other Prsm
                    if (otherResult.MatchedMassTagID > 0) result.PrsmList.Add(otherResult.MatchedMassTagID);

                    // Add other charge state to list and chrom peak to quantitation
                    if (otherResult.ChromPeakSelectedHeight > 0)
                    {
                        result.ChargeStateList.Add(otherResult.ChargeState);
                        result.Quantitation += otherResult.ChromPeakSelectedHeight;
                    }

                    // Update most abundant charge state & scan apex
                    if (otherResult.ChromPeakSelectedHeight > result.ChromPeakSelectedHeight)
                    {
                        result.MostAbundantChargeState = otherResult.ChargeState;
                        result.ScanLC = otherResult.ScanLC;
                    }

                    // Add name and mass if we don't have already
                    if (!havePrsmData && _prsmData.ContainsKey(otherResult.MatchedMassTagID))
                    {
                        result.ProteinName = _prsmData[otherResult.MatchedMassTagID].ProteinName;
                        result.ProteinMass = _prsmData[otherResult.MatchedMassTagID].ProteinMass;
                    }

                    // Update Prsm_ID if it doesn't exist
                    if (result.MatchedMassTagID < 0 && _prsmData.ContainsKey(otherResult.MatchedMassTagID))
                    {
                        result.MatchedMassTagID = otherResult.MatchedMassTagID;
                    }
                    // Or if this spectrum is better than the current one, update Prsm_ID
                    if (_prsmData.ContainsKey(result.MatchedMassTagID) && _prsmData.ContainsKey(otherResult.MatchedMassTagID) &&
                        _prsmData[result.MatchedMassTagID].EValue > _prsmData[otherResult.MatchedMassTagID].EValue)
                    {
                        result.MatchedMassTagID = otherResult.MatchedMassTagID;
                    }

                    results.RemoveAt(j--);
                }
            }
        }

        #endregion

    }
}
