using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting.UtilitiesTests
{
    [TestFixture]
    public class PeakUtilitiesTests
    {
        [Test]
        public void FindPeaksWithinToleranceTests1()
        {
            List<IPeak> peakList = TestUtilities.GeneratePeakList(new ScanSet(6005));

            List<IPeak> filteredList = PeakUtilities.GetPeaksWithinTolerance(peakList, 700, 10);

            Assert.AreEqual(809, peakList.Count);

            TestUtilities.DisplayPeaks(filteredList);

            Assert.AreEqual(42, filteredList.Count);
        }


        [Test]
        public void FindPeaksWithinToleranceLowMZRange()
        {
            List<IPeak> peakList = TestUtilities.GeneratePeakList(new ScanSet(6005));

            List<IPeak> filteredList = PeakUtilities.GetPeaksWithinTolerance(peakList, 400, 100);

            Assert.AreEqual(809, peakList.Count);

            TestUtilities.DisplayPeaks(filteredList);

            Assert.AreEqual(115, filteredList.Count);
        }


        [Test]
        public void FindPeaksWithinTolerance_highMZRangeTest()
        {
            List<IPeak> peakList = TestUtilities.GeneratePeakList(new ScanSet(6005));

            List<IPeak> filteredList = PeakUtilities.GetPeaksWithinTolerance(peakList, 1500, 100);

            Assert.AreEqual(809, peakList.Count);

            TestUtilities.DisplayPeaks(filteredList);

            Assert.AreEqual(115, filteredList.Count);
        }



    }
}
