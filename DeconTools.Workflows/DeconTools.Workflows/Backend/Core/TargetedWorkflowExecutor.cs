
using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using DeconTools.Workflows.Backend.Utilities;
namespace DeconTools.Workflows.Backend.Core
{
    public abstract class TargetedWorkflowExecutor : WorkflowBase
    {

        protected List<string> _datasetPathList;
        protected string _loggingFileName;
        protected string _resultsFolder;
        protected TargetedWorkflowParameters _workflowParameters;

        #region Constructors
        public TargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters)
        {
            this.WorkflowParameters = parameters;
            InitializeWorkflow();
        }

        #endregion

        #region Properties
        public MassTagCollection MassTagsForTargetedAlignment { get; set; }

        public MassTagCollection MassTagsToBeTargeted { get; set; }

        public Run RunCurrent { get; set; }

        public WorkflowExecutorBaseParameters ExecutorParameters { get; set; }
        
        public TargetedAlignerWorkflowParameters TargetedAlignmentWorkflowParameters { get; set; }

        public TargetedWorkflow targetedWorkflow { get; set; }


        #endregion

        #region Public Methods

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

        protected MassTagCollection getMassTagTargets(string massTagFileName)
        {
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


        protected void reportProgress(string reportString)
        {
            Console.WriteLine(reportString);

            writeToLogFile(reportString);
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

        public override void Execute()
        {
            reportProgress(DateTime.Now + "\tstarted processing....");
            reportProgress("Parameters:\n" + this._workflowParameters.ToStringWithDetails());

            int datasetCounter = 0;
            foreach (var dataset in this._datasetPathList)
            {
                datasetCounter++;
                reportProgress(DateTime.Now + "\t---------\t " + dataset + "\t --- file " + datasetCounter + " of " + this._datasetPathList.Count);

                InitializeRun(dataset);

                if (RunCurrent == null)
                {
                    reportProgress("INFO - because run was NOT initialized... moving to next dataset.");
                    continue;
                }

                this.targetedWorkflow.Run = RunCurrent;

                TargetedResultRepository resultRepository = new TargetedResultRepository();

                int mtCounter = 0;

                foreach (var massTag in this.MassTagsToBeTargeted.MassTagList)
                {
                    mtCounter++;
                    if (mtCounter % 500 == 0)
                    {
                        reportProgress(DateTime.Now + "\t\t MassTag " + mtCounter + " of " + this.MassTagsToBeTargeted.MassTagList.Count);
                    }

                    RunCurrent.CurrentMassTag = massTag;
                    try
                    {
                        this.targetedWorkflow.Execute();
                        resultRepository.AddResult(this.targetedWorkflow.Result);

                    }
                    catch (Exception ex)
                    {
                        string errorString = "Error on MT\t" + massTag.ID + "\tchargeState\t" + massTag.ChargeState + "\t" + ex.Message + "\t" + ex.StackTrace;
                        reportProgress(errorString);

                        throw;
                    }

                   

                }
                reportProgress(DateTime.Now + "\t---- PROCESSING COMPLETE    ------------------------------------");

                string outputFileName = this._resultsFolder + Path.DirectorySeparatorChar + RunCurrent.DatasetName + "_results.txt";
                backupResultsFileIfNecessary(RunCurrent.DatasetName, outputFileName);

                TargetedResultToTextExporter exporter = createExporter(outputFileName);
                exporter.ExportResults(resultRepository.Results);

                finalizeRun();


            }
        }


        private void InitializeRun(string dataset)
        {
            string runFilename;


            if (this.ExecutorParameters.CopyRawFileLocal)
            {
                reportProgress(DateTime.Now + "\tStarted copying raw data to local folder: " + this.ExecutorParameters.FolderPathForCopiedRawDataset);



                FileAttributes attr = File.GetAttributes(dataset);

                DirectoryInfo sourceDirInfo;
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    sourceDirInfo = new DirectoryInfo(dataset);
                    runFilename = this.ExecutorParameters.FolderPathForCopiedRawDataset + Path.DirectorySeparatorChar + sourceDirInfo.Name;
                }
                else
                {
                    FileInfo fileinfo = new FileInfo(dataset);
                    sourceDirInfo = fileinfo.Directory;

                    runFilename = this.ExecutorParameters.FolderPathForCopiedRawDataset + Path.DirectorySeparatorChar + Path.GetFileName(dataset);
                }

                DirectoryInfo targetDirInfo = new DirectoryInfo(runFilename);

                FileUtilities.CopyAll(sourceDirInfo, targetDirInfo);
                reportProgress(DateTime.Now + "\tCopying complete.");
            }
            else
            {
                runFilename = dataset;
            }

            try
            {
                reportProgress(DateTime.Now + "\tPeak Loading started");
                RunCurrent = RunUtilities.CreateAndAlignRun(runFilename, null);
                reportProgress(DateTime.Now + "\tPeak Loading completed");

            }
            catch (Exception ex)
            {
                string errorMessage = DateTime.Now + "\t---------\t " + dataset + "\t-------------ERROR! " + ex.Message + "\t" + ex.StackTrace;
                reportProgress(errorMessage);
            }
            return;
        }



        protected virtual TargetedResultToTextExporter createExporter(string outputFileName)
        {
            throw new NotImplementedException();
        }


        protected void finalizeRun()
        {
            if (this.ExecutorParameters.CopyRawFileLocal && this.ExecutorParameters.DeleteLocalDatasetAfterProcessing)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(RunCurrent.Filename);
                dirInfo.Delete(true);

            }


            RunCurrent.Close();
            RunCurrent = null;
            GC.Collect();


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
