using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Runs
{
    [Serializable]
    public class XCaliburRun : DeconToolsRun
    {

        public XCaliburRun()
        {
            this.xyData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.IsDataThresholded = true;
            this.MSFileType = Globals.MSFileType.Finnigan;
        }

        public XCaliburRun(string filename)
            : this()
        {
            this.Filename = filename;
            
            
            try
            {
                
                this.rawData = new DeconToolsV2.Readers.clsRawData(filename, DeconToolsV2.Readers.FileType.FINNIGAN);
            }
            catch (Exception ex)
            {

                throw new Exception("ERROR:  Couldn't open the file.  Details: " + ex.Message);
            }
            this.MinScan = 1;        //  remember that DeconEngine is 1-based
            this.MaxScan = GetMaxPossibleScanIndex();
        }

        public XCaliburRun(string filename, int minScan, int maxScan)
            : this(filename)
        {
            this.MinScan = minScan;
            this.MaxScan = maxScan;
        }

        #region Properties

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

    

        #endregion

        #region Methods
        public override int GetNumMSScans()
        {
            if (rawData == null) return 0;
            return this.rawData.GetNumScans();
        }

        public override void GetMassSpectrum(DeconTools.Backend.Core.ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            int totScans = this.GetNumMSScans();


            double[] xvals = new double[0];
            double[] yvals = new double[0];

            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                this.rawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals);
            }
            else
            {
                this.rawData.GetSummedSpectra(scanSet.getLowestScanNumber(), scanSet.getHighestScanNumber(), minMZ, maxMZ, ref xvals, ref yvals);
            }


            this.xyData.SetXYValues(ref xvals, ref yvals);
            if (xyData.Xvalues == null || xyData.Xvalues.Length == 0) return;
            bool needsFiltering = (minMZ > this.xyData.Xvalues[0] || maxMZ < this.xyData.Xvalues[this.xyData.Xvalues.Length - 1]);
            if (needsFiltering)
            {
                this.FilterXYPointsByMZRange(minMZ, maxMZ);
            }

        }

        #endregion



        public override double GetTime(int scanNum)
        {
            return this.rawData.GetScanTime(scanNum);
        }





        public override int GetMSLevel(int scanNum)
        {
            return this.rawData.GetMSLevel(scanNum);
        }

        internal override int GetMaxPossibleScanIndex()
        {
            return this.GetNumMSScans();        //xcalibur data is 1-based
        }

       

        
    }
}
