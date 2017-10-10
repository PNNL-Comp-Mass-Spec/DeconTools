using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class UIMFDriftTimeExtractor : Task
    {

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList != null, "UIMF Drift Time extractor failed. ResultCollection is null");
            if (resultList == null)
                return;

            Check.Require(resultList.Run != null, "UIMF Drift Time extractor failed. Run is null");
            Check.Require(resultList.Run is UIMFRun, "UIMF Drift Time extractor only works on UIMF files.");

            var uimfRun = (UIMFRun)resultList.Run;
            if (uimfRun == null)
                return;

            foreach (var result in resultList.IsosResultBin)
            {
                if (result is UIMFIsosResult uimfResult)
                {
                    uimfResult.DriftTime = uimfRun.GetDriftTime(uimfResult.ScanSet.PrimaryScanNumber, uimfResult.IMSScanSet.PrimaryScanNumber);
                }
            }


            // = uimfRun.GetDriftTime(uimfRun.CurrentScanSet.PrimaryScanNumber, uimfRun.CurrentScanSet.PrimaryScanNumber);

        }
    }
}
