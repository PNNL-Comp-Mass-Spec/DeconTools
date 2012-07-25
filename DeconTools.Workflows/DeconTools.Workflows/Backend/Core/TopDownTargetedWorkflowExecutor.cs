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
	public class TopDownTargetedWorkflowExecutor : WorkflowBase
	{
		private Dictionary<int, PrsmData> _prsmData;

		protected IsotopicDistributionCalculator IsotopicDistributionCalculator = IsotopicDistributionCalculator.Instance;


		protected string _loggingFileName;
		protected string _resultsFolder;
		protected TargetedResultRepository ResultRepository;


		protected List<long> ReferenceMassTagIDList = new List<long>();

		protected WorkflowParameters _workflowParameters;

		protected BackgroundWorker _backgroundWorker;
		private TargetedWorkflowExecutorProgressInfo _progressInfo = new TargetedWorkflowExecutorProgressInfo();

		#region Constructors
		public TopDownTargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath, BackgroundWorker backgroundWorker = null)
		{
			this.DatasetPath = datasetPath;

			_backgroundWorker = backgroundWorker;

			this.WorkflowParameters = parameters;

			ResultRepository = new TargetedResultRepository();
			InitializeWorkflow();
		}


		public TopDownTargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, Run run, BackgroundWorker backgroundWorker = null)
		{
			Run = run;

			if (Run != null) DatasetPath = Run.DataSetPath;

			_backgroundWorker = backgroundWorker;

			this.WorkflowParameters = parameters;

			ResultRepository = new TargetedResultRepository();
			InitializeWorkflow();
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

		public string DatasetPath { get; set; }

		public TargetCollection MassTagsForTargetedAlignment { get; set; }

		public TargetCollection Targets { get; set; }

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

		public WorkflowExecutorBaseParameters ExecutorParameters { get; set; }

		public TargetedAlignerWorkflowParameters TargetedAlignmentWorkflowParameters { get; set; }

		public TargetedAlignerWorkflow TargetedAlignmentWorkflow { get; set; }

		public TargetedWorkflow TargetedWorkflow { get; set; }

		/// <summary>
		/// These are database targets that are used for lookup when working on peak-matched LcmsFeatures
		/// </summary>
		public TargetCollection MassTagsForReference { get; set; }


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

		protected string TryFindTargetsForCurrentDataset()
		{
			string expectedTargetsFile = ExecutorParameters.TargetsBaseFolder + Path.DirectorySeparatorChar +
										 RunUtilities.GetDatasetName(DatasetPath) + "_targets.txt";

			if (File.Exists(expectedTargetsFile))
			{
				return expectedTargetsFile;
			}
			else
			{
				return String.Empty;
			}

		}


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


		public List<TargetedResultDTO> GetResults()
		{
			return ResultRepository.Results;
		}


		private bool _runIsInitialized;
		public bool RunIsInitialized
		{
			get
			{
				if (Run == null || Run.ResultCollection.MSPeakResultList.Count == 0)
				{
					return false;
				}

				if (DatasetPath != Run.DataSetPath)
				{
					return false;
				}

				return true;

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

		protected string getResultsFolder(string folder)
		{
			DirectoryInfo dirinfo = new DirectoryInfo(folder);

			if (!dirinfo.Exists)
			{
				dirinfo.Create();
			}

			return dirinfo.FullName;

		}

		protected TargetCollection GetMassTagTargets(string massTagFileName)
		{
			return GetMassTagTargets(massTagFileName, new List<int>());
		}

		protected TargetCollection GetMassTagTargets(string massTagFileName, List<int> targetIDsToFilterOn)
		{
			if (String.IsNullOrEmpty(massTagFileName) || !File.Exists(massTagFileName))
			{
				return new TargetCollection();
			}

			MassTagFromTextFileImporter importer = new MassTagFromTextFileImporter(massTagFileName);
			return importer.Import(targetIDsToFilterOn);
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

		protected string getLogFileName(string folderPath)
		{
			string logfilename = "";

			DirectoryInfo logfolderPath = new DirectoryInfo(folderPath);

			if (!logfolderPath.Exists) logfolderPath.Create();

			logfilename = logfolderPath.FullName + Path.DirectorySeparatorChar + "logfile_" + DateTime.Now.Year.ToString() + "_" +
				DateTime.Now.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Now.Day.ToString().PadLeft(2, '0') + "_" + DateTime.Now.Ticks.ToString() + ".txt";

			return logfilename;

		}

		protected List<string> getListDatasetPaths(string fileContainingDatasetPaths)
		{
			List<string> datasetPathList = new List<string>();

			using (StreamReader sr = new StreamReader(fileContainingDatasetPaths))
			{

				while (sr.Peek() != -1)
				{
					datasetPathList.Add(sr.ReadLine());

				}
				sr.Close();
			}

			return datasetPathList;

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


		protected void ReportProcessingProgress(string reportString, int progressCounter)
		{

			if (_backgroundWorker == null)
			{
				if (progressCounter % 100 == 0)
				{
					Console.WriteLine(DateTime.Now + "\t" + reportString);
				}

			}
			else
			{
				int progressPercent = (int)(progressCounter * 100 / (double)Targets.TargetList.Count);

				_progressInfo.ProgressInfoString = reportString;
				_progressInfo.IsGeneralProgress = false;
				_progressInfo.Result = TargetedWorkflow.Result;
				_progressInfo.Time = DateTime.Now;

				_progressInfo.ChromatogramXYData = new XYData();
				_progressInfo.ChromatogramXYData.Xvalues = TargetedWorkflow.ChromatogramXYData.Xvalues;
				_progressInfo.ChromatogramXYData.Yvalues = TargetedWorkflow.ChromatogramXYData.Yvalues;

				_progressInfo.MassSpectrumXYData = new XYData();
				_progressInfo.MassSpectrumXYData.Xvalues = TargetedWorkflow.MassSpectrumXYData.Xvalues;
				_progressInfo.MassSpectrumXYData.Yvalues = TargetedWorkflow.MassSpectrumXYData.Yvalues;

				_backgroundWorker.ReportProgress(progressPercent, _progressInfo);
			}

			if (progressCounter % 100 == 0)
			{
				writeToLogFile(DateTime.Now + "\t" + reportString);
			}

		}

		protected void writeToLogFile(string stringToWrite)
		{

			using (StreamWriter sw = new StreamWriter(new System.IO.FileStream(this._loggingFileName, System.IO.FileMode.Append,
						  System.IO.FileAccess.Write, System.IO.FileShare.Read)))
			{
				sw.AutoFlush = true;
				sw.WriteLine(stringToWrite);
				sw.Flush();

			}

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

		public void InitializeRun(string dataset)
		{
			string runFilename;


			if (this.ExecutorParameters.CopyRawFileLocal)
			{
				ReportGeneralProgress("Started copying raw data to local folder: " + this.ExecutorParameters.FolderPathForCopiedRawDataset);

				FileAttributes attr = File.GetAttributes(dataset);

				DirectoryInfo sourceDirInfo;
				DirectoryInfo targetDirInfo;
				if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				{
					sourceDirInfo = new DirectoryInfo(dataset);
					runFilename = this.ExecutorParameters.FolderPathForCopiedRawDataset + Path.DirectorySeparatorChar + sourceDirInfo.Name;
					targetDirInfo = new DirectoryInfo(runFilename);
					FileUtilities.CopyAll(sourceDirInfo, targetDirInfo);
					ReportGeneralProgress("Copying complete.");
				}
				else
				{
					FileInfo fileinfo = new FileInfo(dataset);
					sourceDirInfo = fileinfo.Directory;
					runFilename = this.ExecutorParameters.FolderPathForCopiedRawDataset + Path.DirectorySeparatorChar + Path.GetFileName(dataset);

					targetDirInfo = new DirectoryInfo(this.ExecutorParameters.FolderPathForCopiedRawDataset);

					if (!File.Exists(runFilename))
					{
						FileUtilities.CopyAll(fileinfo, targetDirInfo);
						ReportGeneralProgress("Copying complete.");
					}
					else
					{
						ReportGeneralProgress("Datafile already exists on local drive. Using existing datafile.");

					}

				}

			}
			else
			{
				runFilename = dataset;
			}

			//create Run
			RunFactory rf = new RunFactory();
			Run = rf.CreateRun(runFilename);

			bool runInstantiationFailed = (Run == null);
			if (runInstantiationFailed)
			{
				ReportGeneralProgress("Run initialization FAILED. Likely a filename problem. Or missing manufacturer .dlls");
				return;
			}
			else
			{
				ReportGeneralProgress("Run initialized successfully.");
			}


			//Retrieve alignment data if it exists
			CopyAlignmentInfoIfExists();




			//check and load chrom source data (_peaks.txt)
			bool peaksFileExists = checkForPeaksFile();
			if (!peaksFileExists)
			{
				ReportGeneralProgress("Creating extracted ion chromatogram (XIC) source data... takes 1-5 minutes.. only needs to be done once.");

				CreatePeaksForChromSourceData();
				ReportGeneralProgress("Done creating XIC source data.");
			}



			ReportGeneralProgress("Peak loading started...");


			string baseFileName;
			baseFileName = this.Run.DataSetPath + "\\" + this.Run.DatasetName;

			string possibleFilename1 = baseFileName + "_peaks.txt";

			if (File.Exists(possibleFilename1))
			{
				//create background worker so that updates don't go out to console.
				//BackgroundWorker bw = new BackgroundWorker();
				//bw.WorkerSupportsCancellation = true;
				//bw.WorkerReportsProgress = true;

				//TODO: keep an eye on errors connected to background worker here.
				PeakImporterFromText peakImporter = new PeakImporterFromText(possibleFilename1, _backgroundWorker);

				peakImporter.ImportPeaks(this.Run.ResultCollection.MSPeakResultList);
			}
			else
			{
				ReportGeneralProgress(DateTime.Now + "\tCRITICAL FAILURE. Chrom source data (_peaks.txt) file not loaded.");
				return;
			}


			ReportGeneralProgress(DateTime.Now + "\tPeak Loading complete.");
			return;
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

		protected TargetCollection CreateTargets(Globals.TargetType targetType, string targetFilePath)
		{
			if (string.IsNullOrEmpty(targetFilePath)) return null;

			switch (targetType)
			{
				case Globals.TargetType.LcmsFeature:
					return GetMSAlignTargets(targetFilePath);
					break;
				case Globals.TargetType.DatabaseTarget:
					return GetMassTagTargets(targetFilePath);
					break;
				default:
					throw new ArgumentOutOfRangeException("targetType");
			}
		}


		protected TargetCollection GetLcmsFeatureTargets(string targetsFilePath)
		{
			LcmsTargetFromFeaturesFileImporter importer =
			   new LcmsTargetFromFeaturesFileImporter(targetsFilePath);

			var lcmsTargetCollection = importer.Import();
			return lcmsTargetCollection;
		}

		protected virtual TargetedResultToTextExporter createExporter(string outputFileName)
		{
			throw new NotImplementedException();
		}


		private void cleanUpLocalFiles()
		{
			throw new NotImplementedException();
		}

		protected void finalizeRun()
		{

			string runfileName = Run.Filename;
			string datasetName = Run.DatasetName;

			Run.Close();
			Run = null;
			GC.Collect();



			if (this.ExecutorParameters.CopyRawFileLocal && this.ExecutorParameters.DeleteLocalDatasetAfterProcessing)
			{
				FileAttributes attr = File.GetAttributes(runfileName);

				if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				{
					DirectoryInfo dirInfo = new DirectoryInfo(runfileName);
					dirInfo.Delete(true);
				}
				else
				{
					FileInfo fileinfo = new FileInfo(runfileName);

					string fileSuffix = fileinfo.Extension;

					DirectoryInfo dirInfo = fileinfo.Directory;

					string expectedPeaksFile = dirInfo.FullName + Path.DirectorySeparatorChar + datasetName + "_peaks.txt";

					if (File.Exists(expectedPeaksFile))
					{
						//File.Delete(expectedPeaksFile);
					}

					FileInfo[] allRawDataFiles = dirInfo.GetFiles("*" + fileSuffix);
					if (allRawDataFiles.Count() > 35)
					{
						foreach (var file in allRawDataFiles)
						{
							try
							{
								file.Delete();
							}
							catch (Exception)
							{

							}


						}
					}


				}




			}




		}

		protected void backupResultsFileIfNecessary(string datasetName, string outputFileName)
		{
			FileInfo outputFileInfo = new FileInfo(outputFileName);


			if (outputFileInfo.Exists)
			{
				string backupFolder = this._resultsFolder + Path.DirectorySeparatorChar + "Backup";
				DirectoryInfo backupFolderInfo = new DirectoryInfo(backupFolder);

				if (!backupFolderInfo.Exists)
				{
					backupFolderInfo.Create();
				}

				string backupFilename = backupFolderInfo.FullName + Path.DirectorySeparatorChar + datasetName + "_results.txt";
				outputFileInfo.CopyTo(backupFilename, true);

				outputFileInfo.Delete();

			}
		}
		#endregion

	}
}
