using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;
using PNNLOmics.Data;
using PRISM;
using ThermoRawFileReader;

namespace DeconTools.Backend.Runs
{
    public sealed class XCaliburRun2 : Run
    {
        private readonly XRawFileIO mRawFileReader;

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="thermoRawFilePath"></param>
        /// <param name="loadTuneInfo"></param>
        public XCaliburRun2(string thermoRawFilePath, bool loadTuneInfo = false)
        {
            Check.Require(File.Exists(thermoRawFilePath), "Run not initialized. File not found: " + thermoRawFilePath);

            DatasetFileOrDirectoryPath = thermoRawFilePath;
            var baseFilename = Path.GetFileName(DatasetFileOrDirectoryPath);
            if (baseFilename == null)
                throw new FileNotFoundException("Unable to determine the filename for " + thermoRawFilePath);

            DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            DatasetDirectoryPath = Path.GetDirectoryName(thermoRawFilePath);

            var options = new ThermoReaderOptions
            {
                LoadMSMethodInfo = false,
                LoadMSTuneInfo = loadTuneInfo
            };

            mRawFileReader = new XRawFileIO(DatasetFileOrDirectoryPath, options);

            IsDataThresholded = true;
            IsMsAbundanceReportedAsAverage = true;
            MSFileType = Globals.MSFileType.Thermo_Raw;
            ContainsMSMSData = true;
            XYData = new XYData();

            ParentScanList = new Dictionary<int, int>();

            MinLCScan = 1;
            MaxLCScan = GetNumMSScans();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="thermoRawFilePath"></param>
        /// <param name="minScan"></param>
        /// <param name="maxScan"></param>
        public XCaliburRun2(string thermoRawFilePath, int minScan, int maxScan)
            : this(thermoRawFilePath)
        {
            MinLCScan = minScan;
            MaxLCScan = maxScan;
        }

        #endregion

        public override XYData XYData { get; set; }

        public override bool IsDataCentroided(int scanNum)
        {
            if (!TryGetScanInfo(scanNum, out var scanInfo))
                return false;

            return scanInfo.IsCentroided;
        }

        public override double GetIonInjectionTimeInMilliseconds(int scanNum)
        {
            if (!TryGetScanInfo(scanNum, out var scanInfo))
                return 0;

            return scanInfo.IonInjectionTime;
        }

        public override double GetMS2IsolationWidth(int scanNum)
        {
            try
            {
                if (!TryGetScanInfo(scanNum, out var scanInfo))
                    return 0;

                if (scanInfo.TryGetScanEvent("MS2 Isolation Width:", out var value, true))
                {
                    return double.Parse(value, CultureInfo.InvariantCulture);
                }

                return 0;
            }
            catch (Exception ex)
            {
                ConsoleMsgUtils.ShowWarning("Warning: Exception looking up the value for \"MS2 Isolation Width\" for scan " + scanNum + ": " + ex.Message);
                return 0;
            }
        }

        public override int GetNumMSScans()
        {
            return mRawFileReader.GetNumScans();
        }

        public override double GetTime(int scanNum)
        {
            if (!TryGetScanInfo(scanNum, out var scanInfo))
                return 0;

            return scanInfo.RetentionTime;
        }

        public override int GetMinPossibleLCScanNum()
        {
            return 1;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            // Note: Thermo Raw file scans are 1 based
            var scanNumberMax = GetNumMSScans();
            return Math.Max(1, scanNumberMax);
        }

        /// <summary>
        /// Get the MS Level
        /// 1 means MS
        /// 2 means MS/MS
        /// 3 means MS^3
        /// </summary>
        /// <param name="scanNum"></param>
        /// <returns></returns>
        public override int GetMSLevelFromRawData(int scanNum)
        {
            if (!TryGetScanInfo(scanNum, out var scanInfo))
                return 0;

            return scanInfo.MSLevel;
        }

        public override double GetTICFromInstrumentInfo(int scanNum)
        {
            if (!TryGetScanInfo(scanNum, out var scanInfo))
                return 0;

            return scanInfo.TotalIonCurrent;
        }

        /// <summary>
        /// Get the names of the items tracked by .FileInfo.TuneMethods
        /// </summary>
        /// <returns></returns>
        /// <remarks>Note that tune methods are only loaded if loadTuneInfo = true when instantiating this class</remarks>
        public string GetTuneData()
        {
            if (mRawFileReader.FileInfo.TuneMethods.Count == 0)
                return string.Empty;

            var tuneMethod = mRawFileReader.FileInfo.TuneMethods[0];
            var tuneNames = new List<string>();

            foreach (var item in tuneMethod.Settings)
            {
                tuneNames.Add(item.Name);
            }

            return string.Join("\n", tuneNames);
        }

        public override PrecursorInfo GetPrecursorInfo(int scanNum)
        {
            var scanInfo = GetScanInfo(scanNum);
            XRawFileIO.ExtractParentIonMZFromFilterText(scanInfo, out var precursorMz, out var msLevel, out var fragmentationType);
            var ionMode = XRawFileIO.DetermineIonizationMode(scanInfo);

            var precursor = new PrecursorInfo
            {
                MSLevel = msLevel
            };

            // Get Precursor MZ
            if (scanInfo == null)
            {
                precursor.PrecursorMZ = 0;
            }
            else
            {
                precursor.PrecursorMZ = precursorMz;
            }

            // Get the Parent scan if MS level is not MS1
            if (precursor.MSLevel > 1)
            {
                var stepBack = 0;
                while (scanNum - stepBack > 0)
                {
                    if (scanNum - stepBack > 0)
                    {
                        var testScanLevel = GetMSLevel(scanNum - stepBack);
                        stepBack++;
                        if (testScanLevel == 1) //the first precursor scan prior
                        {
                            break;
                        }
                    }
                }

                precursor.PrecursorScan = scanNum - (stepBack - 1);

                switch (fragmentationType)
                {
                    case "hcd":
                        precursor.FragmentationType = FragmentionType.HCD;
                        break;
                    case "etd":
                        precursor.FragmentationType = FragmentionType.ETD;
                        break;
                    case "cid":
                        precursor.FragmentationType = FragmentionType.CID;
                        break;
                    default:
                        precursor.FragmentationType = FragmentionType.None;
                        break;
                }
            }
            else
            {
                precursor.PrecursorScan = scanNum;
                precursor.FragmentationType = FragmentionType.None;
            }

            precursor.IonizationMode = ionMode.Equals(IonModeConstants.Positive) ? IonizationMode.Positive : IonizationMode.Negative;

            //TODO: we still need to get charge
            //precursor.PrecursorCharge = 1;

            return precursor;
        }

        public override XYData GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            if (scanSet == null)
                return null;

            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            if (Math.Abs(minMZ) < float.Epsilon && Math.Abs(maxMZ) < float.Epsilon)
            {
                maxMZ = 10000000;
            }

            double[,] massIntensityPairs;

            var spectraAreSummed = scanSet.IndexValues.Count > 1;
            var scanNumFirst = scanSet.IndexValues[0];
            var scanNumLast = scanSet.IndexValues[scanSet.IndexValues.Count - 1];

            string scanDescription;
            if (spectraAreSummed)
            {
                scanDescription = "scan " + scanSet.PrimaryScanNumber + " ( summing scans " + scanNumFirst + " to " + scanNumLast + ")";
            }
            else
            {
                scanDescription = "scan " + scanSet.PrimaryScanNumber;
            }

            try
            {
                if (spectraAreSummed)
                {
                    // Note that GetScanDataSumScans uses the scan filter of the first scan to assure that similar scans are summed
                    mRawFileReader.GetScanDataSumScans(scanNumFirst, scanNumLast, out massIntensityPairs, 0, false);
                }
                else
                {
                    var scanNum = scanSet.PrimaryScanNumber;
                    mRawFileReader.GetScanData2D(scanNum, out massIntensityPairs);
                }
            }
            catch (AccessViolationException)
            {
                var errorMessage = "XCaliburRun2.GetMassSpectrum: Unable to load data for " + scanDescription +
                                      "; possibly a corrupt .Raw file";
                Console.WriteLine(errorMessage);
                Logger.Instance.AddEntry(errorMessage);
                return new XYData();
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("XCaliburRun2.GetMassSpectrum: Unable to load data for " + scanDescription +
                                         ": " + ex.Message + "; possibly a corrupt .Raw file");
                return new XYData();
            }

            var dataPointCount = massIntensityPairs.GetLength(1);

            var xVals = new List<double>();
            var yVals = new List<double>();

            // Note from MEM (October 2013)
            // GetMassListFromScanNum generally returns the data sorted by m/z ascending
            // However, there are edge cases for certain spectra in certain datasets where adjacent data points are out of order and need to be swapped
            // Therefore, we must validate that the data is truly sorted, and if we find a discrepancy, sort it after populating xyData.Xvalues and xyData.Yvalues
            var sortRequired = false;

            for (var i = 0; i < dataPointCount; i++)
            {
                var xValue = massIntensityPairs[0, i];

                if (xValue < minMZ || xValue > maxMZ)
                    continue;

                var yValue = massIntensityPairs[1, i];

                if (i > 0 && xValue < massIntensityPairs[0, i - 1])
                {
                    // Points are out of order; this rarely occurs but it is possible and has been observed
                    sortRequired = true;
                }

                xVals.Add(xValue);
                yVals.Add(yValue);
            }

            var xyData = new XYData
            {
                Xvalues = xVals.ToArray(),
                Yvalues = yVals.ToArray()
            };

            if (sortRequired)
            {
                Array.Sort(xyData.Xvalues, xyData.Yvalues);
            }

            return xyData;
        }

        /// <summary>
        /// Get a description of the given scan
        /// </summary>
        /// <param name="scanNum"></param>
        /// <returns>The filter text for the given scan</returns>
        public override string GetScanInfo(int scanNum)
        {
            if (!TryGetScanInfo(scanNum, out var scanInfo))
                return "Invalid scan number: " + scanNum;

            return scanInfo.FilterText;
        }

        public override int GetParentScan(int scanLC)
        {
            if (!ParentScanList.ContainsKey(scanLC))
            {
                var testScan = scanLC;
                while (testScan >= MinLCScan)
                {
                    var currentMSLevel = GetMSLevel(testScan);

                    if (currentMSLevel == 1)
                    {
                        ParentScanList.Add(scanLC, testScan);
                        return testScan;
                    }
                    testScan--;
                }

                //we got to the MinScan and never found the parent scan
                return -1;
            }

            return ParentScanList[scanLC];
        }

        public override void Close()
        {
            try
            {
                mRawFileReader.CloseRawFile();
            }
            catch (AccessViolationException)
            {
                // Ignore errors here
            }

            base.Close();
        }

        /// <summary>
        /// Gets the clsScanInfo instance for the given scan
        /// </summary>
        /// <param name="scanNumber"></param>
        /// <param name="scanInfo"></param>
        /// <returns>Bool if successful, false if the scan number is out of range</returns>
        private bool TryGetScanInfo(int scanNumber, out clsScanInfo scanInfo)
        {
            if (scanNumber < mRawFileReader.FileInfo.ScanStart || scanNumber > mRawFileReader.FileInfo.ScanEnd)
            {
                scanInfo = new clsScanInfo(0);
                return false;
            }

            var success = mRawFileReader.GetScanInfo(scanNumber, out scanInfo);
            return success;
        }
    }
}
