using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using DeconTools.Workflows.Backend.Utilities;
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

		public override void InitializeWorkflow()
		{
			if (string.IsNullOrEmpty(ExecutorParameters.ResultsFolder))
			{
				_resultsFolder = RunUtilities.GetDatasetParentFolder(DatasetPath);
			}
			else
			{
				_resultsFolder = getResultsFolder(ExecutorParameters.ResultsFolder);
			}


			MassTagsForTargetedAlignment = GetMassTagTargets(ExecutorParameters.TargetsUsedForAlignmentFilePath);


			bool targetsFilePathIsEmpty = (String.IsNullOrEmpty(ExecutorParameters.TargetsFilePath));

			string currentTargetsFilePath;

			if (targetsFilePathIsEmpty)
			{
				currentTargetsFilePath = TryFindTargetsForCurrentDataset();   //check for a _targets file specifically associated with dataset
			}
			else
			{
				currentTargetsFilePath = ExecutorParameters.TargetsFilePath;
			}

			Targets = CreateTargets(ExecutorParameters.TargetType, currentTargetsFilePath);

			Check.Ensure(Targets != null && Targets.TargetList.Count > 0,
						 "Target massTags is empty. Check the path to the massTag data file.");

			if (ExecutorParameters.TargetType == Globals.TargetType.LcmsFeature)
			{
				UpdateTargetMissingInfo();
			}


			_workflowParameters = WorkflowParameters.CreateParameters(ExecutorParameters.WorkflowParameterFile);
			_workflowParameters.LoadParameters(ExecutorParameters.WorkflowParameterFile);

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

			TargetedWorkflow = TopDownTargetedWorkflow.CreateWorkflow(_workflowParameters);
		}



		#endregion

		#region Properties

		public override WorkflowParameters WorkflowParameters
		{
			get
			{
				return ExecutorParameters;
			}
			set
			{
				ExecutorParameters = value as WorkflowExecutorBaseParameters;
			}
		}


		#endregion

		private void UpdateTargetMissingInfo()
		{
			bool canUseReferenceMassTags = MassTagsForReference != null && MassTagsForReference.TargetList.Count > 0;

			foreach (LcmsFeatureTarget target in Targets.TargetList)
			{
				bool isMissingMonoMass = target.MonoIsotopicMass <= 0;

				if (String.IsNullOrEmpty(target.EmpiricalFormula))
				{
					if (ReferenceMassTagIDList.Contains(target.FeatureToMassTagID) && canUseReferenceMassTags)
					{
						var mt = MassTagsForReference.TargetList.First(p => p.ID == target.FeatureToMassTagID);

						//in DMS, Sequest will put an 'X' when it can't differentiate 'I' and 'L'
						//  see:   \\gigasax\DMS_Parameter_Files\Sequest\sequest_ETD_N14_NE.params
						//To create the theoretical isotopic profile, we will change the 'X' to 'L'
						if (mt.Code.Contains("X"))
						{
							mt.Code = mt.Code.Replace('X', 'L');
							mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
						}

						target.Code = mt.Code;
						target.EmpiricalFormula = mt.EmpiricalFormula;
					}
					else if (!String.IsNullOrEmpty(target.Code))
					{
						//Create empirical formula based on code. Assume it is an unmodified peptide
						target.EmpiricalFormula = new PeptideUtils().GetEmpiricalFormulaForPeptideSequence(target.Code);

					}
					else
					{
						if (isMissingMonoMass)
						{
							throw new ApplicationException(
								"Trying to prepare target list, but Target is missing both the 'Code' and the Monoisotopic Mass. One or the other is needed.");
						}
						target.Code = "AVERAGINE";
						target.EmpiricalFormula =
							IsotopicDistributionCalculator.GetAveragineFormulaAsString(target.MonoIsotopicMass);
					}
				}


				if (isMissingMonoMass)
				{
					target.MonoIsotopicMass =
						EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(target.EmpiricalFormula);

					target.MZ = target.MonoIsotopicMass / target.ChargeState + DeconTools.Backend.Globals.PROTON_MASS;
				}



			}
		}


		#region Public Methods

		public override void Execute()
		{
			_loggingFileName = ExecutorParameters.LoggingFolder + "\\" + RunUtilities.GetDatasetName(DatasetPath) + "_log.txt";

			ReportGeneralProgress("Started Processing....");
			ReportGeneralProgress("Dataset = " + DatasetPath);
			ReportGeneralProgress("Parameters:" + "\n" + _workflowParameters.ToStringWithDetails());


			try
			{
				if (!RunIsInitialized)
				{
					InitializeRun(DatasetPath);
				}

				ProcessDataset();
			}
			catch (Exception ex)
			{
				ReportGeneralProgress("--------------------------------------------------------------");
				ReportGeneralProgress("-------------------   ERROR    -------------------------------");
				ReportGeneralProgress("--------------------------------------------------------------");

				try
				{
					finalizeRun();

				}
				catch
				{
				}

				ReportGeneralProgress(ex.Message);
				ReportGeneralProgress(ex.StackTrace);


			}

		}
		
		private void ProcessDataset()
		{


			//apply mass calibration and NET alignment from .txt files, if they exist
			performAlignment();


			bool runIsNotAligned = (!Run.MassIsAligned && !Run.NETIsAligned);     //if one of these two is aligned, the run is considered to be aligned

			//Perform targeted alignment if 1) run is not aligned  2) parameters permit it
			if (runIsNotAligned && this.ExecutorParameters.TargetedAlignmentIsPerformed)
			{
				Check.Ensure(this.MassTagsForTargetedAlignment != null && this.MassTagsForTargetedAlignment.TargetList.Count > 0, "MassTags for targeted alignment have not been defined. Check path within parameter file.");

				ReportGeneralProgress("Performing TargetedAlignment using mass tags from file: " + this.ExecutorParameters.TargetsUsedForAlignmentFilePath);
				ReportGeneralProgress("Total mass tags to be aligned = " + this.MassTagsForTargetedAlignment.TargetList.Count);

				this.TargetedAlignmentWorkflow = new TargetedAlignerWorkflow(this.TargetedAlignmentWorkflowParameters);
				this.TargetedAlignmentWorkflow.SetMassTags(this.MassTagsForTargetedAlignment.TargetList);
				this.TargetedAlignmentWorkflow.Run = Run;
				this.TargetedAlignmentWorkflow.Execute();

				ReportGeneralProgress("Targeted Alignment COMPLETE.");
				ReportGeneralProgress("Targeted Alignment Report: ");
				ReportGeneralProgress(this.TargetedAlignmentWorkflow.GetAlignmentReport1());

				performAlignment();     //now perform alignment, based on alignment .txt files that were outputted from the targetedAlignmentWorkflow

				ReportGeneralProgress("MassAverage = \t" + this.TargetedAlignmentWorkflow.Aligner.Result.MassAverage.ToString("0.00000"));
				ReportGeneralProgress("MassStDev = \t" + this.TargetedAlignmentWorkflow.Aligner.Result.MassStDev.ToString("0.00000"));
				ReportGeneralProgress("NETAverage = \t" + this.TargetedAlignmentWorkflow.Aligner.Result.NETAverage.ToString("0.00000"));
				ReportGeneralProgress("NETStDev = \t" + this.TargetedAlignmentWorkflow.Aligner.Result.NETStDev.ToString("0.00000"));
				ReportGeneralProgress("---------------- END OF Alignment info -------------");
			}

			this.TargetedWorkflow.Run = Run;


			ResultRepository.Results.Clear();

			int mtCounter = 0;
			int totalTargets = Targets.TargetList.Count;

			ReportGeneralProgress("Processing...", 0);





			foreach (var massTag in this.Targets.TargetList)
			{
				mtCounter++;

#if DEBUG
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();

#endif


				Run.CurrentMassTag = massTag;
				try
				{
					this.TargetedWorkflow.Execute();
					ResultRepository.AddResult(this.TargetedWorkflow.Result);

				}
				catch (Exception ex)
				{
					string errorString = "Error on MT\t" + massTag.ID + "\tchargeState\t" + massTag.ChargeState + "\t" + ex.Message + "\t" + ex.StackTrace;
					ReportProcessingProgress(errorString, mtCounter);

					throw;
				}

#if DEBUG
				stopwatch.Stop();
				Console.WriteLine(massTag.ID + "\tprocessing time = " + stopwatch.ElapsedMilliseconds);

#endif


				string progressString = "Percent complete = " + ((double)mtCounter / totalTargets).ToString("0.0") + "\tTarget " + mtCounter + " of " + totalTargets;


				if (_backgroundWorker != null)
				{
					if (_backgroundWorker.CancellationPending)
					{
						return;
					}
				}

				ReportProcessingProgress(progressString, mtCounter);


			}

			ReportGeneralProgress("---- PROCESSING COMPLETE ---------------", 100);

			string outputFileName = this._resultsFolder + Path.DirectorySeparatorChar + Run.DatasetName + "_quant.txt";
			backupResultsFileIfNecessary(Run.DatasetName, outputFileName);

			// Collapse + process results
			ProcessResults(ResultRepository.Results);

			TargetedResultToTextExporter exporter = TargetedResultToTextExporter.CreateExporter(this._workflowParameters, outputFileName);
			exporter.ExportResults(ResultRepository.Results);

			HandleAlignmentInfoFiles();
			finalizeRun();
		}

		#endregion

		#region Private Methods
		private void ProcessResults(List<TargetedResultDTO> results)
		{
			for (int i = 0; i < results.Count; i++)
			{
				var result = (TopDownTargetedResultDTO) results[i];
				result.PrsmList = new List<int> {result.MatchedMassTagID};
				result.ChargeList = new List<int> {result.ChargeState};

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
						if (otherResult.MatchedMassTagID > 0) result.PrsmList.Add(otherResult.MatchedMassTagID);
						if (otherResult.ChromPeakSelectedHeight > 0)
						{
							result.ChargeList.Add(otherResult.ChargeState);
							result.Quantitation += otherResult.ChromPeakSelectedHeight;
						}

						if (!havePrsmData && _prsmData.ContainsKey(otherResult.MatchedMassTagID))
						{
							result.ProteinName = _prsmData[otherResult.MatchedMassTagID].ProteinName;
							result.ProteinMass = _prsmData[otherResult.MatchedMassTagID].ProteinMass;
						}

						// If this spectrum is better than the current one, update Prsm_ID
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

		protected TargetCollection GetMSAlignTargets(string massTagFileName)
		{
			if (string.IsNullOrEmpty(massTagFileName) || !File.Exists(massTagFileName))
			{
				return new TargetCollection();
			}

			var importer = new MassTagFromMSAlignFileImporter(massTagFileName);
			return importer.Import(out _prsmData);
		}

		private void ReportGeneralProgress(string generalProgressString, int progressPercent = 0)
		{
			if (_backgroundWorker == null)
			{
				Console.WriteLine(DateTime.Now + "\t" + generalProgressString);
			}
			else
			{
				_progressInfo.ProgressInfoString = generalProgressString;
				_progressInfo.IsGeneralProgress = true;
				_backgroundWorker.ReportProgress(progressPercent, _progressInfo);
			}

			writeToLogFile(DateTime.Now + "\t" + generalProgressString);
		}

		private void HandleAlignmentInfoFiles()
		{
			FileAttributes attr = File.GetAttributes(Run.Filename);


			FileInfo[] datasetRelatedFiles;

			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
			{
				DirectoryInfo dirInfo = new DirectoryInfo(Run.Filename);
				datasetRelatedFiles = dirInfo.GetFiles(Run.DatasetName + "*.txt");



			}
			else
			{
				FileInfo fi = new FileInfo(Run.Filename);
				DirectoryInfo dirInfo = fi.Directory;
				datasetRelatedFiles = dirInfo.GetFiles(Run.DatasetName + "*.txt");

			}

			foreach (var file in datasetRelatedFiles)
			{
				if (file.Name.Contains("_alignedFeatures") || file.Name.Contains("_MZAlignment") || file.Name.Contains("_NETAlignment"))
				{
					bool allowOverwrite = false;

					string targetCopiedFilename = ExecutorParameters.AlignmentInfoFolder + Path.DirectorySeparatorChar + file.Name;

					//upload alignment data only if it doesn't already exist
					if (!File.Exists(targetCopiedFilename))
					{
						file.CopyTo(ExecutorParameters.AlignmentInfoFolder + Path.DirectorySeparatorChar + file.Name, allowOverwrite);
					}

					if (this.ExecutorParameters.CopyRawFileLocal)
					{
						file.Delete();       //if things were copied locally, we are going to delete anything created. 
					}

				}

			}


		}

		private void CopyAlignmentInfoIfExists()
		{
			if (String.IsNullOrEmpty(ExecutorParameters.AlignmentInfoFolder)) return;

			DirectoryInfo dirInfo = new DirectoryInfo(ExecutorParameters.AlignmentInfoFolder);

			if (dirInfo.Exists)
			{

				FileInfo[] datasetRelatedFiles = dirInfo.GetFiles(Run.DatasetName + "*.txt");

				foreach (var file in datasetRelatedFiles)
				{
					if (file.Name.ToLower() == Run.DatasetName.ToLower() + "_mzalignment.txt" || file.Name.ToLower() == Run.DatasetName.ToLower() + "_netalignment.txt")
					{
						string targetFileName = Run.DataSetPath + Path.DirectorySeparatorChar + file.Name;
						if (!File.Exists(targetFileName))
						{
							file.CopyTo(Run.DataSetPath + Path.DirectorySeparatorChar + file.Name, true);
						}
					}


				}


			}
		}

		private void performAlignment()
		{
			if (string.IsNullOrEmpty(ExecutorParameters.AlignmentInfoFolder))
			{
				RunUtilities.AlignRunUsingAlignmentInfoInFiles(Run);
			}
			else
			{
				RunUtilities.AlignRunUsingAlignmentInfoInFiles(Run, ExecutorParameters.AlignmentInfoFolder);
			}





			if (Run.MassIsAligned)
			{
				ReportGeneralProgress("Run has been mass aligned using info in _MZAlignment.txt file");
			}
			else
			{
				ReportGeneralProgress("FYI - Run has NOT been mass aligned.");
			}

			if (Run.NETIsAligned)
			{
				ReportGeneralProgress("Run has been NET aligned using info in either the _NETAlignment.txt file or the _UMCs.txt file");
			}
			else
			{
				ReportGeneralProgress("Warning - Run has NOT been NET aligned.");
			}
		}

		private void CreatePeaksForChromSourceData()
		{
			PeakDetectAndExportWorkflowParameters parameters = new PeakDetectAndExportWorkflowParameters();
			TargetedWorkflowParameters deconParam = (TargetedWorkflowParameters)this._workflowParameters;

			parameters.PeakBR = deconParam.ChromGenSourceDataPeakBR;
			parameters.PeakFitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;
			parameters.SigNoiseThreshold = deconParam.ChromGenSourceDataSigNoise;
			PeakDetectAndExportWorkflow peakCreator = new PeakDetectAndExportWorkflow(this.Run, parameters, _backgroundWorker);
			peakCreator.Execute();
		}

		private bool checkForPeaksFile()
		{
			string baseFileName;
			baseFileName = this.Run.DataSetPath + "\\" + this.Run.DatasetName;

			string possibleFilename1 = baseFileName + "_peaks.txt";

			if (File.Exists(possibleFilename1))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		protected new TargetCollection CreateTargets(Globals.TargetType targetType, string targetFilePath)
		{
			if (string.IsNullOrEmpty(targetFilePath)) return null;

			switch (targetType)
			{
				case Globals.TargetType.LcmsFeature:
					return GetMSAlignTargets(targetFilePath);
				case Globals.TargetType.DatabaseTarget:
					return GetMassTagTargets(targetFilePath);
				default:
					throw new ArgumentOutOfRangeException("targetType");
			}
		}

		protected override TargetedResultToTextExporter createExporter(string outputFileName)
		{
			throw new NotImplementedException();
		}


		private void cleanUpLocalFiles()
		{
			throw new NotImplementedException();
		}
		#endregion

	}
}
