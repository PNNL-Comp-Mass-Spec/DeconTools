using System.Collections.Generic;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Utilities
{
    [TestFixture]
    public class MathUtilsTests
    {
        [Test]
        public void MedianTest1()
        {
            List<double> testVals = new List<double>();

            var median=  MathUtils.GetMedian(testVals);
            Assert.AreEqual(double.NaN, median);

            testVals = new List<double> {5};

            median = MathUtils.GetMedian(testVals);

            Assert.AreEqual(5, median);
        }

    }
}
