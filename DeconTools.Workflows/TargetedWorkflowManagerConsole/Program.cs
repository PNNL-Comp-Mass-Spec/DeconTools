using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Utilities;


namespace TargetedWorkflowManagerConsole
{
    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;


        static void Main(string[] args)
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            SetConsoleMode(handle, ENABLE_EXTENDED_FLAGS);     // sets it so that keyboard use does not interrupt things.


            if (args == null || args.Length == 0)
            {
                ReportSyntax();
            }

            if (args.Length == 2)
            {
                string parameterFile = args[1];
                string fileContainingDatasetNames = args[0];

                BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
                executorParameters.LoadParameters(args[1]);

                

                using (StreamReader reader = new StreamReader(fileContainingDatasetNames))
                {
                    int datasetCounter = 0;
                    while (reader.Peek() != -1)
                    {
                        datasetCounter++;
                        string datsetName = reader.ReadLine();

                        bool datasetNameContainsPath = datsetName.Contains("\\");

                        string currentDatasetPath = datsetName;

                        if (datasetNameContainsPath)
                        {
                            currentDatasetPath = datsetName;
                        }
                        else
                        {
                            var datasetutil = new DatasetUtilities();

                            currentDatasetPath = datasetutil.GetDatasetPath(datsetName) + "\\" + datsetName + ".raw";

                            if (currentDatasetPath.Contains("purged"))
                            {
                                currentDatasetPath = datasetutil.GetDatasetPathArchived(datsetName) + "\\" + datsetName + ".raw";
                            }
                        }

                        ProcessStartInfo processStartInfo = new ProcessStartInfo();
                        //processStartInfo.UseShellExecute = false;

                        processStartInfo.FileName = @"TargetedWorkflowConsole.exe";
                        processStartInfo.Arguments = currentDatasetPath + " " + parameterFile;

                        try
                        {
                            Console.WriteLine("Working on dataset " + datasetCounter + "\t" + currentDatasetPath);
                            Process p = Process.Start(processStartInfo);
                            p.WaitForExit();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("!!!!!!!!!!!!  Dataset FAILED. See log file for details. Error:");
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);

                        }

                    }

                }

            }
            else
            {
                ReportSyntax();
            }

        }

        private static void reportFileProblem(string p)
        {
            Console.WriteLine("ERROR. Inputted parameter filename does not exist. Inputted file name = " + p);
        }

        private static void ReportSyntax()
        {
            Console.WriteLine();
            Console.WriteLine("This Commandline app requires two arguments.");
            Console.WriteLine("\tArg1 = a text file containing list of paths of datasets to be analyzed");
            Console.WriteLine("\tArg2 = workflow executor parameter file (.xml)");

            Console.WriteLine();
        }
    }
}
