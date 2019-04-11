using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if !Disable_DeconToolsV2
using System.Text.RegularExpressions;
#endif
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    public class RunFactory
    {
        public Run CreateRun(string filename)
        {
            Run run;

            var fullFileName = getFullPath(filename);
            if (string.IsNullOrEmpty(fullFileName))
            {
                throw new Exception("Could determine the filename");
            }

            var extension = Path.GetExtension(fullFileName).ToLower();
            if (extension.Equals(".mzxml") || extension.Equals(".mzml") || extension.Equals(".mz5"))
            {
                pwiz.ProteowizardWrapper.DependencyLoader.AddAssemblyResolver();
            }

#if !Disable_DeconToolsV2
            //check for ICR2LS type extension....
            var match = Regex.Match(extension, @"\.\d\d\d\d\d$");
            if (match.Success)
            {
                return new ICR2LSRun(fullFileName);
            }
#endif

            var fileNameWithoutPathOrExtension = Path.GetFileNameWithoutExtension(fullFileName).ToLower();

            var dirInfo = new DirectoryInfo(fullFileName);
            var isFolder = dirInfo.Exists;

            if (isFolder)
            {
                run = determineIfRunIsABrukerTypeAndCreateRun(fullFileName);

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
                    run = new XCaliburRun2(fullFileName);
                    break;
#if !Disable_DeconToolsV2
                case ".imf":
                    run = new IMFRun(fullFileName);
                    break;
#endif
                case ".txt":
                    run = new MSScanFromTextFileRun(fullFileName);
                    break;
                case ".mzxml":
                    run = new MzRun(fullFileName);
                    break;
                case ".mz5":
                    run = new MzRun(fullFileName);
                    break;
                case ".mzml":
                    run = new MzRun(fullFileName);
                    break;
                case ".uimf":
                    run = new UIMFRun(fullFileName);
                    break;

                case ".db":                            //might want to remove this later
                    run = new UIMFRun(fullFileName);
                    break;

                case ".d":
                    run = new AgilentDRun(fullFileName);
                    break;

                // Deprecated in February 2017
                // case ".yafms":
                //     run = new YAFMSRun(fullFileName);
                //     break;

                default:
                    throw new ApplicationException("File type - " + extension + " -  is not supported in DeconTools");
            }

            Check.Require(run != null, "Run failed to be initialized. Run object is empty. I'm guessing the datafile either 1) corrupt or 2) not supported by the installed instrument manufacturer's dlls, or 3) not supported by DeconTools");
            return run;
        }

        public Run CreateRun(Globals.MSFileType filetype, string f)
        {
            Run run;
            if (filetype == Globals.MSFileType.MZXML_Rawdata)
            {
                pwiz.ProteowizardWrapper.DependencyLoader.AddAssemblyResolver();
            }

            var fileName = getFullPath(f);

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
#if !Disable_DeconToolsV2
                case Globals.MSFileType.Bruker_V2:
#pragma warning disable 618
                    run = new BrukerV2Run(fileName);
#pragma warning restore 618
                    break;
#endif
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
#if !Disable_DeconToolsV2
                case Globals.MSFileType.PNNL_IMS:
                    run = new IMFRun(fileName);
                    break;
#endif
                case Globals.MSFileType.PNNL_UIMF:
                    run = new UIMFRun(fileName);
                    break;
                case Globals.MSFileType.SUNEXTREL:
                    run = null;
                    break;

                // Deprecated in February 2017
                // case Globals.MSFileType.YAFMS:
                //    run = new YAFMSRun(fileName);
                //    break;
                default:
                    run = null;
                    break;
            }

            return run;
        }

        private string getFullPath(string filename)
        {
            var fullFileName = filename.Trim(' ', '"');

            var dirInfo = new DirectoryInfo(fullFileName);
            var fileInfo = new FileInfo(fullFileName);

            if (!dirInfo.Exists && !fileInfo.Exists)
            {
                throw new ApplicationException("Cannot create Run. File/Folder not found: " + filename);
            }

            if (dirInfo.Exists)
            {
                fullFileName = dirInfo.FullName;
            }

            if (fileInfo.Exists)
            {
                fullFileName = fileInfo.FullName;
            }

            return fullFileName;
        }


        private Run determineIfRunIsABrukerTypeAndCreateRun(string folderName)
        {
            Run run = null;


            var serFileInfo = findSerFile(folderName);
            var fidFileInfo = findFIDFile(folderName);


            var apexAcquisitionMethodFileInfo = findAcquisitionMethodFile(folderName);
            var acqusFileInfos = findAcqusFile(folderName);

            var hasSerOrFid = (serFileInfo != null || fidFileInfo != null);
            var hasSettingFile = (apexAcquisitionMethodFileInfo != null || acqusFileInfos != null);

            if (hasSerOrFid && hasSettingFile)
            {
                run = new BrukerV3Run(folderName);
            }
            else
            {
                var analysisBafFileInfo = findAnalysisBafFile(folderName);
                var maxAcquisitionFileInfo = findMaxAcquisitionMethodFile(folderName);

                if (analysisBafFileInfo != null && maxAcquisitionFileInfo != null)
                {
                    run = new BrukerTOF(folderName);
                }

            }

            return run;
        }

        private FileInfo findMaxAcquisitionMethodFile(string folderName)
        {
            var dotMethodFiles = Directory.GetFiles(folderName, "*.method", SearchOption.AllDirectories);

            if (dotMethodFiles.Length == 0)
            {
                return null;
            }

            var acquisitionMethodFiles = dotMethodFiles.Where(p => p.EndsWith("Acquisition.method", StringComparison.OrdinalIgnoreCase)).ToList();

            if (acquisitionMethodFiles.Count == 0)
            {
                return null;
            }

            if (acquisitionMethodFiles.Count == 1)
            {
                return new FileInfo(acquisitionMethodFiles[0]);
            }

            throw new NotImplementedException("Run initialization failed. Multiple 'Acquisition.method' files were found within the dataset folder structure. \nNot sure which one to pick for the settings file.");
        }

        private FileInfo findAnalysisBafFile(string folderName)
        {
            var analysisBafFiles = Directory.GetFiles(folderName, "analysis.baf", SearchOption.TopDirectoryOnly);

            if (analysisBafFiles.Length == 0)
            {
                return null;
            }

            if (analysisBafFiles.Length == 1)
            {
                var fileInfo = new FileInfo(analysisBafFiles[0]);
                return fileInfo;
            }

            throw new NotSupportedException("Multiple analysis.baf files were found within the dataset folder structure. This is not yet supported.");
        }

        private FileInfo findFIDFile(string folderName)
        {
            var fidFiles = Directory.GetFiles(folderName, "fid", SearchOption.AllDirectories);

            if (fidFiles.Length == 0)
            {
                return null;
            }

            if (fidFiles.Length == 1)
            {
                var fidFileInfo = new FileInfo(fidFiles[0]);
                return fidFileInfo;
            }

            throw new NotSupportedException("Multiple fid files were found within the dataset folder structure. This is not yet supported.");
        }

        private List<FileInfo> findAcqusFile(string folderName)
        {
            var acqusFiles = Directory.GetFiles(folderName, "acqus", SearchOption.AllDirectories);

            var acqusFileInfoList = new List<FileInfo>();

            if (acqusFiles.Length == 0)
            {
                return null;
            }

            foreach (var file in acqusFiles)
            {
                var fi = new FileInfo(file);
                acqusFileInfoList.Add(fi);
            }

            return acqusFileInfoList;
        }

        private FileInfo findAcquisitionMethodFile(string folderName)
        {
            var dotMethodFiles = Directory.GetFiles(folderName, "*.method", SearchOption.AllDirectories);

            if (dotMethodFiles.Length == 0)
            {
                return null;
            }

            var acquisitionMethodFiles = dotMethodFiles.Where(p => p.EndsWith("apexAcquisition.method", StringComparison.OrdinalIgnoreCase)).ToList();

            if (acquisitionMethodFiles.Count == 0)
            {
                return null;
            }

            if (acquisitionMethodFiles.Count == 1)
            {
                return new FileInfo(acquisitionMethodFiles[0]);
            }

            throw new NotImplementedException("Run initialization failed. Multiple 'apexAcquisition.method' files were found within the dataset folder structure. \nNot sure which one to pick for the settings file.");
        }

        private FileInfo findSerFile(string folderName)
        {
            var serFiles = Directory.GetFiles(folderName, "ser", SearchOption.AllDirectories);

            if (serFiles.Length == 0)
            {
                return null;
            }

            if (serFiles.Length == 1)
            {
                var serFileInfo = new FileInfo(serFiles[0]);
                return serFileInfo;
            }

            foreach (var file in serFiles)
            {
                var serFileInfo = new FileInfo(file);
                if (serFileInfo.Directory.Name.ToLower() == "0.ser")
                {
                    return serFileInfo;
                }
            }

            throw new NotSupportedException("Multiple ser files were found within the dataset folder structure. This is not yet supported.");
        }
    }
}
