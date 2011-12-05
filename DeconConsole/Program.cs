using System;
using System.IO;
using DeconTools.Backend.Workflows;

namespace DeconConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting..............");

            if (args.Length != 2)
            {
                ReportSyntax();
                return;
            }

            string filename = args[0];
            string parameterFilename = args[1];

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
                var workflow = ScanBasedWorkflow.CreateWorkflow(filename, parameterFilename);
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
            Console.WriteLine();
        }
    }
}
