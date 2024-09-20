using System;
using System.IO;
using DeconTools.Backend.Workflows;

namespace DeconConsole
{
    public static class Program
    {
        // Ignore Spelling: Slysz, Jaitly

        public const string PROGRAM_DATE = "September 19, 2024";

        public static int Main(string[] args)
        {
            Console.WriteLine("Starting..............");

            if (args == null || args.Length == 0)
            {
                ReportSyntax();
                System.Threading.Thread.Sleep(1500);
                return 1;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Not enough command line arguments; expecting 2 or 3");
                ReportSyntax();
                System.Threading.Thread.Sleep(1500);
                return 1;
            }

            if (args.Length > 3)
            {
                Console.WriteLine("Too many command line arguments; expecting 2 or 3");
                ReportSyntax();
                System.Threading.Thread.Sleep(1500);
                return 1;
            }

            var datasetFileOrDirectoryPath = args[0];
            var parameterFilePath = args[1];

            string outputDirectory = null;
            if (args.Length == 3)
            {
                outputDirectory = args[2];

                if (string.IsNullOrWhiteSpace(outputDirectory))
                {
                    return 2;
                }

                if (!Directory.Exists(outputDirectory))
                {
                    Console.WriteLine("Output directory doesn't exist. Will try to create one...");
                    try
                    {
                        Directory.CreateDirectory(outputDirectory);
                        Console.WriteLine("Output directory created. " + outputDirectory);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("ERROR! Couldn't create output directory (" + outputDirectory + "): " + exception.Message);
                        return 3;
                    }
                }
            }

            if (!IsFileValid(datasetFileOrDirectoryPath, out var datasetFileOrDirectory))
            {
                ReportFileProblem(datasetFileOrDirectoryPath, "Dataset file (or directory)");
                return 4;
            }

            if (!IsFileValid(parameterFilePath, out var parameterFile))
            {
                ReportFileProblem(parameterFilePath, "Parameter file");
                return 4;
            }

            Console.WriteLine("{0,-15} {1}", "Dataset:", PRISM.PathUtils.CompactPathString(datasetFileOrDirectoryPath, 120));
            if (!Path.IsPathRooted(datasetFileOrDirectoryPath))
            {
                Console.WriteLine("{0,-15} {1}", "Full path:", PRISM.PathUtils.CompactPathString(datasetFileOrDirectory.FullName, 120));
            }

            Console.WriteLine("{0,-15} {1}", "Parameter file:",  PRISM.PathUtils.CompactPathString(parameterFile.FullName, 120));

            try
            {
                var workflow = ScanBasedWorkflow.CreateWorkflow(datasetFileOrDirectoryPath, parameterFilePath, outputDirectory);
                workflow.Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("-------------------------------------------- ERROR! -------------------------------------------------");

                Console.WriteLine(ex.Message);
                Console.WriteLine(PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex));

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("** NOTE: See log file for more details.");
                Console.WriteLine();

                var errorCode = ex.Message.GetHashCode();
                if (errorCode != 0)
                    return errorCode;

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

        private static void ReportFileProblem(string fileOrDirectoryPath, string fileDescription)
        {
            Console.WriteLine("------------- ERROR! ---------------------");
            Console.WriteLine(string.IsNullOrEmpty(fileOrDirectoryPath)
                                  ? "The path of your " + fileDescription + " has no characters!"
                                  : fileDescription + " not found: " + fileOrDirectoryPath);
            Console.WriteLine();

            System.Threading.Thread.Sleep(1500);
        }

        private static bool IsFileValid(string datasetFileOrDirectoryPath, out FileSystemInfo fileOrDirectory)
        {
            try
            {
                var candidateFile = new FileInfo(datasetFileOrDirectoryPath);
                var candidateDirectory = new DirectoryInfo(datasetFileOrDirectoryPath);

                if (candidateFile.Exists)
                {
                    fileOrDirectory = candidateFile;
                    return true;
                }

                if (candidateDirectory.Exists)
                {
                    fileOrDirectory = candidateDirectory;
                    return true;
                }

                fileOrDirectory = null;
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dataset file/directory validation error: " + ex.Message);
                fileOrDirectory = null;
                return false;
            }
        }

        private static string GetAppVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " (" + PROGRAM_DATE + ")";
        }

        private static void ReportSyntax()
        {
            var exeName = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Console.WriteLine();
            Console.WriteLine("This program will accept 2 or 3 arguments (with spaces between).");
            Console.WriteLine("  Arg1 = instrument data filename");
            Console.WriteLine("  Arg2 = parameter filename");
            Console.WriteLine("  Arg3 = [optional] output directory  [Default = same as directory with the instrument data]");
            Console.WriteLine();
            Console.WriteLine("Example usage:");
            Console.WriteLine("  " + exeName + " QCDataset.raw SampleParameterFile.xml");
            Console.WriteLine();
            Console.WriteLine("Program written by Gordon Slysz for the Department of Energy (PNNL, Richland, WA)");
            Console.WriteLine("Incorporates previous code written by Gordon Anderson and Deep Jaitly");
            Console.WriteLine("Version: " + GetAppVersion());
            Console.WriteLine("Contact info: matthew.monroe@pnnl.gov or proteomics@pnnl.gov");
            Console.WriteLine("Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics");
            Console.WriteLine();

            System.Threading.Thread.Sleep(1000);
        }
    }
}
