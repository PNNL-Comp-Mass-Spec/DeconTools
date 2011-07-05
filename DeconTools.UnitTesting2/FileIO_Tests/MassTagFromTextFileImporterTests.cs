using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data.Importers;
using DeconTools.Backend.FileIO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class MassTagFromTextFileImporterTests
    {

        string massTagTestFile1 = "..\\..\\..\\TestFiles\\FileIOTests\\top40MassTags.txt";
        string massTagTestFile2 = "..\\..\\..\\TestFiles\\FileIOTests\\importedMassTagsFormat2.txt";


        [Test]
        public void test1()
        {
            MassTagCollection mtc = new MassTagCollection();

            MassTagFromTextFileImporter massTagImporter = new MassTagFromTextFileImporter(massTagTestFile1);
            mtc = massTagImporter.Import();

            Assert.AreNotEqual(null, mtc.MassTagList);
            Assert.AreEqual(101, mtc.MassTagList.Count);

            MassTag testMassTag = mtc.MassTagList[0];


            Assert.AreEqual("AVAFGEALRPEFK", testMassTag.PeptideSequence);
            Assert.AreEqual(2, testMassTag.ChargeState);
            Assert.AreEqual(0.3649905m, (decimal)testMassTag.NETVal);
            Assert.AreEqual("C67H103N17O18S0", testMassTag.Peptide.GetEmpiricalFormula());
            Assert.AreEqual(872, testMassTag.RefID);
            Assert.AreEqual("ABA80002 SHMT serine hydroxymethyltransferase", testMassTag.ProteinDescription);
            Assert.AreEqual(1433.76662942m, (decimal)testMassTag.MonoIsotopicMass);
            Assert.AreEqual(717.8905912m, (decimal)testMassTag.MZ);
            Assert.AreEqual(75, testMassTag.ObsCount);
            Assert.AreEqual(4225609, testMassTag.ID);

            
        }


        [Test]
        public void importFromSQLManagmentStyleTextFile_test1()
        {
            MassTagCollection mtc = new MassTagCollection();

            MassTagFromTextFileImporter massTagImporter = new MassTagFromTextFileImporter(massTagTestFile2);
            mtc = massTagImporter.Import();

            Assert.AreNotEqual(null, mtc.MassTagList);
            Assert.AreEqual(13, mtc.MassTagList.Count);

            MassTag testMassTag = mtc.MassTagList[0];


            Assert.AreEqual("AVTTADQVQQEVER", testMassTag.PeptideSequence);
            Assert.AreEqual(0, testMassTag.ChargeState);
            Assert.AreEqual(0.2365603m, (decimal)testMassTag.NETVal);
            Assert.AreEqual("C64H108N20O26S0", testMassTag.Peptide.GetEmpiricalFormula());
            Assert.AreEqual(137, testMassTag.RefID);
            Assert.AreEqual(1572.774283m, (decimal)testMassTag.MonoIsotopicMass);
            Assert.AreEqual(0, (decimal)testMassTag.MZ);
            Assert.AreEqual(6, testMassTag.ObsCount);
            Assert.AreEqual(354885422, testMassTag.ID);
        }

    }
}
