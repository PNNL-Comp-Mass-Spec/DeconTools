using System;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class LcmsTargetFromFeatureTextImporterTests
    {
        [Test]
        public void ImportLcmsFeatureTargetsTest1()
        {
            var testLcmsFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\LCMSFeatureFinder\Standard_tests\QCShew\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_LCMSFeatures.txt";

            var importer = new LcmsTargetFromFeaturesFileImporter(testLcmsFile);
            var targetCollection = importer.Import();

            var testFeature = (from n in targetCollection.TargetList where n.ID == 8642 select (LcmsFeatureTarget)n).First();
            Assert.AreEqual(1855.927991m, (decimal)testFeature.MonoIsotopicMass);
            Assert.AreEqual(3, testFeature.ChargeState);
            Assert.AreEqual(2, testFeature.ChargeStateTargets.Count);

            Assert.AreEqual(330348, testFeature.FeatureToMassTagID);
            Assert.AreEqual(6054, testFeature.ScanLCTarget);

            Console.WriteLine(testFeature);
        }
    }
}
