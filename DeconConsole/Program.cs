using System;
using System.IO;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Workflows;

namespace DeconConsole
{
    public class Program
    {




        public static void Main(string[] args)
        {
            Console.WriteLine("Starting..............");

            Globals.ProjectControllerType controllerType;

            switch (args.Length)
            {
                case 0:
                    ReportSyntax();
                    return;
                case 1:
                    ReportSyntax();
                    return;
                case 2:
                    ReportSyntax();
                    return;

                case 3:
                    controllerType = Globals.ProjectControllerType.STANDARD;
                    break;
                case 4:
                    controllerType = convertArgToControllerType(args[3]);
                    break;
                default:
                    ReportSyntax();
                    return;
            }



            string filename = args[0];
            string fileType = args[1];
            string parameterFilename = args[2];



            if (!IsFileValid(filename))
            {
                reportFileProblem(filename);
                return;
            }
            if (!IsFileValid(parameterFilename))
            {
                reportFileProblem(parameterFilename);
                return;
            }

            DeconTools.Backend.Globals.MSFileType msFiletype = convertArgToFileType(fileType);
            if (msFiletype == Globals.MSFileType.Undefined)
            {
                reportFiletypeProblem(fileType);
                return;
            }


            if (controllerType == Globals.ProjectControllerType.UNDEFINED)
            {
                reportProjectControllerProblem(controllerType);
                return;
            }


            ProjectController runner;

            Console.WriteLine("Dataset = " + filename);

            try
            {
                var workflow = ScanBasedWorkflow.CreateWorkflow(parameterFilename, filename);
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


        private static Globals.ProjectControllerType convertArgToControllerType(string arg)
        {
            if (arg == null || arg.Length == 0) return Globals.ProjectControllerType.UNDEFINED;

            Globals.ProjectControllerType controllerType;

            if (arg.ToLower() == "standard")
            {
                controllerType = Globals.ProjectControllerType.STANDARD;
            }
            else if (arg.ToLower() == "korea_ims_custom1")
            {
                controllerType = Globals.ProjectControllerType.KOREA_IMS_PEAKSONLY_CONTROLLER;
            }
            else
            {
                controllerType = Globals.ProjectControllerType.STANDARD;
            }

            return controllerType;

        }

        private static Globals.MSFileType convertArgToFileType(string filetype)
        {
            Globals.MSFileType msfiletype;

            if (filetype.ToLower() == "agilent_wiff")
            {
                msfiletype = Globals.MSFileType.Agilent_WIFF;
            }
            else if (filetype.ToLower() == "agilent_d")
            {
                msfiletype = Globals.MSFileType.Agilent_D;
            }

            else if (filetype.ToLower() == "ascii")
            {
                msfiletype = Globals.MSFileType.Ascii;
            }
            else if (filetype.ToLower() == "bruker")
            {
                msfiletype = Globals.MSFileType.Bruker;
            }
            else if (filetype.ToLower() == "bruker_ascii")
            {
                msfiletype = Globals.MSFileType.Bruker_Ascii;
            }
            else if (filetype.ToLower() == "finnigan")
            {
                msfiletype = Globals.MSFileType.Finnigan;
            }
            else if (filetype.ToLower() == "icr2ls_rawdata")
            {
                msfiletype = Globals.MSFileType.Agilent_WIFF;
            }
            else if (filetype.ToLower() == "micromass_rawdata")
            {
                msfiletype = Globals.MSFileType.Micromass_Rawdata;
            }
            else if (filetype.ToLower() == "mzxml_rawdata")
            {
                msfiletype = Globals.MSFileType.MZXML_Rawdata;
            }
            else if (filetype.ToLower() == "pnnl_ims")
            {
                msfiletype = Globals.MSFileType.PNNL_IMS;
            }
            else if (filetype.ToLower() == "pnnl_uimf")
            {
                msfiletype = Globals.MSFileType.PNNL_UIMF;
            }
            else if (filetype.ToLower() == "sunextrel")
            {
                msfiletype = Globals.MSFileType.SUNEXTREL;
            }
            else if (filetype.ToLower() == "bruker_v2")
            {
                msfiletype = Globals.MSFileType.Bruker_V2;
            }

            else
            {
                msfiletype = Globals.MSFileType.Undefined;
            }
            return msfiletype;
        }



        private static void reportFiletypeProblem(string fileType)
        {
            Console.WriteLine("------------- ERROR! -----------------------------------");
            Console.WriteLine();
            Console.WriteLine("Inputted file type is not a legal file type. See below.");
            Console.WriteLine();
            Console.WriteLine("------------- ERROR! -----------------------------------");

            ReportSyntax();
        }

        private static void reportFileProblem(string filename)
        {
            Console.WriteLine("------------- ERROR! ---------------------");
            if (filename == null || filename.Length == 0)
            {
                Console.WriteLine("The path of your file has no characters!");
            }
            else
            {
                Console.WriteLine("Inputted filename does not exist");
            }
            Console.WriteLine();
         
         

        }

        private static void reportProjectControllerProblem(Globals.ProjectControllerType controllerType)
        {
            Console.WriteLine("------------- ERROR! -----------------------------------");
            Console.WriteLine();
            Console.WriteLine("Project type is not legal. See below.");
            Console.WriteLine();
            Console.WriteLine("------------- ERROR! -----------------------------------");

            ReportSyntax();
        }



        private static bool IsFileValid(string filename)
        {
            if (Directory.Exists(filename)) return true;

            if (File.Exists(filename)) return true;
            else { return false; }

        }

        private static void ReportSyntax()
        {
            Console.WriteLine("This Commandline app requires three arguments (with spaces between).");
            Console.WriteLine("\tArg1 = filename");
            Console.WriteLine("\tArg2 = file type (see below)");
            Console.WriteLine("\tArg3 = parameter filename");
            Console.WriteLine();

            Console.WriteLine("Optional argument # 4 = Project type");

            Console.WriteLine();
            Console.WriteLine("------------- File types ---------------------");
            Console.WriteLine("\tAgilent_WIFF");
            Console.WriteLine("\tAgilent_D");
            Console.WriteLine("\tAscii");
            Console.WriteLine("\tBruker");
            Console.WriteLine("\tBruker_Ascii");
            Console.WriteLine("\tFinnigan");
            Console.WriteLine("\tICR2LS_Rawdata");
            Console.WriteLine("\tMicromass_Rawdata");
            Console.WriteLine("\tMZXML_Rawdata");
            Console.WriteLine("\tPNNL_IMS");
            Console.WriteLine("\tPNNL_UIMF");
            Console.WriteLine("\tSUNEXTREL");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------- Project types ---------------------");
            Console.WriteLine("\tSTANDARD");
            Console.WriteLine("\tKOREA_IMS_CUSTOM1");

            Console.WriteLine();
            Console.WriteLine();

        }
    }
}
