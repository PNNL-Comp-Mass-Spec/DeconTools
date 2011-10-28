using System;
using System.IO;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.FileIOTests.TargetedResultFileIOTests
{
    [TestFixture]
    public class N14N15TargetedResultFileIOTests
    {
        [Test]
        public void ImporterTest1()
        {
            string importedResultFile = FileRefs.ImportedData + Path.DirectorySeparatorChar + "N14N15TargetedResultsImportedData1.txt";

            var importer = new N14N15TargetedResultFromTextImporter(importedResultFile);
            TargetedResultRepository repo = importer.Import();

            Assert.IsNotNull(repo);
            Assert.IsTrue(repo.Results.Count > 0);

            var testResult1 = (N14N15TargetedResultDTO)repo.Results[26];
            Console.WriteLine(repo.Results.Count);

            Assert.AreEqual("RSPH_PtoA_NL_14_rep3_8Jan08_Raptor_07-11-01", testResult1.DatasetName);
            Assert.AreEqual(27926, testResult1.TargetID);
            Assert.AreEqual(1, testResult1.ChargeState);
            Assert.AreEqual(287, testResult1.ScanLC);
            Assert.AreEqual(283, testResult1.ScanLCStart);
            Assert.AreEqual(292, testResult1.ScanLCEnd);
            Assert.AreEqual(286, testResult1.ScanN15);
            Assert.AreEqual(282, testResult1.ScanN15Start);
            Assert.AreEqual(291, testResult1.ScanN15End);
            Assert.AreEqual(0.1390, (decimal)testResult1.NET);
            Assert.AreEqual(0.1388, (decimal)testResult1.NETN15);
            Assert.AreEqual(1, testResult1.NumChromPeaksWithinTol);
            Assert.AreEqual(1, testResult1.NumChromPeaksWithinTolN15);
            Assert.AreEqual(808.43894, (decimal)testResult1.MonoMass);
            Assert.AreEqual(816.41637, (decimal)testResult1.MonoMassN15);

            Assert.AreEqual(2185831, (decimal)testResult1.Intensity);
            Assert.AreEqual(956636, (decimal)testResult1.IntensityN15);
            Assert.AreEqual(0.062, (decimal)testResult1.FitScore);
            Assert.AreEqual(0.102, (decimal)testResult1.FitScoreN15);
            Assert.AreEqual(1.103, (decimal)testResult1.RatioContributionN14);
            Assert.AreEqual(1.210, (decimal)testResult1.RatioContributionN15);
            Assert.AreEqual(0.4255805m, (decimal)testResult1.Ratio);

            //Dataset	GrowthCondition	GrowthTime	BiolRepNum	MTID	Z	Scan	ScanStart	ScanEnd	ScanN15	ScanN15Start	ScanN15End	NET	NETN15	ChromPeaksWithinTol	ChromPeaksWithinTolN15	MonoMassIso1	MonoMassIso2	MZ1	MZ2	AbundanceIso1	AbundanceIso2	Iso1Fit	Iso2Fit	IScore1	IScore2	Iso1RatioContrib	Iso2RatioContrib	Ratio	NumPeaksInCalc
            //RSPH_PtoA_NL_14_rep3_8Jan08_Raptor_07-11-01	PToA_NL	180	2	27926	1	287	283	292	286	282	291	0.1390	0.1388	1	1	808.43894	816.41637	809.44622	817.42364	2185831	956636	0.062	0.102	0.000	0.000	1.103	1.210	0.425580533942721	0
        }

    }
}
