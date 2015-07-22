using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using DeconTools.Workflows.Backend.Core;

namespace TargetedWorkflowConsole
{
    class Program
    {
        private const uint ENABLE_EXTENDED_FLAGS = 128U;

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private static int Main(string[] args)
        {
            // This command forces the console to not pause if the mouse is depressed
            SetConsoleMode(Process.GetCurrentProcess().MainWindowHandle, ENABLE_EXTENDED_FLAGS);

            // Parase the command line arguments
            if (args == null || args.Length == 0)
            {
                ReportSyntax();
                System.Threading.Thread.Sleep(1500);
                return -1;
            }

            if (args.Length == 2 || args.Length == 3)
            {
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
                    var workflowParameters = WorkflowParameters.CreateParameters(args[1]) as WorkflowExecutorBaseParameters;

                    if (args.Length == 3)
                        workflowParameters.TargetsFilePath = args[2];

                    TargetedWorkflowExecutorFactory.CreateTargetedWorkflowExecutor(workflowParameters, datasetPath).Execute();
                }
                catch (Exception ex)
                {
                    ReportError(ex);
                    System.Threading.Thread.Sleep(1500);
                    return ex.GetHashCode();
                }

                // Success
                return 0;
            }

            ReportSyntax();
            System.Threading.Thread.Sleep(1500);
            return -1;
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
        }
    }
}
