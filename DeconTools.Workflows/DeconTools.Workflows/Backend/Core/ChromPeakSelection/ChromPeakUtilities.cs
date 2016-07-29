using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public class ChromPeakUtilities
    {
        ScanSetFactory _scansetFactory = new ScanSetFactory();
        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods
		public static XYData GetXYDataForChromPeak(ChromPeak peak, Run run)
		{
			double apex = peak.XValue;
			double width = peak.Width;
			double peakWidthSigma = width / 2.35;    // width@half-height = 2.35σ (Gaussian peak theory)
			double sixSigma = 6 * peakWidthSigma;	// width@base = 4σ (Gaussian peak theory)
			double halfSixSigma = sixSigma / 2.0;

			double minScan = apex - halfSixSigma;
			double maxScan = apex + halfSixSigma;

			XYData filteredXYData = run.XYData.TrimData(minScan, maxScan);
			return filteredXYData;
		}

        public ScanSet GetLCScanSetForChromPeak(Peak chromPeak, Run run, int numLCScansToSum)
        {
            ScanSet scanset;

            if (chromPeak == null || chromPeak.XValue == 0)
            {
                return null;
                //throw new NullReferenceException("Trying to use chromPeak to generate mass spectrum, but chrompeak is null");
            }

            var bestScan = (int)chromPeak.XValue;
            bestScan = run.GetClosestMSScan(bestScan, DeconTools.Backend.Globals.ScanSelectionMode.CLOSEST);

                    scanset = _scansetFactory.CreateScanSet(run, bestScan, numLCScansToSum);
            
            if (run.MSFileType == DeconTools.Backend.Globals.MSFileType.PNNL_UIMF)
            {
                // GORD: Update this when fixing CurrentFrameSet
                throw new NotSupportedException("UIMF worflows should use a UIMF specific peak selector.");
            }

            return scanset;
        }


        public ScanSet GetLCScanSetForChromPeakUIMF(Peak chromPeak, Run run, int numLCScansToSum)
        {

            if (chromPeak == null || Math.Abs(chromPeak.XValue) < float.Epsilon)
            {
                return null;
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
                ScanSet lcscanset;
                if (run.CurrentMassTag.MsLevel == 1)
                {
                    lcscanset = new ScanSet(bestLCScan);
                }
                else
                {
                    // TODO: This is hard-coded to work with the "sum all consecutive MS2 frames mode" but we should really look these up by going through the IMSScanCollection
                    lcscanset = new ScanSet(bestLCScan + 1, bestLCScan + 1, bestLCScan + uimfrun.GetNumberOfConsecutiveMs2Frames(bestLCScan));
                }

                return lcscanset;

                // TODO: Hard coded to sum across all IMS Scans.
                int centerScan = (uimfrun.MinIMSScan + uimfrun.MaxIMSScan + 1) / 2;
                uimfrun.CurrentIMSScanSet = new IMSScanSet(centerScan, uimfrun.MinIMSScan, uimfrun.MaxIMSScan);
            }

        }





        /// <summary>
        /// Gets the LC Scanset used in generating a mass spectrum, based on the width of the Chrom peak. 
        /// </summary>
        /// <param name="chromPeak"></param>
        /// <param name="run"></param>
        /// <param name="peakWidthInSigma">number of sigma. In Gaussian peak theory, 4 sigma is often used to define
        /// the base of a Gaussian peak. 2.35 sigma is the peak at half height. </param>
        /// <param name="maxScansToSum"></param>
        /// <returns></returns>
        public ScanSet GetLCScanSetForChromPeakBasedOnPeakWidth(Peak chromPeak, Run run, double peakWidthInSigma = 2, int maxScansToSum = 100)
        {
            ScanSet scanset;

            if (chromPeak == null || chromPeak.XValue == 0)
            {
                return null;
                //throw new NullReferenceException("Trying to use chromPeak to generate mass spectrum, but chrompeak is null");
            }

            var bestScan = (int)chromPeak.XValue;
            bestScan = run.GetClosestMSScan(bestScan, DeconTools.Backend.Globals.ScanSelectionMode.CLOSEST);

            double sigma = chromPeak.Width / 2.35;
            
            int lowerScan = (int)Math.Round(chromPeak.XValue - (peakWidthInSigma * sigma / 2));
            int closestLowerScan = run.GetClosestMSScan(lowerScan, DeconTools.Backend.Globals.ScanSelectionMode.CLOSEST);

            int upperScan = (int)Math.Round(chromPeak.XValue + (peakWidthInSigma * sigma / 2));
            int closestUpperScan = run.GetClosestMSScan(upperScan, DeconTools.Backend.Globals.ScanSelectionMode.CLOSEST);

            scanset = _scansetFactory.CreateScanSet(run, bestScan, closestLowerScan, closestUpperScan);
            _scansetFactory.TrimScans(scanset, maxScansToSum);


            if (run.MSFileType == DeconTools.Backend.Globals.MSFileType.PNNL_UIMF)
            {
                // GORD: Update this when fixing CurrentFrameSet
                throw new NotSupportedException("UIMF worflows should use a UIMF specific peak selector.");
            }

            return scanset;
        }





        #endregion

        #region Private Methods

        #endregion

    }
}
