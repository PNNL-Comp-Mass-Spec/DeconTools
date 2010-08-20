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

        public abstract void ExportIsosResults(List<IsosResult> isosResultList);
        public override void Execute(ResultCollection resultColl)
        {
            if (resultColl.ResultList == null || resultColl.ResultList.Count == 0) return;

            // check if results exceed Trigger value or is the last Scan 
            bool isLastScan;
            if (resultColl.Run is UIMFRun)
            {
                List<FrameSet> uimfFrameSet = ((UIMFRun)resultColl.Run).FrameSetCollection.FrameSetList;

                int lastFrameNum = uimfFrameSet[uimfFrameSet.Count - 1].PrimaryFrame;
                int lastScanNum = resultColl.Run.ScanSetCollection.ScanSetList[resultColl.Run.ScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;

                isLastScan = (((UIMFRun)resultColl.Run).CurrentFrameSet.PrimaryFrame == lastFrameNum &&
                    resultColl.Run.CurrentScanSet.PrimaryScanNumber == lastScanNum);
            }
            else
            {
                int lastScanNum = resultColl.Run.ScanSetCollection.ScanSetList[resultColl.Run.ScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;
                isLastScan = (resultColl.Run.CurrentScanSet.PrimaryScanNumber == lastScanNum);
            }



            if (resultColl.ResultList.Count >= TriggerToExport || isLastScan)
            {
                ExportIsosResults(resultColl.ResultList);
                resultColl.ResultList.Clear();
            }

        }


        #endregion

        #region Private Methods
        #endregion

    }
}
