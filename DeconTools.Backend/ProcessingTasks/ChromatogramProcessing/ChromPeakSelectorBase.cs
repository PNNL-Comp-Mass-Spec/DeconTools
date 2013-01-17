using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public abstract class ChromPeakSelectorBase : Task
    {

        ScanSetFactory _scansetFactory = new ScanSetFactory();

        #region Constructors

        public ChromPeakSelectorBase()
        {
            IsotopicProfileType= Globals.IsotopicProfileType.UNLABELLED;
        }

        #endregion

        #region Properties

        public abstract ChromPeakSelectorParameters Parameters { get; set; }

        public Globals.IsotopicProfileType IsotopicProfileType { get; set; }


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

        protected virtual void SetScansForMSGenerator(ChromPeak chromPeak, Run run, int numLCScansToSum)
        {
			ScanSet scanset;

            if (chromPeak == null || chromPeak.XValue == 0)
            {
                return;
                //throw new NullReferenceException("Trying to use chromPeak to generate mass spectrum, but chrompeak is null");
            }

            var bestScan = (int)chromPeak.XValue;
            bestScan = run.GetClosestMSScan(bestScan, Globals.ScanSelectionMode.CLOSEST);

            switch (Parameters.SummingMode)
            {
                case SummingModeEnum.SUMMINGMODE_STATIC:
					scanset = _scansetFactory.CreateScanSet(run, bestScan, numLCScansToSum);
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
					scanset = _scansetFactory.CreateScanSet(run, bestScan, numLCScansToSum);
                    break;
            }

			if (run.MSFileType == Globals.MSFileType.PNNL_UIMF)
			{
				// GORD: Update this when fixing CurrentFrameSet
				throw new NotSupportedException("UIMF worflows should use a UIMF specific peak selector.");
			}
			
			run.CurrentScanSet = scanset;
        }

        protected virtual void UpdateResultWithChromPeakAndLCScanInfo(TargetedResultBase result, ChromPeak bestPeak)
        {
            result.AddSelectedChromPeakAndScanSet(bestPeak, result.Run.CurrentScanSet, IsotopicProfileType);
            result.WasPreviouslyProcessed = true;    //indicate that this result has been added to...  use this to help control the addition of labelled (N15) data
        }

    }
}
