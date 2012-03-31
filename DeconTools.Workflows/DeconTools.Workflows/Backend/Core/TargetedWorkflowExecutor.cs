﻿
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using DeconTools.Workflows.Backend.Utilities;
namespace DeconTools.Workflows.Backend.Core
{
    public abstract class TargetedWorkflowExecutor : WorkflowBase
    {

        protected string _loggingFileName;
        protected string _resultsFolder;
        
        
        protected WorkflowParameters _workflowParameters;
        protected string DatasetPath;
        private BackgroundWorker _backgroundWorker;
        private TargetedWorkflowExecutorProgressInfo _progressInfo = new TargetedWorkflowExecutorProgressInfo();

        #region Constructors
        public TargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath, BackgroundWorker backgroundWorker = null)
        {
            this.DatasetPath = datasetPath;

            _backgroundWorker = backgroundWorker;

            this.WorkflowParameters = parameters;
            InitializeWorkflow();
        }

        #endregion

        #region Properties
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

        


        #endregion

        #region Public Methods
        public override void Execute()
        {
            _loggingFileName = ExecutorParameters.LoggingFolder + "\\" + RunUtilities.GetDatasetName(DatasetPath) + "_log.txt";

            ReportGeneralProgress("Started Processing....");
            ReportGeneralProgress("Dataset = " + DatasetPath);
            ReportGeneralProgress("Parameters:" + "\n"+  _workflowParameters.ToStringWithDetails());


            try
            {
                InitializeRun(DatasetPath);
                return;
                
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


            bool runIsNotAligned = (!Run.MassIsAligned || !Run.NETIsAligned);

            //Perform targeted alignment if 1) run is not aligned  2) parameters permit it
            if (runIsNotAligned && this.ExecutorParameters.TargetedAlignmentIsPerformed)
            {
                Check.Ensure(this.MassTagsForTargetedAlignment != null && this.MassTagsForTargetedAlignment.TargetList.Count > 0, "MassTags for targeted alignment have not been defined. Check path within parameter file.");

                ReportGeneralProgress("Performing TargetedAlignment using mass tags from file: " + this.ExecutorParameters.MassTagsForAlignmentFilePath);
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

            TargetedResultRepository resultRepository = new TargetedResultRepository();

            int mtCounter = 0;
            int totalTargets = Targets.TargetList.Count;
            foreach (var massTag in this.Targets.TargetList)
            {
                mtCounter++;
                
                Run.CurrentMassTag = massTag;
                try
                {
                    this.TargetedWorkflow.Execute();
                    resultRepository.AddResult(this.TargetedWorkflow.Result);

                }
                catch (Exception ex)
                {
                    string errorString = "Error on MT\t" + massTag.ID + "\tchargeState\t" + massTag.ChargeState + "\t" + ex.Message + "\t" + ex.StackTrace;
                    ReportProcessingProgress(errorString,mtCounter);

                    throw;
                }

                string progressString = "Target " + mtCounter + " of " + totalTargets;

                ReportProcessingProgress(progressString, mtCounter);


            }

            ReportGeneralProgress("---- PROCESSING COMPLETE    ------------------------------------", 100);

            string outputFileName = this._resultsFolder + Path.DirectorySeparatorChar + Run.DatasetName + "_results.txt";
            backupResultsFileIfNecessary(Run.DatasetName, outputFileName);

            TargetedResultToTextExporter exporter = TargetedResultToTextExporter.CreateExporter(this._workflowParameters,outputFileName);
            exporter.ExportResults(resultRepository.Results);

            HandleAlignmentInfoFiles();
            finalizeRun();
        }

        #endregion

        #region Private Methods
        protected string getResultsFolder(string folder)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(folder);

            if (!dirinfo.Exists)
            {
                dirinfo.Create();
            }

            return dirinfo.FullName;

        }

        protected TargetCollection getMassTagTargets(string massTagFileName)
        {
            if (!File.Exists(massTagFileName))
            {

            }


            MassTagFromTextFileImporter importer = new MassTagFromTextFileImporter(massTagFileName);
            return importer.Import();
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
            if (_backgroundWorker==null)
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
                if (progressCounter % 500==0)
                {
                    Console.WriteLine(DateTime.Now + "\t" + reportString);
                }
                
            }
            else
            {
                int progressPercent = (int) (progressCounter*100/(double) Targets.TargetList.Count);

                _progressInfo.ProgressInfoString = reportString;
                _progressInfo.IsGeneralProgress = false;
                _progressInfo.Result = TargetedWorkflow.Result;
                _progressInfo.Time = DateTime.Now;
                _progressInfo.ChromatogramXYData = TargetedWorkflow.ChromatogramXYData;
                _progressInfo.MassSpectrumXYData = TargetedWorkflow.MassSpectrumXYData;
               
                _backgroundWorker.ReportProgress(progressPercent, _progressInfo);
            }

            if (progressCounter % 500 == 0)
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

        protected void InitializeRun(string dataset)
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
                PeakImporterFromText peakImporter = new PeakImporterFromText(possibleFilename1,_backgroundWorker);

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
            RunUtilities.AlignRunUsingAlignmentInfoInFiles(Run);

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
