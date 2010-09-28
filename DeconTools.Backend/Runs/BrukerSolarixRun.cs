using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using DeconTools.Utilities;
using System.Xml.Linq;
using DeconTools.Backend.Runs.CalibrationData;

namespace DeconTools.Backend.Runs
{
    public class BrukerSolarixRun : DeconToolsRun
    {


        internal class brukerNameValuePair
        {
            internal string Name { get; set; }
            internal string Value { get; set; }

        }


   


        #region Constructors
        #endregion

        #region Properties


        public DeconTools.Backend.Runs.CalibrationData.BrukerCalibrationData CalibrationData { get; set; }

        public string SettingsFilePath { get; set; }


        #endregion

        #region Public Methods
        public BrukerSolarixRun(string fileName)
        {

            validateFileSelection(fileName);
            this.Filename = fileName;
            this.DatasetName = getDatasetName(this.Filename);
            this.DataSetPath = getDatasetfolderName(this.Filename);

            this.SettingsFilePath = validateDataFolderStructureAndFindSettingsFilePath();
            loadSettings(this.SettingsFilePath);

            this.RawData = new DeconToolsV2.Readers.clsRawData(this.Filename, DeconToolsV2.Readers.FileType.BRUKER);

        }

        private void validateFileSelection(string fileName)
        {
            bool isDirectory;
            try
            {
                isDirectory = Directory.Exists(fileName);
            }
            catch (Exception ex)
            {

                throw new System.IO.IOException("Error when accessing datafile. Details: " + ex.Message);
            }

            Check.Require(!isDirectory, "Could not initialize Dataset. Looking for a file path, but user supplied a Folder path.");

            bool fileExists;
            try
            {
                fileExists = File.Exists(fileName);
            }
            catch (Exception ex)
            {
                throw new System.IO.IOException("Error when accessing datafile. Details: " + ex.Message);

            }
            Check.Require(fileExists, "Could not initialize Dataset. User supplied a file path that does not exist.");

            bool validFileName = fileName.EndsWith("ser", StringComparison.OrdinalIgnoreCase);
            Check.Require(validFileName, "Could not initialize dataset. Please select a 'ser' file in order to initialize dataset.");
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

        private string getDatasetName(string fullFilePath)
        {
            FileInfo fi = new FileInfo(fullFilePath);
            DirectoryInfo parentDirInfo = fi.Directory;

            return parentDirInfo.Name;

        }

        private string getDatasetfolderName(string fullFilePath)
        {
            FileInfo fi = new FileInfo(fullFilePath);
            DirectoryInfo parentDirInfo = fi.Directory;

            return parentDirInfo.FullName;
        }


        public void loadSettings(string settingsFileName)
        {
            XDocument xdoc = XDocument.Load(settingsFileName);

            List<brukerNameValuePair> paramList = (from node in xdoc.Element("method").Element("paramlist").Elements()
                                                   select new brukerNameValuePair
                                                   {
                                                       Name = node.Element("name").Value,
                                                       Value = node.Element("value").Value
                                                   }).ToList();

            this.CalibrationData = new BrukerCalibrationData();

            this.CalibrationData.ML1 = Convert.ToDouble(paramList.First(p => p.Name == "ML1").Value);
            this.CalibrationData.ML2 =  Convert.ToDouble(paramList.First(p => p.Name == "ML2").Value);
            //this.CalibrationData.NF = Convert.ToInt32(paramList.First(p => p.Name == "NF").Value);
            this.CalibrationData.SW_h =  Convert.ToDouble(paramList.First(p => p.Name == "SW_h").Value);
            this.CalibrationData.TD = Convert.ToInt32(paramList.First(p => p.Name == "TD").Value);
            this.CalibrationData.FR_Low =  Convert.ToDouble(paramList.First(p => p.Name == "FR_low").Value);
            this.CalibrationData.ByteOrder = Convert.ToInt32(paramList.First(p => p.Name == "BYTORDP").Value);

            this.CalibrationData.Display();


        }



        #endregion

        #region Private Methods
        #endregion
    }
}
