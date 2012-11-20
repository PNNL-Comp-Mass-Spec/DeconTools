using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public abstract class IPeakListExporter : Task
    {

        public abstract int TriggerToWriteValue { get; set; }
        public abstract int[] MSLevelsToExport { get; set; }

        public abstract void WriteOutPeaks(List<MSPeakResult> peakResultList);


        public override void Execute(ResultCollection resultList)
        {
            if (resultList.MSPeakResultList == null || resultList.MSPeakResultList.Count == 0) return;

            // check if peak results exceeds Trigger value or is the last Scan 

            int lastScanNum = resultList.Run.ScanSetCollection.ScanSetList[resultList.Run.ScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;
            bool isLastScan = (resultList.Run.CurrentScanSet.PrimaryScanNumber == lastScanNum);
            bool writeOutPeaksNoMatterWhat = resultList.Run is UIMFRun;

          
            //Write out results if exceeds trigger value or is last scan
            if (resultList.MSPeakResultList.Count >= TriggerToWriteValue || isLastScan || writeOutPeaksNoMatterWhat)
            {
                WriteOutPeaks(resultList.MSPeakResultList);
                resultList.MSPeakResultList.Clear();
            }
        }
    }
}
