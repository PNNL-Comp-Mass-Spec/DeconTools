using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using System.IO;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public abstract class IPeakListExporter : Task
    {

        public abstract int TriggerToWriteValue { get; set; }
        public abstract int[] MSLevelsToExport { get; set; }

        public abstract void WriteOutPeaks(ResultCollection resultList);
        protected abstract void CloseOutputFile();


        public override void Execute(ResultCollection resultList)
        {
            if (resultList.MSPeakResultList == null || resultList.MSPeakResultList.Count == 0) return;

            // check if peak results exceeds Trigger value or is the last Scan 
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

            //Write out results if exceeds trigger value or is last scan
            if (resultList.MSPeakResultList.Count >= TriggerToWriteValue || isLastScan)
            {
                WriteOutPeaks(resultList);
                resultList.MSPeakResultList.Clear();

                if (isLastScan)
                {
                    CloseOutputFile();
                }

            }
        }






    }
}
