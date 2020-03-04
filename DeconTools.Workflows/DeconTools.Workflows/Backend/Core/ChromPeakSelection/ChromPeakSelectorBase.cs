using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public abstract class ChromPeakSelectorBase : Task
    {
        protected ChromPeakUtilities ChromPeakUtilities = new ChromPeakUtilities();

        #region Constructors

        protected ChromPeakSelectorBase()
        {
            IsotopicProfileType= DeconTools.Backend.Globals.IsotopicProfileType.UNLABELED;
        }

        #endregion

        #region Public Methods

        public abstract Peak SelectBestPeak(List<ChromPeakQualityData> peakQualityList, bool filterOutFlaggedIsotopicProfiles);


        #endregion

        #region Properties

        public abstract ChromPeakSelectorParameters Parameters { get; set; }

        public DeconTools.Backend.Globals.IsotopicProfileType IsotopicProfileType { get; set; }


        #endregion

        protected virtual void UpdateResultWithChromPeakAndLCScanInfo(TargetedResultBase result, ChromPeak bestPeak)
        {
            result.AddSelectedChromPeakAndScanSet(bestPeak, result.Run.CurrentScanSet, IsotopicProfileType);
            result.WasPreviouslyProcessed = true;    //indicate that this result has been added to...  use this to help control the addition of labeled (N15) data
        }

    }
}
