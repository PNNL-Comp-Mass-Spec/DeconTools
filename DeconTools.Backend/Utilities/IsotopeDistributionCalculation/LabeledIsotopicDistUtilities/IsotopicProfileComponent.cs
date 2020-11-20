using DeconTools.Backend.Core;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation.LabeledIsotopicDistUtilities
{
    public class IsotopicProfileComponent
    {
        public IsotopicProfileComponent(IsotopicProfile iso, double fraction, double percentLabeling = 0)
        {
            IsotopicProfile = iso;
            Fraction = fraction;
            PercentLabeling = percentLabeling;
        }

        public IsotopicProfile IsotopicProfile { get; set; }

        public double Fraction { get; set; }

        public double PercentLabeling { get; set; }

        public string Description { get; set; }
    }
}
