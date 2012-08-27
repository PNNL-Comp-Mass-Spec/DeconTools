using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
namespace DeconTools.Workflows.Backend.Core
{
	public class TopDownTargetedWorkflowExecutor : TargetedWorkflowExecutor
	{
		private Dictionary<int, PrsmData> _prsmData;

		private TargetedWorkflowExecutorProgressInfo _progressInfo = new TargetedWorkflowExecutorProgressInfo();

		#region Constructors
		public TopDownTargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath, BackgroundWorker backgroundWorker = null)
            : base(parameters, datasetPath, backgroundWorker)
		{
		}

	
		#endregion
        
		
		#region Public Methods

        protected override void ExecutePostProcessingHook()
        {
            // Collapse + process results
            PostProcessResults(ResultRepository.Results);
        }

        protected override TargetCollection GetLcmsFeatureTargets(string targetsFilePath)
        {
            return GetMSAlignTargets(targetsFilePath);
        }

        protected override string GetOutputFileName()
        {
            return _resultsFolder + Path.DirectorySeparatorChar + Run.DatasetName + "_quant.txt";
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
                }
            }

            return targets;
        }





		private void PostProcessResults(List<TargetedResultDTO> results)
		{
			for (int i = 0; i < results.Count; i++)
			{
				var result = (TopDownTargetedResultDTO) results[i];
				result.PrsmList = new List<int>();
				if (result.MatchedMassTagID > 0) result.PrsmList.Add(result.MatchedMassTagID);

				result.ChargeStateList = new List<int>();
				if (result.ChromPeakSelectedHeight > 0) result.ChargeStateList.Add(result.ChargeState);

				bool havePrsmData = false;
				if (_prsmData.ContainsKey(result.MatchedMassTagID))
				{
					havePrsmData = true;
					result.ProteinName = _prsmData[result.MatchedMassTagID].ProteinName;
					result.ProteinMass = _prsmData[result.MatchedMassTagID].ProteinMass;
				}

				// Find other results with same target code
				for (int j = i + 1; j < results.Count; j++)
				{
					var otherResult = (TopDownTargetedResultDTO) results[j];

					if (result.PeptideSequence.Equals(otherResult.PeptideSequence))
					{
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
		}
        
        
        
		#endregion

	}
}
