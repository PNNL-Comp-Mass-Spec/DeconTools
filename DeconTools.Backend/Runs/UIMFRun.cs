using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using UIMFLibrary;
using System.Linq;
using System.IO;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Runs
{

    [Serializable]
    public class UIMFRun : DeconToolsRun
    {
        const double FRAME_PRESSURE_STANDARD = 4.00000d;   // 

        private int FrameTypeForMS1;     // the frame type for MS1 scans. Some older UIMF files have '0'. Currently we are moving to '1' for MS1 and '2' for MS2, according to mzXML format.


        private int numBins;

        private GlobalParameters m_globalParameters;

        //private UIMFLibrary.DataReader m_reader;
        private Dictionary<int, double> framePressuresUnsmoothed;

        #region Constructors
        public UIMFRun()
        {
            this.XYData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.MSFileType = Globals.MSFileType.PNNL_UIMF;
            this.frameSetCollection = new FrameSetCollection();
            this.ContainsMSMSData = false;
            this.RawData = null;
            this.framePressuresUnsmoothed = new Dictionary<int, double>();

        }

        public UIMFRun(string fileName)
            : this()
        {

            Check.Require(File.Exists(fileName), "File does not exist.");
            this.Filename = fileName;

            //m_reader = new DataReader();

            SetGlobalParameters();

            string baseFilename = Path.GetFileName(this.Filename);
            this.DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            this.DataSetPath = Path.GetDirectoryName(fileName);

            this.MinFrame = 0;

            this.MaxFrame = GetMaxPossibleFrameIndex();
            this.MinScan = 0;
            this.MaxScan = GetMaxPossibleScanIndex();
            this.numBins = GetNumBins();


            this.FrameTypeForMS1 = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.CurrentFrameType;

            


        }

        private void SetGlobalParameters()
        {
            m_globalParameters = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetGlobalParameters();
        }



        public UIMFRun(string fileName, int minFrame, int maxFrame)
            : this(fileName)
        {
            this.minFrame = minFrame;
            this.maxFrame = maxFrame;
        }

        public UIMFRun(string fileName, int minFrame, int maxFrame, int minScan, int maxScan)
            : this(fileName, minFrame, maxFrame)
        {
            this.MinScan = minScan;
            this.MaxScan = maxScan;

        }


        #endregion

        #region Properties

        private FrameSetCollection frameSetCollection;

        public FrameSetCollection FrameSetCollection
        {
            get { return frameSetCollection; }
            set { frameSetCollection = value; }
        }



        private int minFrame;

        public int MinFrame
        {
            get { return minFrame; }
            set
            {
                Check.Require(value >= 0, "MinFrame must be an integer greater than or equal to 0");
                minFrame = value;
            }
        }
        private int maxFrame;


        public int MaxFrame
        {
            get { return maxFrame; }
            set { maxFrame = value; }
        }


        private FrameSet currentFrameSet;
      
        public FrameSet CurrentFrameSet
        {
            get { return currentFrameSet; }
            set { currentFrameSet = value; }
        }



        #endregion

        #region Public Methods
        public override int GetNumMSScans()
        {
            int numFrames = m_globalParameters.NumFrames;
            int numScansPerFrame = 0;

            FrameParameters frameParam = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(0);
            numScansPerFrame = frameParam.Scans;

            return (numScansPerFrame * numFrames);
        }



        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {

            double[] xvals = new double[1];
            double[] yvals = new double[1];

            throw new NotImplementedException("this 'GetMassSpectrum' method is no longer supported");
        }


        //TODO: change the order of parameters so that frameset is first
        public void GetMassSpectrum(ScanSet scanset, FrameSet frameset, double minMZ, double maxMZ)
        {
            Check.Require(scanset.Count() > 0, "Cannot get spectrum. Number of scans in ScanSet is 0");
            Check.Require(frameset.Count() > 0, "Cannot get spectrum. Number of frames in FrameSet is 0");

            int numBins = this.GetNumBins();
            double[] xvals = new double[numBins];
            int[] yvals = new int[numBins];


            try
            {
                int frameLower = frameset.getLowestFrameNumber();
                int frameUpper = frameset.getHighestFrameNumber();
                int scanLower = scanset.getLowestScanNumber();
                int scanUpper = scanset.getHighestScanNumber();

                int nonZeroLength = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.SumScansNonCached(xvals, yvals, FrameTypeForMS1, frameLower, frameUpper, scanLower, scanUpper);


                if (xvals == null || xvals.Length == 0 || yvals == null || yvals.Length == 0)
                {
                    this.XYData.Xvalues = null;
                    this.XYData.Yvalues = null;
                    return;
                }

                int intensityArrLength = yvals.Length;
                int[] tempIntensities = yvals;
                int zeros = 0;


                for (int k = 0; k < intensityArrLength; k++)
                {
                    if (tempIntensities[k] != 0 && (minMZ <= xvals[k] && maxMZ >= xvals[k]))
                    {
                        xvals[k - zeros] = xvals[k];
                        yvals[k - zeros] = tempIntensities[k];
                    }
                    else
                    {
                        zeros++;
                    }
                }
                // resize arrays cutting off the zeroes at the end.
                Array.Resize(ref xvals, intensityArrLength - zeros);
                Array.Resize(ref yvals, intensityArrLength - zeros);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
            }


            this.XYData.Xvalues = xvals;
            this.XYData.Yvalues = yvals.Select<int, double>(i => i).ToArray();
        }

        public double GetFramePressure(int frameNum)
        {
            return lookupFramePressure(frameNum);
            
        }

        public double GetFramePressureBack(int frameNum)
        {
            return GetFramePressure(frameNum);

        }

        public double GetFramePressureFront(int frameNum)
        {
            double framepressureFront = -1;

            framepressureFront = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frameNum).PressureFront;

            return framepressureFront;
        }



        private double lookupFramePressure(int frameNum)
        {
            if (!this.framePressuresUnsmoothed.ContainsKey(frameNum))
            {
                double pressure = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFramePressureForCalculationOfDriftTime(frameNum);
                this.framePressuresUnsmoothed.Add(frameNum, pressure);
            }

            return this.framePressuresUnsmoothed[frameNum];

            
        }

     
    

        public int GetNumFrames()
        {
            if (m_globalParameters == null)
            {
                SetGlobalParameters();
            }
            return m_globalParameters.NumFrames;


        }

        public override double GetTime(int frameNum)
        {
            double time = 0;
            time = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frameNum).StartTime;
            return time;

        }


        

        public void SmoothFramePressuresInFrameSets()
        {
            Check.Require(FrameSetCollection != null && FrameSetCollection.FrameSetList.Count > 0, "Cannot smooth frame pressures. FrameSet collection has not been defined.");

            int numFrames = GetNumFrames();

            double numPointsToSmooth = 100;

            int lowerFrameBoundary = (int)Math.Round(numPointsToSmooth / 2) - 1;    //zero-based
            int upperFrameBoundary = (int)Math.Round(numFrames - numPointsToSmooth / 2) - 1;   //zero-based

            int maxFrame = this.GetMaxPossibleFrameIndex();
            int minFrame = this.GetMinPossibleFrameIndex();


            foreach (var frame in FrameSetCollection.FrameSetList)
            {
                if (frame.FramePressure == Single.NaN)
                {
                    throw new System.ArgumentOutOfRangeException("Cannot smooth frame pressures.  You need to first populate frame pressures within the Frameset.");
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
                    upperFrame =maxFrame;
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



                frame.FramePressure = getAverageFramePressure(lowerFrame, upperFrame);


            }

        }

        private double getAverageFramePressure(int lowerFrame, int upperFrame)
        {
            List<double> framePressures = new List<double>();

            //int actualLower = lowerFrame;
            //int actualUpper = upperFrame;

            //int minFrame = this.GetMinPossibleFrameIndex();
            //int maxFrame = this.GetMaxPossibleFrameIndex();

            //bool lowerFrameOutOfRange = lowerFrame < minFrame || lowerFrame>maxFrame;
            //if (lowerFrameOutOfRange)
            //{
            //    actualLower = minFrame;
            //}

            //bool upperFrameOutOfRange = upperFrame < minFrame || upperFrame > maxFrame;
            //if (upperFrameOutOfRange)
            //{
            //    actualUpper = maxFrame;
            //}


            for (int i = lowerFrame; i <= upperFrame; i++)
            {
                framePressures.Add(GetFramePressure(i));
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

        public double GetDriftTime(FrameSet frame, int scanNum)
        {


            bool framePressureIsZero = (frame.FramePressure == 0 || frame.FramePressure == Double.NaN);
            if (framePressureIsZero)
            {
                return 0;
            }

            if (Double.IsNaN(frame.AvgTOFLength))
            {
                FrameParameters fp = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frame.PrimaryFrame);
                Check.Require(fp != null, "Could not get drift time - Frame parameters could not be accessed.");

                frame.AvgTOFLength = fp.AverageTOFLength;
            }

            if (Double.IsNaN(frame.FramePressure))
            {
                frame.FramePressure = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFramePressureForCalculationOfDriftTime(frame.PrimaryFrame);
            }

            double driftTime = 0;
            if (frame.AvgTOFLength > 0)
            {
                driftTime = frame.AvgTOFLength * (scanNum + 1) / 1e6;     //note that scanNum is zero-based.  Need to add one here
            }

            driftTime = driftTime * (FRAME_PRESSURE_STANDARD / frame.FramePressure);      // correct the drift time according to the pressure


            return driftTime;

        }

        public int[][] GetFramesAndScanIntensitiesForAGivenMz(int startFrame, int endFrame, int frameType, int startScan, int endScan, double targetMz, double toleranceInMZ)
        {
            return UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFramesAndScanIntensitiesForAGivenMz(startFrame, endFrame, frameType, startScan, endScan, targetMz, toleranceInMZ);
        }


        //for now I am converting this dictionary to a frame and scna list, only because the UIMF library already has that method
        public void GetMassSpectrum(Dictionary<ushort, List<ushort>> frameAndScans, double minMz, double MaxMz)
        {
            List<ushort> frameNumbers = frameAndScans.Keys.ToList<ushort>();
            List<List<ushort>> scanNumbers = new List<List<ushort>>();

            foreach (ushort frameNum in frameNumbers)
            {
                List<ushort> scanList = frameAndScans[frameNum];
                scanNumbers.Add(scanList);
            }

            GetMassSpectrum(frameNumbers, scanNumbers, minMz, MaxMz);

        }

        public void GetMassSpectrum(List<ushort> frameNumbers, List<List<ushort>> scanNumbersForFrameNumbers, double minMz, double maxMz)
        {
            List<double> mzList = new List<double>();
            List<double> intensityList = new List<double>();
            UIMFLibraryAdapter.getInstance(this.Filename).Datareader.SumScansNonCached(frameNumbers, scanNumbersForFrameNumbers, mzList, intensityList, minMz, maxMz);
            this.XYData.Xvalues = mzList.ToArray();
            this.XYData.Yvalues = intensityList.ToArray();
        }


        public Stack<int[]> GetDescendingBpiValuesByFramesAndScans()
        {
            return UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameAndScanListByDescendingIntensity();
        }

        public double GetDriftTime(int frameNum, int scanNum)
        {
            FrameParameters fp = null;
            fp = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frameNum);
            double avgTOFLength = fp.AverageTOFLength;
            double driftTime = avgTOFLength * (scanNum + 1) / 1e6;     //note that scanNum is zero-based.  Need to add one here

            double frameBackpressure = fp.PressureBack;
            if (frameBackpressure != 0)
            {
                driftTime = driftTime * (FRAME_PRESSURE_STANDARD / frameBackpressure);  // correc
            }

            return driftTime;


        }

        public void GetDriftTimeProfile(int frameNum, int startScan, int stopScan, double targetMZ, double toleranceInMZ)
        {
            int[] scanValues = null;
            int[] intensityVals = null;

            UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetDriftTimeProfile(frameNum, frameNum, 0, startScan, stopScan, targetMZ, toleranceInMZ, ref scanValues, ref intensityVals);

            if (scanValues == null || scanValues.Length == 0)
            {
                this.XYData.Xvalues = null;
                this.XYData.Yvalues = null;
            }
            else
            {
                this.XYData.Xvalues = scanValues.Select<int, double>(i => i).ToArray();
                this.XYData.Yvalues = intensityVals.Select<int, double>(i => i).ToArray();
            }

        }


        internal int GetNumScansPerFrame()
        {
            //TODO:  need to make this so it isn't hard coded to frame '0'.  We have an instance of a UIMF file starting at frame '400' and this doesn't work. 
            int numScansPerFrame = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(0).Scans;
            return numScansPerFrame;

        }

        internal override int GetMaxPossibleScanIndex()
        {
            int maxPossibleScanIndex = GetNumScansPerFrame() - 1;
            if (maxPossibleScanIndex < 0) maxPossibleScanIndex = 0;
            return maxPossibleScanIndex;
        }


        private int GetMinPossibleFrameIndex()
        {
            return 0;    //zero-based
        }


        private int GetMaxPossibleFrameIndex()
        {
            int maxPossibleFrameIndex = GetNumFrames() - 1;     //frames begin at '0'
            if (maxPossibleFrameIndex < 0) maxPossibleFrameIndex = 0;
            return maxPossibleFrameIndex;
        }
        public int GetNumBins()
        {
            return m_globalParameters.Bins;
        }

        public override int GetCurrentScanOrFrame()
        {
            return this.CurrentFrameSet.PrimaryFrame;
        }


        public override int GetMSLevelFromRawData(int scanNum)
        {
            return 1;
        }

        public override void Close()
        {
            if (UIMFLibraryAdapter.getInstance(this.Filename).Datareader != null)
            {
                UIMFLibraryAdapter.getInstance(this.Filename).CloseCurrentUIMF();

            }
            base.Close();
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
                this.GetMassSpectrum(scan, frameset, lowerMZ, upperMZ);

                double sumIntensities = this.XYData.Yvalues.Sum();
                frameVals.Add(frame);
                intensityVals.Add(sumIntensities);
            }

            this.XYData.Xvalues = frameVals.ToArray();
            this.XYData.Yvalues = intensityVals.ToArray();
        }
    }
}
