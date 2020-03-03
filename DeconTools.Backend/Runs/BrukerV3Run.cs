using System;
using System.IO;
using System.Linq;
using BrukerDataReader;
using DeconTools.Backend.Core;
using Check = DeconTools.Utilities.Check;

namespace DeconTools.Backend.Runs
{
    public sealed class BrukerV3Run : Run
    {
        readonly FileInfo m_serFileInfo;
        readonly FileInfo m_fidFileInfo;

        readonly DataReader m_rawDataReader;

        internal class BrukerNameValuePair
        {
            internal string Name { get; set; }
            internal string Value { get; set; }

        }

        #region Constructors
        public BrukerV3Run()
        {
            XYData = new XYData();
            IsDataThresholded = true;
            MSFileType = Globals.MSFileType.Bruker;
            ContainsMSMSData = false;
        }

        public BrukerV3Run(string directoryPath)
            : this()
        {
            ValidateSelectionIsFolder(directoryPath);
            DatasetFileOrDirectoryPath = directoryPath;

            m_serFileInfo = findSerFile();
            m_fidFileInfo = findFIDFile();

            if (m_serFileInfo == null && m_fidFileInfo == null)
            {
                throw new FileNotFoundException("Run initialization problem. Could not find a 'ser' or 'fid' file within the directory structure.");
            }

            string filePathForRawDataReader;

            if (m_serFileInfo != null)
            {
                filePathForRawDataReader = m_serFileInfo.FullName;
            }
            else if (m_serFileInfo == null && m_fidFileInfo != null)
            {
                filePathForRawDataReader = m_fidFileInfo.FullName;
            }
            else
            {
                throw new FileNotFoundException("Run initialization problem. Could not find a 'ser' or 'fid' file within the directory structure.");
            }

            var fiSettingsFile = findSettingsFile();
            if (fiSettingsFile == null)
            {
                var fiAcqusFile = findAcqusFile();

                if (fiAcqusFile == null)
                {
                    throw new FileNotFoundException("Run initialization problem. Could not find the settings file ('apexAcquisition.method') within the directory structure.");
                }

                SettingsFilePath = fiAcqusFile.FullName;
            }
            else
            {
                SettingsFilePath = fiSettingsFile.FullName;
            }

            // Instantiate the BrukerDataReader
            // It will read the settings file and define the parameters
            // Parameters are: CalA, CalB, sampleRate, numValueInScan

            m_rawDataReader = new DataReader(filePathForRawDataReader, SettingsFilePath);

            m_rawDataReader.Parameters.Display();

            DatasetName = GetDatasetName(DatasetFileOrDirectoryPath);
            DatasetDirectoryPath = GetDatasetFolderName(DatasetFileOrDirectoryPath);

            MinLCScan = GetMinPossibleLCScanNum();
            MaxLCScan = GetMaxPossibleLCScanNum();

            Check.Ensure(m_rawDataReader != null, "BrukerRun could not be initialized. Failed to connect to raw data.");
        }

        public BrukerV3Run(string fileName, int minScan, int maxScan)
            : this(fileName)
        {
            MinLCScan = minScan;
            MaxLCScan = maxScan;
        }


        #endregion

        #region Properties

        [field: NonSerialized]
        public override XYData XYData { get; set; }

        /// <summary>
        /// File path to the Bruker Solarix 'apexAcquisition.method' file or to the acqus file
        /// </summary>
        public string SettingsFilePath { get; set; }

        #endregion

        #region Public Methods
        public override int GetNumMSScans()
        {
            return m_rawDataReader.GetNumMSScans();
        }

        public override int GetMinPossibleLCScanNum()
        {
            return 0;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans() - 1;
        }


        public override double GetTime(int scanNum)
        {
            return scanNum;
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            if (ContainsMSMSData)
            {
                throw new NotImplementedException("Cannot get MSLevel from BrukerData yet.");
            }

            return 1;
        }

        public override XYData GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {

            var scanValues = scanSet.IndexValues.ToArray();

            var maximumMzToUse = maxMZ;

            if (m_rawDataReader.Parameters.AcquiredMZMaximum > 0 && maxMZ > m_rawDataReader.Parameters.AcquiredMZMaximum)
            {
                maximumMzToUse = m_rawDataReader.Parameters.AcquiredMZMaximum;
            }

            m_rawDataReader.GetMassSpectrum(scanValues, (float)minMZ, (float)maximumMzToUse, out var xVals, out var yVals);

            var xyData = new XYData
            {
                Yvalues = yVals.Select(p => (double)p).ToArray(),
                Xvalues = xVals.Select(p => (double)p).ToArray()
            };

            return xyData;

        }

        public override XYData GetMassSpectrum(ScanSet scanSet)
        {

            var scanValues = scanSet.IndexValues.ToArray();

            m_rawDataReader.GetMassSpectrum(scanValues, out var xVals, out var yVals);
            var xyData = new XYData
            {
                Yvalues = yVals.Select(p => (double)p).ToArray(),
                Xvalues = xVals.Select(p => (double)p).ToArray()
            };

            return xyData;

        }

        #endregion

        #region Private Methods

        private string GetDatasetName(string fullFolderPath)
        {

            var dirInfo = new DirectoryInfo(fullFolderPath);

            if (dirInfo.Name.EndsWith(".d", StringComparison.OrdinalIgnoreCase))
            {
                return dirInfo.Name.Substring(0, dirInfo.Name.Length - ".d".Length);
            }

            return dirInfo.Name;
        }

        private string GetDatasetFolderName(string fullFolderPath)
        {
            var dirInfo = new DirectoryInfo(fullFolderPath);
            return dirInfo.FullName;
        }
        private void ValidateSelectionIsFolder(string folderName)
        {
            bool isDirectory;

            bool isFile;

            try
            {
                isDirectory = Directory.Exists(folderName);
                isFile = File.Exists(folderName);
            }
            catch (Exception ex)
            {
                throw new IOException("Error when accessing datafile. Details: " + ex.Message);
            }

            Check.Require(!isFile, "Could not initialize Dataset. Looking for a folder path, but user supplied a file path.");
            Check.Require(isDirectory, "Could not initialize Dataset. Target dataset folder not found.");

        }
        private FileInfo findFIDFile()
        {
            var fidFiles = Directory.GetFiles(DatasetFileOrDirectoryPath, "fid", SearchOption.AllDirectories);

            if (fidFiles == null || fidFiles.Length == 0)
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
        private FileInfo findSerFile()
        {
            var serFiles = Directory.GetFiles(DatasetFileOrDirectoryPath, "ser", SearchOption.AllDirectories);

            if (serFiles == null || serFiles.Length == 0)
            {
                return null;
            }

            if (serFiles.Length == 1)
            {
                var serFileInfo = new FileInfo(serFiles[0]);
                return serFileInfo;
            }

            foreach (var serFile in serFiles)
            {
                var fi = new FileInfo(serFile);

                if (fi.Directory.Name == "0.ser")
                {
                    return fi;
                }

            }

            throw new NotSupportedException("Multiple ser files were found within the dataset folder structure. This is not yet supported.");
        }


        private FileInfo findSettingsFile()
        {
            var dotMethodFiles = Directory.GetFiles(DatasetFileOrDirectoryPath, "*.method", SearchOption.AllDirectories);

            if (dotMethodFiles == null || dotMethodFiles.Length == 0)
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

        private FileInfo findAcqusFile()
        {
            var acqusFiles = Directory.GetFiles(DatasetFileOrDirectoryPath, "acqus", SearchOption.AllDirectories);

            if (acqusFiles == null || acqusFiles.Length == 0)
            {
                return null;
            }

            if (acqusFiles.Length == 1)
            {
                var acqusFileInfo = new FileInfo(acqusFiles[0]);
                return acqusFileInfo;
            }

            // Often the Bruker file structures contain multiple Acqus files. I will select
            // the one that is in the same folder as the 'ser' file and if that isn't present,
            // the same folder as the 'fid' file. Otherwise, throw errors

            DirectoryInfo favoredDirectory;
            if (m_serFileInfo != null)
            {
                favoredDirectory = m_serFileInfo.Directory;
            }
            else if (m_fidFileInfo != null)
            {
                favoredDirectory = m_fidFileInfo.Directory;
            }
            else
            {
                throw new NotSupportedException("Multiple acqus files were found within the dataset folder structure. Cannot decide which is best to use.");
            }


            foreach (var file in acqusFiles)
            {
                var fi = new FileInfo(file);
                if (fi.Directory.Name.Equals(favoredDirectory.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return fi;
                }
            }

            throw new NotSupportedException("Multiple acqus files were found within the dataset folder structure. Cannot decide which is best to use.");

        }

        #endregion




    }
}
