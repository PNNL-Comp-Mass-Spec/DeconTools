using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;

namespace TargetedWorkflowConsole
{
    internal static class Program
    {
        // Ignore Spelling: workflow

        private const uint ENABLE_EXTENDED_FLAGS = 128U;

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private static int Main(string[] args)
        {
            // This command forces the console to not pause if the mouse is depressed
            SetConsoleMode(Process.GetCurrentProcess().MainWindowHandle, ENABLE_EXTENDED_FLAGS);

            // Parse the command line arguments
            if (args == null || args.Length == 0)
            {
                ReportSyntax();
                System.Threading.Thread.Sleep(1500);
                return -1;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Not enough command line arguments; expecting 2 or 3");
                ReportSyntax();
                System.Threading.Thread.Sleep(1500);
                return -1;
            }

            if (args.Length > 3)
            {
                Console.WriteLine("Too many command line arguments; expecting 2 or 3");
                ReportSyntax();
                System.Threading.Thread.Sleep(1500);
                return -1;
            }

            var datasetPath = args[0];

            if (!File.Exists(datasetPath))
            {
                ReportError("Dataset file not found: " + datasetPath);
                System.Threading.Thread.Sleep(1500);
                return -5;
            }

            var fileInfo = new FileInfo(args[1]);
            if (!fileInfo.Exists)
            {
                ReportError("Parameter file not found: " + fileInfo.FullName);
                System.Threading.Thread.Sleep(1500);
                return -5;
            }

            try
            {
                if (!(WorkflowParameters.CreateParameters(fileInfo.FullName) is WorkflowExecutorBaseParameters workflowParameters))
                {
                    ReportError("Workflow parameters created from " + args[1] + " are null");
                    return -6;
                }

                if (args.Length == 3)
                {
                    workflowParameters.TargetsFilePath = args[2];
                }

                var workflowExecutor = TargetedWorkflowExecutorFactory.CreateTargetedWorkflowExecutor(workflowParameters, datasetPath);
                workflowExecutor.Execute();

                // Success
                return 0;
            }
            catch (Exception ex)
            {
                ReportError(ex);
                System.Threading.Thread.Sleep(1500);
                return ex.GetHashCode();
            }
        }

        private static void ReportError(string message)
        {
            Console.WriteLine();
            Console.WriteLine("=======================================================");
            Console.WriteLine("Error: " + message);
            Console.WriteLine("=======================================================");
            Console.WriteLine();
        }

        private static void ReportError(Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("=======================================================");
            Console.WriteLine("Error: " + ex.Message);
            Console.WriteLine();
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("=======================================================");
            Console.WriteLine();
        }

        private static void ReportSyntax()
        {
            Console.WriteLine();
            Console.WriteLine("This program requires two or three arguments.");
            Console.WriteLine("  Arg1 = dataset path (Thermo .raw file)");
            Console.WriteLine("  Arg2 = workflow executor parameter file (.xml)");
            Console.WriteLine("  Arg3 (optional) = targets file path (overrides targets file defined in the .xml file)");
            Console.WriteLine();
            Console.WriteLine("The input file peptide is specified by <TargetsFilePath> in the workflow executor parameter file");
            Console.WriteLine();
            Console.WriteLine("Supported tab-delimited text formats include:");
            Console.WriteLine("  _TopPIC_PrSMs.txt");
            Console.WriteLine("  " + string.Join("   ", MassTagFromMSAlignFileImporter.GetRequiredTopPICColumns()));
            Console.WriteLine();
            Console.WriteLine("  _MSAlign_ResultTable.txt");
            Console.WriteLine("  " + string.Join("   ", MassTagFromMSAlignFileImporter.GetRequiredMSAlignColumns()));
            Console.WriteLine();
        }
    }
}
