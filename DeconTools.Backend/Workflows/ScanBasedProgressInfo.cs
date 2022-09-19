using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.Workflows
{
    public class ScanBasedProgressInfo
    {
        public ScanBasedProgressInfo(Run run, ScanSet scanSet, IMSScanSet imsScanSet = null)
        {
            CurrentRun = run;
            CurrentScanSet = scanSet;

            if (CurrentRun is UIMFRun uimfRun)
            {
                if (imsScanSet == null)
                {
                    throw new NullReferenceException("ScanBasedProgressInfo error. You need to provide a valid IMSScanSet");
                }

                uimfRun.CurrentIMSScanSet = imsScanSet;
            }
        }

        public float PercentDone { get; set; }

        public Run CurrentRun { get; set; }

        public ScanSet CurrentScanSet { get; set; }

        public int GetScanOrFrameNum()
        {
            if (CurrentRun == null)
            {
                return -1;
            }

            if (CurrentScanSet == null)
            {
                return -1;
            }

            return CurrentScanSet.PrimaryScanNumber;
        }
    }
}