using System;
using System.IO;
using System.Text.RegularExpressions;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using PNNLOmics.Data;



namespace DeconTools.Backend.Runs
{
    public class XCaliburRun2 : Run
    {

        private MSFileReaderLib.MSFileReader_XRawfile _msfileReader;


        #region Constructors
        public XCaliburRun2()
        {
            _msfileReader = new MSFileReaderLib.MSFileReader_XRawfile();


            this.IsDataThresholded = true;
            this.MSFileType = Globals.MSFileType.Finnigan;
            this.ContainsMSMSData = true;
            XYData = new XYData();

        }

        public XCaliburRun2(string filename)
            : this()
        {
            Check.Require(File.Exists(filename), "Run not initialized. File not found");

            this.Filename = filename;
            string baseFilename = Path.GetFileName(this.Filename);
            this.DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            this.DataSetPath = Path.GetDirectoryName(filename);

            _msfileReader.Open(Filename);
            _msfileReader.SetCurrentController(0, 1);

            this.MinLCScan = 1;
            this.MaxLCScan = this.GetNumMSScans();



        }

        public XCaliburRun2(string filename, int minScan, int maxScan)
            : this(filename)
        {
            this.MinLCScan = minScan;
            this.MaxLCScan = maxScan;
        }

        #endregion

        public override XYData XYData { get; set; }

        public override int GetNumMSScans()
        {
            int numSpectra = 0;

            _msfileReader.GetNumSpectra(ref numSpectra);
            return numSpectra;
        }

        public override double GetTime(int scanNum)
        {
            double RTForAGivenScan = 0;
            _msfileReader.RTFromScanNum(scanNum, ref RTForAGivenScan);
            return RTForAGivenScan;
        }

        public override int GetMinPossibleLCScanNum()
        {
            return 1;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            int maxpossibleScanIndex = GetNumMSScans();           // RAW files are 1 based, so we don't subtract 1 here. 
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

            int msLevel = 1;

            if (filter == null)
            {
                return 1;
            }

            int indexOfMSReference = filter.IndexOf("ms");
            if (indexOfMSReference == -1)
            {
                msLevel = 1;
            }
            else if (indexOfMSReference < filter.Length - 2)  // ensure we aren't at the end of the filter string 
            {
                char mslevelFromFilter = filter[indexOfMSReference + 2];

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

        public override PrecursorInfo GetPrecursorInfo(int scanNum)
        {
            PrecursorInfo precursor = new PrecursorInfo();

            string scanFilterString = null;
            _msfileReader.GetFilterForScanNum(scanNum, ref scanFilterString);

            //Get MS Level
            precursor.MSLevel = GetMSLevel(scanNum);


            //Get Precursor MZ
            if (scanFilterString == null)
            {
                precursor.PrecursorMZ = -1;

            }
            else
            {
                precursor.PrecursorMZ = ParseMZValueFromThermoScanInfo(scanFilterString);
            }

            //Get the Parent scan if MS level is not MS1
            if (precursor.MSLevel>1)
            {
                int stepBack = 0;
                while (scanNum - stepBack > 0)
                {
                    if (scanNum - stepBack > 0)
                    {
                        int testScanLevel = GetMSLevel(scanNum - stepBack);
                        stepBack++;
                        if (testScanLevel == 1) //the first precursor scan prior
                        {
                            break;
                        }
                    }
                }
                precursor.PrecursorScan = scanNum - (stepBack - 1);
            }
            else
            {
                precursor.PrecursorScan = scanNum;
            }

           


            //TODO: we still need to get charge
            //precursor.PrecursorCharge = 1;

            return precursor;
        }


        private double ParseMZValueFromThermoScanInfo(string scanInfo)
        {
            double precursorMass = 0;

            //TODO: we might need to improve this.  Seems to be geared towards CID only
            string patternCid = @"(?<mz>[0-9.]+)@cid";

            var matchCid = Regex.Match(scanInfo, patternCid);

            if (matchCid.Success)
            {
                precursorMass = Convert.ToDouble(matchCid.Groups["mz"].Value);
            }
            else
            {
                precursorMass = -1;
            }

            if (precursorMass < 0)//if still -1, check for hcd
            {
                string patternHcd = @"(?<mz>[0-9.]+)@hcd";

                var matchHcd = Regex.Match(scanInfo, patternHcd);

                if (matchHcd.Success)
                {
                    precursorMass = Convert.ToDouble(matchHcd.Groups["mz"].Value);
                }
                else
                {
                    precursorMass = -1;
                }
            }
            return precursorMass;
        }

        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            Check.Require(scanset != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanset.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            double[,] vals;

            bool spectraAreSummed = scanset.IndexValues.Count > 1;
            if (spectraAreSummed)
            {
                int scanNumFirst = scanset.IndexValues[0];
                int scanNumLast = scanset.IndexValues[scanset.IndexValues.Count - 1];
                int backgroundScan1First = 0;
                int backgroundScan1Last = 0;
                int backgroundScan2First = 0;
                int backgroundScan2Last = 0;
                string filter = "p full ms";   //only sum MS1 data

                int intensityCutoffType = 0;
                int intensityCutoffValue = 0;
                int maxNumberOfPeaks = 0;
                int centroidResult = 0;
                double centVal = 0;
                object massList = null;
                object peakFlags = null;
                int arraySize = 0;

                _msfileReader.GetAverageMassList(ref scanNumFirst, ref scanNumLast, ref backgroundScan1First, ref backgroundScan1Last, ref backgroundScan2First, ref backgroundScan2Last,
             filter, intensityCutoffType, intensityCutoffValue, maxNumberOfPeaks, centroidResult, ref centVal, ref massList, ref peakFlags, ref arraySize);

                vals = (double[,])massList;
            }
            else
            {
                int scanNum = scanset.PrimaryScanNumber;
                string filter = null;
                int intensityCutoffType = 0;
                int intensityCutoffValue = 0;
                int maxNumberOfPeaks = 0;
                int centroidResult = 0;

                double centVal = 0;
                object massList = null;
                object peakFlags = null;
                int arraySize = 0;

                _msfileReader.GetMassListFromScanNum(ref scanNum, filter, intensityCutoffType, intensityCutoffValue, maxNumberOfPeaks, centroidResult, ref centVal, ref massList, ref peakFlags, ref arraySize);

                vals = (double[,])massList;
            }

            if (vals == null) return;

            double[] xvals = new double[vals.GetLength(1)];
            double[] yvals = new double[vals.GetLength(1)];

            for (int i = 0; i < vals.GetLength(1); i++)
            {
                xvals[i] = vals[0, i];
                yvals[i] = vals[1, i];
            }

            this.XYData.SetXYValues(ref xvals, ref yvals);

            if (XYData.Xvalues == null || XYData.Xvalues.Length == 0) return;
            bool needsFiltering = (minMZ > this.XYData.Xvalues[0] || maxMZ < this.XYData.Xvalues[this.XYData.Xvalues.Length - 1]);
            if (needsFiltering)
            {
                this.FilterXYPointsByMZRange(minMZ, maxMZ);
            }
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
                int testScan = scanLC;
                while (testScan >= MinLCScan)
                {
                    int currentMSLevel = GetMSLevel(testScan);

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

        private double getMZFromScanInfo(string scanInfo)
        {
            string pattern = @"(?<mz>[0-9.]+)@\w";
            var match = Regex.Match(scanInfo, pattern);

            double mzScanInfo = 0;
            if (match.Success)
            {
                mzScanInfo = Convert.ToDouble(match.Groups["mz"].Value);
            }
            return mzScanInfo;
        }

        public override void Close()
        {
            _msfileReader.Close();
            base.Close();
        }
    }
}
