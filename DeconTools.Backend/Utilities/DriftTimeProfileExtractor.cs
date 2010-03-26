using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.DTO;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities
{
    public class DriftTimeProfileExtractor
    {
        #region Constructors
        public DriftTimeProfileExtractor()
        {

        }
        #endregion

        #region Properties




        #endregion

        #region Public Methods

        public XYData ExtractProfileFromPeakData(Run run, FrameSet frameSet, ScanSetCollection scanSetCollection, double targetMZ, double toleranceInPPM)
        {
            Check.Require(run.ResultCollection.MSPeakResultList != null && run.ResultCollection.MSPeakResultList.Count > 0, "MSPeakResultList hasn't been populated.");

            UIMFRun uimfrun = (UIMFRun)run;
            UIMF_MSGenerator msgen = new UIMF_MSGenerator();

            uimfrun.CurrentFrameSet = frameSet;

            int minScan = scanSetCollection.ScanSetList[0].PrimaryScanNumber;
            int maxScan = scanSetCollection.ScanSetList.Last().PrimaryScanNumber;

            List<MSPeakResult> filteredPeakList = run.ResultCollection.MSPeakResultList.Where(p => p.Frame_num == frameSet.PrimaryFrame)
                .Where(p => p.Scan_num >= minScan && p.Scan_num <= maxScan).ToList();



            XYData driftTimeProfileData = ExtractProfileFromPeakData(filteredPeakList, targetMZ, toleranceInPPM);
            for (int i = 0; i < driftTimeProfileData.Xvalues.Length; i++)
            {
                driftTimeProfileData.Xvalues[i] = uimfrun.GetDriftTime(frameSet.PrimaryFrame, (int)driftTimeProfileData.Xvalues[i]);


            }
            return driftTimeProfileData;

        }


        public XYData ExtractProfileFromPeakData(List<MSPeakResult> peakList, double targetMZ, double toleranceInPPM)
        {

            double toleranceInMZ = toleranceInPPM * targetMZ / 1e6;
            double lowerMZ = targetMZ - toleranceInMZ;

            double upperMZ = targetMZ + toleranceInMZ;


            List<MSPeakResult> filteredPeakList = new List<MSPeakResult>();
            filteredPeakList = filteredPeakList.Where(p => p.MSPeak.XValue >= lowerMZ && p.MSPeak.XValue <= upperMZ).ToList();

            XYData chrom = PeakUtilities.GetChromatogram(peakList, targetMZ, toleranceInMZ);


            return chrom;

        }



        public XYData ExtractProfileFromRawData(Run run, FrameSet frameSet, ScanSetCollection scanSetCollection, double targetMZ, double toleranceInPPM)
        {
            UIMFRun uimfrun = (UIMFRun)run;
            UIMF_MSGenerator msgen = new UIMF_MSGenerator();

            uimfrun.CurrentFrameSet = frameSet;

            XYData driftTimeProfileData = new XYData();

            driftTimeProfileData.Xvalues = new double[scanSetCollection.ScanSetList.Count];
            driftTimeProfileData.Yvalues = new double[scanSetCollection.ScanSetList.Count];

            for (int i = 0; i < scanSetCollection.ScanSetList.Count; i++)
            {
                ScanSet s = scanSetCollection.ScanSetList[i];

                run.CurrentScanSet = s;
                msgen.Execute(run.ResultCollection);

                driftTimeProfileData.Xvalues[i] = uimfrun.GetDriftTime(frameSet.PrimaryFrame, s.PrimaryScanNumber);
                driftTimeProfileData.Yvalues[i] = getdriftDimensionIntensityVal(run.XYData, targetMZ, toleranceInPPM);

            }

            return driftTimeProfileData;

        }

        private double getdriftDimensionIntensityVal(XYData xYData, double targetMZ, double toleranceInPPM)
        {
            //
            double toleranceInMZ = toleranceInPPM * targetMZ / 1e6;
            int indexOfMZValWithinTol = MathUtils.BinarySearchWithTolerance(xYData.Xvalues, targetMZ, 0, xYData.Xvalues.Length - 1, toleranceInMZ);
            double summedIntensity = 0;

            if (indexOfMZValWithinTol == -1) return 0;

            //look to the left of the found point and sum intensities if within tolerance
            if (indexOfMZValWithinTol > 0)
            {

                for (int i = indexOfMZValWithinTol - 1; i >= 0; i--)
                {
                    if (Math.Abs(xYData.Xvalues[i] - targetMZ) <= toleranceInMZ)
                    {
                        summedIntensity += xYData.Yvalues[i];

                    }
                    else
                    {
                        break;
                    }
                }
            }

            //sum intensity of middle point and points to the right, if within tolerance
            for (int i = indexOfMZValWithinTol; i < xYData.Xvalues.Length; i++)
            {
                if (Math.Abs(xYData.Xvalues[i] - targetMZ) <= toleranceInMZ)
                {
                    summedIntensity += xYData.Yvalues[i];

                }
                else
                {
                    break;
                }

            }

            return summedIntensity;

        }



        #endregion

        #region Private Methods
        #endregion
    }
}
