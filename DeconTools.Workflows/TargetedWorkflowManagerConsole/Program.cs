using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using DeconTools.Workflows.Backend.Utilities;

namespace IQ.ConsoleManager
{
    public class Program
    {

        public static void Main(string[] args)
        {

            if (args == null || args.Length == 0)
            {
                ReportSyntax();
            }

        	string targetFile = GetTargetFile(args);

            if (args.Length == 2 || !String.IsNullOrEmpty(targetFile))
            {
                string parameterFile = args[1];
                string fileContainingDatasetNames = args[0];
                
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

                            if (currentDatasetPath.ToLower().Contains("purged"))
                            {
                                string tempPathWhileArchiveIsDown = @"\\protoapps\UserData\Slysz\Data\Yellowstone\RawData";

                                currentDatasetPath = tempPathWhileArchiveIsDown + "\\" + datsetName + ".raw";

                                //currentDatasetPath = datasetutil.GetDatasetPathArchived(datsetName) + "\\" + datsetName + ".raw";
                            }
                        }

                        
                        if (!File.Exists(currentDatasetPath))
                        {
                            Console.WriteLine("Dataset not found! Dataset path = " + currentDatasetPath);
                        }


                        ProcessStartInfo processStartInfo = new ProcessStartInfo();
                        //processStartInfo.UseShellExecute = false;

                        processStartInfo.FileName = @"IQConsole.exe";

						var argString = new StringBuilder();
                    	argString.Append("\"" + currentDatasetPath + "\"");
                    	argString.Append(" ");
						argString.Append("\"" + parameterFile + "\"");
						if (!String.IsNullOrEmpty(targetFile))
						{
							argString.Append(" ");
							argString.Append(targetFile);
						}

                        processStartInfo.Arguments = argString.ToString();

                        Console.WriteLine("Argument line= " + argString);


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

		private static string GetTargetFile(string[] args)
		{
			return (args.Length == 4 && args[2].Equals("-targets")) ? args[3] : String.Empty;
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
