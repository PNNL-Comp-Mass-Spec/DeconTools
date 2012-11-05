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

            switch (Parameters.SummingMode)
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

        protected virtual void SetScansForMSGenerator(ChromPeak chromPeak, Run run, bool sumLCScans)
        {
            if (sumLCScans)
            {
                if (chromPeak == null || chromPeak.XValue == 0)
                {
                    return;
                    //throw new NullReferenceException("Trying to use chromPeak to generate mass spectrum, but chrompeak is null");
                }

                var bestScan = (int)chromPeak.XValue;
                bestScan = run.GetClosestMSScan(bestScan, Globals.ScanSelectionMode.CLOSEST);

                ScanSet scanset;
                switch (Parameters.SummingMode)
                {
                    case SummingModeEnum.SUMMINGMODE_STATIC:
                        scanset = _scansetFactory.CreateScanSet(run, bestScan, Parameters.NumScansToSum);
                        break;
                    case SummingModeEnum.SUMMINGMODE_DYNAMIC:
                        double sigma = chromPeak.Width / 2.35;

                        int lowerScan = (int)Math.Round(chromPeak.XValue - (Parameters.AreaOfPeakToSumInDynamicSumming * sigma));
                        int closestLowerScan = run.GetClosestMSScan(lowerScan, Globals.ScanSelectionMode.CLOSEST);

                        int upperScan = (int)Math.Round(chromPeak.XValue + (Parameters.AreaOfPeakToSumInDynamicSumming * sigma));
                        int closestUpperScan = run.GetClosestMSScan(upperScan, Globals.ScanSelectionMode.CLOSEST);

                        scanset = _scansetFactory.CreateScanSet(run, bestScan, closestLowerScan, closestUpperScan);
                        _scansetFactory.TrimScans(scanset, this.Parameters.MaxScansSummedInDynamicSumming);

                        break;


                    default:
                        scanset = _scansetFactory.CreateScanSet(run, bestScan, this.Parameters.NumScansToSum);
                        break;
                }

                run.CurrentScanSet = scanset;

            }
            else
            {
                if (chromPeak == null || chromPeak.XValue == 0)
                {
                    throw new ApplicationException("Trying to use chromPeak to create target scans for MSGenerator, but chromPeak is null");
                }

                int bestScan = (int)chromPeak.XValue;
                bestScan = run.GetClosestMSScan(bestScan, Globals.ScanSelectionMode.CLOSEST);

                int numScansToSum = 1;
                var scanset = new ScanSetFactory().CreateScanSet(run, bestScan, numScansToSum);


                run.CurrentScanSet = scanset;
                
            }


        }


        protected virtual void UpdateResultWithChromPeakAndLCScanInfo(TargetedResultBase result, ChromPeak bestPeak)
        {
            result.ChromPeakSelected = bestPeak;
            result.ScanSet = result.Run.CurrentScanSet;


            int numMSScansSummed;
            if (result.ScanSet == null || result.ScanSet.IndexValues == null || result.ScanSet.IndexValues.Count == 0)
            {
                numMSScansSummed = 0;
            }
            else
            {
                numMSScansSummed = result.ScanSet.IndexValues.Count;

            }

            result.NumMSScansSummed = numMSScansSummed;
            result.WasPreviouslyProcessed = true;    //indicate that this result has been added to...  use this to help control the addition of labelled (N15) data
        }

    }
}
