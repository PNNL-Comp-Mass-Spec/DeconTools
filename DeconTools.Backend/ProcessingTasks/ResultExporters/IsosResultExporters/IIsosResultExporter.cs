using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public abstract class IIsosResultExporter : Task
    {
        #region Constructors
        #endregion

        #region Properties
        public abstract int TriggerToExport { get; set; }
        #endregion

        #region Public Methods

        public abstract void ExportIsosResults(ResultCollection resultList);
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



            if (resultList.ResultList.Count >= TriggerToExport || isLastScan)
            {
                ExportIsosResults(resultList);
                resultList.ResultList.Clear();
            }

        }


        #endregion

        #region Private Methods
        #endregion

    }
}
