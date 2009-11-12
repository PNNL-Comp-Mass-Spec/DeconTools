using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks
{
    public class UIMFDriftTimeExtractor : Task
    {
        public UIMFDriftTimeExtractor()
        {

        }

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run is UIMFRun, "UIMF Drift Time extractor only works on UIMF files.");
            UIMFRun uimfRun = (UIMFRun)resultList.Run;

            uimfRun.CurrentScanSet.DriftTime = uimfRun.GetDriftTime(uimfRun.CurrentFrameSet.PrimaryFrame, uimfRun.CurrentScanSet.PrimaryScanNumber);

        }
    }
}
