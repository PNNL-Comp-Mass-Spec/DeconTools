using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class MassTagFromTextFileImporterTests
    {

        string massTagTestFile1 = @"..\..\..\TestFiles\FileIOTests\top40MassTags.txt";
        string massTagTestFile2 = @"..\..\..\TestFiles\FileIOTests\importedMassTagsFormat2.txt";
        private string massTagsWithModsFile1 = @"..\..\..\TestFiles\FileIOTests\massTagsWithModsSample.txt";

        [Test]
        public void test1()
        {
            var mtc = new TargetCollection();

            var massTagImporter = new MassTagFromTextFileImporter(massTagTestFile1);
            mtc = massTagImporter.Import();

            Assert.AreNotEqual(null, mtc.TargetList);
            Assert.AreEqual(101, mtc.TargetList.Count);

            var testMassTag = (PeptideTarget)mtc.TargetList[0];


            Assert.AreEqual("AVAFGEALRPEFK", testMassTag.Code);
            Assert.AreEqual(2, testMassTag.ChargeState);
            Assert.AreEqual(0.3649905m, (decimal)testMassTag.NormalizedElutionTime);
            Assert.AreEqual("C67H103N17O18", testMassTag.EmpiricalFormula);
            Assert.AreEqual(872, testMassTag.RefID);
            Assert.AreEqual("ABA80002 SHMT serine hydroxymethyltransferase", testMassTag.ProteinDescription);
            Assert.AreEqual(1433.766629m, (decimal)testMassTag.MonoIsotopicMass);
            Assert.AreEqual(717.8905912m, (decimal)testMassTag.MZ);
            Assert.AreEqual(75, testMassTag.ObsCount);
            Assert.AreEqual(4225609, testMassTag.ID);


        }

        [Test]
        public void ImportPeptidesWithModsTest1()
        {
            var mtc = new TargetCollection();

            var massTagImporter = new MassTagFromTextFileImporter(massTagsWithModsFile1);
            mtc = massTagImporter.Import();

            Assert.AreNotEqual(null, mtc.TargetList);
            Assert.AreEqual(1868, mtc.TargetList.Count);

            var testMassTag = (PeptideTarget)mtc.TargetList[1021];
            Assert.AreEqual(testMassTag.EmpiricalFormula, "C56H82N10O13");
            Assert.AreEqual(1, testMassTag.ModCount);
            Assert.AreEqual(testMassTag.ModDescription, "NH3_Loss:1");


            //250663994	1102.60623	QFPILLDFK	2	C56H82N10O13	1	NH3_Loss:1

        }


        [Test]
        public void ImportPeptidesContainingOnlySequenceInfo()
        {
            var testfile = @"..\\..\\..\\TestFiles\\FileIOTests\\BSAmassTags_MinimalInfo1.txt";

            var mtc = new TargetCollection();

            var massTagImporter = new MassTagFromTextFileImporter(testfile);
            mtc = massTagImporter.Import();

            Assert.AreNotEqual(null, mtc.TargetList);
            foreach (PeptideTarget peptideTarget in mtc.TargetList)
            {
                Console.WriteLine(peptideTarget);
            }
            Assert.AreEqual(121, mtc.TargetList.Count);

            var testMassTag = (PeptideTarget)mtc.TargetList[0];
            Assert.AreEqual("LFTFHADICTLPDTEK", testMassTag.Code);
            Assert.AreEqual("C84H127N19O26S", testMassTag.EmpiricalFormula);

            Assert.IsTrue(testMassTag.MonoIsotopicMass > 0);
        }

        [Test]
        public void ImportTargetsContainingOnlyEmpiricalFormula()
        {
            var testfile = @"..\\..\\..\\TestFiles\\FileIOTests\\QCShew_Bin10_Top10_empiricalFormula_NET_only.txt";

            var mtc = new TargetCollection();

            var massTagImporter = new MassTagFromTextFileImporter(testfile);
            mtc = massTagImporter.Import();

            Assert.AreNotEqual(null, mtc.TargetList);
            foreach (PeptideTarget peptideTarget in mtc.TargetList)
            {
                Console.WriteLine(peptideTarget);
            }

        }

        [Test]
        public void ImportTargetsContainingEmpiricalFormulaAndScanNumber()
        {

            var testfile = @"..\\..\\..\\TestFiles\\FileIOTests\\BSAmassTags_EmpiricalFormula_and_scans.txt";
            var mtc = new TargetCollection();

            var massTagImporter = new MassTagFromTextFileImporter(testfile);
            mtc = massTagImporter.Import();

            Assert.AreNotEqual(null, mtc.TargetList);
            foreach (PeptideTarget peptideTarget in mtc.TargetList)
            {
                Console.WriteLine(peptideTarget);
            }



        }



        /// <summary>
        /// The header for this type of text file is:
        /// mass_tag_id	mass	Avg_GANET	Peptide	obs	pmtQ	StD_GANET	Ref_ID	Description
        /// So, only Monomass is given. Importer will assign charge states and calc MZ
        /// </summary>
        [Test]
        public void importFromSQLManagmentStyleTextFile_test1()
        {
            var mtc = new TargetCollection();

            var massTagImporter = new MassTagFromTextFileImporter(massTagTestFile2);
            mtc = massTagImporter.Import();

            Assert.AreNotEqual(null, mtc.TargetList);
            Assert.AreEqual(37, mtc.TargetList.Count);

            var testMassTag = mtc.TargetList[0] as PeptideTarget;


            Assert.AreEqual("AVTTADQVQQEVER", testMassTag.Code);
            Assert.AreEqual(2, testMassTag.ChargeState);
            Assert.AreEqual(0.2365603m, (decimal)testMassTag.NormalizedElutionTime);
            Assert.AreEqual("C64H108N20O26", testMassTag.EmpiricalFormula);
            Assert.AreEqual(137, testMassTag.RefID);
            Assert.AreEqual(1572.774283m, (decimal)testMassTag.MonoIsotopicMass);
            Assert.AreEqual(787.39441799m, (decimal)testMassTag.MZ);
            Assert.AreEqual(6, testMassTag.ObsCount);
            Assert.AreEqual(354885422, testMassTag.ID);
        }





    }
}
