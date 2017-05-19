using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
#if !Disable_DeconToolsV2
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


            Check.Require(File.Exists(filename), "Run not initialized. File not found: " + filename);

            this.Filename = filename;


            var baseFilename = Path.GetFileName(this.Filename);

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




        public override XYData GetMassSpectrum(DeconTools.Backend.Core.ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            var totScans = this.GetNumMSScans();

            var xvals = new double[0];
            var yvals = new double[0];

            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                this.RawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals);
            }
            else
            {
                this.getSummedSpectrum(scanSet, ref xvals, ref yvals, minMZ, maxMZ);
            }

            var xydata = new XYData();
            xydata.Xvalues = xvals;
            xydata.Yvalues = yvals;
            return xydata;

        }

        public void getSummedSpectrum(ScanSet scanSet, ref double[] xvals, ref double[] yvals, double minX, double maxX)
        {
            // [gord] idea borrowed from Anuj! Jan 2010 

            //the idea is to convert the mz value to a integer. To avoid losing precision, we multiply it by 'precision'
            //the integer is added to a dictionary generic list (sorted)
            //

            var mz_intensityPair = new SortedDictionary<long, double>();
            var precision = 1e5;   // if the precision is set too high, can get artifacts in which the intensities for two m/z values should be added but are separately registered. 
            var tempXvals = new double[0];
            var tempYvals = new double[0];

            var minXLong = (long)(minX * precision + 0.5);
            var maxXLong = (long)(maxX * precision + 0.5);
            for (var scanCounter = 0; scanCounter < scanSet.IndexValues.Count; scanCounter++)
            {
                this.RawData.GetSpectrum(scanSet.IndexValues[scanCounter], ref tempXvals, ref tempYvals);

                for (var i = 0; i < tempXvals.Length; i++)
                {
                    var tempmz = (long)Math.Floor(tempXvals[i] * precision + 0.5);
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
            var summedXVals = mz_intensityPair.Keys.ToList();

            xvals = new double[summedXVals.Count];
            yvals = mz_intensityPair.Values.ToArray();

            for (var i = 0; i < summedXVals.Count; i++)
            {
                xvals[i] = summedXVals[i] / precision;
            }
        }

        #endregion

    }

#endif

}
