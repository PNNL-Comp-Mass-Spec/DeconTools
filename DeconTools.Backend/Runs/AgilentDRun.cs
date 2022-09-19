using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Agilent.MassSpectrometry.DataAnalysis;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using PNNLOmics.Data;
using IonizationMode = PNNLOmics.Data.IonizationMode;

namespace DeconTools.Backend.Runs
{
    public sealed class AgilentDRun : Run
    {
        #region Constructors

        IMsdrDataReader m_reader;
        IBDASpecData m_spec;

        public AgilentDRun()
        {
            MSFileType = Globals.MSFileType.Agilent_D;
            ContainsMSMSData = true;    //not sure if it does, but setting it to 'true' ensures that each scan will be checked.
        }

        /// <summary>
        /// Agilent XCT .D directory
        /// </summary>
        /// <param name="dataFileName">The name of the Agilent data directory; it has a '.d' suffix</param>
        public AgilentDRun(string dataFileName)
            : this()
        {
            var dirInfo = new DirectoryInfo(dataFileName);
            var fileInfo = new FileInfo(dataFileName);

            Check.Require(!fileInfo.Exists, "Dataset's inputted name refers to a file, but should refer to a directory");
            Check.Require(dirInfo.Exists, "Dataset not found.");

            Check.Require(dirInfo.FullName.EndsWith("d", StringComparison.OrdinalIgnoreCase), "Agilent_D dataset directories must end with with the suffix '.d'. Check your directory name.");

            DatasetFileOrDirectoryPath = dirInfo.FullName;
            DatasetName = dirInfo.Name.Substring(0, dirInfo.Name.LastIndexOf(".d", StringComparison.OrdinalIgnoreCase));
            //get dataset name without .d extension
            DatasetDirectoryPath = dirInfo.FullName;

            OpenDataset();

            MinLCScan = GetMinPossibleLCScanNum();
            MaxLCScan = GetMaxPossibleLCScanNum();
        }

        public AgilentDRun(string dataFileName, int minScan, int maxScan)
            : this(dataFileName)
        {
            MinLCScan = minScan;
            MaxLCScan = maxScan;
        }

        private void OpenDataset()
        {
            m_reader = new MassSpecDataReader();
            m_reader.OpenDataFile(DatasetFileOrDirectoryPath);
        }

        #endregion

        #region Properties
        public override XYData XYData { get; set; }

        #endregion

        #region Public Methods
        public override int GetMinPossibleLCScanNum()
        {
            // AgilentD files are 0-based.
            return 0;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans() - 1;
        }

        public override int GetNumMSScans()
        {
            //m_reader=new MassSpecDataReader();
            var msScan = m_reader.FileInformation.MSScanFileInformation;
            return (int)msScan.TotalScansPresent;
        }

        public override double GetTime(int scanNum)
        {
            double time = -1;

            if (m_spec == null || m_spec.ScanId == scanNum)    // get fresh spectrum
            {
                getAgilentSpectrum(scanNum);
            }

            if (m_spec == null) return -1;

            var timeRangeArr = m_spec.AcquiredTimeRange;
            if (timeRangeArr?.Length == 1)
            {
                time = timeRangeArr[0].Start;
            }
            return time;
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            m_spec = m_reader.GetSpectrum(scanNum);

            var level = m_spec.MSLevelInfo;
            if (level == MSLevel.MS)
            {
                return 1;
            }

            if (level == MSLevel.MSMS)
            {
                return 2;
            }

            return 1;
        }

        public override PrecursorInfo GetPrecursorInfo(int scanNum)
        {
            m_spec = m_reader.GetSpectrum(scanNum);

            var precursor = new PrecursorInfo();

            var level = m_spec.MSLevelInfo;
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

            //this returns a list of precursor masses (not sure how there can be more than one)
            var precursorMZlist = m_spec.GetPrecursorIon(out var precursorMassCount);

            //if a mass is returned
            if (precursorMassCount == 1)
            {
                //mass
                precursor.PrecursorMZ = precursorMZlist[0];

                //intensity
                m_spec.GetPrecursorIntensity(out var precursorIntensity);
                precursor.PrecursorIntensity = (float)precursorIntensity;

                //charge
                m_spec.GetPrecursorCharge(out var precursorCharge);
                precursor.PrecursorCharge = precursorCharge;

                //adjust scan number if needed
                precursor.PrecursorScan = scanNum;

                // TODO: Only CID possible in Agilent files?
                precursor.FragmentationType = FragmentionType.CID;
            }
            else if (precursorMassCount > 1)
            {
                throw new NotImplementedException("Strange case where more than one precursor is used to generate one spectrum");
            }
            else
            {
                precursor.PrecursorMZ = 0;
                precursor.PrecursorIntensity = 0;
                precursor.PrecursorCharge = -1;
                precursor.PrecursorScan = scanNum;
            }

            precursor.IonizationMode = m_spec.IonPolarity == IonPolarity.Negative ? IonizationMode.Negative : IonizationMode.Positive;

            return precursor;
        }

        public override XYData GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            if (scanSet == null) return null;

            var xyData = new XYData();
            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                getAgilentSpectrum(scanSet.PrimaryScanNumber);

                var xVals = m_spec.XArray;
                var yVals = m_spec.YArray;

                FilterMassRange(minMZ, maxMZ, ref xVals, ref yVals, filterMassRange: true);

                xyData.Xvalues = xVals;
                xyData.Yvalues = yVals.Select(p => (double)p).ToArray();
            }
            else
            {
                //throw new NotImplementedException("Summing isn't supported for Agilent.D files - yet");

                //this is an implementation of Anuj's summing algorithm 4-2-2012.
                double[] xVals = null;
                float[] yVals = null;

                getSummedSpectrum(scanSet, ref xVals, ref yVals, minMZ, maxMZ);

                FilterMassRange(minMZ, maxMZ, ref xVals, ref yVals, filterMassRange: true);

                xyData.Xvalues = xVals;
                xyData.Yvalues = yVals.Select(p => (double)p).ToArray();
            }

            return xyData;
        }

        private static void FilterMassRange(double minMZ, double maxMZ, ref double[] xVals, ref float[] yVals, bool filterMassRange)
        {
            if (filterMassRange)
            {
                var xValsShortened = new List<double>();
                var yValsShortened = new List<float>();
                for (var i = 0; i < xVals.Length; i++)
                {
                    var tempMass = xVals[i];
                    if (tempMass > minMZ && tempMass < maxMZ)
                    {
                        xValsShortened.Add(xVals[i]);
                        yValsShortened.Add(yVals[i]);
                    }
                }
                xVals = xValsShortened.ToArray();
                yVals = yValsShortened.ToArray();
            }
        }

        public override XYData GetMassSpectrum(ScanSet scanSet)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            if (scanSet == null) return null;

            var xyData = new XYData();

            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                getAgilentSpectrum(scanSet.PrimaryScanNumber);

                xyData.Xvalues = m_spec.XArray;
                xyData.Yvalues = m_spec.YArray.Select(p => (double)p).ToArray();
            }
            else
            {
                //throw new NotImplementedException("Summing isn't supported for Agilent.D files - yet");

                //this is an implementation of Anuj's summing algorithm 4-2-2012.
                double[] xVals = null;
                float[] yVals = null;
                double minMZ = 0; double maxMZ = 0;
                getSummedSpectrum(scanSet, ref xVals, ref yVals, minMZ, maxMZ);
                xyData.Xvalues = xVals;
                xyData.Yvalues = yVals.Select(p => (double)p).ToArray();
            }

            return xyData;
        }

        #endregion

        #region scott

        public void getSummedSpectrum(ScanSet scanSet, ref double[] xVals, ref float[] yVals, double minMZ, double maxMZ)
        {
            // [gord] idea borrowed from Anuj! Jan 2010 [scott] brought back for agilent data that is evenly spaced

            //the idea is to convert the mz value to a integer. To avoid losing precision, we multiply it by 'precision'
            //the integer is added to a dictionary generic list (sorted)
            //

            var mz_intensityPair = new SortedDictionary<long, float>();

            var precision = 1e6;   // if the precision is set too high, can get artifacts in which the intensities for two m/z values should be added but are separately registered.

            //long minXLong = (long)(minMZ * precision + 0.5);
            //long maxXLong = (long)(maxMZ * precision + 0.5);
            for (var scanCounter = 0; scanCounter < scanSet.IndexValues.Count; scanCounter++)
            {
                //this.RawData.GetSpectrum(scanSet.IndexValues[scanCounter], ref tempXvals, ref tempYvals);
                //m_reader.GetSpectrum(this.SpectraID, scanSet.IndexValues[scanCounter], ref tempXvals, ref tempYvals);
                getAgilentSpectrum(scanSet.IndexValues[0] + scanCounter);
                var tempXvals = m_spec.XArray;
                var tempYvals = m_spec.YArray;

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
            xVals = new double[summedXVals.Count];
            yVals = mz_intensityPair.Values.ToArray();

            for (var i = 0; i < summedXVals.Count; i++)
            {
                xVals[i] = summedXVals[i] / precision;
            }
        }

        #endregion

        #region Private Methods
        private void getAgilentSpectrum(int scanNum)
        {
            m_spec = m_reader.GetSpectrum(scanNum, null, null, DesiredMSStorageType.ProfileElsePeak);
        }

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
                    Console.WriteLine("Error occurred when trying to close AgilentD file: " + DatasetName + "\nDetails: " + ex.Message);
                }
            }
            base.Dispose();
        }
    }
}
