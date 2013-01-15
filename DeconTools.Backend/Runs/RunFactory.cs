using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    public class RunFactory
    {

       
        public Run CreateRun(string filename)
        {
            Run run;

            string fullfileName = getFullPath(filename);

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
                    run = new XCaliburRun2(fullfileName);
                    break;
                case ".imf":
                    run = new IMFRun(fullfileName);
                    break;

                case ".txt":
                    run = new MSScanFromTextFileRun(fullfileName);
                    break;
                case ".mzxml":
                    run = new MzRun(fullfileName);
                    break;
                case ".mz5":
                    run = new MzRun(fullfileName);
                    break;
                case ".mzml":
                    run = new MzRun(fullfileName);
                    break;
                case ".uimf":
                    run = new UIMFRun(fullfileName);
                    break;

                case ".db":                            //might want to remove this later
                    run = new UIMFRun(fullfileName);
                    break;

                case ".d":
                    run = new AgilentDRun(fullfileName);
                    break;
                case ".yafms":
                    run = new YAFMSRun(fullfileName);
                    break;

                default:
                    throw new ApplicationException("File type - "+ extension + " -  is not supported in DeconTools");
            }

            Check.Require(run!=null,"Run failed to be initialized. Run object is empty. I'm guessing the datafile either 1) corrupt or 2) not supported by the installed instrument manufacturer's dlls, or 3) not supported by DeconTools");
            return run;
        }
        public Run CreateRun(Globals.MSFileType filetype, string f)
        {
            Run run;

            string fileName = getFullPath(f);

            switch (filetype)
            {
                case Globals.MSFileType.Undefined:
                    run = null;
                    break;
                case Globals.MSFileType.Agilent_WIFF:
                    run = null;
                    break;

                case Globals.MSFileType.Agilent_D:
                    run = new AgilentDRun(fileName);
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
                    run = new XCaliburRun2(fileName);
                    break;
                case Globals.MSFileType.ICR2LS_Rawdata:
                    run = null;
                    break;
                case Globals.MSFileType.Micromass_Rawdata:
                    run = null;
                    break;
                case Globals.MSFileType.MZXML_Rawdata:
                    run = new MzRun(fileName);
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


        private string getFullPath(string filename)
        {
            string fullfileName = filename.Trim(new char[] { ' ', '"' });

            DirectoryInfo dirInfo = new DirectoryInfo(fullfileName);
            FileInfo fileInfo = new FileInfo(fullfileName);

            if (!dirInfo.Exists && !fileInfo.Exists)
            {
				throw new System.ApplicationException("Cannot create Run. File/Folder not found: " + filename);
            }

            if (dirInfo.Exists)
            {
                fullfileName = dirInfo.FullName;
            }

            if (fileInfo.Exists)
            {
                fullfileName = fileInfo.FullName;
            }

            return fullfileName;
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
            else
            {
                FileInfo analysisBafFileInfo = findAnalysisBafFile(folderName);
                FileInfo maxAcquisitionFileInfo = findMaxAcquisitionMethodFile(folderName);

                if (analysisBafFileInfo!=null && maxAcquisitionFileInfo!=null)
                {
                    run = new BrukerTOF(folderName);
                }

            }

            return run;







        }

        private FileInfo findMaxAcquisitionMethodFile(string folderName)
        {
            string[] dotMethodFiles = Directory.GetFiles(folderName, "*.method", SearchOption.AllDirectories);

            if (dotMethodFiles == null || dotMethodFiles.Length == 0)
            {
                return null;
            }

            List<string> acquistionMethodFiles = dotMethodFiles.Where(p => p.EndsWith("Acquisition.method", StringComparison.OrdinalIgnoreCase)).ToList();

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
                throw new NotImplementedException("Run initialization failed. Multiple 'Acquisition.method' files were found within the dataset folder structure. \nNot sure which one to pick for the settings file.");
            }
        }

        private FileInfo findAnalysisBafFile(string folderName)
        {
            string[] analysisBafFiles = Directory.GetFiles(folderName, "analysis.baf", SearchOption.TopDirectoryOnly);

            if (analysisBafFiles.Length == 0)
            {
                return null;
            }
            else if (analysisBafFiles.Length == 1)
            {
                FileInfo fileInfo = new FileInfo(analysisBafFiles[0]);
                return fileInfo;
            }
            else
            {
                throw new NotSupportedException("Multiple analysis.baf files were found within the dataset folder structure. This is not yet supported.");
            }
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
