using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public class O16O18ChromCorrelator : IqChromCorrelatorBase
    {
        public O16O18ChromCorrelator(int numPointsInSmoother, double minRelativeIntensityForChromCorr, double chromTolerance, DeconTools.Backend.Globals.ToleranceUnit toleranceUnit)
            : base(numPointsInSmoother, minRelativeIntensityForChromCorr, chromTolerance, toleranceUnit)
        {

        }


        public override ChromCorrelationData CorrelateData(Run run, IqResult iqResult, int startScan, int stopScan)
        {
            var correlationData = new ChromCorrelationData();

            double o16MzValue = iqResult.Target.TheorIsotopicProfile.Peaklist[0].XValue;
            double o18SingleLabelMzValue = iqResult.Target.TheorIsotopicProfile.Peaklist[2].XValue;
            double o18DoubleLabelMzValue = iqResult.Target.TheorIsotopicProfile.Peaklist[4].XValue;

            var o16ChromXyData = GetBaseChromXYData(run, startScan, stopScan, o16MzValue);
            var o18SingleLabelChromXyData = GetBaseChromXYData(run, startScan, stopScan, o18SingleLabelMzValue);
            var o18DoubleLabelChromXyData = GetBaseChromXYData(run, startScan, stopScan, o18DoubleLabelMzValue);

            double slope, intercept, rsquaredVal;
            GetElutionCorrelationData(o16ChromXyData, o18SingleLabelChromXyData, out slope, out intercept, out rsquaredVal);
            correlationData.AddCorrelationData(slope, intercept, rsquaredVal);

            GetElutionCorrelationData(o16ChromXyData, o18DoubleLabelChromXyData, out slope, out intercept, out rsquaredVal);
            correlationData.AddCorrelationData(slope, intercept, rsquaredVal);

            GetElutionCorrelationData(o18SingleLabelChromXyData, o18DoubleLabelChromXyData, out slope, out intercept, out rsquaredVal);
            correlationData.AddCorrelationData(slope, intercept, rsquaredVal);

            return correlationData;
        }
    }
}
