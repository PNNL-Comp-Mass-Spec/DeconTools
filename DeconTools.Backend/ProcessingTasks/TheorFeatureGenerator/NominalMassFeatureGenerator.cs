using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator
{
    public class NominalMassFeatureGenerator : ITheorFeatureGenerator
    {
        private static readonly IsotopicDistributionCalculator IsotopicDistCalculator = IsotopicDistributionCalculator.Instance;
        private const double LowPeakCutOff = 0.005;

        public override void LoadRunRelatedInfo(ResultCollection results)
        {
            // do nothing
        }

        public override void GenerateTheorFeature(TargetBase mt)
        {
            Check.Require(mt != null, "FeatureGenerator failed. Target must not be null.");
            IsotopicProfile iso = IsotopicDistCalculator.GetAveraginePattern(mt.MZ);
            PeakUtilities.TrimIsotopicProfile(iso, LowPeakCutOff);
            iso.ChargeState = mt.ChargeState;
            mt.IsotopicProfile = iso;
        }
    }
}
