using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using DeconTools.Backend.Workflows;

namespace DeconConsole
{
    public class Program
    {

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;


        public static void Main(string[] args)
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            SetConsoleMode(handle, ENABLE_EXTENDED_FLAGS);

            Console.WriteLine("Starting..............");

            if (args.Length > 3)
            {
                ReportSyntax();
                return;
            }

            string filename = args[0];
            string parameterFilename = args[1];


            string outputFolder = null;
            if (args.Length == 3)
            {
                outputFolder = args[2];

                if (String.IsNullOrWhiteSpace(outputFolder))
                {
                    return;
                }
                
                if (!Directory.Exists(outputFolder))
                {

                    Console.WriteLine("Output folder doesn't exist. Will try to create one...");
                    try
                    {
                        Directory.CreateDirectory(outputFolder);
                        Console.WriteLine("Output folder created. " + outputFolder);

                    }
                    catch (Exception exception)
                    {

                        Console.WriteLine("ERROR! Couldn't create output folder. Message: " + exception.Message);
                        return;

                    }
                }
                
                if (!IsFileValid(outputFolder))
                {
                    
                    return;
                }

            }

            if (!IsFileValid(filename))
            {
                ReportFileProblem(filename);
                return;
            }
            if (!IsFileValid(parameterFilename))
            {
                ReportFileProblem(parameterFilename);
                return;
            }

            Console.WriteLine("Dataset = " + filename);

            try
            {
                var workflow = ScanBasedWorkflow.CreateWorkflow(filename, parameterFilename, outputFolder);
                workflow.Execute();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("****************************************");
            Console.WriteLine("****************************************");
            Console.WriteLine("************** SUCCESS! ****************");
            Console.WriteLine("****************************************");
            Console.WriteLine("****************************************");
            Console.WriteLine();


        }

        private static void ReportFileProblem(string filename)
        {
            Console.WriteLine("------------- ERROR! ---------------------");
            Console.WriteLine(string.IsNullOrEmpty(filename)
                                  ? "The path of your file has no characters!"
                                  : "Inputted filename does not exist");
            Console.WriteLine();
        }

        private static bool IsFileValid(string filename)
        {
            if (Directory.Exists(filename)) return true;

            if (File.Exists(filename)) return true;
            return false;
        }

        private static void ReportSyntax()
        {
            Console.WriteLine("This Commandline app requires two arguments (with spaces between).");
            Console.WriteLine("\tArg1 = filename");
            Console.WriteLine("\tArg2 = parameter filename");
            Console.WriteLine("\tArg3 = [optional] output folder  [Default = same as raw data folder]");
            Console.WriteLine();
        }
    }
}
