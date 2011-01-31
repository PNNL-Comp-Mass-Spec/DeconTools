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

            DirectoryInfo dirInfo = new DirectoryInfo(fullfileName);
            bool isFolder = dirInfo.Exists;

            if (isFolder)
            {
                run = determineIfRunIsABrukerTypeAndCreateRun(fullfileName);

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
                    run = new BrukerV3Run(fileName);
                    break;
                case Globals.MSFileType.Bruker_V2:
                    run = new BrukerV2Run(fileName);
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
                    run = new DeconTools.Backend.Runs.MSScanFromTextFileRun(filename);
                    break;
                case Globals.MSFileType.Bruker:
                    if (parameters.HornTransformParameters.UseScanRange)
                    {
                        run = new BrukerV3Run(filename, parameters.HornTransformParameters.MinScan, parameters.HornTransformParameters.MaxScan);
                    }
                    else
                    {
                        run = new BrukerV3Run(filename);
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

                case Globals.MSFileType.Bruker_V2:
                    if (parameters.HornTransformParameters.UseScanRange)
                    {
                        run = new BrukerV2Run(filename, parameters.HornTransformParameters.MinScan, parameters.HornTransformParameters.MaxScan);
                    }
                    else
                    {
                        run = new BrukerV2Run(filename);
                    }
                    break;


                default:
                    run = null;
                    break;
            }
            return run;
        }



        private Run determineIfRunIsABrukerTypeAndCreateRun(string folderName)
        {
            Run run = null;


            FileInfo serFileInfo = findSerFile(folderName);
            FileInfo fidFileInfo = findFIDFile(folderName);
            
            FileInfo apexAcquisitionMethodFileInfo = findAcquisitionMethodFile(folderName);
            List<FileInfo> acqusFileInfos = findAcqusFile(folderName);

            bool hasSerOrFid = (serFileInfo != null || fidFileInfo != null);
            bool hasSettingFile = (apexAcquisitionMethodFileInfo != null || acqusFileInfos != null);

            if (hasSerOrFid && hasSettingFile)
            {
                run = new BrukerV3Run(folderName);
            }

            return run;







        }

        private FileInfo findFIDFile(string folderName)
        {
            string[] fidFiles = Directory.GetFiles(folderName, "fid", SearchOption.AllDirectories);

            if (fidFiles == null || fidFiles.Length == 0)
            {
                return null;
            }
            else if (fidFiles.Length == 1)
            {
                FileInfo fidFileInfo = new FileInfo(fidFiles[0]);
                return fidFileInfo;
            }
            else
            {
                throw new NotSupportedException("Multiple fid files were found within the dataset folder structure. This is not yet supported.");
            }
        }

        private List<FileInfo> findAcqusFile(string folderName)
        {
            string[] acqusFiles = Directory.GetFiles(folderName, "acqus", SearchOption.AllDirectories);

            List<FileInfo> acqusFileInfoList = new List<FileInfo>();

            if (acqusFiles == null || acqusFiles.Length == 0)
            {
                return null;
            }
            else
            {
                foreach (var file in acqusFiles)
                {
                    FileInfo fi = new FileInfo(file);
                    acqusFileInfoList.Add(fi);
                }
            }

            return acqusFileInfoList;

        }

        private FileInfo findAcquisitionMethodFile(string folderName)
        {
            string[] dotMethodFiles = Directory.GetFiles(folderName, "*.method", SearchOption.AllDirectories);

            if (dotMethodFiles == null || dotMethodFiles.Length == 0)
            {
                return null;
            }

            List<string> acquistionMethodFiles = dotMethodFiles.Where(p => p.EndsWith("apexAcquisition.method", StringComparison.OrdinalIgnoreCase)).ToList();

            if (acquistionMethodFiles.Count == 0)
            {
                return null;
            }
            else if (acquistionMethodFiles.Count == 1)
            {
                return new FileInfo(acquistionMethodFiles[0]);
            }
            else
            {
                throw new NotImplementedException("Run initialization failed. Multiple 'apexAcquisition.method' files were found within the dataset folder structure. \nNot sure which one to pick for the settings file.");
            }
        }


        private FileInfo findSerFile(string folderName)
        {
            string[] serFiles = Directory.GetFiles(folderName, "ser", SearchOption.AllDirectories);

            if (serFiles == null || serFiles.Length == 0)
            {
                return null;
            }
            else if (serFiles.Length == 1)
            {
                FileInfo serFileInfo = new FileInfo(serFiles[0]);
                return serFileInfo;
            }
            else
            {

                foreach (var file in serFiles)
                {
                    FileInfo serFileInfo = new FileInfo(file);
                    if (serFileInfo.Directory.Name.ToLower() == "0.ser")
                    {
                        return serFileInfo;
                    }


                }

                throw new NotSupportedException("Multiple ser files were found within the dataset folder structure. This is not yet supported.");
            }


        }



    }
}
