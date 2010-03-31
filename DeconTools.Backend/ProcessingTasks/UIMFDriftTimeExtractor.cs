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
            Check.Require(resultList.Run != null, "UIMF Drift Time extractor failed. Run is null");
            Check.Require(resultList.Run is UIMFRun, "UIMF Drift Time extractor only works on UIMF files.");
            Check.Require(resultList != null, "UIMF Drift Time extractor failed. ResultCollection is null");

            UIMFRun uimfRun = (UIMFRun)resultList.Run;


            foreach (IsosResult result in resultList.IsosResultBin)
            {
                if (result is UIMFIsosResult)
                {
                    UIMFIsosResult uimfResult = (UIMFIsosResult)result;
                    uimfResult.DriftTime = uimfRun.GetDriftTime(uimfResult.FrameSet.PrimaryFrame, uimfResult.ScanSet.PrimaryScanNumber);
                }
            }


            // = uimfRun.GetDriftTime(uimfRun.CurrentFrameSet.PrimaryFrame, uimfRun.CurrentScanSet.PrimaryScanNumber);

        }
    }
}
