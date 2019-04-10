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
            BasePeak = new MSPeak(0);
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
            PrimaryScanNumber = primaryScanNum;

            for (var i = lowerScan; i <= upperScan; i++)
            {
                IndexValues.Add(i);
            }

        }


        #endregion

        public int NumPeaks { get; set; }

        public int NumIsotopicProfiles { get; set; }

        public Peak BasePeak { get; set; }

        public float TICValue { get; set; }

        public float NETValue { get; set; }

        public List<int> IndexValues { get; set; }

        public int PrimaryScanNumber { get; set; }

        public void AddScan(int scanNumber)
        {
            if (IndexValues != null)
            {
                IndexValues.Add(scanNumber);
            }
            else
            {
                IndexValues = new List<int> {scanNumber};
            }
        }

        public double BackgroundIntensity { get; set; }


        //private List<int> m_MSPeakResultPeakListIndex;       //TODO:  SK ScanSet property added 9-16-10

        //public List<int> MSPeakResultPeakListIndex
        //{
        //    get { return m_MSPeakResultPeakListIndex; }
        //    set { m_MSPeakResultPeakListIndex = value; }
        //}

        internal int GetLowestScanNumber()
        {
            var lowVal = int.MaxValue;

            foreach (var value in IndexValues)
            {
                if (value < lowVal) lowVal = value;
            }
            return lowVal;
        }

        internal int GetHighestScanNumber()
        {
            var highVal = -1;
            foreach (var value in IndexValues)
            {
                if (value > highVal) highVal = value;
            }
            return highVal;
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(PrimaryScanNumber);

            if (IndexValues.Count > 1)    //if there is summing, will show these scans in the string
            {
                sb.Append(" {");
                for (var i = 0; i < IndexValues.Count; i++)
                {
                    var isLast = (i == IndexValues.Count - 1);
                    sb.Append(IndexValues[i]);
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
            if (IndexValues == null || IndexValues.Count == 0) return 0;
            return IndexValues.Count;
        }


    }
}
