using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public class IqChromCorrelator : IqChromCorrelatorBase
    {
        public IqChromCorrelator(int numPointsInSmoother, double minRelativeIntensityForChromCorr = 0.01, double chromToleranceInPPM = 20, DeconTools.Backend.Globals.ToleranceUnit chromToleranceUnit = DeconTools.Backend.Globals.ToleranceUnit.PPM)
            : base(numPointsInSmoother, minRelativeIntensityForChromCorr, chromToleranceInPPM, chromToleranceUnit)
        {
        }

        public override ChromCorrelationData CorrelateData(Run run, IsotopicProfile iso, int startScan, int stopScan)
        {
            {
                return CorrelatePeaksWithinIsotopicProfile(run, iso, startScan, stopScan);
            }
        }
    }
}
