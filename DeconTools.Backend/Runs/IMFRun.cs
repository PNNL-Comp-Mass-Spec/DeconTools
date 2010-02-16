using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Runs
{
    [Serializable]
    public class IMFRun : Run
    {

        public IMFRun()
        {
            this.xyData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.MSFileType = Globals.MSFileType.PNNL_IMS;
        }

        public IMFRun(string filename)
            : this()
        {
            this.Filename = filename;
            this.rawData = new DeconToolsV2.Readers.clsRawData(filename, DeconToolsV2.Readers.FileType.PNNL_IMS);
            this.MinScan = 0;        
            this.MaxScan = GetMaxPossibleScanIndex();
        }

        public IMFRun(string filename, int minScan, int maxScan)
            : this(filename)
        {
            this.MinScan = minScan;
            this.MaxScan = maxScan;
        }


        private DeconToolsV2.Readers.clsRawData rawData;

        public DeconToolsV2.Readers.clsRawData RawData
        {
            get { return rawData; }
            set { rawData = value; }
        }


        public override void GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");
            
            int totScans = this.GetNumMSScans();


            double[] xvals = new double[0];
            double[] yvals = new double[0];

            //if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            //{
            //    this.rawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals);
            //}
            //else
            //{
            //    int upperscan = Math.Min(scanSet.getHighestScanNumber(), this.GetNumMSScans());
            //    int lowerscan = Math.Max(scanSet.getLowestScanNumber(), 1);
            //    this.rawData.GetSummedSpectra(lowerscan, upperscan, minMZ, maxMZ, ref xvals, ref yvals);
            //}

            int upperscan = Math.Min(scanSet.getHighestScanNumber(), this.GetNumMSScans());
            int lowerscan = Math.Max(scanSet.getLowestScanNumber(), 1);
            this.rawData.GetSummedSpectra(lowerscan, upperscan, minMZ, maxMZ, ref xvals, ref yvals);

            this.xyData.SetXYValues(ref xvals, ref yvals);

        }

        public override int GetNumMSScans()
        {
            return this.rawData.GetNumScans();
        }


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


        public override double GetTime(int scanNum)
        {
            return this.rawData.GetScanTime(scanNum);
        }

        public override int GetMSLevel(int scanNum)
        {
            return this.rawData.GetMSLevel(scanNum);
        }

        
    }
}
