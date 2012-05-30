using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class UIMF_TICExtractor : TICExtractor
    {
        public override void GetTIC(DeconTools.Backend.Core.ResultCollection resultList)
        {
            Check.Require(resultList.Run is UIMFRun, "UIMF_TIC_Extractor only works on UIMF files");

            UIMFRun uimfRun = (UIMFRun)resultList.Run;
            float ticValue = uimfRun.GetTIC(uimfRun.CurrentFrameSet, uimfRun.CurrentScanSet);
            resultList.Run.CurrentScanSet.TICValue = (float)ticValue;
        }
    }
}
