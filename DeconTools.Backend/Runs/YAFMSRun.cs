using System;//SK
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using YafmsLibrary;

namespace DeconTools.Backend.Runs
{
    [Obsolete("Unsupported file format")]
    public sealed class YAFMSRun : Run
    {
        readonly YafmsReader m_reader;

        #region Constructors

        public YAFMSRun()
        {
            XYData = new XYData();
            MSFileType = Globals.MSFileType.YAFMS;
            ContainsMSMSData = false;

            //SpectraID is specific to the YAFMS schema. Default is '1'.
            SpectraID = 1;
        }

        public YAFMSRun(string filename)
            : this()
        {
            Check.Require(File.Exists(filename), "Cannot find file - does not exist.");

            Filename = filename;
            var baseFilename = Path.GetFileName(Filename);
            DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            DataSetPath = Path.GetDirectoryName(filename);

            m_reader = new YafmsReader();
            try
            {
                m_reader.OpenYafms(Filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
                throw;
            }

            MinLCScan = GetMinPossibleLCScanNum();
            MaxLCScan = GetMaxPossibleLCScanNum();
        }

        public YAFMSRun(string fileName, int minScan, int maxScan)
            : this(fileName)
        {
            //MinScan has been already defined by the alternate constructor. The inputted parameters must be within the base MinScan and MaxScan.

            Check.Require(minScan >= MinLCScan, "Cannot initialize YAFMS run. Inputted minScan is lower than the minimum possible scan number.");
            Check.Require(maxScan <= MaxLCScan, "Cannot initialize YAFMS run. Inputted MaxScan is greater than the maximum possible scan number.");

            MinLCScan = minScan;
            MaxLCScan = maxScan;
        }

        #endregion

        #region Properties
        /// <summary>
        /// SpectraID is specific to the YafMS schema. Default is '1'
        /// </summary>
        public int SpectraID { get; set; }

        public override XYData XYData { get; set; }

        #endregion

        #region Public Methods

        public override int GetMinPossibleLCScanNum()
        {

            //TODO: YAFMS is a universal format. So sometimes the ScanNum might be 1-based (e.g. if created from XCalibur data)
            return 0;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans();
        }


        public override XYData GetMassSpectrum(ScanSet scanset)
        {
            //if (scanset.IndexValues.Count > 1)
            //{
            //    throw new NotImplementedException("Summing was attempted on YafMS data, but summing hasn't been implemented");
            //}
            //double[] xvals = null;
            //float[] yvals = null;

            // m_reader.GetSpectrum(this.SpectraID, scanset.PrimaryScanNumber, ref xvals, ref yvals);

            //TODO: This simple scanset only method is not implemented in XCaliburRun

            double[] xvals = null;
            float[] yvals = null;

            if (scanset.IndexValues.Count <= 1)
            {
                m_reader.GetSpectrum(SpectraID, scanset.PrimaryScanNumber, ref xvals, ref yvals);
            }
            else
            {
                xvals = new double[0];
                yvals = new float[0];

                getSummedSpectrum(scanset, ref xvals, ref yvals);
            }

            var xydata = new XYData
            {
                Xvalues = xvals,
                Yvalues = yvals.Select(p => (double)p).ToArray()
            };
            return xydata;
        }

        public override XYData GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            //TODO: Update upon error fix....  the YAFMS library is throwing an error if I give an m/z outside it's expected range. So until that is fixed, I'll go get all the m/z values and trim them myself

            double[] xvals = null;
            float[] yvals = null;

            if (scanset.IndexValues.Count <= 1)
            {
                m_reader.GetSpectrum(SpectraID, scanset.PrimaryScanNumber, ref xvals, ref yvals);
            }
            else
            {
                xvals = new double[0];
                yvals = new float[0];

                getSummedSpectrum(scanset, ref xvals, ref yvals, minMZ, maxMZ);
            }

            var xydata = new XYData
            {
                Xvalues = xvals,
                Yvalues = yvals.Select(p => (double)p).ToArray()
            };
            return xydata;
        }

        public void getSummedSpectrum(ScanSet scanSet, ref double[] xvals, ref float[] yvals, double minX, double maxX)
        {
            // [gord] idea borrowed from Anuj! Jan 2010

            //the idea is to convert the mz value to a integer. To avoid losing precision, we multiply it by 'precision'
            //the integer is added to a dictionary generic list (sorted)

            var mz_intensityPair = new SortedDictionary<long, float>();
            var precision = 1e6;   // if the precision is set too high, can get artifacts in which the intensities for two m/z values should be added but are separately registered.
            //double[] tempXvals = new double[0];
            //float[] tempYvals = new float[0];
            double[] tempXvals = null;
            float[] tempYvals = null;

            var minXLong = (long)(minX * precision + 0.5);
            var maxXLong = (long)(maxX * precision + 0.5);
            foreach (var scanNum in scanSet.IndexValues)
            {
                //this.RawData.GetSpectrum(scanSet.IndexValues[scanCounter], ref tempXvals, ref tempYvals);
                m_reader.GetSpectrum(SpectraID, scanNum, ref tempXvals, ref tempYvals);

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

        public void getSummedSpectrum(ScanSet scanSet, ref double[] xvals, ref float[] yvals)
        {
            // [gord] idea borrowed from Anuj! Jan 2010

            //the idea is to convert the mz value to a integer. To avoid losing precision, we multiply it by 'precision'
            //the integer is added to a dictionary generic list (sorted)
            //

            var mz_intensityPair = new SortedDictionary<long, float>();
            var precision = 1e6;   // if the precision is set too high, can get artifacts in which the intensities for two m/z values should be added but are separately registered.
            //double[] tempXvals = new double[0];
            //float[] tempYvals = new float[0];
            double[] tempXvals = null;
            float[] tempYvals = null;

            //long minXLong = (long)(minX * precision + 0.5);
            //long maxXLong = (long)(maxX * precision + 0.5);
            foreach (var scanNum in scanSet.IndexValues)
            {
                //this.RawData.GetSpectrum(scanSet.IndexValues[scanCounter], ref tempXvals, ref tempYvals);
                m_reader.GetSpectrum(SpectraID, scanNum, ref tempXvals, ref tempYvals);

                for (var i = 0; i < tempXvals.Length; i++)
                {
                    var tempmz = (long)Math.Floor(tempXvals[i] * precision + 0.5);
                    //if (tempmz < minXLong || tempmz > maxXLong) continue;

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

        public override int GetNumMSScans()
        {
            var numScans = m_reader.GetTotalNumberScans();
            return numScans;
        }

        public override double GetTime(int scanNum)
        {
            return m_reader.GetRetentionTime(SpectraID, scanNum);
        }

        public override void Close()
        {
            base.Close();

            m_reader?.CloseYafms();
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {

            var msLevel = m_reader.GetMSLevel(SpectraID, scanNum);

            return msLevel;
        }

        #endregion

    }
}
