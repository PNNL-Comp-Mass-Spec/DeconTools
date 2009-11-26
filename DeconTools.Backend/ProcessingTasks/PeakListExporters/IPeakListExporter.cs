using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using System.IO;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public abstract class IPeakListExporter : Task
    {

        public abstract int TriggerToWriteValue { get; set; }
        public abstract int[] MSLevelsToExport { get; set; }

        public abstract void WriteOutPeaks(ResultCollection resultList);



        public override void Execute(ResultCollection resultList)
        {
            if (resultList.MSPeakResultList == null || resultList.MSPeakResultList.Count == 0) return;

            // check if peak results exceeds Trigger value or is the last Scan 
            int lastScanNum = resultList.Run.ScanSetCollection.ScanSetList[resultList.Run.ScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;
            bool isLastScan = (resultList.Run.CurrentScanSet.PrimaryScanNumber == lastScanNum);


            if (resultList.MSPeakResultList.Count >= TriggerToWriteValue || isLastScan)
            {
                WriteOutPeaks(resultList);
                resultList.MSPeakResultList.Clear();
            }
        }






    }
}
