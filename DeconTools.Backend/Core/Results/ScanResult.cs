using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public abstract class ScanResult
    {

        public Run Run { get; set; }



        public abstract int NumPeaks
        {
            get;
            set;
        }

        public abstract int NumIsotopicProfiles
        {
            get;
            set;
        }

        public abstract MSPeak BasePeak
        {
            get;
            set;
        }

        public abstract float TICValue { get; set; }


        public abstract ScanSet ScanSet
        {
            get;
            set;

        }

        public abstract double ScanTime
        {
            get;
            set;
        }

        public abstract int SpectrumType
        {
            get;
            set;
        }



        public abstract void SetNumPeaks(int numPeaks);
        public abstract void SetNumIsotopicProfiles(int numIsotopicProfiles);
        public abstract void SetBasePeak(MSPeak mspeak);




    }
}
