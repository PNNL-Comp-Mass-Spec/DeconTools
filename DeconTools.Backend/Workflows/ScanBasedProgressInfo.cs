using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.Workflows
{
    public class ScanBasedProgressInfo
    {
        public ScanBasedProgressInfo(Run currentRun, ScanSet scanSet, IMSScanSet imsScanSet)
        {
            this.currentRun = currentRun;
            this.currentScanSet = scanSet;
            
            if (currentRun is UIMFRun)
            {
                ((UIMFRun) currentRun).CurrentIMSScanSet = imsScanSet;
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
        private FrameSet currentFrameSet;

        public FrameSet CurrentFrameSet
        {
            get { return currentFrameSet; }
            set { currentFrameSet = value; }
        }

        public int getScanOrFrameNum()
        {
            if (this.currentRun == null) return -1;
            if (this.currentRun is UIMFRun)
            {
                if (currentFrameSet == null) return -1;
                else
                {
                    return currentFrameSet.PrimaryFrame;
                }
            }
            else
            {
                if (currentScanSet == null) return -1;
                else
                {
                    return currentScanSet.PrimaryScanNumber;
                }

            }

        }






    }
}