using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public class SmartChromPeakSelectorUIMF : SmartChromPeakSelector
    {

        #region Constructors
		public SmartChromPeakSelectorUIMF(SmartChromPeakSelectorParameters parameters) : base(parameters)
		{
			
		}
        #endregion

		protected override void SetScansForMSGenerator(Core.ChromPeak chromPeak, Core.Run run, int numLCScansToSum)
        {

            if (chromPeak == null || chromPeak.XValue == 0)
            {
                return;
                //throw new NullReferenceException("Trying to use chromPeak to generate mass spectrum, but chrompeak is null");
            }

            var uimfrun = run as UIMFRun;

            var chromPeakScan = (int)Math.Round(chromPeak.XValue);
            var bestLCScan = uimfrun.GetClosestMS1Frame(chromPeakScan);

			if (numLCScansToSum > 1)
            {
                throw new NotSupportedException("SmartChrompeakSelector is trying to set which frames are summed. But summing across frames isn't supported yet. Someone needs to add the code");
            }
            else
            {
                var lcscanset = new ScanSet(bestLCScan);
                uimfrun.CurrentFrameSet = lcscanset;

				// TODO: Hard coded to sum across all IMS Scans.
				int centerScan = (uimfrun.MinIMSScan + uimfrun.MaxIMSScan + 1) / 2;
				uimfrun.CurrentIMSScanSet = new IMSScanSet(centerScan, uimfrun.MinIMSScan, uimfrun.MaxIMSScan);
            }
            
        }

        protected override void UpdateResultWithChromPeakAndLCScanInfo(Core.TargetedResultBase result, Core.ChromPeak bestPeak)
        {
            var uimfRun = result.Run as UIMFRun;

            result.ChromPeakSelected = bestPeak;

            result.ScanSet = new ScanSet(uimfRun.CurrentFrameSet.PrimaryScanNumber);
        }



    }
}
