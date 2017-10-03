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
            BasePeak = new MSPeak();
            IndexValues = new List<int>();
        }
        public ScanSet(int primaryScanNum)
            : this()
        {
            PrimaryScanNumber = primaryScanNum;
            IndexValues.Add(primaryScanNum);
        }

        public ScanSet(int primaryScanNum, IEnumerable<int> indexArray)
            : this()
        {
            PrimaryScanNumber = primaryScanNum;
            foreach (var indexItem in indexArray)
            {
                IndexValues.Add(indexItem);
            }
        }

        public ScanSet(int primaryScanNum, int lowerScan, int upperScan)
            : this()
        {
            Check.Require(lowerScan <= upperScan, "Lower scan number must be less than or equal to the upper scan number");
            this.PrimaryScanNumber = primaryScanNum;

            for (var i = lowerScan; i <= upperScan; i++)
            {
                this.IndexValues.Add(i);
            }

        }


        #endregion

        public int NumPeaks { get; set; }

        public int NumIsotopicProfiles { get; set; }

        public Peak BasePeak { get; set; }

        public float TICValue { get; set; }

        public float NETValue { get; set; }

        private List<int> indexValues;

        public List<int> IndexValues
        {
            get => indexValues;
            set => indexValues = value;
        }

        public int PrimaryScanNumber { get; set; }


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

        public double BackgroundIntensity { get; set; }


        //private List<int> m_MSPeakResultPeakListIndex;       //TODO:  SK ScanSet property added 9-16-10

        //public List<int> MSPeakResultPeakListIndex
        //{
        //    get { return m_MSPeakResultPeakListIndex; }
        //    set { m_MSPeakResultPeakListIndex = value; }
        //}

        internal virtual int getLowestScanNumber()
        {
            var lowVal = int.MaxValue;

            for (var i = 0; i < IndexValues.Count; i++)
            {
                if (IndexValues[i] < lowVal) lowVal = indexValues[i];

            }
            return lowVal;
        }

        internal int getHighestScanNumber()
        {
            var highVal = -1;
            for (var i = 0; i < IndexValues.Count; i++)
            {
                if (IndexValues[i] > highVal) highVal = indexValues[i];
            }
            return highVal;
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(PrimaryScanNumber);

            if (indexValues.Count > 1)    //if there is summing, will show these scans in the string
            {
                sb.Append(" {");
                for (var i = 0; i < indexValues.Count; i++)
                {
                    var isLast = (i == indexValues.Count - 1);
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


        internal int GetScanCount()
        {
            if (this.indexValues == null || this.indexValues.Count == 0) return 0;
            return this.indexValues.Count;
        }


    }
}
