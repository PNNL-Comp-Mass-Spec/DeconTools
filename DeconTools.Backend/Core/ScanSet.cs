using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class ScanSet
    {

        #region Constructors
        public ScanSet()
        {

        }
        public ScanSet(int primaryScanNum)
        {
            this.primaryScanNumber = primaryScanNum;
            this.IndexValues = new List<int>();
            this.IndexValues.Add(primaryScanNum);
            this.basePeak = new MSPeak();

        }

        public ScanSet(int primaryScanNum, int[] indexArray)
        {
            this.IndexValues = new List<int>();
            this.PrimaryScanNumber = primaryScanNum;
            foreach (int indexItem in indexArray)
            {
                this.IndexValues.Add(indexItem);
            }
        }

        public ScanSet(int primaryScanNum, int lowerScan, int upperScan)
        {
            Check.Require(lowerScan <= upperScan, "Lower scan number must be less than or equal to the upper scan number");
            this.IndexValues = new List<int>();
            this.primaryScanNumber = primaryScanNum;

            for (int i = lowerScan; i <= upperScan; i++)
            {
                this.IndexValues.Add(i);
            }

        }


        #endregion
  
        private int numPeaks;

        public int NumPeaks
        {
            get { return numPeaks; }
            set { numPeaks = value; }
        }

        private int numIsotopicProfiles;

        public int NumIsotopicProfiles
        {
            get { return numIsotopicProfiles; }
            set { numIsotopicProfiles = value; }
        }

        private MSPeak basePeak;

        public MSPeak BasePeak
        {
            get { return basePeak; }
            set { basePeak = value; }
        }

        private float tICValue;

        public float TICValue
        {
            get { return tICValue; }
            set { tICValue = value; }
        }

        public float NETValue { get; set; }

        private List<int> indexValues;

        public virtual List<int> IndexValues
        {
            get { return indexValues; }
            set { indexValues = value; }
        }

        private int primaryScanNumber;

        public int PrimaryScanNumber
        {
            get { return primaryScanNumber; }
            set { primaryScanNumber = value; }
        }


        private double backgroundIntensity;


        public void AddScan(int scanNumber)
        {
            if (this.indexValues != null)
            {
                indexValues.Add(scanNumber);
            }
            else
            {
                indexValues = new List<int>();
                indexValues.Add(scanNumber);
            }
        }
        
        public double BackgroundIntensity
        {
            get { return backgroundIntensity; }
            set { backgroundIntensity = value; }
        }

        private double driftTime;          //TODO:    in the future, want to put this in 'UIMFScanSet'; but will keep it simple for now

        public double DriftTime
        {
            get { return driftTime; }
            set { driftTime = value; }
        }

        //private List<int> m_MSPeakResultPeakListIndex;       //TODO:  SK ScanSet property added 9-16-10

        //public List<int> MSPeakResultPeakListIndex
        //{
        //    get { return m_MSPeakResultPeakListIndex; }
        //    set { m_MSPeakResultPeakListIndex = value; }
        //}

        internal virtual int getLowestScanNumber()
        {
            int lowVal = int.MaxValue;

            for (int i = 0; i < IndexValues.Count; i++)
            {
                if (IndexValues[i] < lowVal) lowVal = indexValues[i];

            }
            return lowVal;
        }

        internal int getHighestScanNumber()
        {
            int highVal = -1;
            for (int i = 0; i < IndexValues.Count; i++)
            {
                if (IndexValues[i] > highVal) highVal = indexValues[i];
            }
            return highVal;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(primaryScanNumber);

            if (indexValues.Count > 1)    //if there is summing, will show these scans in the string
            {
                sb.Append(" {");
                for (int i = 0; i < indexValues.Count; i++)
                {
                    bool isLast = (i == indexValues.Count - 1);
                    sb.Append(indexValues[i]);
                    if (isLast)
                    {
                        sb.Append("}");
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                }
            }
            return sb.ToString();
 
        }


        internal int Count()
        {
            if (this.indexValues == null || this.indexValues.Count == 0) return 0;
            return this.indexValues.Count;
        }

        
    }
}
