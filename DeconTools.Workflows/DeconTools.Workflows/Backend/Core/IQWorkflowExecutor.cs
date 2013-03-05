using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    [Obsolete("Do not use this.... will delete. Use 'IqExecutor'",true)]
    public class IqWorkflowExecutor
    {
        private BackgroundWorker _backgroundWorker;
        private string _resultsFileName;
        private string _resultsFolder;
        private TargetedWorkflowExecutorProgressInfo _progressInfo = new TargetedWorkflowExecutorProgressInfo();
        private string _loggingFileName;

        #region Constructors
        public IqWorkflowExecutor(Run run, IqWorkflow workflow, WorkflowExecutorBaseParameters parameters, BackgroundWorker backgroundWorker = null)
        {



            _backgroundWorker = backgroundWorker;
            Parameters = parameters;

            _resultsFolder = GetResultsFolder(Parameters.ResultsFolder);

        }

        #endregion

        #region Properties

        private Run _run;
        public Run Run
        {
            get { return _run; }
            set
            {
                _run = value;
                if (_run != null) DatasetPath = _run.DataSetPath;

            }
        }

        public WorkflowExecutorBaseParameters Parameters { get; set; }

        public ResultExporter ResultExporter { get; set; }


        public string DatasetPath { get; set; }

        #endregion

        #region Public Methods


        public List<IqResult> Results { get; set; }


        public List<IqTarget> Targets { get; set; }



        public void Execute(IEnumerable<IqTarget> targets)
        {



            foreach (var target in targets)
            {
                Run = target.GetRun();

                if (!RunIsInitialized)
                {
                    //create Run; load _peaks data; do alignment if desired
                    InitializeRun(DatasetPath);
                }



                if (target.HasChildren())
                {
                    Execute(target.ChildTargets());
                }

                var result = target.DoWorkflow();
                Results.Add(result);


                if (!target.HasParent)
                {
                    if (IsDataExported)
                    {
                        ExportResults();
                    }
                }

            }

        }

        public void InitializeRun(string dataset)
        {
            string runFilename;

            if (Parameters.CopyRawFileLocal)
            {
                ReportGeneralProgress("Started copying raw data to local folder: " + Parameters.FolderPathForCopiedRawDataset);

                FileAttributes attr = File.GetAttributes(dataset);

                DirectoryInfo sourceDirInfo;
                DirectoryInfo targetDirInfo;
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    sourceDirInfo = new DirectoryInfo(dataset);
                    runFilename = Parameters.FolderPathForCopiedRawDataset + Path.DirectorySeparatorChar + sourceDirInfo.Name;
                    targetDirInfo = new DirectoryInfo(runFilename);
                    FileUtilities.CopyAll(sourceDirInfo, targetDirInfo);
                    ReportGeneralProgress("Copying complete.");
                }
                else
                {
                    FileInfo fileinfo = new FileInfo(dataset);
                    sourceDirInfo = fileinfo.Directory;
                    runFilename = Parameters.FolderPathForCopiedRawDataset + Path.DirectorySeparatorChar + Path.GetFileName(dataset);

                    targetDirInfo = new DirectoryInfo(Parameters.FolderPathForCopiedRawDataset);

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
            //RunFactory rf = new RunFactory();
            //Run = rf.CreateRun(runFilename);

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
            //CopyAlignmentInfoIfExists();

            //check and load chrom source data (_peaks.txt)
            bool peaksFileExists = CheckForPeaksFile();
            if (!peaksFileExists)
            {
                ReportGeneralProgress("Creating _Peaks.txt file for extracted ion chromatogram (XIC) source data ... takes 1-5 minutes");

                CreatePeaksForChromSourceData();
                ReportGeneralProgress("Done creating _Peaks.txt file");
            }
            else
            {
                ReportGeneralProgress("Using existing _Peaks.txt file");
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
                ReportGeneralProgress("CRITICAL FAILURE. Chrom source data (_peaks.txt) file not loaded.");
                return;
            }

            // Grab the primary LC Scan numbers if they are not already filled out
            if (!Run.PrimaryLcScanNumbers.Any())
            {
                Run.PrimaryLcScanNumbers = RunUtilities.FindPrimaryLcScanNumbers(this.Run.ResultCollection.MSPeakResultList);
            }

            ReportGeneralProgress("Peak Loading complete.");
            return;
        }

        private void CreatePeaksForChromSourceData()
        {
            var parameters = new PeakDetectAndExportWorkflowParameters();

            parameters.PeakBR = Parameters.ChromGenSourceDataPeakBR;
            parameters.PeakFitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;
            parameters.SigNoiseThreshold = Parameters.ChromGenSourceDataSigNoise;
            parameters.ProcessMSMS = Parameters.ChromGenSourceDataProcessMsMs;
            parameters.IsDataThresholded = Parameters.ChromGenSourceDataIsThresholded;

            var peakCreator = new PeakDetectAndExportWorkflow(this.Run, parameters, _backgroundWorker);
            peakCreator.Execute();
        }


        private void CopyAlignmentInfoIfExists()
        {
            if (String.IsNullOrEmpty(Parameters.AlignmentInfoFolder)) return;

            DirectoryInfo dirInfo = new DirectoryInfo(Parameters.AlignmentInfoFolder);

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



        protected void ReportGeneralProgress(string generalProgressString, int progressPercent = 0)
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

        protected void writeToLogFile(string stringToWrite)
        {

            if (!string.IsNullOrEmpty(_loggingFileName))
            {
                using (StreamWriter sw = new StreamWriter(new System.IO.FileStream(this._loggingFileName, System.IO.FileMode.Append,
                              System.IO.FileAccess.Write, System.IO.FileShare.Read)))
                {
                    sw.AutoFlush = true;
                    sw.WriteLine(stringToWrite);
                    sw.Flush();

                }
            }

        }

        private bool CheckForPeaksFile()
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


        protected void ReportProcessingProgress(string reportString, int progressCounter, IqResult result)
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
                //TODO: fix this
                int progressPercent = 0;
                _backgroundWorker.ReportProgress(progressPercent, result);
            }

            if (progressCounter % 100 == 0)
            {
                writeToLogFile(DateTime.Now + "\t" + reportString);
            }

        }


        private void ExportResults()
        {
            ResultExporter.WriteOutResults(_resultsFileName, Results);

        }

        protected bool IsDataExported { get; set; }

        protected virtual string GetOutputFileName()
        {
            return _resultsFolder + Path.DirectorySeparatorChar + Run.DatasetName + "_results.txt";
        }


        #endregion

        #region Private Methods
        protected string GetResultsFolder(string folder)
        {
            string outputFolder;

            if (string.IsNullOrEmpty(folder))
            {
                outputFolder = DatasetPath + Path.DirectorySeparatorChar + "Results";
            }
            else
            {
                outputFolder = folder;
            }


            DirectoryInfo dirinfo = new DirectoryInfo(outputFolder);

            if (!dirinfo.Exists)
            {
                dirinfo.Create();
            }

            return dirinfo.FullName;

        }

        public bool RunIsInitialized
        {
            get
            {
                if (Run.ResultCollection.MSPeakResultList.Count == 0)
                {
                    return false;
                }

                if (!DatasetPath.Contains(Run.DataSetPath))
                {
                    return false;
                }

                return true;
            }
        }


        #endregion

    }
}
