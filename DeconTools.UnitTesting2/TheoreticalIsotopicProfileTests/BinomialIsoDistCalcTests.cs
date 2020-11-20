using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.TheoreticalIsotopicProfileTests
{
    [TestFixture]
    public class BinomialIsoDistCalcTests
    {
        [Test]
        public void Test1()
        {
            var calc = new BionomialExpansionIsotopicProfileCalculator();

            calc.LoadElementData();
        }
    }
}
