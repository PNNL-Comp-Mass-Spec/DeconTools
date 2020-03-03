#if !Disable_DeconToolsV2
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs.CalibrationData;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    [Obsolete("No path to access; use BrukerV3Run.", false)]
    public sealed class BrukerV2Run : Run
    {
        internal class brukerNameValuePair
        {
            internal string Name { get; set; }
            internal string Value { get; set; }
        }

        //C++ engine calibration settings

#region Constructors

        public BrukerV2Run()
        {
            XYData = new XYData();
            MSFileType = Globals.MSFileType.Bruker_V2;
            IsDataThresholded = false;
            ContainsMSMSData = false;

        }

        public BrukerV2Run(string folderName)
            : this()
        {
            validateSelectionIsFolder(folderName);
            DatasetFileOrDirectoryPath = folderName;

            var serFileInfo = findSerFile();
            var fidFileInfo = findFIDFile();

            string filePathForDeconEngine;

            if (serFileInfo == null && fidFileInfo == null)
            {
                throw new FileNotFoundException("Run initialization problem. Could not find a 'ser' or 'fid' file within the directory structure.");
            }

            //if there is a ser file, 'fid' files will be ignored.
            if (serFileInfo != null)
            {
                filePathForDeconEngine = serFileInfo.FullName;
            }
            else if (serFileInfo == null && fidFileInfo != null)
            {
                filePathForDeconEngine = fidFileInfo.FullName;
            }
            else
            {
                throw new FileNotFoundException("Run initialization problem. Could not find a 'ser' or 'fid' file within the directory structure.");
            }

            var settingsfileInfo = findSettingsFile();
            if (settingsfileInfo == null)
            {
                throw new FileNotFoundException("Run initialization problem. Could not find the settings file ('apexAcquisition.method') within the directory structure.");
            }

            SettingsFilePath = settingsfileInfo.FullName;

            DatasetName = GetDatasetName(DatasetFileOrDirectoryPath);
            DatasetDirectoryPath = GetDatasetFolderName(DatasetFileOrDirectoryPath);

            loadSettings(SettingsFilePath);

            try
            {
                rawData = new DeconToolsV2.Readers.clsRawData();
                rawData.LoadFile(filePathForDeconEngine, DeconToolsV2.Readers.FileType.BRUKER);
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR:  Couldn't open the file.  Details: " + ex.Message);
            }

            Check.Ensure(rawData != null, "Run initialization problem. Details:  DeconEngine tried to load the file but failed.");

            applySettings();
            Check.Ensure(rawData != null, "Run initialization problem. Details:  Run was loaded but after FFT settings were applied, there was a problem.");


            MinLCScan = GetMinPossibleLCScanNum();        //  remember that DeconEngine is 1-based
            MaxLCScan = GetMaxPossibleLCScanNum();

            Check.Ensure(MaxLCScan != 0, "Run initialization problem. Details:  When initializing the run, the run's maxScan was determined to be '0'. Probably a run accessing error.");
        }

        public BrukerV2Run(string fileName, int minScan, int maxScan)
            : this(fileName)
        {
            MinLCScan = minScan;
            MaxLCScan = maxScan;
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

            throw new NotSupportedException("Multiple ser files were found within the dataset folder structure. This is not yet supported.");
        }

#endregion

#region Properties

        /// <summary>
        /// .NET framework Calibration settings
        /// </summary>
        public BrukerCalibrationData CalibrationData { get; set; }

        [field: NonSerialized]
        private XYData xyData;
        public override XYData XYData
        {
            get => xyData;
            set => xyData = value;
        }

        /// <summary>
        /// File path to the Bruker Solarix 'apexAcquisition.method' file
        /// </summary>
        public string SettingsFilePath { get; set; }

        [field: NonSerialized]
        private DeconToolsV2.Readers.clsRawData rawData;
        public DeconToolsV2.Readers.clsRawData RawData
        {
            get => rawData;
            set => rawData = value;
        }

#endregion

#region Public Methods

        //NOTE: code duplication here... see BrukerRun too
        public override XYData GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            var totScans = GetNumMSScans();

            var xvals = new double[0];
            var yvals = new double[0];

            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                RawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals, false);
            }
            else    // need to sum spectra
            {
                //assume:  each scan has exactly same x values

                //get first spectrum
                RawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals, false);

                //
                var summedYvals = new double[xvals.Length];
                yvals.CopyTo(summedYvals, 0);

                for (var i = 1; i < scanSet.IndexValues.Count; i++)
                {
                    RawData.GetSpectrum(scanSet.IndexValues[i], ref xvals, ref yvals, false);

                    for (var n = 0; n < xvals.Length; n++)
                    {
                        summedYvals[n] += yvals[n];
                    }
                }

                yvals = summedYvals;
            }

            var xydata = new XYData
            {
                Xvalues = xvals,
                Yvalues = yvals
            };

            xydata = xydata.TrimData(minMZ, maxMZ);

            return xydata;
        }

        public override double GetTime(int scanNum)
        {
            return rawData.GetScanTime(scanNum);
        }

        public override int GetNumMSScans()
        {
            if (rawData == null) return 0;
            return rawData.GetNumScans();
        }

        public override int GetMinPossibleLCScanNum()
        {
            return 1;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans();
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            if (!ContainsMSMSData) return 1;    // if we know the run doesn't contain MS/MS data, don't waste time checking
            int mslevel = (byte)rawData.GetMSLevel(scanNum);

            addToMSLevelData(scanNum, mslevel);

            return mslevel;
        }

#endregion

#region Private Methods
        private void applySettings()
        {
            var deconEngineCalibrationSettings = convertCalibrationSettingsToDeconEngineSettings(CalibrationData);
            RawData.SetFFTCalibrationValues(deconEngineCalibrationSettings);
        }

        private DeconToolsV2.CalibrationSettings convertCalibrationSettingsToDeconEngineSettings(BrukerCalibrationData brukerCalibrationData)
        {
            Check.Require(brukerCalibrationData != null, "Problem with calibration data in dataset. Calibration data is empty.");

            var deconEngineCalibrationsettings = new DeconToolsV2.CalibrationSettings
            {
                ByteOrder = CalibrationData.ByteOrder,
                FRLow = CalibrationData.FR_Low,
                ML1 = CalibrationData.ML1,
                ML2 = CalibrationData.ML2,
                NF = CalibrationData.NF,
                SW_h = CalibrationData.SampleRate,
                TD = CalibrationData.NumValuesInScan
            };

            return deconEngineCalibrationsettings;
        }

        private void validateSelectionIsFolder(string folderName)
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

        [Obsolete("Unused")]
        private string validateDataFolderStructureAndFindSettingsFilePath()
        {
            var fi = new FileInfo(DatasetFileOrDirectoryPath);
            var parentDirInfo = fi.Directory;

            var folderList = parentDirInfo.GetDirectories();

            if (folderList == null || folderList.Length == 0)
            {
                throw new IOException("Could not initialize dataset. No 'XMASS_Method.m' folder exists within the file structure.");
            }

            var settingsFolder = folderList.First(p => p.Name.ToLower() == "xmass_method.m");
            if (settingsFolder == null)
            {
                throw new IOException("Could not initialize dataset. No 'XMASS_Method.m' folder exists within the file structure.");
            }

            var filesWithinSettingsFolder = settingsFolder.GetFiles();
            if (filesWithinSettingsFolder == null || filesWithinSettingsFolder.Length == 0)
            {
                throw new IOException("Could not initialize dataset. No 'apexAcquisition.method' file exists within the file structure.");
            }

            var settingsFile = filesWithinSettingsFolder.First(p => p.Name.ToLower() == "apexacquisition.method");
            if (settingsFile == null)
            {
                throw new IOException("Could not initialize dataset. No 'apexAcquisition.method' file exists within the file structure.");
            }

            var settingsFilePath = settingsFile.FullName;

            return settingsFilePath;
        }

        private string GetDatasetName(string fullFolderPath)
        {

            var dirInfo = new DirectoryInfo(fullFolderPath);
            return dirInfo.Name;
        }

        private string GetDatasetFolderName(string fullFolderPath)
        {
            var dirInfo = new DirectoryInfo(fullFolderPath);
            return dirInfo.FullName;
        }

        public void loadSettings(string settingsFileName)
        {
            var xdoc = XDocument.Load(settingsFileName);
            var paramList = new List<brukerNameValuePair>();

            var paramNodes = (from node in xdoc.Element("method").Element("paramlist").Elements() select node);

            foreach (var node in paramNodes)
            {
                var nameValuePair = new brukerNameValuePair
                {
                    Name = getNameFromNode(node),
                    Value = getValueFromNode(node)
                };

                paramList.Add(nameValuePair);
            }

            CalibrationData = new BrukerCalibrationData
            {
                ML1 = Convert.ToDouble(paramList.First(p => p.Name == "ML1").Value),
                ML2 = Convert.ToDouble(paramList.First(p => p.Name == "ML2").Value),
                SampleRate = Convert.ToDouble(paramList.First(p => p.Name == "SW_h").Value) * 2,
                NumValuesInScan = Convert.ToInt32(paramList.First(p => p.Name == "TD").Value),
                FR_Low = Convert.ToDouble(paramList.First(p => p.Name == "FR_low").Value),
                ByteOrder = Convert.ToInt32(paramList.First(p => p.Name == "BYTORDP").Value)
            };

            //this.CalibrationData.NF = Convert.ToInt32(paramList.First(p => p.Name == "NF").Value);

            if (CalibrationData.SampleRate > CalibrationData.FR_Low)
            {
                CalibrationData.FR_Low = 0;        // from ICR2LS.   Not sure why this is done. It has dramatic effects on BrukerSolarix Data.  TODO: understand and document this parameter
            }

            CalibrationData.Display();
        }

        private string getNameFromNode(XElement node)
        {
            var elementNames = (from n in node.Elements() select n.Name.LocalName);
            var attributeNames = (from n in node.Attributes() select n.Name.LocalName);

            var nameIsAnXMLElement = elementNames.Contains("name");

            var nameIsAnAttribute = attributeNames.Contains("name");

            string nameValue;
            if (nameIsAnXMLElement)
            {
                nameValue = node.Element("name").Value;
            }
            else if (nameIsAnAttribute)
            {
                nameValue = node.Attribute("name").Value;
            }
            else
            {
                nameValue = string.Empty;
            }

            return nameValue;
        }

        private string getValueFromNode(XElement node)
        {
            var elementNames = (from n in node.Elements() select n.Name.LocalName);

            var isAnXMLElement = elementNames.Contains("value") || elementNames.Contains("Value");

            string valueString;
            if (isAnXMLElement)
            {
                if (elementNames.Contains("value"))
                {
                    valueString = node.Element("value").Value;
                }
                else
                {
                    valueString = node.Element("Value").Value;
                }
            }
            else
            {
                valueString = string.Empty;
            }

            return valueString;
        }

#endregion
    }
}
#endif
