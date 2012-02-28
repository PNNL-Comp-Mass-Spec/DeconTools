using System;
using System.IO;
using Agilent.MassSpectrometry.DataAnalysis;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using PNNLOmics.Data;

namespace DeconTools.Backend.Runs
{
    public sealed class AgilentDRun : Run
    {
        #region Constructors

        IMsdrDataReader m_reader;
        IBDASpecData m_spec;

        /// <summary>
        /// Agilent XCT .D datafolder
        /// </summary>
        /// <param name="folderName">The name of the Agilent data folder. Folder has a '.d' suffix</param>

        public AgilentDRun()
        {
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.MSFileType = Globals.MSFileType.Agilent_D;
            this.ContainsMSMSData = true;    //not sure if it does, but setting it to 'true' ensures that each scan will be checked. 
        }

        public AgilentDRun(string dataFileName)
            : this()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dataFileName);
            FileInfo fileInfo = new FileInfo(dataFileName);

            Check.Require(!fileInfo.Exists, "Dataset's inputted name refers to a file, but should refer to a Folder");
            Check.Require(dirInfo.Exists, "Dataset not found.");

            Check.Require(dirInfo.FullName.EndsWith("d", StringComparison.OrdinalIgnoreCase), "Agilent_D dataset folders must end with with the suffix '.d'. Check your folder name.");


            Filename = dirInfo.FullName;
            DatasetName = dirInfo.Name.Substring(0, dirInfo.Name.LastIndexOf(".d", StringComparison.OrdinalIgnoreCase));
            //get dataset name without .d extension
            DataSetPath = dirInfo.FullName;

            OpenDataset();

            MinScan = GetMinPossibleScanNum();
            MaxScan = GetMaxPossibleScanNum();
        }


        public AgilentDRun(string dataFileName, int minScan, int maxScan)
            : this(dataFileName)
        {
            MinScan = minScan;
            MaxScan = maxScan;

        }



        private void OpenDataset()
        {
            m_reader = new MassSpecDataReader();
            m_reader.OpenDataFile(this.Filename);
        }




        #endregion

        #region Properties
        public override XYData XYData { get; set; }

        #endregion

        #region Public Methods
        public override int GetMinPossibleScanNum()
        {
            // AgilentD files are 0-based.
            return 0;
        }

        public override int GetMaxPossibleScanNum()
        {
            return GetNumMSScans() - 1;
        }

        public override int GetNumMSScans()
        {
            //m_reader=new MassSpecDataReader();
            IBDAMSScanFileInformation msscan = m_reader.FileInformation.MSScanFileInformation;
            return (int)msscan.TotalScansPresent;
        }

        public override double GetTime(int scanNum)
        {
            double time = -1;

            if (m_spec == null || m_spec.ScanId == scanNum)    // get fresh spectrum
            {
                getAgilentSpectrum(scanNum);
            }

            if (m_spec == null) return -1;

            IRange[] timeRangeArr = m_spec.AcquiredTimeRange;
            if (timeRangeArr != null && timeRangeArr.Length == 1)
            {
                time = timeRangeArr[0].Start;
            }
            return time;
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            m_spec = m_reader.GetSpectrum(scanNum, null, null);

            MSLevel level = m_spec.MSLevelInfo;
            if (level == MSLevel.MS)
            {
                return 1;
            }
            else if (level == MSLevel.MSMS)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }

        public override PrecursorInfo GetPrecursorInfo(int scanNum)
        {
            m_spec = m_reader.GetSpectrum(scanNum, null, null);

            PrecursorInfo precursor = new PrecursorInfo();

            MSLevel level = m_spec.MSLevelInfo;
            if (level == MSLevel.MS)
            {
                precursor.MSLevel = 1;
            }
            else if (level == MSLevel.MSMS)
            {
                precursor.MSLevel = 2;
            }
            else
            {
                precursor.MSLevel = 1;
            }

            int precursorMassCount;
            double precursorIntensity;
            int precursorCharge;
            bool getCharge;

            //this returns a list of precursor masses (not sure how there can be more than one)
            double[] precursorMZlist = m_spec.GetPrecursorIon(out precursorMassCount);

            //if a mass is returned
            if (precursorMassCount == 1)
            {
                //mass
                precursor.PrecursorMZ = precursorMZlist[0];

                //intensity
                m_spec.GetPrecursorIntensity(out precursorIntensity);
                precursor.PrecursorIntensity = (float)precursorIntensity;

                //charge
                getCharge = m_spec.GetPrecursorCharge(out precursorCharge);
                precursor.PrecursorCharge = precursorCharge;

                //adjust scan number if needed
                precursor.PrecursorScan = scanNum;


            }
            else if (precursorMassCount > 1)
            {

                throw new NotImplementedException("Strange case where more than one precursor is used to generate one spectra");

            }
            else
            {
                precursor.PrecursorMZ = 0;
                precursor.PrecursorIntensity = 0;
                precursor.PrecursorCharge = -1;
                precursor.PrecursorScan = scanNum;
            }
            return precursor;
        }

        private void getAgilentSpectrum(int scanNum)
        {
            m_spec = m_reader.GetSpectrum(scanNum, null, null, DesiredMSStorageType.Profile);
        }

        public override void GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            if (scanSet == null) return;

            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                getAgilentSpectrum(scanSet.PrimaryScanNumber);
            }
            else
            {
                throw new NotImplementedException("Summing isn't supported for Agilent.D files - yet");

            }
            this.XYData.SetXYValues(m_spec.XArray, m_spec.YArray);
        }
        #endregion

        #region Private Methods

        #endregion

        public override void Dispose()
        {

            if (m_reader != null)
            {
                try
                {
                    m_reader.CloseDataFile();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occurred when trying to close AgilentD file: " + this.DatasetName + "\nDetails: " + ex.Message);

                }

            }
            base.Dispose();
        }
    }
}
