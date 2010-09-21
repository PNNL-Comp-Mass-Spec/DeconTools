using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using System.IO;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    public class YAFMSRun : DeconToolsRun
    {
        YafmsLibrary.YafmsReader m_reader;


        #region Constructors

        public YAFMSRun()
        {
            this.XYData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.MSFileType = Globals.MSFileType.YAFMS;
            this.ContainsMSMSData = false;

            //SpectraID is specific to the YAFMS schema. Default is '1'. 
            this.SpectraID = 1;
        }


        public YAFMSRun(string filename)
            : this()
        {
            Check.Require(File.Exists(filename), "Cannot find file - does not exist.");

            this.Filename = filename;
            string baseFilename = Path.GetFileName(this.Filename);
            this.DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            this.DataSetPath = Path.GetDirectoryName(filename);

            m_reader = new YafmsLibrary.YafmsReader();
            try
            {
                m_reader.OpenYafms(this.Filename);
            }
            catch (Exception ex)
            {
                throw ex;
            }



            this.MinScan = 0;
            this.MaxScan = GetMaxPossibleScanIndex();
        }

        public YAFMSRun(string fileName, int minScan, int maxScan)
            : this(fileName)
        {
            //MinScan has been already defined by the alternate constructor. The inputted parameters must be within the base MinScan and MaxScan. 

            Check.Require(minScan >= MinScan, "Cannot initialize YAFMS run. Inputted minScan is lower than the minimum possible scan number.");
            Check.Require(maxScan <= MaxScan, "Cannot initialize YAFMS run. Inputted MaxScan is greater than the maximum possible scan number.");


            this.MinScan = minScan;
            this.MaxScan = maxScan;

        }


        #endregion

        #region Properties
        /// <summary>
        /// SpectraID is specific to the YafMS schema. Default is '1'
        /// </summary>
        public int SpectraID { get; set; }


        #endregion

        #region Public Methods


        public override void GetMassSpectrum(ScanSet scanset)
        {
            if (scanset.IndexValues.Count > 1)
            {
                throw new NotImplementedException("Summing was attempted on YafMS data, but summing hasn't been implemented");
            }
            double[] xvals = null;
            float[] yvals = null;

            m_reader.GetSpectrum(this.SpectraID, scanset.PrimaryScanNumber, ref xvals, ref yvals);

            this.XYData.SetXYValues(xvals, yvals);
        }

        public override void GetMassSpectrum(DeconTools.Backend.Core.ScanSet scanset, double minMZ, double maxMZ)
        {
            //TODO: Update upon error fix....  the YAFMS library is throwing an error if I give an m/z outside it's expected range. So until that is fixed, I'll go get all the m/z values and trim them myself

            GetMassSpectrum(scanset);

            this.XYData = this.XYData.TrimData(minMZ, maxMZ);
            return;
            
            //
            
            if (scanset.IndexValues.Count > 1)
            {
                throw new NotImplementedException("Summing was attempted on YafMS data, but summing hasn't been implemented");
            }


            double[] xvals = null;
            float[] yvals = null;

            m_reader.GetSpectrum(this.SpectraID, scanset.PrimaryScanNumber, ref xvals, ref yvals, minMZ, maxMZ);
            
            this.XYData.SetXYValues(xvals, yvals);
        }


        public override int GetNumMSScans()
        {
            int numScans = m_reader.GetTotalNumberScans();
            return numScans;
        }

        public override double GetTime(int scanNum)
        {
            return m_reader.GetRetentionTime(this.SpectraID, scanNum);
        }


        public override void Close()
        {
            base.Close();

            if (m_reader != null)
            {
                m_reader.CloseYafms();
            }

        }


        public override int GetMSLevelFromRawData(int scanNum)
        {

            int msLevel = m_reader.GetMSLevel(this.SpectraID, scanNum);
            
            return msLevel;
        }


        #endregion

        #region Private Methods


        #endregion


    }
}
