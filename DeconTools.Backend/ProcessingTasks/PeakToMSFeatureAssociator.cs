using DeconTools.Backend.Core;
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


            foreach (var msFeature in resultList.IsosResultBin)
            {

                foreach (var peak in msFeature.IsotopicProfile.Peaklist)
                {
                    var targetMZ = peak.XValue;
                    toleranceInPPM = 0.1d;

                    var toleranceInMZ = toleranceInPPM * targetMZ / 1e6;

                    //binary search to find peak
                    var indexOfPeak = PeakUtilities.getIndexOfClosestValue(resultList.Run.PeakList, targetMZ, 0, resultList.Run.PeakList.Count - 1, toleranceInMZ);
                    if (indexOfPeak != -1)
                    {
                        var foundPeak = resultList.Run.PeakList[indexOfPeak];

                        if (foundPeak is MSPeak msPeak)
                        {
                            msPeak.MSFeatureID = peak.MSFeatureID;
                        }
                    }
                }

            }

        }
    }
}
