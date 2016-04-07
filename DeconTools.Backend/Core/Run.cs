using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PNNLOmics.Data;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public abstract class Run : IDisposable
    {
        public Run()
        {
            ScanSetCollection = new ScanSetCollection();
            ResultCollection = new ResultCollection(this);
            XYData = new XYData();
            MSLevelList = new Dictionary<int, byte>();
            PrimaryLcScanNumbers = new List<int>();

            IsMsAbundanceReportedAsAverage = false;
            
        }

        #region Properties

#if !Disable_DeconToolsV2
        [field: NonSerialized]
        private DeconToolsV2.Peaks.clsPeak[] _deconToolsPeakList;
        public DeconToolsV2.Peaks.clsPeak[] DeconToolsPeakList        //I need to change this later; don't want anything connected to DeconEngine in this class
        {
            get { return _deconToolsPeakList; }
            set { _deconToolsPeakList = value; }
        }
#endif

        public string Filename { get; set; }

        public string DatasetName { get; set; }

        // Directory containing the dataset file
        public string DataSetPath { get; set; }

        /// <summary>
        /// Contains the LCMSWarp mass and NET alignment information needed for Targeted workflows
        /// </summary>
        public MultiAlignEngine.Alignment.clsAlignmentFunction AlignmentInfo
        {
            get
            {
                return _alignmentInfo;
            }
            set
            {
                _alignmentInfo = value;
                if (_alignmentInfo != null)
                {
                    if (_alignmentInfo.marrNETFncTimeInput != null && _alignmentInfo.marrNETFncNETOutput != null)
                    {
                        NetAlignmentInfo = new NetAlignmentInfoBasic(MinLCScan, MaxLCScan);
                        NetAlignmentInfo.SetScanToNETAlignmentData(_alignmentInfo.marrNETFncTimeInput.Select(p => (double)p).ToArray(), _alignmentInfo.marrNETFncNETOutput.Select(p => (double)p).ToArray());
                    }
                }
            }
        }


        public MassAlignmentInfo MassAlignmentInfo { get; set; }

        private NetAlignmentInfo _netAlignmentInfo;
        public NetAlignmentInfo NetAlignmentInfo
        {
            get
            {
                if (_netAlignmentInfo==null)
                {
                    _netAlignmentInfo = new NetAlignmentInfoBasic(MinLCScan, MaxLCScan);
                }
                return _netAlignmentInfo;
            }
            set
            {
                _netAlignmentInfo = value;
            }
        }


        private Globals.MSFileType mSFileType;
        public Globals.MSFileType MSFileType
        {
            get { return mSFileType; }
            set { mSFileType = value; }
        }

        public int MinLCScan { get; set; }
        public int MaxLCScan { get; set; }

        private bool areRunResultsSerialized;   //this is a flag to indicate whether or not Run's results were written to disk
        public bool AreRunResultsSerialized
        {
            get { return areRunResultsSerialized; }
            set { areRunResultsSerialized = value; }
        }

        [field: NonSerialized]
        private List<Peak> peakList;
        public List<Peak> PeakList
        {
            get { return peakList; }
            set { peakList = value; }
        }

        public ScanSetCollection ScanSetCollection { get; set; }

        private ResultCollection resultCollection;
        public ResultCollection ResultCollection
        {
            get { return resultCollection; }
            set { resultCollection = value; }
        }

        private ScanSet currentScanSet;
        public virtual ScanSet CurrentScanSet
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

        private MultiAlignEngine.Alignment.clsAlignmentFunction _alignmentInfo;
       
        public virtual bool ContainsMSMSData { get; set; }

        private List<int> msLevelScanIndexList { get; set; }

        public IList<int> MSLevelMappings { get; set; }

        public List<int> PrimaryLcScanNumbers { get; set; }

        public bool MassIsAligned
        {
            get
            {
                return MassAlignmentInfo != null;
            }
        }

        public bool NETIsAligned
        {
            get
            {
                return NetAlignmentInfo != null;
            }
        }

        public double CurrentBackgroundIntensity { get; set; }


        #endregion

        public abstract XYData XYData { get; set; }
        public abstract int GetNumMSScans();
        public abstract double GetTime(int scanNum);
        public abstract int GetMSLevelFromRawData(int scanNum);



        /// <summary>
        /// Returns Scan information.
        /// </summary>
        /// <param name="scanNum"></param>
        /// <returns></returns>
        public virtual string GetScanInfo(int scanNum)
        {
            var sb = new StringBuilder();
            sb.Append(scanNum.ToString());
            return sb.ToString();
        }

		/// <summary>
		/// Returns the MSLevel for the given scan
		/// </summary>
		/// <param name="scanNum">Scan number (or frame number for UIMF files)</param>
		/// <returns>1 for MS1, 2 for MS2</returns>
		/// <remarks>UIMF calibration frames will be reported as MSLevel 0</remarks>
        public virtual int GetMSLevel(int scanNum)
		{
			byte msLevel;

            // check to see if we have a value already stored
            if (this.MSLevelList.TryGetValue(scanNum, out msLevel))
            {
                // Return the cached value
                return msLevel;
            }
            
            // Look up MSLevel from Raw data
		    var mslevel = GetMSLevelFromRawData(scanNum);

		    this.MSLevelList.Add(scanNum, (byte)mslevel);

		    return mslevel;
		}

        public virtual PrecursorInfo GetPrecursorInfo(int scanNum)
        {
            throw new NotImplementedException("GetPrecursorInfo is not available for this Run type.");
        }

        public virtual bool IsDataCentroided(int scanNum)
        {
            //Default is false
            return false;
        }


        public virtual XYData GetMassSpectrum(ScanSet scanset)
        {
            //set a wide mz range so we get everything
            return GetMassSpectrum(scanset, 0, 100000);
        }

        public abstract XYData GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ);





        public virtual double GetTICFromInstrumentInfo(int scanNum)
        {
            return 0;
        }



        public TargetBase CurrentMassTag { get; set; }

        public virtual XYData GetMassSpectrum(ScanSet lcScanset, ScanSet imsScanset, double minMZ, double maxMZ)
        {
            throw new NotSupportedException("This overload of GetMassSpectrum is only supported in IMS dataset types");
        }

        #region Methods

        public abstract int GetMinPossibleLCScanNum();

        public abstract int GetMaxPossibleLCScanNum();
        //{
        ////default:  scan is zero-based
        //int maxpossibleScanIndex = GetNumMSScans() - 1;           
        //if (maxpossibleScanIndex < 0) maxpossibleScanIndex = 0;

        //return maxpossibleScanIndex;
        //}

        internal void ResetXYPoints()
        {
            XYData = new XYData
            {
                Xvalues = new double[1],
                Yvalues = new double[1]
            };
            //TODO:  re-examine this. I do this to allow
        }

        internal void FilterXYPointsByMZRange(double minMZ, double maxMZ)
        {
            //var filteredXValues = new List<double>();
            //var filteredYValues = new List<double>();

            var numPointsToAdd = 0;

            for (var i = 0; i < XYData.Xvalues.Length; i++)
            {
                if (!(XYData.Xvalues[i] >= minMZ))
                {
                    continue;
                }

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

            var xvals = new double[numPointsToAdd];
            var yvals = new double[numPointsToAdd];

            var counter = 0;
            for (var i = 0; i < XYData.Xvalues.Length; i++)
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

        public virtual int GetCurrentScanOrFrame()
        {
            return this.CurrentScanSet.PrimaryScanNumber;
        }

        public virtual int GetClosestMSScan(int inputScan, Globals.ScanSelectionMode scanSelectionMode)
        {


            switch (scanSelectionMode)
            {
                case Globals.ScanSelectionMode.ASCENDING:
                    for (var i = inputScan; i <= this.MaxLCScan; i++)     // MaxScan is an index value
                    {
                        if (this.GetMSLevel(i) == 1) return i;
                    }
                    // reached end of scans. Don't want to throw a nasty error, so return what was inputted.
                    return inputScan;

                case Globals.ScanSelectionMode.DESCENDING:
                    for (var i = inputScan; i >= this.MinLCScan; i--)
                    {
                        if (this.GetMSLevel(i) == 1) return i;
                    }
                    //reached starting scan. No MS-level scan was found.  Simply return what was inputted. 
                    return inputScan;

                case Globals.ScanSelectionMode.CLOSEST:
                    var upperScan = -1;
                    var lowerScan = -1;
                    if (this.GetMSLevel(inputScan) == 1) return inputScan;
                    for (var i = inputScan; i <= this.MaxLCScan; i++)
                    {
                        if (this.GetMSLevel(i) == 1)
                        {
                            upperScan = i;
                            break;
                        }
                    }
                    for (var i = inputScan; i >= this.MinLCScan; i--)
                    {
                        if (this.GetMSLevel(i) == 1)
                        {
                            lowerScan = i;
                            break;
                        }
                    }

                    if (upperScan == -1 && lowerScan == -1) return inputScan;
                    if (upperScan == -1 && lowerScan != -1) return lowerScan;
                    if (lowerScan == -1 && upperScan != -1) return upperScan;

                    if (Math.Abs(upperScan - inputScan) < Math.Abs(lowerScan - inputScan))
                    {
                        return upperScan;
                    }
                    else
                    {
                        return lowerScan;
                    }

                default:

                    return inputScan;
            }

        }

        /// <summary>
        /// Purpose is to return an array of MS1-level Scan values for the entire Run. 
        /// </summary>
        /// <returns>
        /// List of Scan numbers pertaining to MS1-level scans only. 
        /// </returns>
        public List<int> GetMSLevelScanValues()
        {
            return GetMSLevelScanValues(this.MinLCScan, this.MaxLCScan);
        }

        public List<int> GetMSLevelScanValues(int minScan, int maxScan)
        {
            if (this.msLevelScanIndexList == null)
            {
                msLevelScanIndexList = new List<int>();

                for (var i = minScan; i <= maxScan; i++)
                {
                    if (this.ContainsMSMSData)   // contains MS1 and MS2
                    {
                        if (this.GetMSLevel(i) == 1)
                        {
                            msLevelScanIndexList.Add(i);
                        }
                    }
                    else      // all MS are MS1
                    {
                        msLevelScanIndexList.Add(i);
                    }
                }
            }
            else
            {

            }
            return this.msLevelScanIndexList;
        }

        protected void addToMSLevelData(int scanNum, int mslevel)
        {
            if (this.MSLevelList.ContainsKey(scanNum))
            {
                // do nothing
            }
            else
            {
                this.MSLevelList.Add(scanNum, (byte)mslevel);
            }

        }

        protected Dictionary<int, byte> MSLevelList { get; set; }

        protected Dictionary<int, int> ParentScanList { get; set; }

        /// <summary>
        /// This indicates whether or not the intensity values from a summed/averaged mass spectrum is reported as an average or not.
        /// e.g. Thermo .raw reports values as an average
        /// </summary>
        public bool IsMsAbundanceReportedAsAverage { get; set; }

        public SortedDictionary<int, byte> GetMSLevels(int minScan, int maxScan)
        {
            return null;
        }

        public virtual void Close()
        {
            if (this.PeakList != null)
            {
                this.PeakList.Clear();
            }
            this.ResultCollection.ClearAllResults();
            this.XYData = null;

            GC.Collect();
        }

        public virtual string GetCurrentScanOrFrameInfo()
        {
            if (this.CurrentScanSet != null)
            {
                return "Scan = " + this.CurrentScanSet.PrimaryScanNumber.ToString();
            }
            else
            {
                return "Scan = NULL";
            }
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            this.Close();
        }

        #endregion

        #region Mass and NET Alignment
     

        public virtual void UpdateNETValuesInScanSetCollection()
        {
            if (NetAlignmentInfo == null) return;

            if (ScanSetCollection == null) return;

            foreach (var scan in ScanSetCollection.ScanSetList)
            {
                scan.NETValue=  (float) NetAlignmentInfo.GetNETValueForScan(scan.PrimaryScanNumber);
            }
        }



        /// <summary>
        /// Returns the adjusted m/z after alignment
        /// </summary>
        /// <param name="observedMZ"></param>
        /// <param name="scan"></param>
        /// <returns></returns>
        public double GetAlignedMZ(double observedMZ, double scan=-1)
        {
            if (MassAlignmentInfo == null) return observedMZ;

            var ppmShift = MassAlignmentInfo.GetPpmShift(observedMZ, (int) scan);


            var alignedMZ = observedMZ - (ppmShift * observedMZ / 1e6);
            return alignedMZ;
        }
    
        /// <summary>
        /// The method returns the m/z that you should look for, when m/z alignment is considered
        /// </summary>
        /// <param name="theorMZ"></param>
        /// <param name="scan"> </param>
        /// <returns></returns>
        public double GetTargetMZAligned(double theorMZ, double scan=-1)
        {
            if (MassAlignmentInfo == null) return theorMZ;

            var ppmShift =MassAlignmentInfo.GetPpmShift(theorMZ, (int) scan);

            var alignedMZ = theorMZ + (ppmShift * theorMZ / 1e6);
            return alignedMZ;
        }
        

        #endregion

        public virtual int GetParentScan(int scanLC)
        {
            throw new NotImplementedException();
        }

        public virtual double GetIonInjectionTimeInMilliseconds(int scanNum)
        {
            return 0;
        }


        public virtual double GetMS2IsolationWidth(int scanNum)
        {
            return 0;
        }
    }
}
