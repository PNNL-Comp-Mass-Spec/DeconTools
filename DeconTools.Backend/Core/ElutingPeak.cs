using System;
using System.Collections.Generic;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Core
{
    public class ElutingPeak:IDisposable
    {
        #region Constructors

        public ElutingPeak()
        {
            PeakList = new List<MSPeakResult>();
            //IsosResultList = new List<StandardIsosResult>();
            ID = -1;
            ScanStart = -1;
            ScanEnd = -1;
            ScanMaxIntensity = -1;

            RetentionTime = 0;
            Intensity = 0;
            SummedIntensity = 0;
            AggregateIntensity = 0;
            Mass = 0;
            ChargeState = 0;

            NumberOfPeaks = 0;
            NumberOfPeaksFlag = 0;
            NumberOfPeaksMode = "Current"; //"Current" for current peak or "NewPeak" for possible next peak after this one

        }

        #endregion

        #region Properties

        public int ID { get; set; }
        public ChromPeak ChromPeak { get; set; }
        public List<MSPeakResult> PeakList { get; set; }

        //public List<StandardIsosResult> IsosResultList { get; set; }

        public float RetentionTime { get; set; }

        public double Mass { get; set; }

        public double Intensity { get; set; }

        /// <summary>
        /// Summed intensity across time
        /// </summary>
        public double SummedIntensity { get; set; }

        public int ScanStart { get; set; }

        public int ScanEnd { get; set; }

        public int ScanMaxIntensity { get; set; }

        public int NumberOfPeaks { get; set; }

        public int NumberOfPeaksFlag { get; set; }

        public string NumberOfPeaksMode { get; set; }

        public ScanSet ScanSet { get; set; }

        public int ChargeState { get; set; }

        /// <summary>
        /// deisotoping fit score
        /// </summary>
        public float FitScore { get; set; }

        /// <summary>
        /// Aggregate intensity across mass
        /// </summary>
        public double AggregateIntensity { get; set; }


        #endregion

        public MSPeakResult GetMSPeakResultRepresentative()
        {
            if (PeakList == null || PeakList.Count == 0)
                return null;

            return PeakList[0];
        }

        #region IDisposable Members

        public void Dispose()
        {
            ChromPeak = null;
            PeakList = null;
            ScanSet = null;
        }

        #endregion
    }
}
