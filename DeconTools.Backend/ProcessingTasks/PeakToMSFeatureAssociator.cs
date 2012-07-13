﻿using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class PeakToMSFeatureAssociator : Task
    {
        private double toleranceInPPM;

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public override void Execute(ResultCollection resultList)
        {
            //don't do anything if there aren't any MSFeatures
            if (resultList.IsosResultBin == null || resultList.IsosResultBin.Count == 0) return;

            //don't do anything if there aren't any peaks
            if (resultList.Run.PeakList == null || resultList.Run.PeakList.Count == 0) return;


            foreach (var msfeature in resultList.IsosResultBin)
            {

                foreach (var peak in msfeature.IsotopicProfile.Peaklist)
                {
                    double targetMZ = peak.XValue;
                    toleranceInPPM = 0.1d;

                    double toleranceInMZ = toleranceInPPM * targetMZ / 1e6;

                    //binary search to find peak
                    int indexOfPeak = PeakUtilities.getIndexOfClosestValue(resultList.Run.PeakList, targetMZ, 0, resultList.Run.PeakList.Count - 1, toleranceInMZ);
                    if (indexOfPeak != -1)
                    {
                        Peak foundpeak = resultList.Run.PeakList[indexOfPeak];

                        if (foundpeak is MSPeak)
                        {
                            ((MSPeak)foundpeak).MSFeatureID = peak.MSFeatureID;
                        }
                    }
                }


            }




        }
    }
}
