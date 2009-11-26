using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class MSPeakResult
    {
        private MSPeak mSPeak;

        public MSPeak MSPeak
        {
            get { return mSPeak; }
            set { mSPeak = value; }
        }

        private ScanSet scanSet;

        public ScanSet ScanSet
        {
            get { return scanSet; }
            set { scanSet = value; }
        }

    }
}
