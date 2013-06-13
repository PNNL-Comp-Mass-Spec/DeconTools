using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public class O16O18ChromCorrelator:IqChromCorrelatorBase
    {
        public O16O18ChromCorrelator(int numPointsInSmoother, double minRelativeIntensityForChromCorr, double chromTolerance, DeconTools.Backend.Globals.ToleranceUnit toleranceUnit) : base(numPointsInSmoother, minRelativeIntensityForChromCorr, chromTolerance, toleranceUnit)
        {

        }

        public override ChromCorrelationData CorrelateData(Run run, IqResult iqResult, int startScan, int stopScan)
        {
            var correlationData = new ChromCorrelationData();
            
            if (iqResult.Target.TheorIsotopicProfile==null|| iqResult.Target.TheorIsotopicProfile.Peaklist.Count < 5) return correlationData;

            double o16MZValue = iqResult.Target.TheorIsotopicProfile.Peaklist[0].XValue;

            double o18MZValue = run.GetTargetMZAligned(iqResult.Target.TheorIsotopicProfile.Peaklist[4].XValue,iqResult.LcScanObs);

            bool o16O18ChromDataIsOK;
            var o16ChromXyData = GetBaseChromXYData(run, startScan, stopScan, o16MZValue);
            var o18ChromXyData = GetBaseChromXYData(run, startScan, stopScan, o18MZValue);

            double slope, intercept,rsquaredVal;
            GetElutionCorrelationData(o16ChromXyData, o18ChromXyData, out slope, out intercept, out rsquaredVal);

            correlationData.AddCorrelationData(slope, intercept, rsquaredVal);

           
            return correlationData;





        }

    }
}
