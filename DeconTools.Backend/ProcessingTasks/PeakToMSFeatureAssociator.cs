using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
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

        public override void Execute(ResultCollection resultColl)
        {
            //don't do anything if there aren't any MSFeatures
            if (resultColl.IsosResultBin == null || resultColl.IsosResultBin.Count == 0) return;

            //don't do anything if there aren't any peaks
            if (resultColl.Run.PeakList == null || resultColl.Run.PeakList.Count == 0) return;


            foreach (var msfeature in resultColl.IsosResultBin)
            {

                foreach (var peak in msfeature.IsotopicProfile.Peaklist)
                {
                    double targetMZ = peak.XValue;
                    toleranceInPPM = 0.1d;

                    double toleranceInMZ = toleranceInPPM * targetMZ / 1e6;

                    //binary search to find peak
                    int indexOfPeak = PeakUtilities.getIndexOfClosestValue(resultColl.Run.PeakList, targetMZ, 0, resultColl.Run.PeakList.Count - 1, toleranceInMZ);
                    if (indexOfPeak != -1)
                    {
                        IPeak foundpeak = resultColl.Run.PeakList[indexOfPeak];

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
