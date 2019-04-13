using System;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public class ChromPeakUtilities
    {
        readonly ScanSetFactory _scanSetFactory = new ScanSetFactory();

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public static XYData GetXYDataForChromPeak(ChromPeak peak, Run run)
        {
            var apex = peak.XValue;
            double width = peak.Width;
            var peakWidthSigma = width / 2.35;    // width@half-height = 2.35σ (Gaussian peak theory)
            var sixSigma = 6 * peakWidthSigma;	// width@base = 4σ (Gaussian peak theory)
            var halfSixSigma = sixSigma / 2.0;

            var minScan = apex - halfSixSigma;
            var maxScan = apex + halfSixSigma;

            var filteredXYData = run.XYData.TrimData(minScan, maxScan);
            return filteredXYData;
        }

        public ScanSet GetLCScanSetForChromPeak(Peak chromPeak, Run run, int numLCScansToSum)
        {
            if (chromPeak == null || Math.Abs(chromPeak.XValue) < double.Epsilon)
            {
                return null;
                //throw new NullReferenceException("Trying to use chromPeak to generate mass spectrum, but chromePeak is null");
            }

            var bestScan = (int)chromPeak.XValue;
            bestScan = run.GetClosestMSScan(bestScan, DeconTools.Backend.Globals.ScanSelectionMode.CLOSEST);

            var scanSet = _scanSetFactory.CreateScanSet(run, bestScan, numLCScansToSum);

            if (run.MSFileType == DeconTools.Backend.Globals.MSFileType.PNNL_UIMF)
            {
                // GORD: Update this when fixing CurrentFrameSet
                throw new NotSupportedException("UIMF workflows should use a UIMF specific peak selector.");
            }

            return scanSet;
        }


        public ScanSet GetLCScanSetForChromPeakUIMF(Peak chromPeak, Run run, int numLCScansToSum)
        {

            if (chromPeak == null || Math.Abs(chromPeak.XValue) < float.Epsilon)
            {
                return null;
                //throw new NullReferenceException("Trying to use chromPeak to generate mass spectrum, but chromPeak is null");
            }

            if (!(run is UIMFRun uimfRun))
                throw new InvalidCastException("run is not of type UIMFRun; actually type " + run.GetType());

            var chromPeakScan = (int)Math.Round(chromPeak.XValue);
            var bestLCScan = uimfRun.GetClosestMS1Frame(chromPeakScan);

            if (numLCScansToSum > 1)
            {
                throw new NotSupportedException("SmartChromPeakSelector is trying to set which frames are summed. But summing across frames isn't supported yet. Someone needs to add the code");
            }

            ScanSet lcScanSet;
            if (run.CurrentMassTag.MsLevel == 1)
            {
                lcScanSet = new ScanSet(bestLCScan);
            }
            else
            {
                // TODO: This is hard-coded to work with the "sum all consecutive MS2 frames mode" but we should really look these up by going through the IMSScanCollection
                lcScanSet = new ScanSet(bestLCScan + 1, bestLCScan + 1, bestLCScan + uimfRun.GetNumberOfConsecutiveMs2Frames(bestLCScan));
            }

            return lcScanSet;

            // TODO: Hard coded to sum across all IMS Scans.
            //var centerScan = (uimfRun.MinIMSScan + uimfRun.MaxIMSScan + 1) / 2;
            //uimfRun.CurrentIMSScanSet = new IMSScanSet(centerScan, uimfRun.MinIMSScan, uimfRun.MaxIMSScan);
        }


        /// <summary>
        /// Gets the LC ScanSet used in generating a mass spectrum, based on the width of the Chrom peak.
        /// </summary>
        /// <param name="chromPeak"></param>
        /// <param name="run"></param>
        /// <param name="peakWidthInSigma">number of sigma. In Gaussian peak theory, 4 sigma is often used to define
        /// the base of a Gaussian peak. 2.35 sigma is the peak at half height. </param>
        /// <param name="maxScansToSum"></param>
        /// <returns></returns>
        public ScanSet GetLCScanSetForChromPeakBasedOnPeakWidth(Peak chromPeak, Run run, double peakWidthInSigma = 2, int maxScansToSum = 100)
        {
            if (chromPeak == null || Math.Abs(chromPeak.XValue) < double.Epsilon)
            {
                return null;
                //throw new NullReferenceException("Trying to use chromPeak to generate mass spectrum, but chromPeak is null");
            }

            var bestScan = (int)chromPeak.XValue;
            bestScan = run.GetClosestMSScan(bestScan, DeconTools.Backend.Globals.ScanSelectionMode.CLOSEST);

            var sigma = chromPeak.Width / 2.35;

            var lowerScan = (int)Math.Round(chromPeak.XValue - (peakWidthInSigma * sigma / 2));
            var closestLowerScan = run.GetClosestMSScan(lowerScan, DeconTools.Backend.Globals.ScanSelectionMode.CLOSEST);

            var upperScan = (int)Math.Round(chromPeak.XValue + (peakWidthInSigma * sigma / 2));
            var closestUpperScan = run.GetClosestMSScan(upperScan, DeconTools.Backend.Globals.ScanSelectionMode.CLOSEST);

            var scanSet = _scanSetFactory.CreateScanSet(run, bestScan, closestLowerScan, closestUpperScan);
            _scanSetFactory.TrimScans(scanSet, maxScansToSum);


            if (run.MSFileType == DeconTools.Backend.Globals.MSFileType.PNNL_UIMF)
            {
                // GORD: Update this when fixing CurrentFrameSet
                throw new NotSupportedException("UIMF workflows should use a UIMF specific peak selector.");
            }

            return scanSet;
        }





        #endregion

        #region Private Methods

        #endregion

    }
}
