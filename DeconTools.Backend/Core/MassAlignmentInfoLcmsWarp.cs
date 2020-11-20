
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Data;

namespace DeconTools.Backend.Core
{
    public class MassAlignmentInfoLcmsWarp : MassAlignmentInfo
    {
        #region Properties
        public MultiAlignEngine.Alignment.clsAlignmentFunction AlignmentInfo { get; set; }

        #endregion

        #region Public Methods

        public void SetMassAlignmentData(List<MassAlignmentDataItem> massAlignmentData)
        {
            var mzVals = massAlignmentData.Select(p => p.mz).ToArray();
            var mzPPMCorrVals = massAlignmentData.Select(p => p.mzPPMCorrection).ToArray();
            var scanVals = massAlignmentData.Select(p => p.scan).ToArray();
            var scanPPMCorrVals = massAlignmentData.Select(p => p.scanPPMCorrection).ToArray();

            if (AlignmentInfo == null) AlignmentInfo = new MultiAlignEngine.Alignment.clsAlignmentFunction(MultiAlignEngine.Alignment.enmCalibrationType.HYBRID_CALIB, MultiAlignEngine.Alignment.enmAlignmentType.NET_MASS_WARP);

            AlignmentInfo.marrMassFncMZInput = new float[mzVals.Length];
            AlignmentInfo.marrMassFncMZPPMOutput = new float[mzVals.Length];
            AlignmentInfo.marrMassFncTimeInput = new float[mzVals.Length];
            AlignmentInfo.marrMassFncTimePPMOutput = new float[mzVals.Length];

            AlignmentInfo.SetMassCalibrationFunctionWithMZ(ref mzVals, ref mzPPMCorrVals);
            AlignmentInfo.SetMassCalibrationFunctionWithTime(ref scanVals, ref scanPPMCorrVals);
        }

        public void SetMassAlignmentData(ViperMassCalibrationData viperCalibrationData)
        {
            var numDataPoints = 3;

            var mzVals = new float[numDataPoints];
            var mzPPMCorrVals = new float[numDataPoints];

            mzVals[0] = 0;
            mzVals[1] = 1000;
            mzVals[2] = 2000;

            for (var index = 0; index < mzPPMCorrVals.Length; index++)
            {
                mzPPMCorrVals[index] = (float)viperCalibrationData.MassError;
            }

            //scanVals[0] = (MinLCScan + MaxLCScan) / (float)2;
            //for (int index = 0; index < numDataPoints; index++)
            //{
            //    scanPPMCorrVals[index] = (float)viperCalibrationData.MassError;
            //}

            if (AlignmentInfo == null)
                AlignmentInfo =
                    new MultiAlignEngine.Alignment.clsAlignmentFunction(MultiAlignEngine.Alignment.enmCalibrationType.HYBRID_CALIB,
                                                                        MultiAlignEngine.Alignment.enmAlignmentType.NET_MASS_WARP);

            AlignmentInfo.marrMassFncMZInput = new float[mzVals.Length];
            AlignmentInfo.marrMassFncMZPPMOutput = new float[mzVals.Length];

            // ReSharper disable CommentTypo
            //this.AlignmentInfo.marrMassFncTimeInput = new float[mzVals.Length];
            //this.AlignmentInfo.marrMassFncTimePPMOutput = new float[mzVals.Length];
            // ReSharper restore CommentTypo

            AlignmentInfo.SetMassCalibrationFunctionWithMZ(ref mzVals, ref mzPPMCorrVals);
        }

        #endregion

        #region Private Methods

        #endregion

        public override double GetPpmShift(double mz, int scan = -1)
        {
            if (AlignmentInfo == null) return 0;

            var alignmentInfoContainsScanInfo = (AlignmentInfo.marrMassFncTimeInput != null && AlignmentInfo.marrMassFncTimeInput.Length > 0);
            var alignmentInfoContainsMZInfo = (AlignmentInfo.marrMassFncMZInput != null && AlignmentInfo.marrMassFncMZInput.Length > 0);

            var canUseScanWhenGettingPPMShift = alignmentInfoContainsScanInfo && alignmentInfoContainsMZInfo && scan >= 0;

            float mzForGettingAlignmentInfo;
            if (alignmentInfoContainsMZInfo)
            {
                //check if mz is less than lower limit
                if (mz < AlignmentInfo.marrMassFncMZInput[0])
                {
                    mzForGettingAlignmentInfo = AlignmentInfo.marrMassFncMZInput[0];
                }
                else if (mz > AlignmentInfo.marrMassFncMZInput[AlignmentInfo.marrMassFncMZInput.Length - 1])   //check if mz is greater than upper limit
                {
                    mzForGettingAlignmentInfo = AlignmentInfo.marrMassFncMZInput[AlignmentInfo.marrMassFncMZInput.Length - 1];
                }
                else
                {
                    mzForGettingAlignmentInfo = (float)mz;
                }
            }
            else
            {
                return 0;
            }

            if (canUseScanWhenGettingPPMShift)
            {
                var scanForGettingAlignmentInfo = (float)scan;

                if (scanForGettingAlignmentInfo < AlignmentInfo.marrMassFncTimeInput[0])
                {
                    scanForGettingAlignmentInfo = AlignmentInfo.marrMassFncTimeInput[0];
                }
                else if (scanForGettingAlignmentInfo > AlignmentInfo.marrMassFncTimeInput[AlignmentInfo.marrMassFncTimeInput.Length - 1])
                {
                    scanForGettingAlignmentInfo = AlignmentInfo.marrMassFncTimeInput[AlignmentInfo.marrMassFncTimeInput.Length - 1];
                }
                else
                {
                    scanForGettingAlignmentInfo = scan;
                }

                var ppmShift = AlignmentInfo.GetPPMShiftFromTimeMZ(scanForGettingAlignmentInfo, mzForGettingAlignmentInfo);
                return ppmShift;
            }
            else
            {
                var ppmShift = AlignmentInfo.GetPPMShiftFromMZ(mzForGettingAlignmentInfo);
                return ppmShift;
            }
        }
    }
}
