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

        //C++ engine calibration settings



        #region Constructors

        public BrukerSolarixRun(string fileName)
        {
            validateFileSelection(fileName);
            this.Filename = fileName;
            this.DatasetName = getDatasetName(this.Filename);
            this.DataSetPath = getDatasetfolderName(this.Filename);

            this.SettingsFilePath = validateDataFolderStructureAndFindSettingsFilePath();
            loadSettings(this.SettingsFilePath);

            this.RawData = new DeconToolsV2.Readers.clsRawData(this.Filename, DeconToolsV2.Readers.FileType.BRUKER);
            applySettings();


        }

        #endregion

        #region Properties

        /// <summary>
        /// .NET framework Calibration settings
        /// </summary>
        public DeconTools.Backend.Runs.CalibrationData.BrukerCalibrationData CalibrationData { get; set; }

        /// <summary>
        /// File path to the Bruker Solarix 'apexAcquisition.method' file
        /// </summary>
        public string SettingsFilePath { get; set; }


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

        #endregion
    }
}
