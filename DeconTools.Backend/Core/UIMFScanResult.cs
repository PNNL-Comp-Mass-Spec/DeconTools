using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class UIMFScanResult : ScanResult
    {
        public UIMFScanResult(FrameSet frameset)
        {
            this.frameset = frameset;
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


        public int FrameNum { get; set; }



        private FrameSet frameset;

        public FrameSet Frameset
        {
            get { return frameset; }
            set { frameset = value; }
        }

        private double framePressureFront;

        public double FramePressureFront
        {
            get { return framePressureFront; }
            set { framePressureFront = value; }
        }

        private double framePressureBack;

        public double FramePressureBack
        {
            get { return framePressureBack; }
            set { framePressureBack = value; }
        }


        public override void SetNumPeaks(int numPeaks)
        {
            this.numPeaks += numPeaks;                      //number of peaks is summed across all scans in one frame
        }

        public override void SetNumIsotopicProfiles(int numIsotopicProfiles)
        {
            this.numIsotopicProfiles += numIsotopicProfiles;    //number of peaks is summed across all scans in one frame
        }

        public override void SetBasePeak(MSPeak mspeak)
        {

            if (this.basePeak == null || mspeak.Height > this.basePeak.Height)     //best peak from all scans in one frame
            {
                this.basePeak = mspeak;
            }

        }


        private int spectrumType;
        public override int SpectrumType
        {
            get { return spectrumType; }
            set { spectrumType = value; }
        }


    }
}
