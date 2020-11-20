using DeconTools.Backend.Core;

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

            var o16MzValue = iqResult.Target.TheorIsotopicProfile.Peaklist[0].XValue;
            var o18SingleLabelMzValue = iqResult.Target.TheorIsotopicProfile.Peaklist[2].XValue;
            var o18DoubleLabelMzValue = iqResult.Target.TheorIsotopicProfile.Peaklist[4].XValue;

            var o16ChromXyData = GetBaseChromXYData(run, startScan, stopScan, o16MzValue);
            var o18SingleLabelChromXyData = GetBaseChromXYData(run, startScan, stopScan, o18SingleLabelMzValue);
            var o18DoubleLabelChromXyData = GetBaseChromXYData(run, startScan, stopScan, o18DoubleLabelMzValue);

            GetElutionCorrelationData(o16ChromXyData, o18SingleLabelChromXyData, out var slope, out var intercept, out var rSquaredVal);
            correlationData.AddCorrelationData(slope, intercept, rSquaredVal);

            GetElutionCorrelationData(o16ChromXyData, o18DoubleLabelChromXyData, out slope, out intercept, out rSquaredVal);
            correlationData.AddCorrelationData(slope, intercept, rSquaredVal);

            GetElutionCorrelationData(o18SingleLabelChromXyData, o18DoubleLabelChromXyData, out slope, out intercept, out rSquaredVal);
            correlationData.AddCorrelationData(slope, intercept, rSquaredVal);

            return correlationData;
        }
    }
}
