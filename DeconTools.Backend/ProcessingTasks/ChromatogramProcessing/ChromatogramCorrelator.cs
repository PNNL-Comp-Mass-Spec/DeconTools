using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public class ChromatogramCorrelator : ChromatogramCorrelatorBase
    {
        public ChromatogramCorrelator(int numPointsInSmoother, int chromToleranceInPPM=20, double minRelativeIntensityForChromCorr=0.01)
            : base(numPointsInSmoother, chromToleranceInPPM, minRelativeIntensityForChromCorr)
        {
        }

        public override ChromCorrelationData CorrelateData(Run run, TargetedResultBase result, int startScan, int stopScan)
        {
            {
                return CorrelatePeaksWithinIsotopicProfile(run, result.IsotopicProfile, startScan, stopScan);
            }
        }
    }
}
