using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using UIMFLibrary;

namespace DeconTools.Backend.Runs
{

    [Serializable]
    public class UIMFRun : Run
    {
        private int numBins;

        #region Constructors
        public UIMFRun()
        {
            this.XYData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.MSFileType = Globals.MSFileType.PNNL_UIMF;
            this.frameSetCollection = new FrameSetCollection();
        }

        public UIMFRun(string fileName)
            : this()
        {
            this.Filename = fileName;
            this.rawData = new DeconToolsV2.Readers.clsRawData(fileName, DeconToolsV2.Readers.FileType.PNNL_UIMF);
            this.MinFrame = 1;
            this.MaxFrame = GetMaxPossibleFrameIndex();
            this.MinScan = 0;
            this.MaxScan = GetMaxPossibleScanIndex();
            this.numBins = GetNumBins();



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

        [field: NonSerialized]
        private XYData xyData;
        public override XYData XYData
        {
            get
            {
                return xyData;
            }
            set
            {
                xyData = value;
            }
        }
        
      
        private FrameSetCollection frameSetCollection;

        public FrameSetCollection FrameSetCollection
        {
            get { return frameSetCollection; }
            set { frameSetCollection = value; }
        }


        [field: NonSerialized]
        private DeconToolsV2.Readers.clsRawData rawData;

        public DeconToolsV2.Readers.clsRawData RawData
        {
            get { return rawData; }
            set { rawData = value; }
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
            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(this.Filename);

            int numFrames = Convert.ToInt32(adapter.Datareader.GetGlobalParameters("NumFrames"));

            int numScansPerFrame = this.GetNumScansPerFrame();


            return (numScansPerFrame * numFrames);

        }

        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {

            double[] xvals = new double[1];
            double[] yvals = new double[1];

            Check.Require(scanset.Count() > 0, "Cannot get spectrum. Number of scans in ScanSet is 0");
            if (scanset.Count() == 1)
            {
                rawData.GetSpectrum(scanset.getLowestScanNumber(), ref xvals, ref yvals);
            }
            else
            {
                rawData.GetSummedSpectra(scanset.getLowestScanNumber(), scanset.getHighestScanNumber(), minMZ, maxMZ, ref xvals, ref yvals);
            }



            this.xyData.SetXYValues(ref xvals, ref yvals);



        }

        public override void GetMassSpectrum(ScanSet scanset, FrameSet frameset, double minMZ, double maxMZ)
        {
            double[] xvals = new double[1];
            double[] yvals = new double[1];

            Check.Require(scanset.Count() > 0, "Cannot get spectrum. Number of scans in ScanSet is 0");
            Check.Require(frameset.Count() > 0, "Cannot get spectrum. Number of frames in FrameSet is 0");


            try
            {

                Check.Require(UIMFLibraryAdapter.getInstance(this.Filename).Datareader != null, "Data reader is null");

                rawData.GetSummedFrameAndScanSpectra(UIMFLibraryAdapter.getInstance(this.Filename).Datareader,
                    ref xvals, ref yvals, frameset.getLowestFrameNumber(),
                    frameset.getHighestFrameNumber(), scanset.getLowestScanNumber(),
                    scanset.getHighestScanNumber(), minMZ, maxMZ, this.numBins);



            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {


            }



            this.XYData.Xvalues = xvals;
            this.XYData.Yvalues = yvals;
        }
        public void GetMassSpectrum2(ScanSet scanset, FrameSet frameset, double minMZ, double maxMZ)
        {
            //Check.Require(scanset.Count() > 0, "Cannot get spectrum. Number of scans in ScanSet is 0");
            //Check.Require(frameset.Count() > 0, "Cannot get spectrum. Number of frames in FrameSet is 0");

            //UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(this.Filename);

            //int numBins = this.GetNumBins();
            //float[] xvals = new float[numBins];
            //int[] yvals = new int[numBins];

            //List<float> trimmedXvalues = new List<float>();
            //List<int> trimmedYValues = new List<int>();

            //try
            //{

            //    int summedSpecLength = adapter.Datareader.SumScans(xvals, yvals, 0,
            //           frameset.getLowestFrameNumber(), frameset.getHighestFrameNumber(),
            //           scanset.getLowestScanNumber(), scanset.getHighestScanNumber());


            //    for (int i = 0; i < xvals.Length; i++)
            //    {
            //        bool isWithinMZRange = (xvals[i] >= minMZ && xvals[i] <= maxMZ);
            //        bool isIntensityAbove0 = (yvals[i] > 0);


            //        if (isWithinMZRange && isIntensityAbove0)
            //        {
            //            trimmedXvalues.Add(xvals[i]);
            //            trimmedYValues.Add(yvals[i]);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{

            //    throw ex;
            //}
            //finally
            //{
            //}

            //this.XYData.Xvalues = XYData.ConvertFloatsToDouble(trimmedXvalues.ToArray());
            //this.XYData.Yvalues = XYData.ConvertIntsToDouble(trimmedYValues.ToArray());
        }

        public double GetFramePressure(int frameNum)
        {
            return this.rawData.GetFramePressure(frameNum);
        }

        public double GetFramePressureFront(int frameNum)
        {
            UIMFLibrary.DataReader datareader = new UIMFLibrary.DataReader();
            bool check = datareader.OpenUIMF(this.Filename);
            double framepressureFront = Convert.ToDouble(datareader.GetFrameParameters(frameNum, "PressureFront"));
            return framepressureFront;
        }

        public double GetFramePressureBack(int frameNum)
        {
            UIMFLibrary.DataReader datareader = new UIMFLibrary.DataReader();
            bool check = datareader.OpenUIMF(this.Filename);
            double framepressure = Convert.ToDouble(datareader.GetFrameParameters(frameNum, "PressureBack"));
            return framepressure;

        }

        public int GetNumFrames()
        {
            UIMFLibrary.DataReader datareader = new UIMFLibrary.DataReader();
            bool check = datareader.OpenUIMF(this.Filename);
            int numFrames = Convert.ToInt32(datareader.GetGlobalParameters("NumFrames"));
            datareader.CloseUIMF(this.Filename);
            return numFrames;

        }

        public override int GetMSLevel(int scanNum)
        {
            return this.rawData.GetMSLevel(scanNum);
        }
        internal void CreateFrameSetCollection(int minFrame, int maxFrame)      // this is an early simplistic version
        {
            for (int i = minFrame; i <= maxFrame; i++)
            {
                this.frameSetCollection.FrameSetList.Add(new FrameSet(i));
            }

        }
        public override double GetTime(int scanNum)
        {
            return -1;   //TODO:  figure out what to return
        }

        public double GetDriftTime(int frameNum, int scanNum)
        {
            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(this.Filename);
            DataReader datareader = adapter.Datareader;

            double avgTOFLength = Convert.ToDouble(datareader.GetFrameParameters(frameNum, "AverageTOFLength"));

            double driftTime = avgTOFLength * (scanNum+1) / 1e6;     //note that scanNum is zero-based.  Need to add one here
            return driftTime;


        }

        internal int GetNumScansPerFrame()
        {
            UIMFLibrary.DataReader datareader = new UIMFLibrary.DataReader();
            bool check = datareader.OpenUIMF(this.Filename);


            int numScansPerFrame = Convert.ToInt32(datareader.GetFrameParameters(1, "Scans"));

            datareader.CloseUIMF(this.Filename);
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
            int maxPossibleFrameIndex = GetNumFrames() - 1;
            if (maxPossibleFrameIndex < 0) maxPossibleFrameIndex = 0;
            return maxPossibleFrameIndex;
        }
        public int GetNumBins()
        {

            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(this.Filename);
            int numBins = Convert.ToInt32(adapter.Datareader.GetGlobalParameters("Bins"));
            return numBins;
        }

        public override int GetCurrentScanOrFrame()
        {
            return this.CurrentFrameSet.PrimaryFrame;
        }

        #endregion

    }
}
