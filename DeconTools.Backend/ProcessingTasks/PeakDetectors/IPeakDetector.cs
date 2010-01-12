using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using System.IO;

namespace DeconTools.Backend.ProcessingTasks
{
    public abstract class IPeakDetector:Task
    {

        public abstract void FindPeaks(ResultCollection resultList);

        protected abstract void addDataToScanResult(ResultCollection resultList, ScanResult scanresult);

        protected MSPeak getBasePeak(List<IPeak> peakList)
        {
            if (peakList == null || peakList.Count == 0) return new MSPeak();

            IPeak maxPeak;
            if (!(peakList[0] is MSPeak)) return null;
            maxPeak = peakList[0];
          

            foreach (IPeak peak in peakList)
            {
                if (peak.Height >= maxPeak.Height)
                {
                    maxPeak = peak;
                }
               
            }
            return (MSPeak)maxPeak;

        }


        public override void Execute(ResultCollection resultList)
        {
            if (resultList.Run.XYData == null ||
                resultList.Run.XYData.Xvalues == null ||
                resultList.Run.XYData.Yvalues == null ||
                resultList.Run.XYData.Xvalues.Length == 0 ||
                resultList.Run.XYData.Yvalues.Length == 0)
            {
                resultList.AddLog("Peak Detector aborted; XY data is empty; Scan = " + resultList.Run.CurrentScanSet.PrimaryScanNumber.ToString());


            }

            FindPeaks(resultList);
            //addDataToScanResult(resultList, resultList.GetCurrentScanResult());
            
        }
    }
}
