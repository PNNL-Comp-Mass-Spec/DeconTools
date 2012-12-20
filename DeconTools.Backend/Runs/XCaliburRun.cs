using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    [Obsolete("Not used anymore", true)]
    [Serializable]
    public sealed class XCaliburRun : DeconToolsRun
    {

        public XCaliburRun()
        {
            this.IsDataThresholded = true;
            this.MSFileType = Globals.MSFileType.Finnigan;
            this.ContainsMSMSData = true;
        }

        public XCaliburRun(string filename)
            : this()
        {


            Check.Require(File.Exists(filename),"Run not initialized. File not found");

            this.Filename = filename;


            string baseFilename = Path.GetFileName(this.Filename);

            this.DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            

            this.DataSetPath = Path.GetDirectoryName(filename);


            try
            {

                RawData = new DeconToolsV2.Readers.clsRawData(filename, DeconToolsV2.Readers.FileType.FINNIGAN);
            }
            catch (Exception ex)
            {

                throw new Exception("ERROR:  Couldn't open the file.  Details: " + ex.Message);
            }
            this.MinLCScan = GetMinPossibleLCScanNum();        //  remember that DeconEngine is 1-based
            this.MaxLCScan = GetMaxPossibleLCScanNum();


        }

        public XCaliburRun(string filename, int minScan, int maxScan)
            : this(filename)
        {
            this.MinLCScan = minScan;
            this.MaxLCScan = maxScan;
        }

        #region Properties



        #endregion

        #region Methods

        public override int GetMinPossibleLCScanNum()
        {
            return 1;
        }
        public override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans();        // xcalbur scans are 1-based
        }




        public override void GetMassSpectrum(DeconTools.Backend.Core.ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            int totScans = this.GetNumMSScans();
            bool alreadyFiltered = false;

            double[] xvals = new double[0];
            double[] yvals = new double[0];

            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                this.RawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals);
            }
            else
            {
                this.getSummedSpectrum(scanSet, ref xvals, ref yvals, minMZ, maxMZ);
                alreadyFiltered = true;       //summing will filter the values.... no need to repeat it below.
            }

            if (this.XYData == null)
            {
                this.XYData = new XYData();
            }


            this.XYData.SetXYValues(ref xvals, ref yvals);
            if (alreadyFiltered) return;

            if (XYData.Xvalues == null || XYData.Xvalues.Length == 0) return;
            bool needsFiltering = (minMZ > this.XYData.Xvalues[0] || maxMZ < this.XYData.Xvalues[this.XYData.Xvalues.Length - 1]);
            if (needsFiltering)
            {
                this.FilterXYPointsByMZRange(minMZ, maxMZ);
            }

        }

        public void getSummedSpectrum(ScanSet scanSet, ref double[] xvals, ref double[] yvals, double minX, double maxX)
        {
            // [gord] idea borrowed from Anuj! Jan 2010 

            //the idea is to convert the mz value to a integer. To avoid losing precision, we multiply it by 'precision'
            //the integer is added to a dictionary generic list (sorted)
            //

            SortedDictionary<long, double> mz_intensityPair = new SortedDictionary<long, double>();
            double precision = 1e5;   // if the precision is set too high, can get artifacts in which the intensities for two m/z values should be added but are separately registered. 
            double[] tempXvals = new double[0];
            double[] tempYvals = new double[0];

            long minXLong = (long)(minX * precision + 0.5);
            long maxXLong = (long)(maxX * precision + 0.5);
            for (int scanCounter = 0; scanCounter < scanSet.IndexValues.Count; scanCounter++)
            {
                this.RawData.GetSpectrum(scanSet.IndexValues[scanCounter], ref tempXvals, ref tempYvals);

                for (int i = 0; i < tempXvals.Length; i++)
                {
                    long tempmz = (long)Math.Floor(tempXvals[i] * precision + 0.5);
                    if (tempmz < minXLong || tempmz > maxXLong) continue;

                    if (mz_intensityPair.ContainsKey(tempmz))
                    {
                        mz_intensityPair[tempmz] += tempYvals[i];
                    }
                    else
                    {
                        mz_intensityPair.Add(tempmz, tempYvals[i]);
                    }
                }
            }

            if (mz_intensityPair.Count == 0) return;
            List<long> summedXVals = mz_intensityPair.Keys.ToList();

            xvals = new double[summedXVals.Count];
            yvals = mz_intensityPair.Values.ToArray();

            for (int i = 0; i < summedXVals.Count; i++)
            {
                xvals[i] = summedXVals[i] / precision;
            }
        }

        #endregion








    }
}
