using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public abstract class IScanResultExporter : Task
    {
        #region Properties
        #endregion

        #region Public Methods
        public abstract void ExportScanResults(ResultCollection resultList);
        public override void Execute(ResultCollection resultList)
        {
            if (resultList.ResultList == null || resultList.ResultList.Count == 0) return;

            // check if results exceed Trigger value or is the last Scan 
            bool isLastScan;
            if (resultList.Run is UIMFRun)
            {
                List<FrameSet> uimfFrameSet = ((UIMFRun)resultList.Run).FrameSetCollection.FrameSetList;

                int lastFrameNum = uimfFrameSet[uimfFrameSet.Count - 1].PrimaryFrame;
                int lastScanNum = resultList.Run.ScanSetCollection.ScanSetList[resultList.Run.ScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;

                isLastScan = (((UIMFRun)resultList.Run).CurrentFrameSet.PrimaryFrame == lastFrameNum &&
                    resultList.Run.CurrentScanSet.PrimaryScanNumber == lastScanNum);
            }
            else
            {
                int lastScanNum = resultList.Run.ScanSetCollection.ScanSetList[resultList.Run.ScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;
                isLastScan = (resultList.Run.CurrentScanSet.PrimaryScanNumber == lastScanNum);
            }

            if (isLastScan)
            {
                ExportScanResults(resultList);
                CloseOutputFile();
            }

        }

        protected virtual void CloseOutputFile()
        {
            //do nothing here. 
        }

        #endregion

        #region Private Methods
        #endregion


    }
}
