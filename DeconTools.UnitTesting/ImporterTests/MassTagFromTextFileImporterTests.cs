using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Data.Importers;
using NUnit.Framework;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting.ImporterTests
{



    public class MassTagFromTextFileImporterTests
    {
        string massTagTestFile1 = "..\\..\\TestFiles\\samplePeptides.txt";

        [Test]
        public void test1()
        {
            MassTagCollection mtc = new MassTagCollection();

            MassTagFromTextFileImporter massTagImporter = new MassTagFromTextFileImporter(massTagTestFile1);
            massTagImporter.Import(mtc);
            Assert.AreEqual(3, mtc.MassTagList.Count);

            Assert.AreEqual("SAMPLER", mtc.MassTagList[0].PeptideSequence);
            Assert.AreEqual(2, mtc.MassTagList[0].ChargeState);
            Assert.AreEqual(0.34m, (decimal)mtc.MassTagList[0].NETVal);
            Assert.AreEqual("C33H58N10O11S1", mtc.MassTagList[0].Peptide.GetEmpiricalFormula());
        }
    }
}
