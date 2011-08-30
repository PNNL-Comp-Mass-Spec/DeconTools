using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    public class XCaliburRun2 : Run
    {

        XRAWFILE2Lib.XRawfile xraw = new XRAWFILE2Lib.XRawfile();


        #region Constructors
        public XCaliburRun2()
        {
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
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

            xraw.Open(this.Filename);

            xraw.SetCurrentController(0, 1);

            this.MinScan = 1;
            this.MaxScan = this.GetNumMSScans();



        }

        public XCaliburRun2(string filename, int minScan, int maxScan)
            : this(filename)
        {
            this.MinScan = minScan;
            this.MaxScan = maxScan;
        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public override XYData XYData { get; set; }

        public override int GetNumMSScans()
        {
            int numSpectra = 0;
            xraw.GetNumSpectra(ref numSpectra);
            return numSpectra;
        }

        public override double GetTime(int scanNum)
        {
            double RTForAGivenScan = 0;
            xraw.RTFromScanNum(scanNum, ref RTForAGivenScan);
            return RTForAGivenScan;
        }


        internal override int GetMaxPossibleScanIndex()
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
            xraw.GetFilterForScanNum(scanNum, ref filter);

            int msLevel = 1;


            if (filter == null)
            {
                return 1;
            }

            int indexOfMSReference = filter.IndexOf("ms");
            if (indexOfMSReference == -1)
            {
                msLevel= 1;
            }
            else if (indexOfMSReference < filter.Length-2)  // ensure we aren't at the end of the filter string 
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
                msLevel= 1;    //  the 'ms' was found right at the end of the scan description. Probably never happens.
            }

            return msLevel;


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

                xraw.GetAverageMassList(ref scanNumFirst, ref scanNumLast, ref backgroundScan1First, ref backgroundScan1Last, ref backgroundScan2First, ref backgroundScan2Last,
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

                xraw.GetMassListFromScanNum(ref scanNum, filter, intensityCutoffType, intensityCutoffValue, maxNumberOfPeaks, centroidResult, ref centVal, ref massList, ref peakFlags, ref arraySize);

                vals = (double[,])massList;

            }



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

        public override string GetScanInfo(ScanSet scanSet)
        {
            string filter = null;
            xraw.GetFilterForScanNum(scanSet.PrimaryScanNumber, ref filter);
            return filter;

        }

        public override void Close()
        {
            xraw.Close();
            base.Close();
        }
    }


}
