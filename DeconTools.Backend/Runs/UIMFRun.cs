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
        const double FramePressureStandard = 4.00000d;

        /// <summary>
        /// The frame type for MS1 scans. Some older UIMF files have '0'. Currently we are moving to '1' for MS1 and '2' for MS2, according to mzXML format.
        /// </summary>
        // private DataReader.FrameType _frameTypeForMS1;
        private GlobalParams _globalParams;

        //private UIMFLibrary.DataReader m_reader;
        private readonly Dictionary<int, double> _framePressuresUnsmoothed;

        /// <summary>
        /// Stores the list of all frame numbers and their corresponding frame type
        /// </summary>
        private SortedDictionary<int, UIMFData.FrameType> _frameList;

        #region Constructors

        public UIMFRun()
        {
            XYData = new XYData();
            MSFileType = Globals.MSFileType.PNNL_UIMF;
            IMSScanSetCollection = new IMSScanSetCollection();
            _framePressuresUnsmoothed = new Dictionary<int, double>();
            //_frameTypeForMS1 = DataReader.FrameType.MS1;   //default is MS1

        }

        public UIMFRun(string uimfFilePath)
            : this()
        {

            Check.Require(File.Exists(uimfFilePath), "UIMF file does not exist.");
            DatasetFileOrDirectoryPath = uimfFilePath;

            _globalParams = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetGlobalParams();

            Check.Ensure(_globalParams != null, "UIMF file's Global parameters could not be initialized. Check UIMF file to make sure it is a valid file.");

            var baseFilename = Path.GetFileName(DatasetFileOrDirectoryPath);
            if (baseFilename != null)
            {
                DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            }
            DatasetDirectoryPath = Path.GetDirectoryName(uimfFilePath);

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
            MinIMSScan = minScan;
            MaxIMSScan = maxScan;

        }

        #endregion

        private void GetMSLevelInfo()
        {
            _frameList = new SortedDictionary<int, UIMFData.FrameType>(UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetMasterFrameList());

            MS1Frames = new List<int>();
            MS2Frames = new List<int>();

            var ms1Frames = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFrameNumbers(UIMFData.FrameType.MS1);
            if (ms1Frames != null && ms1Frames.Length != 0)
            {
                MS1Frames = ms1Frames.ToList();
            }

            var ms2Frames = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFrameNumbers(UIMFData.FrameType.MS2);
            if (ms2Frames != null && ms2Frames.Length != 0)
            {
                MS2Frames = ms2Frames.ToList();
            }
        }

        private bool CheckRunForMSMSData()
        {

            if (MS2Frames.Count == 0) return false;

            return true;
        }

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

        /// <summary>
        /// The total number of frames in the run
        /// </summary>
        /// <returns></returns>
        public int GetNumFrames()
        {
            if (_globalParams == null)
            {
                _globalParams = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetGlobalParams();
            }
            return _globalParams.NumFrames;
        }

        public int GetNumBins()
        {
            return _globalParams.Bins;
        }

        /// <summary>
        /// Return the total number of scans in this UIMF file (across all frames)
        /// </summary>
        /// <returns></returns>
        public override int GetNumMSScans()
        {

            var numScansPerFrame = GetNumScansPerFrame();

            return numScansPerFrame * (MaxLCScan - MinLCScan + 1);
        }

        /// <summary>
        /// Determine the number of scans in each frame
        /// </summary>
        /// <returns>Scan count</returns>
        /// <remarks>If frames have a varying number of scans, returns the maximum scan count in any frame</remarks>
        internal int GetNumScansPerFrame()
        {
            var minFrame = MinLCScan;
            var maxFrame = MaxLCScan;

            var reader = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader;

            var frameList = reader.GetMasterFrameList();

            // Keys in this dictionary are frame number; values are number of scans in that frame
            // Nominally all frames have the same number of scans, but this is not always the case
            var scanCountsByFrame = new Dictionary<int, int>();

            for (var frame = minFrame; frame <= maxFrame; frame++)
            {
                if (!frameList.ContainsKey(frame))
                    continue;

                var scansPerFrame = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFrameParams(frame).Scans;
                scanCountsByFrame.Add(frame, scansPerFrame);
            }

            var maxScansPerFrame = scanCountsByFrame.Values.Max();

            return maxScansPerFrame;

        }

        public override int GetMinPossibleLCScanNum()
        {
            return 1;    //one-based frame num
        }

        public override int GetMaxPossibleLCScanNum()
        {
            var maxPossibleFrameNumber = _globalParams.NumFrames;
            var minPossibleFrameNumber = GetMinPossibleLCScanNum();
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
            return GetNumScansPerFrame();
        }

        public override int GetCurrentScanOrFrame()
        {
            return CurrentScanSet.PrimaryScanNumber;
        }

        /// <summary>
        /// Returns the MSLevel for the given frame
        /// </summary>
        /// <param name="frameNum">Frame number</param>
        /// <returns>1 for MS1 frames, 2 for MS2 frames, 0 for calibration frames, </returns>
        public override int GetMSLevelFromRawData(int frameNum)
        {
            if (MS1Frames.BinarySearch(frameNum) >= 0) return 1;
            if (MS2Frames.BinarySearch(frameNum) >= 0) return 2;

            var fp = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFrameParams(frameNum);

            if (fp.FrameType == UIMFData.FrameType.MS1) return 1;
            if (fp.FrameType == UIMFData.FrameType.MS2) return 2;
            if (fp.FrameType == UIMFData.FrameType.Calibration) return 0;

            return 1;

        }

        public override XYData GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            throw new NotImplementedException("this 'GetMassSpectrum' method is no longer supported");
        }

        /// <summary>
        /// Returns the mass spectrum for a specified LC ScanSet and a IMS ScanSet.
        /// </summary>
        /// <param name="lcScanSet"></param>
        /// <param name="imsScanSet"></param>
        /// <param name="minMZ"></param>
        /// <param name="maxMZ"></param>
        public override XYData GetMassSpectrum(ScanSet lcScanSet, ScanSet imsScanSet, double minMZ, double maxMZ)
        {
            Check.Require(imsScanSet.GetScanCount() > 0, "Cannot get spectrum. Number of scans in ScanSet (imsScanSet) is 0");
            Check.Require(lcScanSet.GetScanCount() > 0, "Cannot get spectrum. Number of frames in FrameSet (lcScanSet) is 0");

            var frameLower = lcScanSet.GetLowestScanNumber();
            var frameUpper = lcScanSet.GetHighestScanNumber();
            var scanLower = imsScanSet.GetLowestScanNumber();
            var scanUpper = imsScanSet.GetHighestScanNumber();

            // TODO: If lowest and highest scan numbers are both 0, should we be summing the mass spectrum?

            var frameType = (UIMFData.FrameType)GetMSLevel(lcScanSet.PrimaryScanNumber);

            try
            {
                // Obtain an instance of the reader
                var uimfReader = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader;

                // Prior to January 2015 the SpectrumCache class in the UIMFReader used Dictionary<int, int> for ListOfIntensityDictionaries
                // This caused some datasets, e.g. EXP-Mix5_1um_pos_19Jan15_Columbia_DI, to run out of memory when caching 10 spectra
                // The UIMFLibrary now uses List<int, int>, which takes up less memory (at the expense having slower lookups by BinNumber, though this does not affect DeconTools' use of the UIMFLibrary)

                uimfReader.SpectraToCache = 10;
                uimfReader.MaxSpectrumCacheMemoryMB = 750;

                var nonZeroLength = uimfReader.GetSpectrum(
                    frameLower, frameUpper, frameType, scanLower, scanUpper, minMZ, maxMZ, out var xVals, out var yVals);

                var xyData = new XYData();

                if (xVals == null || xVals.Length == 0)
                {
                    xyData.Xvalues = null;
                    xyData.Yvalues = null;
                    return xyData;
                }

                xyData.Xvalues = xVals;
                xyData.Yvalues = yVals.Select<int, double>(i => i).ToArray();

                if (xyData.Xvalues[0] < minMZ || xyData.Xvalues[xyData.Xvalues.Length - 1] > maxMZ)
                {
                    xyData = xyData.TrimData(minMZ, maxMZ);
                }

                return xyData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UIMF GetMassSpectrum: " + ex.Message);
                throw;
            }


        }

        public override double GetTime(int frameNum)
        {
            var fp = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFrameParams(frameNum);
            var time = fp.GetValueDouble(FrameParamKeyType.StartTimeMinutes);
            return time;
        }

        /// <summary>
        /// Gets the number of consecutive MS2 frames including (if MS2) or following (if MS1) the specified frame number
        /// </summary>
        /// <param name="frameNumber"></param>
        /// <returns></returns>
        public int GetNumberOfConsecutiveMs2Frames(int frameNumber)
        {
            var count = 0;
            if (_frameList[frameNumber] == UIMFData.FrameType.MS2)
            {
                count = 1;
            }
            foreach (var kvp in _frameList.Where(x => x.Key > frameNumber))
            {
                if (kvp.Value == UIMFData.FrameType.MS2)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            return count;
        }

        public double GetDriftTime(int frameNum, int scanNum)
        {
            var fp = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFrameParams(frameNum);
            var avgTOFLength = fp.GetValueDouble(FrameParamKeyType.AverageTOFLength);
            var driftTime = avgTOFLength * (scanNum + 1) / 1e6;     //note that scanNum is zero-based.  Need to add one here

            var framePressure = GetFramePressure(frameNum);

            if (Math.Abs(framePressure) > float.Epsilon)
            {
                driftTime = driftTime * (FramePressureStandard / framePressure);  // correction
            }

            return driftTime;


        }

        public double GetFramePressure(int frameNum)
        {
            if (!_framePressuresUnsmoothed.ContainsKey(frameNum))
            {
                var pressure = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFramePressureForCalculationOfDriftTime(frameNum);
                _framePressuresUnsmoothed.Add(frameNum, pressure);
            }

            return _framePressuresUnsmoothed[frameNum];

        }

        public double GetFramePressureBack(int frameNum)
        {
            return GetFramePressure(frameNum);

        }

        public double GetFramePressureFront(int frameNum)
        {
            var fp = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFrameParams(frameNum);
            var framePressureFront = fp.GetValueDouble(FrameParamKeyType.PressureFront);

            return framePressureFront;
        }

        public void SmoothFramePressuresInFrameSets()
        {
            Check.Require(ScanSetCollection != null && ScanSetCollection.ScanSetList.Count > 0, "Cannot smooth frame pressures. FrameSet collection has not been defined.");

            var numFrames = GetNumFrames();

            double numPointsToSmooth = 100;

            var lowerFrameBoundary = (int)Math.Round(numPointsToSmooth / 2) - 1;    //zero-based
            var upperFrameBoundary = (int)Math.Round(numFrames - numPointsToSmooth / 2) - 1;   //zero-based

            var maxFrame = _globalParams.NumFrames;
            var minFrame = GetMinPossibleLCScanNum();

            if (ScanSetCollection == null)
            {
                return;
            }

            foreach (var scanSet in ScanSetCollection.ScanSetList)
            {
                var frame = (LCScanSetIMS)scanSet;
                if (double.IsNaN(frame.FramePressureSmoothed))
                {
                    throw new ArgumentOutOfRangeException(nameof(frame.FramePressureSmoothed), "Cannot smooth frame pressures. You need to first populate frame pressures within the FrameSet.");
                }

                int lowerFrame;
                int upperFrame;


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
            var framePressures = new List<double>();

            for (var i = lowerFrame; i <= upperFrame; i++)
            {

                var framePressure = GetFramePressure(i);
                if (framePressure > 0)
                {
                    framePressures.Add(framePressure);
                }


            }

            if (framePressures.Count > 0)
            {
                return framePressures.Average();
            }

            return 0;
        }

        public void GetFrameDataAllFrameSets()
        {
            Check.Require(ScanSetCollection != null && ScanSetCollection.ScanSetList.Count > 0, "Cannot get frame data. FrameSet collection has not been defined.");

            Console.Write("Loading frame parameters ");
            var dtLastProgress = DateTime.UtcNow;

            if (ScanSetCollection == null)
            {
                return;
            }

            foreach (var scanSet in ScanSetCollection.ScanSetList)
            {
                var frame = (LCScanSetIMS)scanSet;

                var fp = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFrameParams(frame.PrimaryScanNumber);
                frame.AvgTOFLength = fp.GetValueDouble(FrameParamKeyType.AverageTOFLength);
                frame.FramePressureUnsmoothed =
                    UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFramePressureForCalculationOfDriftTime(frame.PrimaryScanNumber);

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
        //    return UIMFLibraryAdapter.getInstance(this.DatasetFileOrDirectoryPath).DataReader.GetFramesAndScanIntensitiesForAGivenMz(startFrame, endFrame, frameType, startScan, endScan, targetMz, toleranceInMZ);
        //}

        //// Anuj added this
        //for now I am converting this dictionary to a frame and scan list, only because the UIMF library already has that method
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

        //// Anuj added this
        //public void GetMassSpectrum(List<ushort> frameNumbers, List<List<ushort>> scanNumbersForFrameNumbers, double minMz, double maxMz)
        //{
        //    List<double> mzList = new List<double>();
        //    List<double> intensityList = new List<double>();
        //    UIMFLibraryAdapter.getInstance(this.DatasetFileOrDirectoryPath).DataReader.SumScansNonCached(frameNumbers, scanNumbersForFrameNumbers, mzList, intensityList, minMz, maxMz);
        //    this.XYData.Xvalues = mzList.ToArray();
        //    this.XYData.Yvalues = intensityList.ToArray();
        //}

        public Stack<int[]> GetDescendingBpiValuesByFramesAndScans()
        {
            return UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFrameAndScanListByDescendingIntensity();
        }

        //public void GetDriftTimeProfile(int frameStartIndex, int frameStopIndex, int startScan, int stopScan, double targetMZ, double toleranceInMZ)
        //{
        //    int[] scanValues = null;
        //    int[] intensityVals = null;

        //    UIMFLibraryAdapter.getInstance(this.DatasetFileOrDirectoryPath).DataReader.GetDriftTimeProfile(frameStartIndex, frameStopIndex, this.FrameTypeForMS1, startScan, stopScan, targetMZ, toleranceInMZ, ref scanValues, ref intensityVals);

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

            UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetDriftTimeProfile(frameNum, frameNum, UIMFData.FrameType.MS1, startScan, stopScan, targetMZ, toleranceInMZ, ref scanValues, ref intensityVals);


            var xyData = new XYData();

            if (scanValues == null || scanValues.Length == 0)
            {
                xyData.Xvalues = null;
                xyData.Yvalues = null;
            }
            else
            {
                xyData.Xvalues = scanValues.Select<int, double>(i => i).ToArray();
                xyData.Yvalues = intensityVals.Select<int, double>(i => i).ToArray();
            }

            return xyData;

        }

        public override void Close()
        {
            if (UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader != null)
            {
                UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).CloseCurrentUIMF();
            }

            base.Close();
        }

        public float GetTIC(int lcScan, int imsScan)
        {
            var frameScans = UIMFLibraryAdapter.getInstance(DatasetFileOrDirectoryPath).Reader.GetFrameScans(lcScan);
            var query = (from item in frameScans where item.Scan == imsScan select item.TIC).ToList();

            if (query.Count == 0)
                return 0;

            return (float)query.FirstOrDefault();

        }

        public XYData GetChromatogram(int startFrame, int stopFrame, int startIMSScan, int stopIMSScan, double targetMZ, double toleranceInPPM)
        {
            var toleranceInMZ = toleranceInPPM / 1e6 * targetMZ;
            var lowerMZ = targetMZ - toleranceInMZ;
            var upperMZ = targetMZ + toleranceInMZ;

            var frameVals = new List<double>();
            var intensityVals = new List<double>();

            for (var frame = startFrame; frame <= stopFrame; frame++)
            {
                var frameSet = new ScanSet(frame);
                var scan = new ScanSet(startIMSScan, startIMSScan, stopIMSScan);
                var xyData = GetMassSpectrum(frameSet, scan, lowerMZ, upperMZ);


                double sumIntensities = 0;

                if (xyData?.Yvalues != null && xyData.Yvalues.Length > 0)
                {
                    sumIntensities = xyData.Yvalues.Sum();
                }

                frameVals.Add(frame);
                intensityVals.Add(sumIntensities);
            }

            var chromXYData = new XYData
            {
                Xvalues = frameVals.ToArray(),
                Yvalues = intensityVals.ToArray()
            };



            return chromXYData;
        }

        public override string GetCurrentScanOrFrameInfo()
        {
            var sb = new StringBuilder();

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

            var closestLCScan = MinLCScan;
            var smallestDiff = Int32.MaxValue;

            foreach (var t in MS1Frames)
            {
                var currentDiff = Math.Abs(t - lcScan);
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
