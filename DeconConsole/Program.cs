using System;
using System.IO;
using DeconTools.Backend.Workflows;

namespace DeconConsole
{
    public class Program
    {

        public const string PROGRAM_DATE = "January 26, 2015";
        
        static int Main(string[] args)
        {
            Console.WriteLine("Starting..............");

            if (args.Length < 2 || args.Length > 3)
            {
                ReportSyntax();
                return 1;
            }

            string filename = args[0];
            string parameterFilename = args[1];

            string outputFolder = null;
            if (args.Length == 3)
            {
                outputFolder = args[2];

                if (String.IsNullOrWhiteSpace(outputFolder))
                {
                    return 2;
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
                        return 3;

                    }
                }

            }

            if (!IsFileValid(filename))
            {
                ReportFileProblem(filename, "Dataset file (or folder)");
                return 4;
            }
            if (!IsFileValid(parameterFilename))
            {
                ReportFileProblem(parameterFilename, "Parameter file");
                return 4;
            }

            Console.WriteLine("Dataset = " + filename);

            try
            {
                var workflow = ScanBasedWorkflow.CreateWorkflow(filename, parameterFilename, outputFolder);
                workflow.Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("-------------------------------------------- ERROR! -------------------------------------------------");

                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("** NOTE: See log file for more details.");
                Console.WriteLine();

                int errorCode = ex.Message.GetHashCode();
                if (errorCode != 0)
                    return errorCode;
                else
                    return -1;
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("****************************************");
            Console.WriteLine("****************************************");
            Console.WriteLine("************** SUCCESS! ****************");
            Console.WriteLine("****************************************");
            Console.WriteLine("****************************************");
            Console.WriteLine();

            return 0;
        }

        private static void ReportFileProblem(string filename, string fileDescription)
        {
            Console.WriteLine("------------- ERROR! ---------------------");
            Console.WriteLine(string.IsNullOrEmpty(filename)
                                  ? "The path of your " + fileDescription + " has no characters!"
                                  : fileDescription + " not found: " + filename);
            Console.WriteLine();
        }

        private static bool IsFileValid(string filename)
        {
            if (Directory.Exists(filename)) return true;

            if (File.Exists(filename)) return true;
            return false;
        }

        private static string GetAppVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " (" + PROGRAM_DATE + ")";
        }

        private static void ReportSyntax()
        {
            string exeName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Console.WriteLine();
            Console.WriteLine("This program will accept 2 or 3 arguments (with spaces between).");
            Console.WriteLine("  Arg1 = filename");
            Console.WriteLine("  Arg2 = parameter filename");
            Console.WriteLine("  Arg3 = [optional] output folder  [Default = same as raw data folder]");
            Console.WriteLine();
            Console.WriteLine("Example usage:");
            Console.WriteLine("  " + exeName + " QCDataset.raw SampleParameterFile.xml");
            Console.WriteLine();
            Console.WriteLine("Program written by Gordon Slysz for the Department of Energy (PNNL, Richland, WA)");
            Console.WriteLine("Incorporates previous code written by Gordon Anderson and Deep Jaitly");
            Console.WriteLine("Version: " + GetAppVersion());
			Console.WriteLine("Contact info: matthew.monroe@pnnl.gov or samuel.payne@pnnl.gov");
			Console.WriteLine("Website: http://omics.pnl.gov/software or http://panomics.pnnl.gov");
            Console.WriteLine();

            System.Threading.Thread.Sleep(1000);
        }
    }
}
