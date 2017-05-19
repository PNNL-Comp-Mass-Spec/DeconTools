using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

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

            var uimfRun = (UIMFRun)resultList.Run;


            foreach (var result in resultList.IsosResultBin)
            {
                if (result is UIMFIsosResult)
                {
                    var uimfResult = (UIMFIsosResult)result;
                    uimfResult.DriftTime = uimfRun.GetDriftTime(uimfResult.ScanSet.PrimaryScanNumber, uimfResult.IMSScanSet.PrimaryScanNumber);
                }
            }


            // = uimfRun.GetDriftTime(uimfRun.CurrentScanSet.PrimaryScanNumber, uimfRun.CurrentScanSet.PrimaryScanNumber);

        }
    }
}
