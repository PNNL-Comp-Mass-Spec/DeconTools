using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class StandardScanResult : ScanResult
    {

        public StandardScanResult(ScanSet scanset)
        {
            this.ScanSet = scanset;

        }


        private int numPeaks;

        public override int NumPeaks
        {
            get { return numPeaks; }
            set { numPeaks = value; }
        }
        private int numIsotopicProfiles;

        public override int NumIsotopicProfiles
        {
            get { return numIsotopicProfiles; }
            set { numIsotopicProfiles = value; }
        }
        private MSPeak basePeak;

        public override MSPeak BasePeak
        {
            get { return basePeak; }
            set { basePeak = value; }
        }
        private float tICValue;

        public override float TICValue
        {
            get { return tICValue; }
            set { tICValue = value; }
        }


        private ScanSet scanSet;

        public override ScanSet ScanSet
        {
            get { return scanSet; }
            set { scanSet = value; }
        }

        private double scanTime;

        public override double ScanTime
        {
            get { return scanTime; }
            set { scanTime = value; }
        }

        private int spectrumType;
        public override int SpectrumType
        {
            get { return spectrumType; }
            set { spectrumType = value; }
        }




        public override void SetNumPeaks(int numPeaks)
        {
            this.numPeaks = numPeaks;
        }

        public override void SetNumIsotopicProfiles(int numIsotopicProfiles)
        {
            this.numIsotopicProfiles = numIsotopicProfiles;
        }

        public override void SetBasePeak(MSPeak mspeak)
        {
            this.basePeak = mspeak;
        }
    }
}
