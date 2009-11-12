using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DeconTools.Backend;

namespace DeconConsole
{
    class Program
    {



        static void Main(string[] args)
        {
            Console.WriteLine("Starting..............");
            
            if (args.Length != 3)
            {
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

            OldSchoolProcRunner runner;

            try
            {
                runner = new OldSchoolProcRunner(filename, msFiletype, parameterFilename);
                runner.Execute();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                Console.ReadLine();
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
            Console.WriteLine("Total Features Found = " + runner.Project.RunCollection[0].ResultCollection.ResultList.Count);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

    



          


        }

        private static Globals.MSFileType convertArgToFileType(string filetype)
        {
            Globals.MSFileType msfiletype;
            
            if (filetype.ToLower() == "agilent_tof")
            {
                msfiletype = Globals.MSFileType.Agilent_TOF;
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
                msfiletype = Globals.MSFileType.Agilent_TOF;
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
            Console.WriteLine();
            Console.ReadLine();
            

        }

        private static bool IsFileValid(string filename)
        {
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
            Console.WriteLine();
            Console.WriteLine("------------- File types ---------------------");
            Console.WriteLine("\tAgilent_TOF");

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
            Console.ReadLine();


        }
    }
}
