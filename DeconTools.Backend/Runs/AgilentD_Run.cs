using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using System.IO;
using Agilent.MassSpectrometry.DataAnalysis;

namespace DeconTools.Backend.Runs
{
    public class AgilentD_Run : Run
    {
        #region Constructors

        IMsdrDataReader m_reader;
        IBDASpecData m_spec;

        /// <summary>
        /// Agilent XCT .D datafolder
        /// </summary>
        /// <param name="folderName">The name of the Agilent data folder. Folder has a '.d' suffix</param>

        public AgilentD_Run()
        {
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.MSFileType = Globals.MSFileType.Agilent_D;
            this.ContainsMSMSData = true;    //not sure if it does, but setting it to 'true' ensures that each scan will be checked. 
        }

        public AgilentD_Run(string dataFileName)
            : this()
        {
            bool isFile = (File.Exists(dataFileName));
            bool isFolder = (Directory.Exists(dataFileName));

            Check.Require(!isFile, "Dataset's inputted name refers to a file, but should refer to a Folder");
            Check.Require(isFolder, "Dataset not found.");

            string nameWithExtension = Path.GetFileName(dataFileName);

            this.Filename = dataFileName;
            this.DatasetName = nameWithExtension.Substring(0, nameWithExtension.LastIndexOf(".d"));    //get dataset name without .d extension
            this.DataSetPath = Path.GetDirectoryName(dataFileName);

            OpenDataset();

            this.MinScan = 0;        // AgilentD files are 0-based. 
            this.MaxScan = GetNumMSScans() - 1;   //
        }


        public AgilentD_Run(string dataFileName, int minScan, int maxScan)
            : this(dataFileName)
        {
            this.MinScan = minScan;
            this.MaxScan = maxScan;

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
            MSLevel level;
            if (m_spec == null || m_spec.ScanId == scanNum)    // get fresh spectrum
            {
                getAgilentSpectrum(scanNum);      // this might be very slow
            }

            level = m_spec.MSLevelInfo;


            if (level == MSLevel.MS)
                return 1;
            else if (level == MSLevel.MSMS)
            {
                return 2;
            }
            else
            {
                return 1;
            }
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



    }
}
