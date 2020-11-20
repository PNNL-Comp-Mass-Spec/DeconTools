using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public class ChromatogramCorrelator : ChromatogramCorrelatorBase
    {
        public ChromatogramCorrelator(int numPointsInSmoother, double minRelativeIntensityForChromCorr = 0.01, double chromToleranceInPPM = 20, Globals.ToleranceUnit chromToleranceUnit = Globals.ToleranceUnit.PPM)
            : base(numPointsInSmoother, minRelativeIntensityForChromCorr, chromToleranceInPPM, chromToleranceUnit)
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
