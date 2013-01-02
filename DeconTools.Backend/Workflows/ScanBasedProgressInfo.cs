using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.Workflows
{
    public class ScanBasedProgressInfo
    {
        public ScanBasedProgressInfo(Run currentRun, ScanSet scanSet, IMSScanSet imsScanSet = null)
        {
            this.currentRun = currentRun;
            this.currentScanSet = scanSet;

            if (currentRun is UIMFRun)
            {
                if (imsScanSet == null)
                    throw new NullReferenceException("ScanBasedProgressInfo error. You need to provide a valid IMSScanSet");
                ((UIMFRun)currentRun).CurrentIMSScanSet = imsScanSet;
            }


        }

        private float percentDone;

        public float PercentDone
        {
            get { return percentDone; }
            set { percentDone = value; }
        }

        private Run currentRun;

        public Run CurrentRun
        {
            get { return currentRun; }
            set { currentRun = value; }
        }

        private ScanSet currentScanSet;

        public ScanSet CurrentScanSet
        {
            get { return currentScanSet; }
            set { currentScanSet = value; }
        }


        public int GetScanOrFrameNum()
        {
            if (currentRun == null) return -1;

            if (CurrentScanSet == null) return -1;
            return CurrentScanSet.PrimaryScanNumber;
        }






    }
}