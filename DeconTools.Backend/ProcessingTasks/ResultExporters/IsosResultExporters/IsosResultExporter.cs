using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public abstract class IsosResultExporter : Task
    {
        #region Constructors
        #endregion

        #region Properties
        public abstract int TriggerToExport { get; set; }

        protected List<int> MSFeatureIDsWritten = new List<int>();

        #endregion

        #region Public Methods

        public abstract void ExportIsosResults(List<IsosResult> isosResultList);
        public override void Execute(ResultCollection resultList)
        {
            if (resultList.ResultList == null || resultList.ResultList.Count == 0) return;

            // check if results exceed Trigger value or is the last Scan 
            bool isLastScan;
            if (resultList.Run is UIMFRun)
            {
                var uimfRun = (UIMFRun) resultList.Run;

                var lcScanSet = resultList.Run.ScanSetCollection.ScanSetList;

                int lastFrameNum = lcScanSet[lcScanSet.Count - 1].PrimaryScanNumber;
                int lastIMSScanNum = uimfRun.IMSScanSetCollection.ScanSetList[uimfRun.IMSScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;

                isLastScan = uimfRun.CurrentScanSet.PrimaryScanNumber == lastFrameNum &&
                    uimfRun.CurrentIMSScanSet.PrimaryScanNumber == lastIMSScanNum;
            }
            else
            {
                int lastScanNum = resultList.Run.ScanSetCollection.ScanSetList[resultList.Run.ScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;
                isLastScan = (resultList.Run.CurrentScanSet.PrimaryScanNumber == lastScanNum);
            }



            if (resultList.ResultList.Count >= TriggerToExport || isLastScan)
            {
                ExportIsosResults(resultList.ResultList);
                MSFeatureIDsWritten.Clear();
                resultList.ResultList.Clear();
            }

        }


        #endregion

        #region Private Methods
        #endregion

    }
}
