using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Utilities;
using DeconTools.Workflows.Backend.Utilities.Logging;


namespace IQ.Console
{
    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;


        static int Main(string[] args)
        {
            //IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            //SetConsoleMode(handle, ENABLE_EXTENDED_FLAGS);     // sets it so that keyboard use does not interrupt things.


            var options = new IqConsoleOptions();


            List<string> datasetList = new List<string>();


            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {

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
                    }

                    if (string.IsNullOrEmpty(options.OutputFolder))
                    {
                        options.OutputFolder = RunUtilities.GetDatasetParentFolder(currentDatasetPath);
                    }
                    
                    var executorParameters = GetExecutorParameters(options);

                    IqLogger.Log.Info("IQ analyzing dataset " + datasetCounter + " of " + numDatasets + ". Dataset = " + dataset);

                    TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, currentDatasetPath);
                    executor.Execute();

                }


            }

            return 0;



        }

        private static BasicTargetedWorkflowExecutorParameters GetExecutorParameters(IqConsoleOptions options)
        {
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.TargetsFilePath = options.TargetFile;
            executorParameters.ResultsFolder = options.OutputFolder + Path.DirectorySeparatorChar + "Results";
            executorParameters.LoggingFolder = options.OutputFolder + Path.DirectorySeparatorChar + "Logs";
            executorParameters.AlignmentInfoFolder = options.OutputFolder + Path.DirectorySeparatorChar + "AlignmentInfo";
            executorParameters.TargetedAlignmentIsPerformed = options.IsAlignmentPerformed;
            executorParameters.TargetsUsedForAlignmentFilePath = options.TargetFileForAlignment;
            executorParameters.WorkflowParameterFile = options.WorkflowParameterFile;
            executorParameters.TargetedAlignmentWorkflowParameterFile = options.AlignmentParameterFile;


            if (!string.IsNullOrEmpty(options.TemporaryWorkingFolder))
            {
                executorParameters.CopyRawFileLocal = true;

                executorParameters.FolderPathForCopiedRawDataset = options.TemporaryWorkingFolder;
                executorParameters.DeleteLocalDatasetAfterProcessing = true;
            }
            return executorParameters;
        }

        private static void ReportError(string message)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("=======================================================");
            System.Console.WriteLine("Error: " + message);
            System.Console.WriteLine("=======================================================");
            System.Console.WriteLine();
        }

        private static void ReportError(Exception ex)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("=======================================================");
            System.Console.WriteLine("Error: " + ex.Message);
            System.Console.WriteLine();
            System.Console.WriteLine("Stack trace:");
            System.Console.WriteLine(ex.StackTrace);
            System.Console.WriteLine("=======================================================");
            System.Console.WriteLine();
        }

        private static void ReportSyntax()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("This Commandline app requires two arguments.");
            System.Console.WriteLine("\tArg1 = dataset path");
            System.Console.WriteLine("\tArg2 = workflow executor parameter file (.xml)");

            System.Console.WriteLine();
        }
    }
}
