using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public abstract class Run
    {

        public Run()
        {
            this.scanSetCollection = new ScanSetCollection();
            this.ResultCollection = new ResultCollection(this);
            this.XYData = new XYData();
        }

        #region Properties

        [field: NonSerialized]
        private DeconToolsV2.Peaks.clsPeak[] deconToolsPeakList;
        public DeconToolsV2.Peaks.clsPeak[] DeconToolsPeakList        //I need to change this later; don't want anything connected to DeconEngine in this class
        {
            get { return deconToolsPeakList; }
            set { deconToolsPeakList = value; }
        }

        private string filename;
        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        private Globals.MSFileType mSFileType;
        public Globals.MSFileType MSFileType
        {
            get { return mSFileType; }
            set { mSFileType = value; }
        }

        private MSParameters msParameters;
        public MSParameters MSParameters            //this probably should move to a 'Project' level since each run doesn't need to have individual parameters, i think
        {
            get { return msParameters; }
            set { msParameters = value; }
        }

        private int minScan;
        public int MinScan
        {
            get { return minScan; }
            set { minScan = value; }
        }

        private int maxScan;
        public int MaxScan
        {
            get { return maxScan; }
            set { maxScan = value; }
        }

        private bool areRunResultsSerialized;   //this is a flag to indicate whether or not Run's results were written to disk
        public bool AreRunResultsSerialized
        {
            get { return areRunResultsSerialized; }
            set { areRunResultsSerialized = value; }
        }

        [field: NonSerialized]


        private List<IPeak> peakList;     
        public List<IPeak> PeakList
        {
            get { return peakList; }
            set { peakList = value; }
        }

        private ScanSetCollection scanSetCollection;
        public ScanSetCollection ScanSetCollection
        {
            get { return scanSetCollection; }
            set { scanSetCollection = value; }
        }

        private ResultCollection resultCollection;
        public ResultCollection ResultCollection
        {
            get { return resultCollection; }
            set { resultCollection = value; }
        }

        private ScanSet currentScanSet;
        public ScanSet CurrentScanSet
        {
            get { return currentScanSet; }
            set { currentScanSet = value; }
        }

        private bool isDataThresholded;
        public bool IsDataThresholded          //not crazy about having this here; may want to change later
        {
            get { return isDataThresholded; }
            set { isDataThresholded = value; }
        }

        private Dictionary<int, double> scanToNETAlignmentData;
        public Dictionary<int, double> ScanToNETAlignmentData
        {
            get { return scanToNETAlignmentData; }
            set { scanToNETAlignmentData = value; }
        }





        #endregion


        public abstract XYData XYData { get; set; }
        public abstract int GetNumMSScans();
        public abstract double GetTime(int scanNum);
        public abstract int GetMSLevel(int scanNum);
        public abstract void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ);
        public virtual float GetTIC(double minMZ, double maxMZ)
        {
            if (this.XYData == null || this.XYData.Xvalues == null || this.XYData.Yvalues == null) return -1;

            double summedIntensities = 0;
            for (int i = 0; i < this.XYData.Yvalues.Length; i++)
            {
                if (this.XYData.Xvalues[i] > minMZ && this.XYData.Xvalues[i] < maxMZ)
                {
                    summedIntensities += this.XYData.Yvalues[i];
                }
            }

            return (float)summedIntensities;
        }

        public MassTag CurrentMassTag { get; set; }

        public virtual void GetMassSpectrum(ScanSet scanset, FrameSet frameset, double minMZ, double maxMZ)
        {


        }

        #region Methods

        internal virtual int GetMaxPossibleScanIndex()
        {
            int maxpossibleScanIndex = GetNumMSScans() - 1;           // IMF files are 1 based, so we don't subtract 1 here. 
            if (maxpossibleScanIndex < 0) maxpossibleScanIndex = 0;

            return maxpossibleScanIndex;
        }

        internal void ResetXYPoints()
        {
            this.XYData = new XYData();
            this.XYData.Xvalues = new double[1];   //TODO:  re-examine this. I do this to allow
            this.XYData.Yvalues = new double[1];
        }

        internal void FilterXYPointsByMZRange(double minMZ, double maxMZ)
        {
            List<double> filteredXValues = new List<double>();
            List<double> filteredYValues = new List<double>();

            int numPointsToAdd = 0;

            for (int i = 0; i < XYData.Xvalues.Length; i++)
            {
                if (XYData.Xvalues[i] >= minMZ)
                {
                    if (XYData.Xvalues[i] <= maxMZ)
                    {
                        //filteredXValues.Add(XYData.Xvalues[i]);
                        //filteredYValues.Add(XYData.Yvalues[i]);
                        numPointsToAdd++;
                    }
                    else
                    {
                        break;
                    }



                }

            }

            double[] xvals = new double[numPointsToAdd];
            double[] yvals = new double[numPointsToAdd];

            int counter = 0;
            for (int i = 0; i < XYData.Xvalues.Length; i++)
            {

                if (XYData.Xvalues[i] >= minMZ)
                {
                    if (XYData.Xvalues[i] <= maxMZ)
                    {
                        xvals[counter] = XYData.Xvalues[i];
                        yvals[counter] = XYData.Yvalues[i];
                        counter++;
                    }
                    else
                    {
                        break;
                    }
                }
            }


            //XYData.Xvalues = filteredXValues.ToArray();
            //XYData.Yvalues = filteredYValues.ToArray();

            XYData.Xvalues = xvals;
            XYData.Yvalues = yvals;


        }


        #endregion

        //public void CreateScanSetCollection()
        //{
        //    Check.Require(GetNumMSScans() > 0, "Cannot create set of scans to be analyzed; Reason: number of MS scans is 0");
        //    CreateScanSetCollection(0, GetNumMSScans() - 1);
        //}

        //public void CreateScanSetCollection(int minScan, int maxScan)
        //{
        //    for (int i = minScan; i <= maxScan; i++)
        //    {
        //        this.ScanSetCollection.ScanSetList.Add(new ScanSet(i));
        //    }
        //}


        public virtual float GetNETValueForScan(int scanNum)
        {
            if (this.ScanToNETAlignmentData == null || this.ScanToNETAlignmentData.Count == 0) return -1;
            if (this.ScanToNETAlignmentData.ContainsKey(scanNum))
            {
                return (float)this.ScanToNETAlignmentData[scanNum];
            }
            else
            {
                return calculateNET(scanNum);
            }

        }


        public virtual int GetCurrentScanOrFrame()
        {
            return this.CurrentScanSet.PrimaryScanNumber;
        }

        public virtual void UpdateNETAlignment()
        {


            foreach (ScanSet scan in ScanSetCollection.ScanSetList)
            {
                if (this.ScanToNETAlignmentData.ContainsKey(scan.PrimaryScanNumber))
                {
                    scan.NETValue = (float)this.ScanToNETAlignmentData[scan.PrimaryScanNumber];
                }
                else
                {
                    scan.NETValue = calculateNET(scan.PrimaryScanNumber);
                }

            }
        }

        private float calculateNET(int scanNum)
        {
            if (scanNum < 2) return 0;
            int maxScan = this.GetNumMSScans();
            
          
            double lowerNET = 0;
            double upperNET = 1;
            int lowerScan = 1;
            int upperScan = maxScan;


            bool found = false;
            int currentScan=scanNum;

            while (!found && currentScan>0)
            {
                currentScan--;
                if (this.ScanToNETAlignmentData.ContainsKey(currentScan))
                {
                    lowerScan = currentScan;
                    lowerNET = this.ScanToNETAlignmentData[lowerScan];
                    found = true;
                }
            }

            found=false;
            currentScan=scanNum;
            while (!found && currentScan < maxScan)
            {
                currentScan++;
                if (this.ScanToNETAlignmentData.ContainsKey(currentScan))
                {
                    upperScan = currentScan;
                    upperNET = this.ScanToNETAlignmentData[upperScan];
                    found = true;
                }

            }

            double slope = (upperNET - lowerNET) / (upperScan - lowerScan);
            double yintercept = (upperNET - slope * upperScan);

            return (float)(scanNum * slope + yintercept);
          
        }
    }
}
