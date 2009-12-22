using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public abstract class IScanSet
    {

        public abstract List<int> IndexValues
        {
            get;
            set;
        }

        public abstract int PrimaryScanValue
        {
            get;
            set;
        }


        private double backgroundIntensity;

        public double BackgroundIntensity
        {
            get { return backgroundIntensity; }
            set { backgroundIntensity = value; }
        }

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


        internal virtual int getLowestScanNumber()
        {
            int lowVal = int.MaxValue;

            for (int i = 0; i < IndexValues.Count; i++)
            {
                if (IndexValues[i] < lowVal) lowVal = IndexValues[i];

            }
            return lowVal;
        }

        internal int getHighestScanNumber()
        {
            int highVal = -1;
            for (int i = 0; i < IndexValues.Count; i++)
            {
                if (IndexValues[i] > highVal) highVal = IndexValues[i];
            }
            return highVal;
        }


        internal int Count()
        {
            if (this.IndexValues == null || this.IndexValues.Count == 0) return 0;
            return this.IndexValues.Count;
        }




    }
}
