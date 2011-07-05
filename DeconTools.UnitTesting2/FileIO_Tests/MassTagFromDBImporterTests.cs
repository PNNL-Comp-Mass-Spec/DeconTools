using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Data.Importers;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class MassTagFromDBImporterTests
    {
        string massTagstring = "339661, 1880720, 127913, 1100499, 1239111, 994489, 417866, 106915, 1149424, 2763428, 2763428, 2763428, 239704, 44696, 213135, 971852, 24917, 101068, 243782, 24826, 194781, 194781, 1709835, 614192, 614192, 25982, 313378, 232945, 2193778, 323142, 1844543, 3176757, 3176757, 56475, 311742, 1116349, 987418, 27168, 306160, 1220666";
        MassTagCollection massTagColl = new MassTagCollection();


        public void setUpTests()
        {
            string[] massTagArr = massTagstring.Split(new char[] { ',' });

            foreach (string s in massTagArr)
            {
                massTagColl.MassTagIDList.Add(Convert.ToInt64(s));
            }


        }


        [Test]
        public void get40MassTagsTest()
        {

            setUpTests();
            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky",massTagColl.MassTagIDList);
            massTagColl =importer.Import();

            Assert.AreEqual(43, massTagColl.MassTagList.Count);
            Assert.AreEqual(24917, massTagColl.MassTagList[1].ID);
            Assert.AreEqual("TQLKEFIDAQI", massTagColl.MassTagList[1].PeptideSequence);
            Assert.AreEqual(1304.6975526m, (decimal)massTagColl.MassTagList[1].MonoIsotopicMass);
            Assert.AreEqual(2, massTagColl.MassTagList[1].ChargeState);
            Assert.AreEqual(3192, massTagColl.MassTagList[1].ObsCount);
            Assert.AreEqual(653.35605279, (decimal)massTagColl.MassTagList[1].MZ);
            Assert.AreEqual(0.3989965, (decimal)massTagColl.MassTagList[1].NETVal);
            Assert.AreEqual(359, massTagColl.MassTagList[1].RefID);
            Assert.AreEqual("thioredoxin 1 (TrxA)", massTagColl.MassTagList[1].ProteinDescription);
        }


        [Test]
        public void test1()
        {
        }


    }
}
