using System;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public abstract class ScanResult
    {

        protected ScanResult(ScanSet scanSet)
        {
            ScanSet = scanSet;
        }

        protected ScanResult()
        {
            ScanSet = null;
        }

        public Run Run { get; set; }
        public int NumPeaks { get; set; }
        public int NumIsotopicProfiles { get; set; }
        public Peak BasePeak { get; set; }
        public float TICValue { get; set; }
        public ScanSet ScanSet { get; set; }
        public double ScanTime { get; set; }
        public int SpectrumType { get; set; }
        public string Description { get; set; }
    }
}
