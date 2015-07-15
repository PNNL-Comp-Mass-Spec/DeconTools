using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public abstract class ScanResultExporter : Task
    {
        #region Properties
        #endregion

        #region Public Methods
        public abstract void ExportScanResults(ResultCollection resultList);
        public abstract void ExportScanResult(ScanResult scanResult);

        int _indexOfLastScanResultWritten = -1;
        public override void Execute(ResultCollection resultList)
        {
            if (resultList.ScanResultList == null || resultList.ScanResultList.Count == 0) return;

            int currentScanResultIndex = resultList.ScanResultList.Count - 1;
            
            bool resultNotWritten = (currentScanResultIndex != _indexOfLastScanResultWritten);
            if (resultNotWritten)
            {
                ExportScanResult(resultList.ScanResultList[currentScanResultIndex]);
                _indexOfLastScanResultWritten = currentScanResultIndex;
            }



            //// check if results exceed Trigger value or is the last Scan 
            //bool isLastScan;
            //if (resultList.Run is UIMFRun)
            //{
            //    List<FrameSet> uimfFrameSet = ((UIMFRun)resultList.Run).FrameSetCollection.FrameSetList;

            //    int lastFrameNum = uimfFrameSet[uimfFrameSet.Count - 1].PrimaryFrame;
            //    int lastScanNum = resultList.Run.ScanSetCollection.ScanSetList[resultList.Run.ScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;

            //    isLastScan = (((UIMFRun)resultList.Run).CurrentScanSet.PrimaryScanNumber == lastFrameNum &&
            //        resultList.Run.CurrentScanSet.PrimaryScanNumber == lastScanNum);
            //}
            //else
            //{
            //    int lastScanNum = resultList.Run.ScanSetCollection.ScanSetList[resultList.Run.ScanSetCollection.ScanSetList.Count - 1].PrimaryScanNumber;
            //    isLastScan = (resultList.Run.CurrentScanSet.PrimaryScanNumber == lastScanNum);
            //}

            //if (isLastScan)
            //{
            //    ExportScanResults(resultList);
            //    CloseOutputFile();
            //}

        }

        protected virtual void CloseOutputFile()
        {
            //do nothing here. 
        }

        #endregion

        #region Private Methods

        protected string DblToString(double value, byte digitsAfterDecimal, bool limitDecimalsForLargeValues = false)
        {
            return PNNLOmics.Utilities.StringUtilities.DblToString(value, digitsAfterDecimal, limitDecimalsForLargeValues);
        }
        
        #endregion


    }
}
