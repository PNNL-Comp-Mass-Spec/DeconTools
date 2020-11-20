using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public abstract class IsosResultExporter : Task
    {
        protected const int MAX_SECONDS_BETWEEN_EXPORT = 60;

        #region Constructors

        protected IsosResultExporter()
        {
            LastExportTime = DateTime.UtcNow;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Number of features to cache in memory before writing to disk
        /// </summary>
        public abstract int TriggerToExport { get; set; }

        /// <summary>
        /// Last export time; features will be written to disk if 60 seconds elapses
        /// </summary>
        protected DateTime LastExportTime;

        protected List<int> MSFeatureIDsWritten = new List<int>();

        #endregion

        #region Public Methods

        public abstract void ExportIsosResults(List<IsosResult> isosResultList);
        public override void Execute(ResultCollection resultList)
        {
            if (resultList.ResultList == null || resultList.ResultList.Count == 0) return;

            // check if results exceed Trigger value or is the last Scan
            bool isLastScan;
            if (resultList.Run is UIMFRun uimfRun)
            {
                var lcScanSet = resultList.Run.ScanSetCollection.ScanSetList;

                var lastFrameNum = lcScanSet[lcScanSet.Count - 1].PrimaryScanNumber;
                var lastIMSScanNum = uimfRun.IMSScanSetCollection.ScanSetList[uimfRun.IMSScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;

                isLastScan = uimfRun.CurrentScanSet.PrimaryScanNumber == lastFrameNum &&
                    uimfRun.CurrentIMSScanSet.PrimaryScanNumber == lastIMSScanNum;
            }
            else
            {
                var lastScanNum = resultList.Run.ScanSetCollection.ScanSetList[resultList.Run.ScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;
                isLastScan = (resultList.Run.CurrentScanSet.PrimaryScanNumber == lastScanNum);
            }

            if (resultList.ResultList.Count >= TriggerToExport ||
                DateTime.UtcNow.Subtract(LastExportTime).TotalSeconds >= MAX_SECONDS_BETWEEN_EXPORT ||
                isLastScan)
            {
                ExportIsosResults(resultList.ResultList);
                MSFeatureIDsWritten.Clear();
                resultList.ResultList.Clear();

                LastExportTime = DateTime.UtcNow;
            }
        }

        #endregion

    }
}
