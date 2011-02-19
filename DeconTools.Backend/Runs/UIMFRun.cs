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

        private int numBins;

        private GlobalParameters m_globalParameters;

        //private UIMFLibrary.DataReader m_reader;


        #region Constructors
        public UIMFRun()
        {
            this.XYData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.MSFileType = Globals.MSFileType.PNNL_UIMF;
            this.frameSetCollection = new FrameSetCollection();
            this.ContainsMSMSData = false;
            this.RawData = null;

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

            this.MinFrame = 1;

            this.MaxFrame = GetMaxPossibleFrameIndex();
            this.MinScan = 0;
            this.MaxScan = GetMaxPossibleScanIndex();
            this.numBins = GetNumBins();



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
                Check.Require(value > 0, "MinFrame must be an integer greater than 0");
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

            FrameParameters frameParam = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(1);
            numScansPerFrame = frameParam.Scans;

            return (numScansPerFrame * numFrames);
        }

        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {

            double[] xvals = new double[1];
            double[] yvals = new double[1];

            throw new NotImplementedException("this 'GetMassSpectrum' method is no longer supported");

            //Check.Require(scanset.Count() > 0, "Cannot get spectrum. Number of scans in ScanSet is 0");
            //if (scanset.Count() == 1)
            //{
            //    RawData.GetSpectrum(scanset.getLowestScanNumber(), ref xvals, ref yvals);
            //}
            //else
            //{
            //    RawData.GetSummedSpectra(scanset.getLowestScanNumber(), scanset.getHighestScanNumber(), minMZ, maxMZ, ref xvals, ref yvals);
            //}



            //this.XYData.SetXYValues(ref xvals, ref yvals);



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

                int nonZeroLength = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.SumScansNonCached(xvals, yvals, 0, frameLower, frameUpper, scanLower, scanUpper);


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
            return this.GetFramePressureBack(frameNum);
        }

        public double GetFramePressureFront(int frameNum)
        {
            double framepressureFront = -1;

            framepressureFront = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frameNum).PressureFront;

            return framepressureFront;
        }

        public double GetFramePressureBack(int frameNum)
        {
            double framepressure = -1;
            framepressure = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frameNum).PressureBack;
            return framepressure;

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

            int lowerFrameBoundary = (int)Math.Round(numPointsToSmooth / 2);
            int upperFrameBoundary = (int)Math.Round(numFrames - numPointsToSmooth / 2);



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
                    lowerFrame = 1;
                    upperFrame = (int)numPointsToSmooth;
                }
                else if (frame.PrimaryFrame > upperFrameBoundary)
                {
                    lowerFrame = (numFrames - (int)numPointsToSmooth + 1);
                    upperFrame = numFrames;
                }
                else
                {
                    lowerFrame = frame.PrimaryFrame - (int)Math.Round(numPointsToSmooth / 2) + 1;     //frame is 1-based
                    upperFrame = frame.PrimaryFrame + (int)Math.Round(numPointsToSmooth / 2);
                }

                frame.FramePressure = getAverageFramePressure(lowerFrame, upperFrame);


            }

        }

        private double getAverageFramePressure(int startFrame, int stopFrame)
        {
            List<double> framePressures = new List<double>();


            FrameParameters fp = null;

            for (int frame = startFrame; frame <= stopFrame; frame++)
            {
                fp = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frame);
                framePressures.Add(fp.PressureBack);
            }

            return framePressures.Average();



        }





        public void GetFrameDataAllFrameSets()
        {
            Check.Require(FrameSetCollection != null && FrameSetCollection.FrameSetList.Count > 0, "Cannot get frame data. FrameSet collection has not been defined.");

            foreach (var frame in FrameSetCollection.FrameSetList)
            {
                FrameParameters fp = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frame.PrimaryFrame);
                frame.AvgTOFLength = fp.AverageTOFLength;
                frame.FramePressure = fp.PressureBack;

            }

        }

        public double GetDriftTime(FrameSet frame, int scanNum)
        {
            FrameParameters fp = null;
            fp = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(frame.PrimaryFrame);
            Check.Require(fp != null, "Could not get drift time - Frame parameters could not be accessed.");


            if (Double.IsNaN(frame.AvgTOFLength))
            {
                frame.AvgTOFLength = fp.AverageTOFLength;
            }

            if (Double.IsNaN(frame.FramePressure))
            {
                frame.FramePressure = fp.PressureBack;
            }

            double driftTime = -1;
            if (frame.AvgTOFLength > 0)
            {
                driftTime = frame.AvgTOFLength * (scanNum + 1) / 1e6;     //note that scanNum is zero-based.  Need to add one here
            }

            if (frame.FramePressure != 0 && frame.FramePressure != Double.NaN)
            {
                driftTime = driftTime * (FRAME_PRESSURE_STANDARD / frame.FramePressure);      // correc
            }

            return driftTime;

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
                driftTime = driftTime * (FRAME_PRESSURE_STANDARD / frameBackpressure);      // correc
            }

            return driftTime;


        }

        public void GetDriftTimeProfile(int frameNum, int startScan, int stopScan, double targetMZ, double toleranceInMZ)
        {
            int[] scanValues = null;
            int[] intensityVals = null;

            UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetDriftTimeProfile(frameNum, 0, startScan, stopScan, targetMZ, toleranceInMZ, ref scanValues, ref intensityVals);

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
            //TODO:  need to make this so it isn't hard coded to frame '1'.  We have an instance of a UIMF file starting at frame '400' and this doesn't work. 
            int numScansPerFrame = UIMFLibraryAdapter.getInstance(this.Filename).Datareader.GetFrameParameters(1).Scans;
            return numScansPerFrame;

        }

        internal override int GetMaxPossibleScanIndex()
        {
            int maxPossibleScanIndex = GetNumScansPerFrame() - 1;
            if (maxPossibleScanIndex < 0) maxPossibleScanIndex = 0;
            return maxPossibleScanIndex;
        }

        private int GetMaxPossibleFrameIndex()
        {
            int maxPossibleFrameIndex = GetNumFrames();     //frames begin at '1'
            if (maxPossibleFrameIndex < 0) maxPossibleFrameIndex = 1;
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
