using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public class IqChromCorrelator : IqChromCorrelatorBase
    {
        public IqChromCorrelator(int numPointsInSmoother, double minRelativeIntensityForChromCorr = 0.01, double chromToleranceInPPM = 20, DeconTools.Backend.Globals.ToleranceUnit chromToleranceUnit = DeconTools.Backend.Globals.ToleranceUnit.PPM)
            : base(numPointsInSmoother, minRelativeIntensityForChromCorr, chromToleranceInPPM, chromToleranceUnit)
        {
        }

        public override ChromCorrelationData CorrelateData(Run run, IqResult iqResult, int startScan, int stopScan)
        {
            Check.Require(iqResult!=null,"ChromCorrelator failed. IqResult is null.");
            Check.Require(iqResult.ObservedIsotopicProfile!=null, "ChromCorrelator failed. ObservedIsotopicProfile is null for IqResult");

            return CorrelatePeaksWithinIsotopicProfile(run, iqResult.ObservedIsotopicProfile, startScan, stopScan);
            }
    }
}
