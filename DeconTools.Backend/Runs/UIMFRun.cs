using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Utilities;
using UIMFLibrary;

namespace DeconTools.Backend.Runs
{

    [Serializable]
    public sealed class UIMFRun : DeconToolsRun
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
            FrameSetCollection = new FrameSetCollection();
            RawData = null;
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

            MinFrame = GetMinPossibleFrameNumber(); // frame number is 1-based
            MaxFrame = GetMaxPossibleFrameNumber();
            MinScan = GetMinPossibleScanNum();
            MaxScan = GetMaxPossibleScanNum();

            GetMSLevelInfo();
            ContainsMSMSData = CheckRunForMSMSData();

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

			if(numMs2Frames > 0)
			{
				if(numMs2Frames % numMs1Frames != 0)
				{
					throw new InvalidOperationException("The number of MS2 frames associated with each MS1 frame must remain constant for the entire run.\n\tERROR: The number of MS2 frames (" + numMs2Frames + ") is not divisible by the number of MS1 frames (" + numMs1Frames + ").");
				}

				_numOfConsecutiveMs2Frames = numMs2Frames / numMs1Frames;
			}
        }

        private bool CheckRunForMSMSData()
        {

            if (MS2Frames.Count == 0) return false;

            return true;
        }



        public UIMFRun(string fileName, int minFrame, int maxFrame)
            : this(fileName)
        {
            this._minFrame = minFrame;
            this.MaxFrame = maxFrame;
        }

        public UIMFRun(string fileName, int minFrame, int maxFrame, int minScan, int maxScan)
            : this(fileName, minFrame, maxFrame)
        {
            this.MinScan = minScan;
            this.MaxScan = maxScan;

        }


        #endregion

        #region Properties

        public FrameSetCollection FrameSetCollection { get; set; }

        private int _minFrame;
        public int MinFrame
        {
            get { return _minFrame; }
            set
            {
                Check.Require(value >= 1, "MinFrame must be an integer greater than or equal to 1");
                _minFrame = value;
            }
        }

        public int MaxFrame { get; set; }

        public FrameSet CurrentFrameSet { get; set; }

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

        public int GetMinPossibleFrameNumber()
        {
            return 1;    //one-based
        }

        public int GetMaxPossibleFrameNumber()
        {
            int maxPossibleFrameNumber = _globalParameters.NumFrames;

            int minPossibleFrameNumber = GetMinPossibleFrameNumber();
            if (maxPossibleFrameNumber < minPossibleFrameNumber)
            {
                maxPossibleFrameNumber = minPossibleFrameNumber;
            }

            return maxPossibleFrameNumber;
        }

        internal int GetNumScansPerFrame()
        {
            //TODO:  check this and make sure it is correct
            int minFrame = MinFrame;

            int numScansPerFrame = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetFrameParameters(minFrame).Scans;
            return numScansPerFrame;

        }

        public override int GetMinPossibleScanNum()
        {
            return 0;
        }

        public override int GetMaxPossibleScanNum()
        {
            return GetNumScansPerFrame() - 1;

        }

        public override int GetCurrentScanOrFrame()
        {
            return this.CurrentFrameSet.PrimaryFrame;
        }

        public override int GetMSLevelFromRawData(int frameNum)
        {
            if (MS1Frames.Contains(frameNum)) return 1;
            if (MS2Frames.Contains(frameNum)) return 2;

            FrameParameters fp = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frameNum);
            return (int)fp.FrameType;

        }

        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            throw new NotImplementedException("this 'GetMassSpectrum' method is no longer supported");
        }

        public override void GetMassSpectrum(FrameSet frameset, ScanSet scanset, double minMZ, double maxMZ)
        {
            Check.Require(scanset.Count() > 0, "Cannot get spectrum. Number of scans in ScanSet is 0");
            Check.Require(frameset.Count() > 0, "Cannot get spectrum. Number of frames in FrameSet is 0");

            int frameLower = frameset.getLowestFrameNumber();
            int frameUpper = frameset.getHighestFrameNumber();
            int scanLower = scanset.getLowestScanNumber();
            int scanUpper = scanset.getHighestScanNumber();

            double[] xvals = null;
            int[] yvals = null;

            var frameType = (DataReader.FrameType)GetMSLevel(frameset.PrimaryFrame);

            int nonZeroLength = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetSpectrum(frameLower,
                frameUpper, frameType, scanLower, scanUpper, out xvals, out yvals);

            if (xvals == null || xvals.Length == 0)
            {
                this.XYData.Xvalues = null;
                this.XYData.Yvalues = null;
                return;
            }

            XYData.Xvalues = xvals;
            XYData.Yvalues = yvals.Select<int, double>(i => i).ToArray();

            if (XYData.Xvalues[0] < minMZ || XYData.Xvalues[XYData.Xvalues.Length - 1] > maxMZ)
            {
                XYData = XYData.TrimData(minMZ, maxMZ);
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

        public double GetDriftTime(FrameSet frame, int scanNum)
        {
            bool framePressureIsZero = (frame.FramePressure == 0 || double.IsNaN(frame.FramePressure));
            if (framePressureIsZero)
            {
                return 0;
            }

            if (Double.IsNaN(frame.AvgTOFLength))
            {
                FrameParameters fp = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetFrameParameters(frame.PrimaryFrame);
                Check.Require(fp != null, "Could not get drift time - Frame parameters could not be accessed.");

                frame.AvgTOFLength = fp.AverageTOFLength;
            }

            if (Double.IsNaN(frame.FramePressure))
            {
                frame.FramePressure = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetFramePressureForCalculationOfDriftTime(frame.PrimaryFrame);
            }

            double driftTime = 0;
            if (frame.AvgTOFLength > 0)
            {
                driftTime = frame.AvgTOFLength * (scanNum + 1) / 1e6;     //note that scanNum is zero-based.  Need to add one here
            }

            driftTime = driftTime * (FramePressureStandard / frame.FramePressure);      // correct the drift time according to the pressure


            return driftTime;

        }

        public double GetDriftTime(int frameNum, int scanNum)
        {
            FrameParameters fp = null;
            fp = UIMFLibraryAdapter.getInstance(Filename).Datareader.GetFrameParameters(frameNum);
            double avgTOFLength = fp.AverageTOFLength;
            double driftTime = avgTOFLength * (scanNum + 1) / 1e6;     //note that scanNum is zero-based.  Need to add one here

            double frameBackpressure = fp.PressureBack;
            if (frameBackpressure != 0)
            {
                driftTime = driftTime * (FramePressureStandard / frameBackpressure);  // correc
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
            Check.Require(FrameSetCollection != null && FrameSetCollection.FrameSetList.Count > 0, "Cannot smooth frame pressures. FrameSet collection has not been defined.");

            int numFrames = GetNumFrames();

            double numPointsToSmooth = 100;

            int lowerFrameBoundary = (int)Math.Round(numPointsToSmooth / 2) - 1;    //zero-based
            int upperFrameBoundary = (int)Math.Round(numFrames - numPointsToSmooth / 2) - 1;   //zero-based

            int maxFrame = _globalParameters.NumFrames;
            int minFrame = GetMinPossibleFrameNumber();


            foreach (var frame in FrameSetCollection.FrameSetList)
            {
                if (double.IsNaN(frame.FramePressure))
                {
                    throw new ArgumentOutOfRangeException("Cannot smooth frame pressures.  You need to first populate frame pressures within the Frameset.");
                }

                int lowerFrame = -1;
                int upperFrame = -1;


                if (frame.PrimaryFrame < lowerFrameBoundary)
                {
                    lowerFrame = minFrame;
                    upperFrame = (int)(numPointsToSmooth) - 1;     // zero-based
                }
                else if (frame.PrimaryFrame > upperFrameBoundary)
                {
                    lowerFrame = maxFrame - (int)numPointsToSmooth + 1;
                    upperFrame = maxFrame;
                }
                else
                {
                    lowerFrame = frame.PrimaryFrame - (int)Math.Round(numPointsToSmooth / 2) + 1;
                    upperFrame = frame.PrimaryFrame + (int)Math.Round(numPointsToSmooth / 2);
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



                frame.FramePressure = GetAverageFramePressure(lowerFrame, upperFrame);


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
            Check.Require(FrameSetCollection != null && FrameSetCollection.FrameSetList.Count > 0, "Cannot get frame data. FrameSet collection has not been defined.");

            foreach (var frame in FrameSetCollection.FrameSetList)
            {
                FrameParameters fp = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frame.PrimaryFrame);
                frame.AvgTOFLength = fp.AverageTOFLength;
                frame.FramePressure = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFramePressureForCalculationOfDriftTime(frame.PrimaryFrame);

            }

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


        //public void GetDriftTimeProfile(int frameNum, int startScan, int stopScan, double targetMZ, double toleranceInMZ)
        //{
        //    int[] scanValues = null;
        //    int[] intensityVals = null;

        //    UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetDriftTimeProfile(frameNum, frameNum, 0, startScan, stopScan, targetMZ, toleranceInMZ, ref scanValues, ref intensityVals);

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




        public override void Close()
        {
            if (UIMFLibraryAdapter.getInstance(this.Filename).Datareader != null)
            {
                UIMFLibraryAdapter.getInstance(this.Filename).CloseCurrentUIMF();

            }
            base.Close();
        }

        public float GetTIC(FrameSet frameSet, ScanSet scanSet)
        {

            return (float)UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetTIC(frameSet.PrimaryFrame, scanSet.PrimaryScanNumber);

        }

        public void GetChromatogram(int startFrame, int stopFrame, int startIMSScan, int stopIMSScan, double targetMZ, double toleranceInPPM)
        {
            double toleranceInMZ = toleranceInPPM / 1e6 * targetMZ;
            double lowerMZ = targetMZ - toleranceInMZ;
            double upperMZ = targetMZ + toleranceInMZ;

            List<double> frameVals = new List<double>();
            List<double> intensityVals = new List<double>();

            for (int frame = startFrame; frame <= stopFrame; frame++)
            {
                FrameSet frameset = new FrameSet(frame);
                ScanSet scan = new ScanSet(startIMSScan, startIMSScan, stopIMSScan);
                this.GetMassSpectrum(frameset, scan, lowerMZ, upperMZ);

                double sumIntensities = this.XYData.Yvalues.Sum();
                frameVals.Add(frame);
                intensityVals.Add(sumIntensities);
            }

            this.XYData.Xvalues = frameVals.ToArray();
            this.XYData.Yvalues = intensityVals.ToArray();
        }

        public override string GetCurrentScanOrFrameInfo()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Frame = ");
            if (this.CurrentFrameSet != null)
            {
                sb.Append(this.CurrentFrameSet.PrimaryFrame);
            }
            else
            {
                sb.Append("NULL");
            }

            sb.Append("; ");
            sb.Append(base.GetCurrentScanOrFrameInfo());

            return sb.ToString();
        }

        #endregion



    }
}
