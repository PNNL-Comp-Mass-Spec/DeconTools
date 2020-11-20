using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public abstract class IPeakListExporter : Task
    {
        public abstract int TriggerToWriteValue { get; set; }
        public abstract int[] MSLevelsToExport { get; set; }

        public abstract void WriteOutPeaks(List<MSPeakResult> peakList);
        public abstract void WriteOutPeaks(StreamWriter sw, List<MSPeakResult> peakList);

        public override void Execute(ResultCollection resultList)
        {
            if (resultList.MSPeakResultList == null || resultList.MSPeakResultList.Count == 0) return;

            // check if peak results exceeds Trigger value or is the last Scan 

            var lastScanNum = resultList.Run.ScanSetCollection.ScanSetList[resultList.Run.ScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;

            var isLastScan = true;
            var writeOutPeaksNoMatterWhat = false;

            if (resultList.Run is UIMFRun)
            {
                isLastScan = false;  // this doesn't matter since we are writing out the peaks no matter what.
                writeOutPeaksNoMatterWhat = true;
            }
            else
            {
                isLastScan = (resultList.Run.CurrentScanSet.PrimaryScanNumber == lastScanNum);
            }

            //Write out results if exceeds trigger value or is last scan
            if (resultList.MSPeakResultList.Count >= TriggerToWriteValue || isLastScan || writeOutPeaksNoMatterWhat)
            {
                WriteOutPeaks(resultList.MSPeakResultList);
                resultList.MSPeakResultList.Clear();
            }
        }
    }
}
