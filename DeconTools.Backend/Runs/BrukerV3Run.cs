using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using System.Xml.Linq;
using DeconTools.Backend.Runs.CalibrationData;

namespace DeconTools.Backend.Runs
{
    public class BrukerV3Run : Run
    {
        FileInfo m_serFileInfo;
        FileInfo m_settingsfileInfo;
        FileInfo m_fidFileInfo;
        FileInfo m_acqusFileInfo;

        internal class brukerNameValuePair
        {
            internal string Name { get; set; }
            internal string Value { get; set; }

        }

        #region Constructors
        public BrukerV3Run()
        {
            this.xyData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.IsDataThresholded = true;
            this.MSFileType = Globals.MSFileType.Bruker;
            this.ContainsMSMSData = false;
        }

        public BrukerV3Run(string folderName)
            : this()
        {
            validateSelectionIsFolder(folderName);
            this.Filename = folderName;

            m_serFileInfo = findSerFile();
            m_fidFileInfo = findFIDFile();

            //TODO: change this name once things are in place.
            string filePathForDeconEngine = "";


            if (m_serFileInfo == null && m_fidFileInfo == null)
            {
                throw new FileNotFoundException("Run initialization problem. Could not find a 'ser' or 'fid' file within the directory structure.");
            }

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
            bool standardSettingsFileNotFound = (m_settingsfileInfo == null);
            if (standardSettingsFileNotFound)
            {
                m_acqusFileInfo = findAcqusFile();

                if (m_acqusFileInfo == null)
                {
                    throw new FileNotFoundException("Run initialization problem. Could not find the settings file ('apexAcquisition.method') within the directory structure.");
                }
                else
                {
                    this.SettingsFilePath = m_acqusFileInfo.FullName;
                    loadSettingsFromAcqusFile(this.SettingsFilePath);
                }
            }
            else
            {
                this.SettingsFilePath = m_settingsfileInfo.FullName;
                loadSettingsFromApexMethodFile(this.SettingsFilePath);
            }




        }





        #endregion

        #region Properties
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

        #endregion

        #region Public Methods
        public override int GetNumMSScans()
        {
            throw new NotImplementedException();
        }

        public override double GetTime(int scanNum)
        {
            throw new NotImplementedException();
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            throw new NotImplementedException();
        }

        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods
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

        private FileInfo findAcqusFile()
        {
            string[] acqusFiles = Directory.GetFiles(this.Filename, "acqus", SearchOption.AllDirectories);

            if (acqusFiles == null || acqusFiles.Length == 0)
            {
                return null;
            }
            else if (acqusFiles.Length == 1)
            {
                FileInfo acqusFileInfo = new FileInfo(acqusFiles[0]);
                return acqusFileInfo;
            }
            else
            {
                //often the Bruker file structures contain multiple Acqus files. I will select 
                //the one that is in the same folder as the 'ser' file and if that isn't present,
                //the same folder as the 'fid' file. Otherwise, throw errors

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
                    FileInfo fi = new FileInfo(file);
                    if (fi.Directory == favoredDirectory)
                    {
                        return fi;
                    }
                }

                throw new NotSupportedException("Multiple acqus files were found within the dataset folder structure. Cannot decide which is best to use.");
            }
        }

        internal void loadSettingsFromAcqusFile(string settingsFileName)
        {
            this.CalibrationData = new BrukerCalibrationData();

            using (StreamReader sw = new StreamReader(File.OpenRead(settingsFileName)))
            {


            }



        }

        internal void loadSettingsFromApexMethodFile(string settingsFileName)
        {
            XDocument xdoc = XDocument.Load(settingsFileName);
            List<brukerNameValuePair> paramList = new List<brukerNameValuePair>();

            try
            {
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
