using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace DeconTools.Backend.Runs
{
    public class RunFactory
    {

        public Run CreateRun(string fullfileName)
        {
            Run run;

            string extension = Path.GetExtension(fullfileName).ToLower();

            //check for ICR2LS type extension....
            Match match = Regex.Match(extension, @"\.\d\d\d\d\d$");
            if (match.Success)
            {
                return new ICR2LSRun(fullfileName);
            }


            string fileNameWithoutPathOrExtension = Path.GetFileNameWithoutExtension(fullfileName).ToLower();

            bool mightBeBrukerData = (fileNameWithoutPathOrExtension.Contains("acqus") || fileNameWithoutPathOrExtension.Contains("fid") || 
                fileNameWithoutPathOrExtension.Contains("ser") || fullfileName.ToLower().Contains("0.ser"));

            if (mightBeBrukerData)
            {
                run = determineBrukerTypeAndCreateRun(fullfileName);

                if (run != null)
                {
                    return run;
                }
                else
                {
                    // there was likely some problem... but will let the remaining code execute and see if ms filetype can
                    // be determined from the extension. 
                }
            }

            switch (extension)
            {
                case ".raw":
                    run = new XCaliburRun(fullfileName);
                    break;
                case ".imf":
                    run = new IMFRun(fullfileName);
                    break;

                case ".txt":
                    run = new MSScanFromTextFileRun(fullfileName);
                    break;
                case ".mzxml":
                    run = new MZXMLRun(fullfileName);
                    break;

                case ".uimf":
                    run = new UIMFRun(fullfileName);
                    break;

                case ".db":                            //might want to remove this later
                    run = new UIMFRun(fullfileName);  
                    break;

                case ".d":
                    run = new AgilentD_Run(fullfileName);
                    break;
                case ".yafms":
                    run = new YAFMSRun(fullfileName);
                    break;
                
                default:
                    run = null;
                    break;
            }
            return run;
        }

        private Run determineBrukerTypeAndCreateRun(string fullfileName)
        {
            Run run = null;

            string filenameWithoutPathOrExtension = Path.GetFileNameWithoutExtension(fullfileName).ToLower();

            FileInfo fileInfo = new FileInfo(fullfileName);
            DirectoryInfo parentDirInfo = fileInfo.Directory;

            DirectoryInfo[] folderList = parentDirInfo.GetDirectories();

            //check for Bruker solarix-type folder (xmass_method.m)
            if (folderList != null && folderList.Length >0)
            {

                var folder = (from n in folderList where n.Name.ToLower().Contains("xmass_method.m") select n);


                bool hasBrukerSolarixTypeFolder = (folder.Count()>0);
                if (hasBrukerSolarixTypeFolder)
                {
                    run = new BrukerSolarixRun(fullfileName);
                }
            }


            if (filenameWithoutPathOrExtension.Contains("acqus"))
            {
                run = new BrukerRun(parentDirInfo.FullName);
            }

            return run;





       

        }



        public Run CreateRun(Globals.MSFileType filetype, string fileName)
        {
            Run run;

            switch (filetype)
            {
                case Globals.MSFileType.Undefined:
                    run = null;
                    break;
                case Globals.MSFileType.Agilent_WIFF:
                    run = null;
                    break;

                case Globals.MSFileType.Agilent_D:
                    run = new AgilentD_Run(fileName);
                    break;
                case Globals.MSFileType.Ascii:
                    run = new MSScanFromTextFileRun(fileName);
                    break;
                case Globals.MSFileType.Bruker:
                    run = new BrukerRun(fileName);
                    break;
                case Globals.MSFileType.Bruker_Ascii:
                    run = null;
                    break;
                case Globals.MSFileType.Finnigan:
                    run = new XCaliburRun(fileName);
                    break;
                case Globals.MSFileType.ICR2LS_Rawdata:
                    run = null;
                    break;
                case Globals.MSFileType.Micromass_Rawdata:
                    run = null;
                    break;
                case Globals.MSFileType.MZXML_Rawdata:
                    run = new MZXMLRun(fileName);
                    break;
                case Globals.MSFileType.PNNL_IMS:
                    run = new IMFRun(fileName);
                    break;
                case Globals.MSFileType.PNNL_UIMF:
                    run = new UIMFRun(fileName);
                    break;
                case Globals.MSFileType.SUNEXTREL:
                    run = null;
                    break;
                case Globals.MSFileType.YAFMS:
                    run = new YAFMSRun(fileName);
                    break;
                
                default:
                    run = null;
                    break;
            }

            return run;
        }



        public Run CreateRun(Globals.MSFileType fileType, string filename, OldDecon2LSParameters parameters)
        {
            Run run;

            switch (fileType)
            {
                case Globals.MSFileType.Undefined:
                    run = null;
                    break;
                case Globals.MSFileType.Agilent_WIFF:
                    run = null;
                    break;

                case Globals.MSFileType.Agilent_D:
                    if (parameters.HornTransformParameters.UseScanRange)
                    {
                        run = new AgilentD_Run(filename, parameters.HornTransformParameters.MinScan, parameters.HornTransformParameters.MaxScan);
                    }
                    else
                    {
                        run = new AgilentD_Run(filename);
                    }
                    break;
                case Globals.MSFileType.Ascii:
                    run = null;
                    break;
                case Globals.MSFileType.Bruker:
                    if (parameters.HornTransformParameters.UseScanRange)
                    {
                        run = new BrukerRun(filename, parameters.HornTransformParameters.MinScan, parameters.HornTransformParameters.MaxScan);
                    }
                    else
                    {
                        run = new BrukerRun(filename);
                    }
                    break;
                case Globals.MSFileType.Bruker_Ascii:
                    run = null;
                    break;
                case Globals.MSFileType.Finnigan:
                    if (parameters.HornTransformParameters.UseScanRange)
                    {
                        run = new XCaliburRun(filename, parameters.HornTransformParameters.MinScan, parameters.HornTransformParameters.MaxScan);
                    }
                    else
                    {
                        run = new XCaliburRun(filename);
                    }
                    break;
                case Globals.MSFileType.ICR2LS_Rawdata:
                    if (parameters.HornTransformParameters.UseScanRange)
                    {
                        run = new ICR2LSRun(filename, parameters.HornTransformParameters.MinScan, parameters.HornTransformParameters.MaxScan);
                    }
                    else
                    {
                        run = new ICR2LSRun(filename);
                    }
                    break;
                case Globals.MSFileType.Micromass_Rawdata:
                    run = null;
                    break;
                case Globals.MSFileType.MZXML_Rawdata:
                    if (parameters.HornTransformParameters.UseScanRange)
                    {
                        run = new MZXMLRun(filename, parameters.HornTransformParameters.MinScan, parameters.HornTransformParameters.MaxScan);
                    }
                    else
                    {
                        run = new MZXMLRun(filename);
                    }
                    break;
                case Globals.MSFileType.PNNL_IMS:
                    if (parameters.HornTransformParameters.UseScanRange)
                    {
                        run = new IMFRun(filename, parameters.HornTransformParameters.MinScan, parameters.HornTransformParameters.MaxScan);
                    }
                    else
                    {
                        run = new IMFRun(filename);
                    }
                    break;
                case Globals.MSFileType.PNNL_UIMF:
                    if (parameters.HornTransformParameters.UseScanRange)  //TODO: add UseFrameRange here...
                    {
                        //note! I am (temporarily?) using the 'MinScan' property from the Decon2LS parameter file in the MinFrame property of the UIMFRun
                        //We might want to add separate MinFrame and MaxFrame (and UseFrameRange) parameters to the XML file. 
                        run = new UIMFRun(filename, parameters.HornTransformParameters.MinScan, parameters.HornTransformParameters.MaxScan);
                    }
                    else
                    {
                        run = new UIMFRun(filename);
                    }
                    break;
                case Globals.MSFileType.SUNEXTREL:
                    run = null;
                    break;
                case Globals.MSFileType.YAFMS:
                    if (parameters.HornTransformParameters.UseScanRange)
                    {
                        run = new YAFMSRun(filename, parameters.HornTransformParameters.MinScan, parameters.HornTransformParameters.MaxScan);
                    }
                    else
                    {
                        run = new YAFMSRun(filename);
                    }
                    break;
                   
                case Globals.MSFileType.Bruker_12T_Solarix:
                    if (parameters.HornTransformParameters.UseScanRange)
                    {
                        run = new BrukerSolarixRun(filename, parameters.HornTransformParameters.MinScan, parameters.HornTransformParameters.MaxScan);
                    }
                    else
                    {
                        run = new BrukerSolarixRun(filename);
                    }
                    break;


                default:
                    run = null;
                    break;
            }
            return run;
        }
    }
}
