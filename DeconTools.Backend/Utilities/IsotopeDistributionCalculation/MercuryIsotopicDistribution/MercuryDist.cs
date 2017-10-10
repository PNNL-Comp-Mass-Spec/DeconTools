using DeconTools.Backend.Core;

namespace DeconTools.Backend.Utilities.MercuryIsotopeDistribution
{
    public class MercuryDist
    {
        public IsotopicProfile IsotopicProfile { get; set; }

        public XYData Xydata { get; set; }

        public MercuryDist()
        {
            IsotopicProfile = new IsotopicProfile();
            Xydata = new XYData();

        }
    }
}
