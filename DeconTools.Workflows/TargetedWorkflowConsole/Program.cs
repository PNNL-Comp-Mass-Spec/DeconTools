using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Configuration;
using System.Xml.Linq;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Utilities;


namespace IQ.Console
{
    class Program
    {

        static int Main(string[] args)
        {

            var options = new IqConsoleOptions();

            List<string> datasetList = new List<string>();


            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return -1;
            }

            string inputFile = options.InputFile;



            bool inputFileIsAListOfDatasets = inputFile.ToLower().EndsWith(".txt");
            if (inputFileIsAListOfDatasets)
            {
                using (StreamReader reader = new StreamReader(inputFile))
                {

                    while (reader.Peek() != -1)
                    {

                        string datsetName = reader.ReadLine();
                        datasetList.Add(datsetName);

                    }
                }
            }
            else
            {


                datasetList.Add(options.InputFile);

            }


            int numDatasets = datasetList.Count;
            int datasetCounter = 0;

            foreach (var dataset in datasetList)
            {
                datasetCounter++;

                bool datasetNameContainsPath = dataset.Contains("\\");

                string currentDatasetPath = dataset;

                if (datasetNameContainsPath)
                {
                    currentDatasetPath = dataset;
                }
                else
                {

                    if (string.IsNullOrEmpty(options.TemporaryWorkingFolder))
                    {
                        IqLogger.Log.Fatal("Trying to grab .raw file from DMS, but no temporary working folder was declared. Use option -f. ");
                        break;

                    }


                    if (string.IsNullOrEmpty(options.OutputFolder))
                    {
                        options.OutputFolder = options.TemporaryWorkingFolder;
                    }



                    var datasetutil = new DatasetUtilities();

                    //TODO: figure out how to do this while supporting other file types
                    currentDatasetPath = datasetutil.GetDatasetPath(dataset) + "\\" + dataset + ".raw";

                    if (currentDatasetPath.ToLower().Contains("purged"))
                    {
                        currentDatasetPath = datasetutil.GetDatasetPathArchived(dataset) + "\\" + dataset + ".raw";
                    }
                }


                if (!File.Exists(currentDatasetPath))
                {
                    IqLogger.Log.Fatal("!!!!!!!!! Dataset not found! Dataset path = " + currentDatasetPath);
                    return -2;
                }

                if (string.IsNullOrEmpty(options.OutputFolder))
                {
                    options.OutputFolder = RunUtilities.GetDatasetParentFolder(currentDatasetPath);
                }

                var executorParameters = GetExecutorParameters(options);

                IqLogger.Log.Info("IQ analyzing dataset " + datasetCounter + " of " + numDatasets + ". Dataset = " + dataset);


                if (options.UseOldIq)
                {
                    TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters,
                                                                                          currentDatasetPath);

                    //executor.Targets.TargetList = executor.Targets.TargetList.Take(10).ToList();
                    executor.MsgfFdrScoreCutoff = 0.1;
                    executor.Execute();
                }
                else
                {
                    var run = new RunFactory().CreateRun(currentDatasetPath);
                    var executor = new IqExecutor(executorParameters, run);

                    executor.LoadAndInitializeTargets(executorParameters.TargetsFilePath);
                    executor.SetupMassAndNetAlignment();
                    executor.DoAlignment();


                    foreach (var iqTarget in executor.Targets)
                    {
                        TargetedWorkflowParameters workflowParameters = new O16O18WorkflowParameters();

                        if (iqTarget.ElutionTimeTheor > 0.7 || iqTarget.ElutionTimeTheor < 0.15)
                        {

                            //TODO: remove the hard-coded value
                            workflowParameters.ChromNETTolerance = 0.1;
                        }
                        else
                        {
                            //TODO: remove the hard-coded value
                            workflowParameters.ChromNETTolerance = 0.025;
                        }

                        if (run.MassAlignmentInfo != null && run.MassAlignmentInfo.StdevPpmShiftData > 0)
                            workflowParameters.ChromGenTolerance = run.MassAlignmentInfo.StdevPpmShiftData * 3;

                        //define workflows for parentTarget and childTargets
                        // Note: this is currently hard-coded to user O16O18IqWorkflow
                        var parentWorkflow = new ChromPeakDeciderIqWorkflow(run, workflowParameters);
                        var childWorkflow = new O16O18IqWorkflow(run, workflowParameters);

                        IqWorkflowAssigner workflowAssigner = new IqWorkflowAssigner();
                        workflowAssigner.AssignWorkflowToParent(parentWorkflow, iqTarget);
                        workflowAssigner.AssignWorkflowToChildren(childWorkflow, iqTarget);
                    }

                    executor.Execute();

                    run.Dispose();
                    run = null;
                }
            }

            return 0;



        }

        private static BasicTargetedWorkflowExecutorParameters GetExecutorParameters(IqConsoleOptions options)
        {
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.TargetsFilePath = options.TargetFile;
            executorParameters.OutputFolderBase = options.OutputFolder;
            executorParameters.TargetedAlignmentIsPerformed = options.IsAlignmentPerformed;
            executorParameters.WorkflowParameterFile = options.WorkflowParameterFile;
            executorParameters.TargetedAlignmentWorkflowParameterFile = options.AlignmentParameterFile;
            executorParameters.IsMassAlignmentPerformed = options.IsMassAlignmentPerformed;
            executorParameters.IsNetAlignmentPerformed = options.IsNetAlignmentPerformed;
            executorParameters.ReferenceTargetsFilePath = options.ReferenceTargetFile;
            
            executorParameters.TargetType = options.UseInputScan
                                                ? Globals.TargetType.LcmsFeature
                                                : Globals.TargetType.DatabaseTarget;

            if (!string.IsNullOrEmpty(options.TemporaryWorkingFolder))
            {
                executorParameters.CopyRawFileLocal = true;

                executorParameters.FolderPathForCopiedRawDataset = options.TemporaryWorkingFolder;
                executorParameters.DeleteLocalDatasetAfterProcessing = true;
            }

            if (!string.IsNullOrWhiteSpace(options.WorkflowParameterFile))
            {
                // Load workflow parameter file to update the options

                Dictionary<string, string> parameterList = LoadParameters(options.WorkflowParameterFile);

                executorParameters.CopyRawFileLocal = GetParameter(parameterList, "CopyRawFileLocal", executorParameters.CopyRawFileLocal);
                executorParameters.DeleteLocalDatasetAfterProcessing = GetParameter(parameterList, "DeleteLocalDatasetAfterProcessing", executorParameters.DeleteLocalDatasetAfterProcessing);

                // Skip: FileContainingDatasetPaths
                executorParameters.FolderPathForCopiedRawDataset = GetParameter(parameterList, "FolderPathForCopiedRawDataset", executorParameters.FolderPathForCopiedRawDataset);

                // Skip: LoggingFolder

                executorParameters.TargetsFilePath = GetParameter(parameterList, "TargetsFilePath", executorParameters.TargetsFilePath);

                string targetType = GetParameter(parameterList, "TargetType", "DatabaseTarget");
                if (string.Equals(targetType, "LcmsFeature", StringComparison.CurrentCultureIgnoreCase))
                    executorParameters.TargetType = Globals.TargetType.LcmsFeature;

                if (string.Equals(targetType, "DatabaseTarget", StringComparison.CurrentCultureIgnoreCase))
                    executorParameters.TargetType = Globals.TargetType.DatabaseTarget;


                executorParameters.OutputFolderBase = GetParameter(parameterList, "ResultsFolder", executorParameters.OutputFolderBase);

                executorParameters.WorkflowParameterFile = GetParameter(parameterList, "WorkflowParameterFile", executorParameters.WorkflowParameterFile);

                // ToDo: Use this: <WorkflowType>TopDownTargetedWorkflowExecutor1</WorkflowType>

                executorParameters.IsMassAlignmentPerformed = GetParameter(parameterList, "IsMassAlignmentPerformed", executorParameters.IsMassAlignmentPerformed);
                executorParameters.IsNetAlignmentPerformed = GetParameter(parameterList, "IsNetAlignmentPerformed", executorParameters.IsNetAlignmentPerformed);                
                executorParameters.ReferenceTargetsFilePath = GetParameter(parameterList, "ReferenceTargetFile", executorParameters.ReferenceTargetsFilePath);
                
            }
            return executorParameters;
        }

        private static bool GetParameter(IDictionary<string, string> parameterList, string parameterName, bool valueIfMissing)
        {
            var boolString = GetParameter(parameterList, parameterName, string.Empty);

            if (string.IsNullOrWhiteSpace(boolString))
                return valueIfMissing;
            
            bool boolValue;
            if (bool.TryParse(boolString, out boolValue))
                return boolValue;

            return valueIfMissing;

        }

        private static string GetParameter(IDictionary<string, string> parameterList, string parameterName, string valueIfMissing)
        {
            string value;

            if (parameterList.TryGetValue(parameterName, out value))
                return value;

            return valueIfMissing;
        }

        private static Dictionary<string, string> LoadParameters(string workflowParameterFilePath)
        {
            Check.Require(File.Exists(workflowParameterFilePath), "Workflow parameter file could not be loaded. File not found.");
            XDocument doc = XDocument.Load(workflowParameterFilePath);
            var query = doc.Element("WorkflowParameters").Elements();

            var parameterTableFromXML = new Dictionary<string, string>();
            foreach (var item in query)
            {
                string paramName = item.Name.ToString();
                string paramValue = item.Value;

                if (!parameterTableFromXML.ContainsKey(paramName))
                {
                    parameterTableFromXML.Add(paramName, paramValue);
                }
            }

            return parameterTableFromXML;
        }
    }
}
