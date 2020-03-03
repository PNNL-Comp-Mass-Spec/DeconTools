using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;
using PNNLOmics.Data;
using ThermoRawFileReader;


namespace DeconTools.Backend.Runs
{
    public sealed class XCaliburRun2 : Run
    {

        private readonly MSFileReaderLib.MSFileReader_XRawfile _msFileReader;

        #region Constructors
        public XCaliburRun2()
        {
            _msFileReader = new MSFileReaderLib.MSFileReader_XRawfile();

            IsDataThresholded = true;
            IsMsAbundanceReportedAsAverage = true;
            MSFileType = Globals.MSFileType.Finnigan;
            ContainsMSMSData = true;
            XYData = new XYData();

            ParentScanList = new Dictionary<int, int>();

        }

        public XCaliburRun2(string rawfilePath)
            : this()
        {
            Check.Require(File.Exists(rawfilePath), "Run not initialized. File not found: " + rawfilePath);

            DatasetFileOrDirectoryPath = rawfilePath;
            var baseFilename = Path.GetFileName(DatasetFileOrDirectoryPath);
            if (baseFilename == null)
                throw new FileNotFoundException("Unable to determine the filename for " + rawfilePath);

            DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            DatasetDirectoryPath = Path.GetDirectoryName(rawfilePath);

            _msFileReader.Open(DatasetFileOrDirectoryPath);
            _msFileReader.SetCurrentController(0, 1);

            MinLCScan = 1;
            MaxLCScan = GetNumMSScans();

        }

        public XCaliburRun2(string filename, int minScan, int maxScan)
            : this(filename)
        {
            MinLCScan = minScan;
            MaxLCScan = maxScan;
        }

        #endregion

        public override XYData XYData { get; set; }

        public override bool IsDataCentroided(int scanNum)
        {
            var isCentroided = 0;
            _msFileReader.IsCentroidScanForScanNum(scanNum, ref isCentroided);

            return isCentroided != 0;
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public override double GetIonInjectionTimeInMilliseconds(int scanNum)
        {
            try
            {
                object value = null;
                _msFileReader.GetTrailerExtraValueForScanNum(scanNum, "Ion Injection Time (ms):", ref value);

                return Convert.ToDouble(value);
            }
            catch (AccessViolationException ex)
            {
                Console.WriteLine("Warning: Exception calling _msFileReader.GetTrailerExtraValueForScanNum for scan " + scanNum + ": " + ex.Message);
                return 0;
            }
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public override double GetMS2IsolationWidth(int scanNum)
        {
            try
            {
                object value = null;
                _msFileReader.GetTrailerExtraValueForScanNum(scanNum, "MS2 Isolation Width:", ref value);

                return Convert.ToDouble(value);
            }
            catch (AccessViolationException ex)
            {
                Console.WriteLine("Warning: Exception calling _msFileReader.GetTrailerExtraValueForScanNum for scan " + scanNum + ": " + ex.Message);
                return 0;
            }
        }

        public override int GetNumMSScans()
        {
            var numSpectra = 0;

            _msFileReader.GetNumSpectra(ref numSpectra);
            return numSpectra;
        }

        public override double GetTime(int scanNum)
        {
            double rtForAGivenScan = 0;
            _msFileReader.RTFromScanNum(scanNum, ref rtForAGivenScan);
            return rtForAGivenScan;
        }

        public override int GetMinPossibleLCScanNum()
        {
            return 1;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            var maxPossibleScanIndex = GetNumMSScans();           // RAW files are 1 based, so we don't subtract 1 here.
            if (maxPossibleScanIndex < 1) maxPossibleScanIndex = 1;

            return maxPossibleScanIndex;
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            // Parse the scan's filter string to determine the MS Level (typically MS1, MS2, or MS3)

            // Example of MS1:
            // FTMS + p NSI Full ms [400.00-2000.00]

            // Example of MS2:
            // ITMS + c NSI d Full ms2 408.25@cid35.00 [100.00-420.00]

            string filter = null;
            _msFileReader.GetFilterForScanNum(scanNum, ref filter);

            if (string.IsNullOrWhiteSpace(filter))
            {
                return 1;
            }

            if (XRawFileIO.ExtractMSLevel(filter, out var msLevel, out var _))
            {
                return msLevel;
            }

            return 1;

        }

        public double GetCollisionEnergyInfoFromInstrumentInfo(int scanNum)
        {
            return 0;
        }

        public override double GetTICFromInstrumentInfo(int scanNum)
        {
            int pnNumPackets = 0, pnNumChannels = 0, pbUniformTime = 0;
            double pdStartTime = 0, pdLowMass = 0, pdHighMass = 0, pdTIC = 0, pdBasePeakMass = 0, pdBasePeakIntensity = 0, pdFrequency = 0;
            _msFileReader.GetScanHeaderInfoForScanNum(scanNum, ref pnNumPackets, ref pdStartTime, ref pdLowMass, ref pdHighMass, ref pdTIC,
                                                      ref pdBasePeakMass, ref pdBasePeakIntensity, ref pnNumChannels, ref pbUniformTime,
                                                      ref pdFrequency);

            return pdTIC;
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public string GetTuneData()
        {
            try
            {

                // ReSharper disable IdentifierTypo
                object pvarLabels = null, pvarValues = null;
                // ReSharper restore IdentifierTypo

                var pnArraySize = 0;
                _msFileReader.GetTuneData(0, ref pvarLabels, ref pvarValues, ref pnArraySize);

                var labels = (string[])pvarLabels;
                var values = (string[])pvarValues;

                for (var index = 0; index < labels.Length; index++)
                {
                    var label = labels[index];
                    var value = values[index];

                    Console.WriteLine(label + "\t" + value);
                }

                return labels.ToString();
            }
            catch (AccessViolationException ex)
            {
                Console.WriteLine("Warning: Exception calling _msFileReader.GetTuneData: " + ex.Message);
                return string.Empty;
            }

        }

        public override PrecursorInfo GetPrecursorInfo(int scanNum)
        {

            var scanInfo = GetScanInfo(scanNum);
            XRawFileIO.ExtractParentIonMZFromFilterText(scanInfo, out var precursorMz, out var msLevel, out var fragmentationType);
            var ionMode = XRawFileIO.DetermineIonizationMode(scanInfo);

            var precursor = new PrecursorInfo {
                MSLevel = msLevel
            };

            //Get Precursor MZ
            if (scanInfo == null)
            {
                precursor.PrecursorMZ = -1;
            }
            else
            {
                precursor.PrecursorMZ = precursorMz;
            }

            //Get the Parent scan if MS level is not MS1
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

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public override XYData GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            // Note that we're using function attribute HandleProcessCorruptedStateExceptions
            // to force .NET to properly catch critical errors thrown by the XRawFile DLL

            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            if (scanSet == null)
                return null;

            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            double[,] vals = null;
            var spectraAreSummed = scanSet.IndexValues.Count > 1;
            var scanNumFirst = scanSet.IndexValues[0];
            var scanNumLast = scanSet.IndexValues[scanSet.IndexValues.Count - 1];

            string scanDescription;
            if (spectraAreSummed)
            {
                scanDescription = "scan " + scanSet.PrimaryScanNumber + "( summing scans " + scanNumFirst + " to " + scanNumLast + ")";
            }
            else
            {
                scanDescription = "scan " + scanSet.PrimaryScanNumber;
            }

            try
            {


                if (spectraAreSummed)
                {
                    var backgroundScan1First = 0;
                    var backgroundScan1Last = 0;
                    var backgroundScan2First = 0;
                    var backgroundScan2Last = 0;
                    const string filter = "p full ms"; //only sum MS1 data

                    const int intensityCutoffType = 0;
                    const int intensityCutoffValue = 0;
                    const int maxNumberOfPeaks = 0;
                    const int centroidResult = 0;
                    double centVal = 0;
                    object massList = null;
                    object peakFlags = null;
                    var arraySize = 0;

                    _msFileReader.GetAverageMassList(
                        ref scanNumFirst,
                        ref scanNumLast,
                        ref backgroundScan1First,
                        ref backgroundScan1Last,
                        ref backgroundScan2First,
                        ref backgroundScan2Last,
                        filter,
                        intensityCutoffType,
                        intensityCutoffValue,
                        maxNumberOfPeaks,
                        centroidResult,
                        ref centVal,
                        ref massList,
                        ref peakFlags,
                        ref arraySize);

                    vals = (double[,])massList;
                }
                else
                {
                    var scanNum = scanSet.PrimaryScanNumber;

                    const int intensityCutoffType = 0;
                    const int intensityCutoffValue = 0;
                    const int maxNumberOfPeaks = 0;
                    const int centroidResult = 0;

                    double centVal = 0;
                    object massList = null;
                    object peakFlags = null;
                    var arraySize = 0;

                    _msFileReader.GetMassListFromScanNum(
                        ref scanNum,
                        null,
                        intensityCutoffType,
                        intensityCutoffValue,
                        maxNumberOfPeaks,
                        centroidResult,
                        ref centVal,
                        ref massList,
                        ref peakFlags,
                        ref arraySize);

                    vals = (double[,])massList;

                }
            }
            catch (AccessViolationException)
            {
                var errorMessage = "XCaliburRun2.GetMassSpectrum: Unable to load data for " + scanDescription +
                                      "; possibly a corrupt .Raw file";
                Console.WriteLine(errorMessage);
                Logger.Instance.AddEntry(errorMessage);

            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("XCaliburRun2.GetMassSpectrum: Unable to load data for " + scanDescription +
                                         ": " + ex.Message + "; possibly a corrupt .Raw file");
            }

            if (vals == null) return null;

            var length = vals.GetLength(1);

            var xVals = new List<double>();
            var yVals = new List<double>();

            // Note from MEM (October 2013)
            // GetMassListFromScanNum generally returns the data sorted by m/z ascending
            // However, there are edge cases for certain spectra in certain datasets where adjacent data points are out of order and need to be swapped
            // Therefore, we must validate that the data is truly sorted, and if we find a discrepancy, sort it after populating xyData.Xvalues and xyData.Yvalues
            var sortRequired = false;

            for (var i = 0; i < length; i++)
            {
                var xValue = vals[0, i];

                if (xValue < minMZ || xValue > maxMZ) continue;

                var yValue = vals[1, i];

                if (i > 0 && xValue < vals[0, i - 1])
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
                Array.Sort(xyData.Xvalues, xyData.Yvalues);

            return xyData;
        }

        public override string GetScanInfo(int scanNum)
        {
            string filter = null;
            _msFileReader.GetFilterForScanNum(scanNum, ref filter);
            return filter;
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

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public override void Close()
        {
            try
            {
                _msFileReader.Close();
            }
            catch (AccessViolationException)
            {
               // Ignore errors here
            }

            base.Close();
        }
    }
}
