using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class UIMF_TICExtractor : TICExtractor
    {
        public override void GetTIC(DeconTools.Backend.Core.ResultCollection resultList)
        {
            Check.Require(resultList.Run is UIMFRun, "UIMF_TIC_Extractor only works on UIMF files");

            var uimfRun = (UIMFRun)resultList.Run;
            var ticValue = uimfRun.GetTIC(uimfRun.CurrentScanSet.PrimaryScanNumber, uimfRun.CurrentIMSScanSet.PrimaryScanNumber);
            uimfRun.CurrentIMSScanSet.TICValue = ticValue;
        }
    }
}
