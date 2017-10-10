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
    public class XCaliburRun2 : Run
    {

        private readonly MSFileReaderLib.MSFileReader_XRawfile _msfileReader;

        #region Constructors
        public XCaliburRun2()
        {
            _msfileReader = new MSFileReaderLib.MSFileReader_XRawfile();

            IsDataThresholded = true;
            IsMsAbundanceReportedAsAverage = true;
            MSFileType = Globals.MSFileType.Finnigan;
            ContainsMSMSData = true;
            XYData = new XYData();

            ParentScanList = new Dictionary<int, int>();

        }

        public XCaliburRun2(string filename)
            : this()
        {
            Check.Require(File.Exists(filename), "Run not initialized. File not found: " + filename);

            Filename = filename;
            var baseFilename = Path.GetFileName(Filename);
            if (baseFilename == null)
                throw new FileNotFoundException("Unable to determine the filename for " + Filename);

            DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            DataSetPath = Path.GetDirectoryName(filename);

            _msfileReader.Open(Filename);
            _msfileReader.SetCurrentController(0, 1);

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
            _msfileReader.IsCentroidScanForScanNum(scanNum, ref isCentroided);

            return isCentroided != 0;
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public override double GetIonInjectionTimeInMilliseconds(int scanNum)
        {
            try
            {
                object value = null;
                _msfileReader.GetTrailerExtraValueForScanNum(scanNum, "Ion Injection Time (ms):", ref value);

                return Convert.ToDouble(value);
            }
            catch (AccessViolationException ex)
            {
                Console.WriteLine("Warning: Exception calling _msfileReader.GetTrailerExtraValueForScanNum for scan " + scanNum + ": " + ex.Message);
                return 0;
            }
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public override double GetMS2IsolationWidth(int scanNum)
        {
            try
            {
                object value = null;
                _msfileReader.GetTrailerExtraValueForScanNum(scanNum, "MS2 Isolation Width:", ref value);

                return Convert.ToDouble(value);
            }
            catch (AccessViolationException ex)
            {
                Console.WriteLine("Warning: Exception calling _msfileReader.GetTrailerExtraValueForScanNum for scan " + scanNum + ": " + ex.Message);
                return 0;
            }
        }

        public override int GetNumMSScans()
        {
            var numSpectra = 0;

            _msfileReader.GetNumSpectra(ref numSpectra);
            return numSpectra;
        }

        public override double GetTime(int scanNum)
        {
            double rtForAGivenScan = 0;
            _msfileReader.RTFromScanNum(scanNum, ref rtForAGivenScan);
            return rtForAGivenScan;
        }

        public override int GetMinPossibleLCScanNum()
        {
            return 1;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            var maxpossibleScanIndex = GetNumMSScans();           // RAW files are 1 based, so we don't subtract 1 here.
            if (maxpossibleScanIndex < 1) maxpossibleScanIndex = 1;

            return maxpossibleScanIndex;
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            //Thermo's API doesn't seem to expose an easy method for getting the MS Level
            //so we have to get it from the 'Filter' or scan description string

            //example of MS1:
            //FTMS + p NSI Full ms [400.00-2000.00]

            //example of MS2:
            //ITMS + c NSI d Full ms2 408.25@cid35.00 [100.00-420.00]

            string filter = null;
            _msfileReader.GetFilterForScanNum(scanNum, ref filter);

            var msLevel = 1;

            if (filter == null)
            {
                return 1;
            }

            var indexOfMSReference = filter.IndexOf("ms");
            if (indexOfMSReference == -1)
            {
                msLevel = 1;
            }
            else if (indexOfMSReference < filter.Length - 2)  // ensure we aren't at the end of the filter string
            {
                var mslevelFromFilter = filter[indexOfMSReference + 2];

                switch (mslevelFromFilter)
                {
                    case ' ':
                        msLevel = 1;
                        break;
                    case '1':
                        msLevel = 1;
                        break;
                    case '2':
                        msLevel = 2;
                        break;
                    case '3':
                        msLevel = 3;
                        break;
                    case '4':
                        msLevel = 4;
                        break;
                    case '5':
                        msLevel = 5;
                        break;

                    default:
                        msLevel = 1;
                        break;
                }
            }
            else
            {
                msLevel = 1;    //  the 'ms' was found right at the end of the scan description. Probably never happens.
            }

            return msLevel;
        }

        public double GetCollisionEnergyInfoFromInstrumentInfo(int scanNum)
        {
            return 0;
        }

        public override double GetTICFromInstrumentInfo(int scanNum)
        {
            int pnNumPackets = 0, pnNumChannels = 0, pbUniformTime = 0;
            double pdStartTime = 0, pdLowMass = 0, pdHighMass = 0, pdTIC = 0, pdBasePeakMass = 0, pdBasePeakIntensity = 0, pdFrequency = 0;
            _msfileReader.GetScanHeaderInfoForScanNum(scanNum, ref pnNumPackets, ref pdStartTime, ref pdLowMass, ref pdHighMass, ref pdTIC,
                                                      ref pdBasePeakMass, ref pdBasePeakIntensity, ref pnNumChannels, ref pbUniformTime,
                                                      ref pdFrequency);

            return pdTIC;
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public string GetTuneData()
        {
            try
            {

                object pvarLabels = null, pvarValues = null;
                var pnArraySize = 0;
                _msfileReader.GetTuneData(0, ref pvarLabels, ref pvarValues, ref pnArraySize);

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
                Console.WriteLine("Warning: Exception calling _msfileReader.GetTuneData: " + ex.Message);
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
        public override XYData GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            // Note that we're using function attribute HandleProcessCorruptedStateExceptions
            // to force .NET to properly catch critical errors thrown by the XRawfile DLL

            Check.Require(scanset != null, "Can't get mass spectrum; inputted set of scans is null");
            if (scanset == null)
                return null;

            Check.Require(scanset.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            double[,] vals = null;
            var spectraAreSummed = scanset.IndexValues.Count > 1;
            var scanNumFirst = scanset.IndexValues[0];
            var scanNumLast = scanset.IndexValues[scanset.IndexValues.Count - 1];

            string scanDescription;
            if (spectraAreSummed)
            {
                scanDescription = "scan " + scanset.PrimaryScanNumber + "( summing scans " + scanNumFirst + " to " + scanNumLast + ")";
            }
            else
            {
                scanDescription = "scan " + scanset.PrimaryScanNumber;
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

                    _msfileReader.GetAverageMassList(
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
                    var scanNum = scanset.PrimaryScanNumber;

                    const int intensityCutoffType = 0;
                    const int intensityCutoffValue = 0;
                    const int maxNumberOfPeaks = 0;
                    const int centroidResult = 0;

                    double centVal = 0;
                    object massList = null;
                    object peakFlags = null;
                    var arraySize = 0;

                    _msfileReader.GetMassListFromScanNum(
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

            var xvals = new List<double>();
            var yvals = new List<double>();

            // Note from MEM (October 2013)
            // GetMassListFromScanNum generally returns the data sorted by m/z ascending
            // However, there are edge cases for certain spectra in certain datasets where adjacent data points are out of order and need to be swapped
            // Therefore, we must validate that the data is truly sorted, and if we find a discrepancy, sort it after populating xydata.Xvalues and xydata.Yvalues
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

                xvals.Add(xValue);
                yvals.Add(yValue);
            }

            var xydata = new XYData
            {
                Xvalues = xvals.ToArray(),
                Yvalues = yvals.ToArray()
            };

            if (sortRequired)
                Array.Sort(xydata.Xvalues, xydata.Yvalues);

            return xydata;
        }

        public override string GetScanInfo(int scanNum)
        {
            string filter = null;
            _msfileReader.GetFilterForScanNum(scanNum, ref filter);
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
                _msfileReader.Close();
            }
            catch (AccessViolationException)
            {
               // Ignore errors here
            }

            base.Close();
        }
    }
}
