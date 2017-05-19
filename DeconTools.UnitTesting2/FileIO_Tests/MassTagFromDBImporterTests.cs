using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class MassTagFromDBImporterTests
    {
        List<long> _targetIDList = new List<long>{ 
                                       339661, 1880720, 127913, 1100499, 1239111, 994489, 417866, 106915, 1149424, 2763428,
                                       2763428, 2763428, 239704, 44696, 213135, 971852, 24917, 101068, 243782, 24826,
                                       194781, 194781, 1709835, 614192, 614192, 25982, 313378, 232945, 2193778, 323142,
                                       1844543, 3176757, 3176757, 56475, 311742, 1116349, 987418, 27168, 306160, 1220666,
                                       189684639};

     
        [Test]
        public void Get40MassTagsTest()
        {




            var importer = new MassTagFromSqlDbImporter("MT_Shewanella_ProdTest_P352", "porky", _targetIDList);
            var targetCollection = importer.Import();

            var testPeptide1 = (PeptideTarget)targetCollection.TargetList[2];

            Assert.AreEqual(72, targetCollection.TargetList.Count);
            Assert.AreEqual(24917, testPeptide1.ID);
            Assert.AreEqual("TQLKEFIDAQI", testPeptide1.Code);
            Assert.AreEqual(1304.6975526m, (decimal)testPeptide1.MonoIsotopicMass);
            Assert.AreEqual(2, testPeptide1.ChargeState);
            Assert.AreEqual(3192, testPeptide1.ObsCount);
            Assert.AreEqual(653.35605279, (decimal)testPeptide1.MZ);
            Assert.AreEqual(0.3989965, (decimal)testPeptide1.NormalizedElutionTime);
            Assert.AreEqual(359, testPeptide1.RefID);
            Assert.AreEqual("thioredoxin 1 (TrxA)", testPeptide1.ProteinDescription);
        }


        [Test]
        public void checkModImport_Test1()
        {
            var importer = new MassTagFromSqlDbImporter("MT_Shewanella_ProdTest_P352", "porky", _targetIDList);
            var targetCollection = importer.Import();

            var testPeptide1 = (PeptideTarget)targetCollection.TargetList[70];

            Assert.AreEqual(72, targetCollection.TargetList.Count);
            Assert.AreEqual(189684639, testPeptide1.ID);
            Assert.AreEqual("QALYEAENVLRDEQIALQK", testPeptide1.Code);
            Assert.AreEqual(2213.1326995m, (decimal)testPeptide1.MonoIsotopicMass);

            Assert.AreEqual(1, testPeptide1.ModCount);
            Assert.AreEqual("NH3_Loss:1", testPeptide1.ModDescription);
            Assert.AreEqual(true, testPeptide1.ContainsMods);
            Assert.AreEqual(696, testPeptide1.ObsCount);


            var peptideUtils = new PeptideUtils();
            Assert.AreEqual("C97H159N27O33", peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide1.Code));


            Assert.AreNotEqual("C97H159N27O33", testPeptide1.EmpiricalFormula);
            Assert.AreEqual("C97H156N26O33", testPeptide1.EmpiricalFormula);



        }


    }
}
