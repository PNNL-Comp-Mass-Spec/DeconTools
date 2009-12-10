using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    public class BrukerRun : Run
    {

        public BrukerRun()
        {
            this.xyData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.IsDataThresholded = true;
            this.MSFileType = Globals.MSFileType.Bruker;
        }

        public BrukerRun(string folderName)
            : this()
        {
            this.Filename = folderName;
            try
            {
                this.rawData = new DeconToolsV2.Readers.clsRawData();
                this.rawData.LoadFile(folderName, DeconToolsV2.Readers.FileType.BRUKER);
            }
            catch (Exception ex)
            {

                throw new Exception("ERROR:  Couldn't open the file.  Details: " + ex.Message);
            }
            this.MinScan = 1;        //  remember that DeconEngine is 1-based
            this.MaxScan = GetMaxPossibleScanIndex();
        }

        public BrukerRun(string filename, int minScan, int maxScan)
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
            else    // need to sum spectra
            {
                //assume:  each scan has exactly same x values
                
                //get first spectrum
                this.rawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals);

                //
                double[] summedYvals = new double[xvals.Length];
                yvals.CopyTo(summedYvals, 0);

                for (int i = 1; i < scanSet.IndexValues.Count; i++)
                {
                    this.rawData.GetSpectrum(scanSet.IndexValues[i], ref xvals, ref yvals);

                    for (int n = 0; n < xvals.Length; n++)
                    {
                        summedYvals[n] += yvals[n];
                    }
                }

                yvals = summedYvals;
            }
            this.xyData.SetXYValues(ref xvals, ref yvals);

            if (xyData.Xvalues == null || xyData.Xvalues.Length == 0) return;
            bool needsFiltering = (minMZ > this.xyData.Xvalues[0] || maxMZ < this.xyData.Xvalues[this.xyData.Xvalues.Length - 1]);
            if (needsFiltering)
            {
                this.FilterXYPointsByMZRange(minMZ, maxMZ);
            }

        }

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
            return this.GetNumMSScans();
        }
        #endregion





    }
}
