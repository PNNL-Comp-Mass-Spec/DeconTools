using System;
using System.Collections.Generic;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class MassTagFromDBImporterTests
    {
        private const string MTDB = "MT_Shewanella_ProdTest_Formic_P966";
        private const string MTS_SERVER = "Roadrunner";

        readonly List<long> _targetIDList = new List<long> {
            339661, 1880720, 127913, 1100499, 1239111, 994489, 417866, 106915, 1149424, 2763428,
            2763428, 2763428, 239704, 44696, 213135, 971852, 24917, 101068, 243782, 24826,
            194781, 194781, 1709835, 614192, 614192, 25982, 313378, 232945, 2193778, 323142,
            1844543, 3176757, 3176757, 56475, 311742, 1116349, 987418, 27168, 306160, 1220666,
            171907699};

        [Test]
        public void GetMassTagsTest()
        {
            var importer = new MassTagFromSqlDbImporter(MTDB, MTS_SERVER, _targetIDList)
            {
                ChargeStateRangeBasedOnDatabase = true
            };

            var targetCollection = importer.Import();

            var testPeptide = FindPeptide(targetCollection, 24917);

            Assert.LessOrEqual(68, targetCollection.TargetList.Count);
            Assert.AreEqual(24917, testPeptide.ID);
            Assert.AreEqual("TQLKEFIDAQI", testPeptide.Code);
            Assert.AreEqual(1304.6975526, testPeptide.MonoIsotopicMass, 0.00001);
            Assert.AreEqual(2, testPeptide.ChargeState);
            Assert.LessOrEqual(21, testPeptide.ObsCount);

            if (testPeptide.ChargeState == 1)
                Assert.AreEqual(1305.70482909, testPeptide.MZ, 0.00001);
            else
                Assert.AreEqual(653.35605279, testPeptide.MZ, 0.00001);

            Assert.AreEqual(0.3919196426, testPeptide.NormalizedElutionTime, 0.00001);
            Assert.AreEqual(371, testPeptide.RefID);
            Assert.AreEqual("thioredoxin 1, TrxA", testPeptide.ProteinDescription);
        }

        [Test]
        public void checkModImport_Test1()
        {
            var importer = new MassTagFromSqlDbImporter(MTDB, MTS_SERVER, _targetIDList)
            {
                ChargeStateRangeBasedOnDatabase = true
            };

            var targetCollection = importer.Import();

            var testPeptide = FindPeptide(targetCollection, 171907699);

            Assert.LessOrEqual(68, targetCollection.TargetList.Count);
            Assert.AreEqual(171907699, testPeptide.ID);

            Assert.AreEqual("VYMQPASQGTGIIAGGAMR", testPeptide.Code);
            Assert.AreEqual(1938.9290428, testPeptide.MonoIsotopicMass, 0.00001);

            Assert.AreEqual(2, testPeptide.ModCount);
            Assert.AreEqual("Plus1Oxy:3,Plus1Oxy:18", testPeptide.ModDescription);
            Assert.AreEqual(true, testPeptide.ContainsMods);
            Assert.LessOrEqual(59, testPeptide.ObsCount);

            var peptideUtils = new PeptideUtils();

            // This empirical formula has 25 oxygens since it ignores the two Plus1Oxy mods
            Assert.AreEqual("C81H134N24O25S2", peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide.Code));

            Assert.AreNotEqual("C97H159N27O33", testPeptide.EmpiricalFormula);

            // This empirical formula has 27 oxygens since it includes the two Plus1Oxy mods
            Assert.AreEqual("C81H134N24O27S2", testPeptide.EmpiricalFormula);
        }

        private PeptideTarget FindPeptide(TargetCollection targetCollection, int massTagID)
        {
            foreach (var peptide in targetCollection.TargetList)
            {
                if (peptide.ID == massTagID)
                {
                    if (peptide is PeptideTarget massTag)
                        return massTag;

                    Assert.Fail("Peptide found is not a PeptideTarget instance: " + peptide);
                }
            }

            Assert.Fail("Peptide with ID {0} not found", massTagID);
            return null;
        }
    }
}
