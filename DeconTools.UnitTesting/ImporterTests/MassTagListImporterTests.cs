using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Data.Importers;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting.ImporterTests
{
    [TestFixture]
    public class MassTagListImporterTests
    {
        string mt_sourceFile1 = "..\\..\\TestFiles\\QC_Shew_08_03_ADH-pt5_d_9Dec08_Falcon_08-10-24_SMART_Probs.csv";

        [Test]
        public void test1()
        {
            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(mt_sourceFile1,',');
            mtidImporter.Import(massTagColl);

            Assert.AreEqual(8353, massTagColl.MassTagIDList.Count);
        }


    }
}
