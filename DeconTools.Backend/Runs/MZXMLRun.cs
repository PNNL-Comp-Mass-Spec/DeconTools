using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    public class MZXMLRun : Run
    {

       


        #region Constructors

        public MZXMLRun()
        {
            this.xyData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.IsDataThresholded = false;      //TODO: check this
            this.MSFileType = Globals.MSFileType.MZXML_Rawdata;
        }
        
        public MZXMLRun(string filename)
            : this()
        {
            this.Filename = filename;


            try
            {

                this.rawData = new DeconToolsV2.Readers.clsRawData(filename, DeconToolsV2.Readers.FileType.MZXMLRAWDATA);
            }
            catch (Exception ex)
            {

                throw new Exception("ERROR:  Couldn't open the file.  Details: " + ex.Message);
            }
            this.MinScan = 1;        //  remember that DeconEngine is 1-based
            this.MaxScan = GetMaxPossibleScanIndex();

        }

        public MZXMLRun(string filename, int minScan, int maxScan)
            : this(filename)
        {
            this.MinScan = minScan;
            this.MaxScan = maxScan;
        }


        #endregion


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

        [field: NonSerialized]
        private DeconToolsV2.Readers.clsRawData rawData;
        public DeconToolsV2.Readers.clsRawData RawData
        {
            get { return rawData; }
            set { rawData = value; }
        }



        #region Public Methods
        public override int GetNumMSScans()
        {
            if (rawData == null) return 0;
            return this.rawData.GetNumScans();
        }

        public override double GetTime(int scanNum)
        {
            return this.rawData.GetScanTime(scanNum);
        }

        public override int GetMSLevel(int scanNum)
        {
            return this.rawData.GetMSLevel(scanNum);
        }

        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            Check.Require(scanset != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanset.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            int totScans = this.GetNumMSScans();


            double[] xvals = new double[0];
            double[] yvals = new double[0];

            if (scanset.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                this.rawData.GetSpectrum(scanset.IndexValues[0], ref xvals, ref yvals);
            }
            else
            {
                throw new NotImplementedException("Summing is not yet supported for MZXML files.");
                //this.rawData.GetSummedSpectra(scanset.getLowestScanNumber(), scanset.getHighestScanNumber(), minMZ, maxMZ, ref xvals, ref yvals);
            }


            this.xyData.SetXYValues(ref xvals, ref yvals);
        }

        #endregion


    }
}
