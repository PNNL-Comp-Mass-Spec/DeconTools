using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using UIMFLibrary;

namespace DeconTools.Backend.Runs
{

    [Serializable]
    public sealed class UIMFRun : Run
    {
        const double FramePressureStandard = 4.00000d;   // 

        /// <summary>
        /// The frame type for MS1 scans. Some older UIMF files have '0'. Currently we are moving to '1' for MS1 and '2' for MS2, according to mzXML format.
        /// </summary>
        // private DataReader.FrameType _frameTypeForMS1;
        private GlobalParameters _globalParameters;

        //private UIMFLibrary.DataReader m_reader;
        private Dictionary<int, double> _framePressuresUnsmoothed;

        // In IMS experiments that are a set number of MS2 frames with different collision energies that are run after each MS1 frame. This number will be the same for all MS1 frames.
        private int _numOfConsecutiveMs2Frames;

        #region Constructors
        public UIMFRun()
        {
            XYData = new XYData();
            MSFileType = Globals.MSFileType.PNNL_UIMF;
            IMSScanSetCollection = new IMSScanSetCollection();
            _framePressuresUnsmoothed = new Dictionary<int, double>();
            //_frameTypeForMS1 = DataReader.FrameType.MS1;   //default is MS1

        }

        public UIMFRun(string fileName)
            : this()
        {

            Check.Require(File.Exists(fileName), "UIMF file does not exist.");
            Filename = fileName;


            _globalParameters = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetGlobalParameters();

            Check.Ensure(_globalParameters != null, "UIMF file's Global parameters could not be initialized. Check UIMF file to make sure it is a valid file.");

            string baseFilename = Path.GetFileName(Filename);
            DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            DataSetPath = Path.GetDirectoryName(fileName);

            MinLCScan = GetMinPossibleLCScanNum();
            MaxLCScan = GetMaxPossibleLCScanNum();

            MinIMSScan = GetMinPossibleIMSScanNum();
            MaxIMSScan = GetMaxPossibleIMSScanNum();

            GetMSLevelInfo();
            ContainsMSMSData = CheckRunForMSMSData();

        }

        public UIMFRun(string fileName, int minFrame, int maxFrame)
            : this(fileName)
        {
            MinLCScan = minFrame;
            MaxLCScan = maxFrame;
        }

        public UIMFRun(string fileName, int minFrame, int maxFrame, int minScan, int maxScan)
            : this(fileName, minFrame, maxFrame)
        {
            this.MinIMSScan = minScan;
            this.MaxIMSScan = maxScan;

        }



        private void GetMSLevelInfo()
        {

            //TODO:  use GetMasterFrameList() from the UIMF library

            MS1Frames = new List<int>();
            MS2Frames = new List<int>();

            var ms1Frames = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetFrameNumbers(DataReader.FrameType.MS1);
            if (ms1Frames != null && ms1Frames.Length != 0)
            {
                MS1Frames = ms1Frames.ToList();
            }

            var ms2Frames = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetFrameNumbers(DataReader.FrameType.MS2);
            if (ms2Frames != null && ms2Frames.Length != 0)
            {
                MS2Frames = ms2Frames.ToList();
            }

            // Figure out the number of consecutive MS2 frames associated with each MS1 frame
            _numOfConsecutiveMs2Frames = 0;
            int numMs1Frames = MS1Frames.Count;
            int numMs2Frames = MS2Frames.Count;

            if (numMs2Frames > 0)
            {
                if (numMs1Frames == 1)
                {
                    // If there is only 1 MS1 frame, then there is only 1 set of MS2 frames, so we use the count of MS2 frames
                    _numOfConsecutiveMs2Frames = numMs2Frames;
                }
                else
                {
                    // Subtract the frame numbers of the first 2 MS1 frames to figure out how many MS2 frame are in between
                    _numOfConsecutiveMs2Frames = MS1Frames[1] - MS1Frames[0] - 1;
                }

                ValidateMSMSConsistency(MS1Frames);
            }
        }

        /// <summary>
        /// This method will verify that the number of MS/MS frame in between each MS1 frame stays constant throughout the UIMF file.
        /// </summary>
        /// <returns>True if the MS/MS data is valid. Otherwise, an exception is thrown.</returns>
        private bool ValidateMSMSConsistency(IList<int> ms1Frames)
        {
            for (int i = 1; i < ms1Frames.Count; i++)
            {
                if (ms1Frames[i] - ms1Frames[i - 1] - 1 != _numOfConsecutiveMs2Frames)
                {
                    throw new InvalidOperationException("The number of MS2 frames associated with each MS1 frame must remain constant for the entire run.\n\tERROR: An incorrect number of MS/MS frames were detected between MS1 Frame #s " + ms1Frames[i - 1] + " and " + ms1Frames[i] + ".");
                }
            }

            return true;
        }

        private bool CheckRunForMSMSData()
        {

            if (MS2Frames.Count == 0) return false;

            return true;
        }





        #endregion

        #region Properties

        public override XYData XYData { get; set; }



        public IMSScanSetCollection IMSScanSetCollection { get; set; }


        public IMSScanSet CurrentIMSScanSet { get; set; }

        public override ScanSet CurrentScanSet { get; set; }

        public int MinIMSScan { get; set; }
        public int MaxIMSScan { get; set; }

        [Obsolete("CurrentFrameSet not used anymore. Use CurrentScanSet.", true)]
        public ScanSet CurrentFrameSet { get; set; }

        public List<int> MS1Frames { get; set; }
        public List<int> MS2Frames { get; set; }


        #endregion

        #region Public Methods

        public int GetNumFrames()
        {
            if (_globalParameters == null)
            {
                _globalParameters = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetGlobalParameters();
            }
            return _globalParameters.NumFrames;
        }

        public int GetNumBins()
        {
            return _globalParameters.Bins;
        }

        public override int GetNumMSScans()
        {
            int numFrames = _globalParameters.NumFrames;
            int numScansPerFrame = 0;

            FrameParameters frameParam = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetFrameParameters(1);
            numScansPerFrame = frameParam.Scans;

            return (numScansPerFrame * numFrames);
        }

        internal int GetNumScansPerFrame()
        {
            //TODO:  check this and make sure it is correct
            int minFrame = MinLCScan;

            int numScansPerFrame = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetFrameParameters(minFrame).Scans;
            return numScansPerFrame;

        }

        public override int GetMinPossibleLCScanNum()
        {
            return 1;    //one-based frame num
        }

        public override int GetMaxPossibleLCScanNum()
        {
            int maxPossibleFrameNumber = _globalParameters.NumFrames;
            int minPossibleFrameNumber = GetMinPossibleLCScanNum();
            if (maxPossibleFrameNumber < minPossibleFrameNumber)
            {
                maxPossibleFrameNumber = minPossibleFrameNumber;
            }

            return maxPossibleFrameNumber;
        }


        public int GetMinPossibleIMSScanNum()
        {
            return 0;
        }

        public int GetMaxPossibleIMSScanNum()
        {
            return GetNumScansPerFrame() - 1;
        }




        public override int GetCurrentScanOrFrame()
        {
            return this.CurrentScanSet.PrimaryScanNumber;
        }



        /// <summary>
        /// Returns the MSLevel for the given frame
        /// </summary>
        /// <param name="scanNum">Frame number</param>
        /// <returns>1 for MS1 frames, 2 for MS2 frames, 0 for calibration frames, </returns>
        public override int GetMSLevelFromRawData(int frameNum)
        {
            if (MS1Frames.BinarySearch(frameNum) >= 0) return 1;
            if (MS2Frames.BinarySearch(frameNum) >= 0) return 2;

            FrameParameters fp = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frameNum);

            if (fp.FrameType == DataReader.FrameType.MS1) return 1;
            if (fp.FrameType == DataReader.FrameType.MS2) return 2;
            if (fp.FrameType == DataReader.FrameType.Calibration) return 0;

            return (int)1;

        }


        public override XYData GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            throw new NotImplementedException("this 'GetMassSpectrum' method is no longer supported");
        }

        /// <summary>
        /// Returns the mass spectrum for a specified LC Scanset and a IMS Scanset. 
        /// </summary>
        /// <param name="lcScanset"></param>
        /// <param name="imsScanset"></param>
        /// <param name="minMZ"></param>
        /// <param name="maxMZ"></param>
        public override XYData GetMassSpectrum(ScanSet lcScanset, ScanSet imsScanset, double minMZ, double maxMZ)
        {
            Check.Require(imsScanset.GetScanCount() > 0, "Cannot get spectrum. Number of scans in ScanSet is 0");
            Check.Require(lcScanset.GetScanCount() > 0, "Cannot get spectrum. Number of frames in FrameSet is 0");

            int frameLower = lcScanset.getLowestScanNumber();
            int frameUpper = lcScanset.getHighestScanNumber();
            int scanLower = imsScanset.getLowestScanNumber();
            int scanUpper = imsScanset.getHighestScanNumber();

            // TODO: If lowest and highest scan numbers are both 0, should we be summing the mass spectrum?

            double[] xvals = null;
            int[] yvals = null;

            var frameType = (DataReader.FrameType)GetMSLevel(lcScanset.PrimaryScanNumber);

            try
            {
                // Obtain an instance of the reader
                var uimfReader = UIMFLibraryAdapter.getInstance(Filename).Datareader;

                // Prior to January 2015 the SpectrumCache class in the UIMFReader used Dictionary<int, int> for ListOfIntensityDictionaries
                // This caused some datasets, e.g. EXP-Mix5_1um_pos_19Jan15_Columbia_DI, to run out of memory when caching 10 spectra
                // The UIMFLibrary now uses List<int, int>, which takes up less memory (at the expense having slower lookups by BinNumber, though this does not affect DeconTools' use of the UIMFLibrry)

                uimfReader.SpectraToCache = 10;
                uimfReader.MaxSpectrumCacheMemoryMB = 750;

                int nonZeroLength = uimfReader.GetSpectrum(frameLower,
                    frameUpper, frameType, scanLower, scanUpper, minMZ, maxMZ, out xvals, out yvals);

                XYData xydata = new XYData();

                if (xvals == null || xvals.Length == 0)
                {
                    xydata.Xvalues = null;
                    xydata.Yvalues = null;
                    return xydata;
                }

                xydata.Xvalues = xvals;
                xydata.Yvalues = yvals.Select<int, double>(i => i).ToArray();

                if (xydata.Xvalues[0] < minMZ || xydata.Xvalues[xydata.Xvalues.Length - 1] > maxMZ)
                {
                    xydata = xydata.TrimData(minMZ, maxMZ);
                }

                return xydata;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UIMF GetMassSpectrum: " + ex.Message);
                throw;
            }


        }

        public override double GetTime(int frameNum)
        {
            double time = 0;
            time = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frameNum).StartTime;
            return time;

        }

        public int GetNumberOfConsecutiveMs2Frames()
        {
            return _numOfConsecutiveMs2Frames;
        }

        public double GetDriftTime(int frameNum, int scanNum)
        {
            FrameParameters fp = null;
            fp = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetFrameParameters(frameNum);
            double avgTOFLength = fp.AverageTOFLength;
            double driftTime = avgTOFLength * (scanNum + 1) / 1e6;     //note that scanNum is zero-based.  Need to add one here

            var framePressure = GetFramePressure(frameNum);

            if (framePressure != 0)
            {
                driftTime = driftTime * (FramePressureStandard / framePressure);  // correc
            }

            return driftTime;


        }

        public double GetFramePressure(int frameNum)
        {
            if (!this._framePressuresUnsmoothed.ContainsKey(frameNum))
            {
                double pressure = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFramePressureForCalculationOfDriftTime(frameNum);
                this._framePressuresUnsmoothed.Add(frameNum, pressure);
            }

            return this._framePressuresUnsmoothed[frameNum];

        }

        public double GetFramePressureBack(int frameNum)
        {
            return GetFramePressure(frameNum);

        }

        public double GetFramePressureFront(int frameNum)
        {
            double framepressureFront = -1;

            framepressureFront = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetFrameParameters(frameNum).PressureFront;

            return framepressureFront;
        }

        public void SmoothFramePressuresInFrameSets()
        {
            Check.Require(ScanSetCollection != null && ScanSetCollection.ScanSetList.Count > 0, "Cannot smooth frame pressures. FrameSet collection has not been defined.");

            int numFrames = GetNumFrames();

            double numPointsToSmooth = 100;

            int lowerFrameBoundary = (int)Math.Round(numPointsToSmooth / 2) - 1;    //zero-based
            int upperFrameBoundary = (int)Math.Round(numFrames - numPointsToSmooth / 2) - 1;   //zero-based

            int maxFrame = _globalParameters.NumFrames;
            int minFrame = GetMinPossibleLCScanNum();


            foreach (LCScanSetIMS frame in ScanSetCollection.ScanSetList)
            {
                if (double.IsNaN(frame.FramePressureSmoothed))
                {
                    throw new ArgumentOutOfRangeException("Cannot smooth frame pressures.  You need to first populate frame pressures within the Frameset.");
                }

                int lowerFrame = -1;
                int upperFrame = -1;


                if (frame.PrimaryScanNumber < lowerFrameBoundary)
                {
                    lowerFrame = minFrame;
                    upperFrame = (int)(numPointsToSmooth) - 1;     // zero-based
                }
                else if (frame.PrimaryScanNumber > upperFrameBoundary)
                {
                    lowerFrame = maxFrame - (int)numPointsToSmooth + 1;
                    upperFrame = maxFrame;
                }
                else
                {
                    lowerFrame = frame.PrimaryScanNumber - (int)Math.Round(numPointsToSmooth / 2) + 1;
                    upperFrame = frame.PrimaryScanNumber + (int)Math.Round(numPointsToSmooth / 2);
                }

                //for short UIMF files (with few frames),  we need to ensure we are within bounds
                if (lowerFrame < minFrame)
                {
                    lowerFrame = minFrame;
                }

                if (upperFrame > maxFrame)
                {
                    upperFrame = maxFrame;
                }



                frame.FramePressureSmoothed = GetAverageFramePressure(lowerFrame, upperFrame);


            }

        }

        private double GetAverageFramePressure(int lowerFrame, int upperFrame)
        {
            List<double> framePressures = new List<double>();

            for (int i = lowerFrame; i <= upperFrame; i++)
            {

                double framePressure = GetFramePressure(i);
                if (framePressure > 0)
                {
                    framePressures.Add(framePressure);
                }


            }

            if (framePressures.Count > 0)
            {
                return framePressures.Average();
            }
            else
            {
                return 0;
            }
        }

        public void GetFrameDataAllFrameSets()
        {
            Check.Require(ScanSetCollection != null && ScanSetCollection.ScanSetList.Count > 0, "Cannot get frame data. FrameSet collection has not been defined.");

            Console.Write("Loading frame parameters ");
            DateTime dtLastProgress = DateTime.UtcNow;

            foreach (LCScanSetIMS frame in ScanSetCollection.ScanSetList)
            {
                FrameParameters fp = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frame.PrimaryScanNumber);
                frame.AvgTOFLength = fp.AverageTOFLength;
                frame.FramePressureUnsmoothed = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFramePressureForCalculationOfDriftTime(frame.PrimaryScanNumber);

                if (DateTime.UtcNow.Subtract(dtLastProgress).TotalSeconds >= 1)
                {
                    Console.Write(".");
                    dtLastProgress = DateTime.UtcNow;
                }
            }

            Console.WriteLine();

        }

        //public int[][] GetFramesAndScanIntensitiesForAGivenMz(int startFrame, int endFrame, int frameType, int startScan, int endScan, double targetMz, double toleranceInMZ)
        //{
        //    return UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFramesAndScanIntensitiesForAGivenMz(startFrame, endFrame, frameType, startScan, endScan, targetMz, toleranceInMZ);
        //}

        ////Anuj added this
        //for now I am converting this dictionary to a frame and scna list, only because the UIMF library already has that method
        //public void GetMassSpectrum(Dictionary<ushort, List<ushort>> frameAndScans, double minMz, double MaxMz)
        //{
        //    List<ushort> frameNumbers = frameAndScans.Keys.ToList<ushort>();
        //    List<List<ushort>> scanNumbers = new List<List<ushort>>();

        //    foreach (ushort frameNum in frameNumbers)
        //    {
        //        List<ushort> scanList = frameAndScans[frameNum];
        //        scanNumbers.Add(scanList);
        //    }

        //    GetMassSpectrum(frameNumbers, scanNumbers, minMz, MaxMz);

        //}

        ////Anuj added this
        //public void GetMassSpectrum(List<ushort> frameNumbers, List<List<ushort>> scanNumbersForFrameNumbers, double minMz, double maxMz)
        //{
        //    List<double> mzList = new List<double>();
        //    List<double> intensityList = new List<double>();
        //    UIMFLibraryAdapter.getInstance(this.Filename).Datareader.SumScansNonCached(frameNumbers, scanNumbersForFrameNumbers, mzList, intensityList, minMz, maxMz);
        //    this.XYData.Xvalues = mzList.ToArray();
        //    this.XYData.Yvalues = intensityList.ToArray();
        //}

        public Stack<int[]> GetDescendingBpiValuesByFramesAndScans()
        {
            return UIMFLibraryAdapter.getInstance(Filename).Datareader.GetFrameAndScanListByDescendingIntensity();
        }



        //public void GetDriftTimeProfile(int frameStartIndex, int frameStopIndex, int startScan, int stopScan, double targetMZ, double toleranceInMZ)
        //{
        //    int[] scanValues = null;
        //    int[] intensityVals = null;

        //    UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetDriftTimeProfile(frameStartIndex, frameStopIndex, this.FrameTypeForMS1, startScan, stopScan, targetMZ, toleranceInMZ, ref scanValues, ref intensityVals);

        //    if (scanValues == null || scanValues.Length == 0)
        //    {
        //        this.XYData.Xvalues = null;
        //        this.XYData.Yvalues = null;
        //    }
        //    else
        //    {
        //        this.XYData.Xvalues = scanValues.Select<int, double>(i => i).ToArray();
        //        this.XYData.Yvalues = intensityVals.Select<int, double>(i => i).ToArray();
        //    }

        //}


        public XYData GetDriftTimeProfile(int frameNum, int startScan, int stopScan, double targetMZ, double toleranceInMZ)
        {
            int[] scanValues = null;
            int[] intensityVals = null;

            UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetDriftTimeProfile(frameNum, frameNum, DataReader.FrameType.MS1, startScan, stopScan, targetMZ, toleranceInMZ, ref scanValues, ref intensityVals);


            XYData xydata = new XYData();

            if (scanValues == null || scanValues.Length == 0)
            {
                xydata.Xvalues = null;
                xydata.Yvalues = null;
            }
            else
            {
                xydata.Xvalues = scanValues.Select<int, double>(i => i).ToArray();
                xydata.Yvalues = intensityVals.Select<int, double>(i => i).ToArray();
            }

            return xydata;

        }




        public override void Close()
        {
            if (UIMFLibraryAdapter.getInstance(this.Filename).Datareader != null)
            {
                UIMFLibraryAdapter.getInstance(this.Filename).CloseCurrentUIMF();
            }

            base.Close();
        }

        public float GetTIC(int lcScan, int imsScan)
        {

            return (float)UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetTIC(lcScan, imsScan);


        }

        public XYData GetChromatogram(int startFrame, int stopFrame, int startIMSScan, int stopIMSScan, double targetMZ, double toleranceInPPM)
        {
            double toleranceInMZ = toleranceInPPM / 1e6 * targetMZ;
            double lowerMZ = targetMZ - toleranceInMZ;
            double upperMZ = targetMZ + toleranceInMZ;

            List<double> frameVals = new List<double>();
            List<double> intensityVals = new List<double>();

            for (int frame = startFrame; frame <= stopFrame; frame++)
            {
                var frameset = new ScanSet(frame);
                ScanSet scan = new ScanSet(startIMSScan, startIMSScan, stopIMSScan);
                var xydata = this.GetMassSpectrum(frameset, scan, lowerMZ, upperMZ);


                double sumIntensities = 0;

                if (xydata != null && xydata.Yvalues != null && xydata.Yvalues.Length > 0)
                {
                    sumIntensities = xydata.Yvalues.Sum();
                }

                frameVals.Add(frame);
                intensityVals.Add(sumIntensities);
            }

            XYData chromXYData = new XYData();


            chromXYData.Xvalues = frameVals.ToArray();
            chromXYData.Yvalues = intensityVals.ToArray();

            return chromXYData;
        }

        public override string GetCurrentScanOrFrameInfo()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Frame = ");
            if (CurrentScanSet != null)
            {
                sb.Append(CurrentScanSet.PrimaryScanNumber);
            }
            else
            {
                sb.Append("NULL");
            }

            sb.Append("; IMS_Scan= ");
            sb.Append(CurrentIMSScanSet.PrimaryScanNumber);

            return sb.ToString();
        }



        #endregion

        public int GetClosestMS1Frame(int lcScan)
        {
            if (MS1Frames == null || MS1Frames.Count == 0)
            {
                throw new ApplicationException("Cannot get closest MS1 frames. MSFrame list is empty");
            }

            if (MS1Frames.Contains(lcScan))
            {
                return lcScan;
            }

            int closestLCScan = MinLCScan;
            int smallestDiff = Int32.MaxValue;

            foreach (int t in MS1Frames)
            {
                int currentDiff = Math.Abs(t - lcScan);
                if (currentDiff < smallestDiff)
                {
                    closestLCScan = t;
                    smallestDiff = currentDiff;
                }
            }

            return closestLCScan;




        }
    }
}
