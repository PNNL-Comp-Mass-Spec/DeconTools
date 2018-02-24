using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Utilities;
using PRISM;


namespace IQ.Console
{
    class Program
    {

        static int Main(string[] args)
        {

            var options = new IqConsoleOptions();

            var datasetList = new List<string>();


            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return -1;
            }

            var inputFile = options.InputFile;



            var inputFileIsAListOfDatasets = inputFile.ToLower().EndsWith(".txt");
            if (inputFileIsAListOfDatasets)
            {
                using (var reader = new StreamReader(inputFile))
                {

                    while (reader.Peek() != -1)
                    {

                        var datsetName = reader.ReadLine();
                        datasetList.Add(datsetName);

                    }
                }
            }
            else
            {


                datasetList.Add(options.InputFile);

            }



                {
                }


                {
                }


                IqLogger.Log.Info("IQ analyzing dataset " + datasetCounter + " of " + numDatasets + ". Dataset = " + dataset);


                {
                    {

                        {
                        }





                }
            }

            return 0;



        }

        private static BasicTargetedWorkflowExecutorParameters GetExecutorParameters(IqConsoleOptions options)
        {
            var executorParameters = new BasicTargetedWorkflowExecutorParameters
            {
                TargetsFilePath = options.TargetFile,
                OutputFolderBase = options.OutputFolder,
                TargetedAlignmentIsPerformed = options.IsAlignmentPerformed,
                WorkflowParameterFile = options.WorkflowParameterFile,
                TargetedAlignmentWorkflowParameterFile = options.AlignmentParameterFile,
                IsMassAlignmentPerformed = options.IsMassAlignmentPerformed,
                IsNetAlignmentPerformed = options.IsNetAlignmentPerformed,
                ReferenceTargetsFilePath = options.ReferenceTargetFile,
                TargetType = options.UseInputScan
                                 ? Globals.TargetType.LcmsFeature
                                 : Globals.TargetType.DatabaseTarget
            };


            if (!string.IsNullOrEmpty(options.TemporaryWorkingFolder))
            {
                executorParameters.CopyRawFileLocal = true;

                executorParameters.FolderPathForCopiedRawDataset = options.TemporaryWorkingFolder;
                executorParameters.DeleteLocalDatasetAfterProcessing = true;
            }

            if (!string.IsNullOrWhiteSpace(options.WorkflowParameterFile))
            {
                // Load workflow parameter file to update the options

                var parameterList = LoadParameters(options.WorkflowParameterFile);

                executorParameters.CopyRawFileLocal = GetParameter(parameterList, "CopyRawFileLocal", executorParameters.CopyRawFileLocal);
                executorParameters.DeleteLocalDatasetAfterProcessing = GetParameter(parameterList, "DeleteLocalDatasetAfterProcessing", executorParameters.DeleteLocalDatasetAfterProcessing);

                // Skip: FileContainingDatasetPaths
                executorParameters.FolderPathForCopiedRawDataset = GetParameter(parameterList, "FolderPathForCopiedRawDataset", executorParameters.FolderPathForCopiedRawDataset);

                // Skip: LoggingFolder

                executorParameters.TargetsFilePath = GetParameter(parameterList, "TargetsFilePath", executorParameters.TargetsFilePath);

                var targetType = GetParameter(parameterList, "TargetType", "DatabaseTarget");
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
            Check.Require(File.Exists(workflowParameterFilePath), "Workflow parameter file could not be loaded. File not found: " + workflowParameterFilePath);
            var doc = XDocument.Load(workflowParameterFilePath);
            var xElement = doc.Element("WorkflowParameters");
            if (xElement == null)
            {
                return new Dictionary<string, string>();
            }
            
            var query = xElement.Elements();

            var parameterTableFromXML = new Dictionary<string, string>();
            foreach (var item in query)
            {
                var paramName = item.Name.ToString();
                var paramValue = item.Value;

                if (!parameterTableFromXML.ContainsKey(paramName))
                {
                    parameterTableFromXML.Add(paramName, paramValue);
                }
            }

            return parameterTableFromXML;
        }

        /// <summary>
        /// Process the given dataset
        /// </summary>
        /// <param name="options"></param>
        /// <param name="datasetNameOrPath"></param>
        /// <returns>True if success or a non fatal error; false if a fatal error</returns>
        private static bool ProcessDataset(IqConsoleOptions options, string datasetNameOrPath)
        {
            try
            {
                string currentDatasetPath;

                var candidatefile = new FileInfo(datasetNameOrPath);
                if (candidatefile.Exists)
                {
                    currentDatasetPath = candidatefile.FullName;
                }
                else
                {

                    var datasetPathHasSepChar = datasetNameOrPath.Contains(Path.PathSeparator.ToString());

                    if (datasetPathHasSepChar)
                    {
                        IqLogger.LogWarning("Dataset file not found: " + datasetNameOrPath);
                        // Non-fatal error; return true
                        return true;
                    }

                    if (string.IsNullOrEmpty(options.TemporaryWorkingFolder))
                    {
                        IqLogger.LogWarning("Trying to grab .raw file from DMS, but no temporary working folder was declared. Use option -f");
                        return false;

                    }

                    if (string.IsNullOrEmpty(options.OutputFolder))
                    {
                        options.OutputFolder = options.TemporaryWorkingFolder;
                    }

                    var datasetutil = new DatasetUtilities();

                    //TODO: figure out how to do this while supporting other file types
                    currentDatasetPath = Path.Combine(datasetutil.GetDatasetPath(datasetNameOrPath), datasetNameOrPath + ".raw");

                    if (currentDatasetPath.ToLower().Contains("purged"))
                    {
                        IqLogger.LogWarning("Cannot process a purged dataset; copy the .raw file locally");
                        // Non-fatal error; return true
                        return true;
                    }
                }

                if (!File.Exists(currentDatasetPath))
                {
                    IqLogger.LogWarning("!!!!!!!!! Dataset not found! Dataset path = " + currentDatasetPath);
                    // Non-fatal error; return true
                    return true;
                }

                if (string.IsNullOrEmpty(options.OutputFolder))
                {
                    options.OutputFolder = RunUtilities.GetDatasetParentFolder(currentDatasetPath);
                }

                var executorParameters = GetExecutorParameters(options);

                if (!string.IsNullOrWhiteSpace(options.TargetsFile))
                {
                    // Override any targets file defined with the XML file
                    executorParameters.TargetsFilePath = options.TargetsFile;
                }

                if (string.IsNullOrWhiteSpace(executorParameters.TargetsFilePath))
                {
                    IqLogger.LogWarning("Targets file must be defined, either via argument -t or in the Workflow parameter file via the TargetsFilePath entry");
                    return false;
                }

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

                        var workflowAssigner = new IqWorkflowAssigner();
                        workflowAssigner.AssignWorkflowToParent(parentWorkflow, iqTarget);
                        workflowAssigner.AssignWorkflowToChildren(childWorkflow, iqTarget);
                    }

                    executor.Execute();

                    run.Dispose();
                }

                return true;
            }
            catch (Exception ex)
            {
                string datasetNameToShow;
                if (datasetNameOrPath.Contains(Path.PathSeparator.ToString()))
                    datasetNameToShow = clsPathUtils.CompactPathString(datasetNameOrPath, 60);
                else
                    datasetNameToShow = datasetNameOrPath;

                IqLogger.LogError("Error processing dataset " + datasetNameToShow, ex);
                return false;
            }

        }

    }
}
