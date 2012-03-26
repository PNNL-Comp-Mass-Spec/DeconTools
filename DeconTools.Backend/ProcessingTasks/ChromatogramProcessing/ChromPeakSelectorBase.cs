using System;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public abstract class ChromPeakSelectorBase : Task
    {

        ScanSetFactory _scansetFactory = new ScanSetFactory();

        #region Constructors
        #endregion

        #region Properties

        public abstract ChromPeakSelectorParameters Parameters { get; set; }


        #endregion
        protected ScanSet CreateSummedScanSet(ChromPeak chromPeak, Run run)
        {
            if (chromPeak == null || chromPeak.XValue == 0) return null;

            int bestScan = (int)chromPeak.XValue;
            bestScan = run.GetClosestMSScan(bestScan, Globals.ScanSelectionMode.CLOSEST);

            switch (this.Parameters.SummingMode)
            {
                case SummingModeEnum.SUMMINGMODE_STATIC:
                    return _scansetFactory.CreateScanSet(run, bestScan, Parameters.NumScansToSum);

                case SummingModeEnum.SUMMINGMODE_DYNAMIC:
                    double sigma = chromPeak.Width / 2.35;

                    int lowerScan = (int)Math.Round(chromPeak.XValue - (Parameters.AreaOfPeakToSumInDynamicSumming * sigma));
                    int closestLowerScan = run.GetClosestMSScan(lowerScan, Globals.ScanSelectionMode.CLOSEST);

                    int upperScan = (int)Math.Round(chromPeak.XValue + (Parameters.AreaOfPeakToSumInDynamicSumming * sigma));
                    int closestUpperScan = run.GetClosestMSScan(upperScan, Globals.ScanSelectionMode.CLOSEST);

                    ScanSet scanset = _scansetFactory.CreateScanSet(run, bestScan, closestLowerScan, closestUpperScan);
                    _scansetFactory.TrimScans(scanset, this.Parameters.MaxScansSummedInDynamicSumming);

                    return scanset;

                default:
                    return _scansetFactory.CreateScanSet(run, bestScan, this.Parameters.NumScansToSum);
            }



        }

    }
}
