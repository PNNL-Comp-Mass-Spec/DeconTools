using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using DeconTools.Utilities;
using System.Xml.Linq;
using DeconTools.Backend.Runs.CalibrationData;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Runs
{
    public class BrukerV2Run : Run
    {
        FileInfo m_serFileInfo;
        FileInfo m_settingsfileInfo;
        FileInfo m_fidFileInfo;

        internal class brukerNameValuePair
        {
            internal string Name { get; set; }
            internal string Value { get; set; }

        }

        //C++ engine calibration settings



        #region Constructors

        public BrukerV2Run()
        {
            this.XYData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.MSFileType = Globals.MSFileType.Bruker_V2;
            this.IsDataThresholded = false;
            this.ContainsMSMSData = false;

        }

        public BrukerV2Run(string folderName)
            : this()
        {
            validateSelectionIsFolder(folderName);
            this.Filename = folderName;

            m_serFileInfo = findSerFile();
            m_fidFileInfo = findFIDFile();

            string filePathForDeconEngine = "";

            if (m_serFileInfo == null && m_fidFileInfo == null)
            {
                throw new FileNotFoundException("Run initialization problem. Could not find a 'ser' or 'fid' file within the directory structure.");
            }

            //if there is a ser file, 'fid' files will be ignored.  
            if (m_serFileInfo != null)
            {
                filePathForDeconEngine = m_serFileInfo.FullName;
            }
            else if (m_serFileInfo == null && m_fidFileInfo != null)
            {
                filePathForDeconEngine = m_fidFileInfo.FullName;
            }
            else
            {
                throw new FileNotFoundException("Run initialization problem. Could not find a 'ser' or 'fid' file within the directory structure.");
            }



            m_settingsfileInfo = findSettingsFile();
            if (m_settingsfileInfo == null)
            {
                throw new FileNotFoundException("Run initialization problem. Could not find the settings file ('apexAcquisition.method') within the directory structure.");
            }

            this.SettingsFilePath = m_settingsfileInfo.FullName;

            this.DatasetName = getDatasetName(this.Filename);
            this.DataSetPath = getDatasetfolderName(this.Filename);


            loadSettings(this.SettingsFilePath);


            try
            {
                this.rawData = new DeconToolsV2.Readers.clsRawData();
                this.rawData.LoadFile(filePathForDeconEngine, DeconToolsV2.Readers.FileType.BRUKER);
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR:  Couldn't open the file.  Details: " + ex.Message);
            }

            Check.Ensure(this.rawData != null, "Run initialization problem. Details:  DeconEngine tried to load the file but failed.");

            applySettings();
            Check.Ensure(this.rawData != null, "Run initialization problem. Details:  Run was loaded but after FFT settings were applied, there was a problem.");


            this.MinScan = 1;        //  remember that DeconEngine is 1-based
            this.MaxScan = GetMaxPossibleScanIndex();

            Check.Ensure(this.MaxScan != 0, "Run initialization problem. Details:  When initializing the run, the run's maxScan was determined to be '0'. Probably a run accessing error.");


        }

        private FileInfo findFIDFile()
        {
            string[] fidFiles = Directory.GetFiles(this.Filename, "fid", SearchOption.AllDirectories);

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

        private FileInfo findSettingsFile()
        {
            string[] dotMethodFiles = Directory.GetFiles(this.Filename, "*.method", SearchOption.AllDirectories);

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

        private FileInfo findSerFile()
        {
            string[] serFiles = Directory.GetFiles(this.Filename, "ser", SearchOption.AllDirectories);

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
                throw new NotSupportedException("Multiple ser files were found within the dataset folder structure. This is not yet supported.");
            }


        }

        public BrukerV2Run(string fileName, int minScan, int maxScan)
            : this(fileName)
        {
            this.MinScan = minScan;
            this.MaxScan = maxScan;
        }




        #endregion

        #region Properties

        /// <summary>
        /// .NET framework Calibration settings
        /// </summary>
        public DeconTools.Backend.Runs.CalibrationData.BrukerCalibrationData CalibrationData { get; set; }



        [field: NonSerialized]
        private XYData xyData;
        public override XYData XYData
        {
            get
            {
                return xyData;
            }
            set
            {
                xyData = value;
            }
        }

        /// <summary>
        /// File path to the Bruker Solarix 'apexAcquisition.method' file
        /// </summary>
        public string SettingsFilePath { get; set; }

        [field: NonSerialized]
        private DeconToolsV2.Readers.clsRawData rawData;
        public DeconToolsV2.Readers.clsRawData RawData
        {
            get { return rawData; }
            set { rawData = value; }
        }

        #endregion

        #region Public Methods

        //NOTE: code duplication here... see BrukerRun too
        public override void GetMassSpectrum(DeconTools.Backend.Core.ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            int totScans = this.GetNumMSScans();

            double[] xvals = new double[0];
            double[] yvals = new double[0];

            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                this.RawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals);
            }
            else    // need to sum spectra
            {
                //assume:  each scan has exactly same x values

                //get first spectrum
                this.RawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals);

                //
                double[] summedYvals = new double[xvals.Length];
                yvals.CopyTo(summedYvals, 0);

                for (int i = 1; i < scanSet.IndexValues.Count; i++)
                {
                    this.RawData.GetSpectrum(scanSet.IndexValues[i], ref xvals, ref yvals);

                    for (int n = 0; n < xvals.Length; n++)
                    {
                        summedYvals[n] += yvals[n];
                    }
                }

                yvals = summedYvals;
            }
            this.XYData.SetXYValues(ref xvals, ref yvals);

            if (this.XYData.Xvalues == null || this.XYData.Xvalues.Length == 0) return;
            bool needsFiltering = (minMZ > this.XYData.Xvalues[0] || maxMZ < this.XYData.Xvalues[this.XYData.Xvalues.Length - 1]);
            if (needsFiltering)
            {
                this.FilterXYPointsByMZRange(minMZ, maxMZ);
            }

        }

        public override double GetTime(int scanNum)
        {
            return this.rawData.GetScanTime(scanNum);
        }

        public override int GetNumMSScans()
        {
            if (rawData == null) return 0;
            return this.rawData.GetNumScans();
        }

        internal override int GetMaxPossibleScanIndex()
        {
            return this.GetNumMSScans();
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {

            if (!ContainsMSMSData) return 1;    // if we know the run doesn't contain MS/MS data, don't waste time checking
            int mslevel = (byte)this.rawData.GetMSLevel(scanNum);

            addToMSLevelData(scanNum, mslevel);

            return mslevel;
        }

        #endregion



        #region Private Methods
        private void applySettings()
        {
            DeconToolsV2.CalibrationSettings deconEngineCalibrationSettings = convertCalibrationSettingsToDeconEngineSettings(this.CalibrationData);
            this.RawData.SetFFTCalibrationValues(deconEngineCalibrationSettings);

        }

        private DeconToolsV2.CalibrationSettings convertCalibrationSettingsToDeconEngineSettings(BrukerCalibrationData brukerCalibrationData)
        {
            Check.Require(brukerCalibrationData != null, "Problem with calibration data in dataset. Calibration data is empty.");

            DeconToolsV2.CalibrationSettings deconEngineCalibrationsettings = new DeconToolsV2.CalibrationSettings();
            deconEngineCalibrationsettings.ByteOrder = this.CalibrationData.ByteOrder;
            deconEngineCalibrationsettings.FRLow = this.CalibrationData.FR_Low;
            deconEngineCalibrationsettings.ML1 = this.CalibrationData.ML1;
            deconEngineCalibrationsettings.ML2 = this.CalibrationData.ML2;
            deconEngineCalibrationsettings.NF = this.CalibrationData.NF;
            deconEngineCalibrationsettings.SW_h = this.CalibrationData.SW_h;
            deconEngineCalibrationsettings.TD = this.CalibrationData.TD;

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
                throw new System.IO.IOException("Error when accessing datafile. Details: " + ex.Message);
            }

            Check.Require(!isFile, "Could not initialize Dataset. Looking for a folder path, but user supplied a file path.");
            Check.Require(isDirectory, "Could not initialize Dataset. Target dataset folder not found.");

        }

        private string validateDataFolderStructureAndFindSettingsFilePath()
        {
            string settingsFilePath = "";

            FileInfo fi = new FileInfo(this.Filename);
            DirectoryInfo parentDirInfo = fi.Directory;

            DirectoryInfo[] folderList = parentDirInfo.GetDirectories();

            if (folderList == null || folderList.Length == 0)
            {
                throw new System.IO.IOException("Could not initialize dataset. No 'XMASS_Method.m' folder exists within the file structure.");
            }

            DirectoryInfo settingsFolder = folderList.First(p => p.Name.ToLower() == "xmass_method.m");
            if (settingsFolder == null)
            {
                throw new System.IO.IOException("Could not initialize dataset. No 'XMASS_Method.m' folder exists within the file structure.");
            }

            FileInfo[] filesWithinSettingsFolder = settingsFolder.GetFiles();
            if (filesWithinSettingsFolder == null || filesWithinSettingsFolder.Length == 0)
            {
                throw new System.IO.IOException("Could not initialize dataset. No 'apexAcquisition.method' file exists within the file structure.");
            }

            FileInfo settingsFile = filesWithinSettingsFolder.First(p => p.Name.ToLower() == "apexacquisition.method");
            if (settingsFile == null)
            {
                throw new System.IO.IOException("Could not initialize dataset. No 'apexAcquisition.method' file exists within the file structure.");
            }

            settingsFilePath = settingsFile.FullName;

            return settingsFilePath;










        }

        private string getDatasetName(string fullFolderPath)
        {

            DirectoryInfo dirInfo = new DirectoryInfo(fullFolderPath);
            return dirInfo.Name;

        }

        private string getDatasetfolderName(string fullFolderPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(fullFolderPath);
            return dirInfo.FullName;
        }


        public void loadSettings(string settingsFileName)
        {
            XDocument xdoc = XDocument.Load(settingsFileName);
            List<brukerNameValuePair> paramList = new List<brukerNameValuePair>();

            try
            {
                //List<brukerNameValuePair> paramList = (from node in xdoc.Element("method").Element("paramlist").Elements()
                //                                       select new brukerNameValuePair
                //                                       {
                //                                           Name = node.Element("name").Value,
                //                                           Value = node.Element("value").Value
                //                                       }).ToList();


                var paramNodes = (from node in xdoc.Element("method").Element("paramlist").Elements() select node);

                foreach (var node in paramNodes)
                {
                    brukerNameValuePair nameValuePair = new brukerNameValuePair();
                    nameValuePair.Name = getNameFromNode(node);
                    nameValuePair.Value = getValueFromNode(node);

                    paramList.Add(nameValuePair);
                }



            }
            catch (Exception)
            {

                throw;
            }




            this.CalibrationData = new BrukerCalibrationData();

            this.CalibrationData.ML1 = Convert.ToDouble(paramList.First(p => p.Name == "ML1").Value);
            this.CalibrationData.ML2 = Convert.ToDouble(paramList.First(p => p.Name == "ML2").Value);
            //this.CalibrationData.NF = Convert.ToInt32(paramList.First(p => p.Name == "NF").Value);
            this.CalibrationData.SW_h = Convert.ToDouble(paramList.First(p => p.Name == "SW_h").Value) * 2;   // //  from Gordon A.:  SW_h is the digitizer rate and Bruker entered it as the nyquist frequency so it needs to be multiplied by 2.
            this.CalibrationData.TD = Convert.ToInt32(paramList.First(p => p.Name == "TD").Value);
            this.CalibrationData.FR_Low = Convert.ToDouble(paramList.First(p => p.Name == "FR_low").Value);
            this.CalibrationData.ByteOrder = Convert.ToInt32(paramList.First(p => p.Name == "BYTORDP").Value);


            if (this.CalibrationData.SW_h > this.CalibrationData.FR_Low)
            {
                this.CalibrationData.FR_Low = 0;        // from ICR2LS.   Not sure why this is done. It has dramatic effects on BrukerSolerix Data.  TODO: understand and document this parameter
            }



            this.CalibrationData.Display();


        }

        private string getNameFromNode(XElement node)
        {
            var elementNames = (from n in node.Elements() select n.Name.LocalName);
            var attributeNames = (from n in node.Attributes() select n.Name.LocalName);

            bool nameIsAnXMLElement = elementNames.Contains("name");

            bool nameIsAnAttribute = attributeNames.Contains("name");

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
                nameValue = String.Empty;
            }

            return nameValue;
        }

        private string getValueFromNode(XElement node)
        {
            var elementNames = (from n in node.Elements() select n.Name.LocalName);

            bool isAnXMLElement = elementNames.Contains("value") || elementNames.Contains("Value");

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
                valueString = String.Empty;
            }

            return valueString;
        }



        #endregion
    }
}
