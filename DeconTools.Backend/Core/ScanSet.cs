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


        

      


        internal int Count()
        {
            if (this.indexValues == null || this.indexValues.Count == 0) return 0;
            return this.indexValues.Count;
        }

        
    }
}
