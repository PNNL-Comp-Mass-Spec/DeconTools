using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class UIMF_TICExtractor : I_TIC_Extractor
    {

        public override void getTIC(DeconTools.Backend.Core.ResultCollection resultList)
        {
            Check.Require(resultList.Run is UIMFRun, "UIMF_TIC_Extractor only works on UIMF files");

            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(resultList.Run.Filename);

            UIMFRun uimfRun = (UIMFRun)resultList.Run;


            //float ticValue=0;
            //adapter.Datareader.GetTIC(ref ticValue, uimfRun.CurrentFrameSet.PrimaryFrame, uimfRun.CurrentScanSet.PrimaryScanNumber);

            double ticValue = adapter.Datareader.GetTIC(uimfRun.CurrentFrameSet.PrimaryFrame, uimfRun.CurrentScanSet.PrimaryScanNumber);

            resultList.Run.CurrentScanSet.TICValue = (float)ticValue;

        }
    }
}
