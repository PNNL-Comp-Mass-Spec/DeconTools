using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Data.Importers;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting.ImporterTests
{
    [TestFixture]
    public class MassTagFromDBImporterTests
    {
        string mt_sourceFile1 = "..\\..\\TestFiles\\QC_Shew_08_03_ADH-pt5_d_9Dec08_Falcon_08-10-24_SMART_Probs.csv";

        string massTagstring = "24708 27430 126187 24854 44655 56525 42382 69542 24733 31353 191843 72912 76061 24827 24977 40913 27992 191844 132113 625775 52649 24752 98196 104749 24879 191799 191810 110029 112118 105741 42132 112381 24864 29438 106917 24839 31341 27418 191214 126146";
        List<long> testMassTagIDs1;

        public void setUpTests()
        {
            string[] massTagArr = massTagstring.Split(new char[] { ' ' });
            testMassTagIDs1 = new List<long>();

            foreach (string s in massTagArr)
            {
                testMassTagIDs1.Add(Convert.ToInt64(s));
            }


        }


        [Test]
        public void get40MassTagsTest()
        {
            MassTagCollection mtCollection = new MassTagCollection();
           
            setUpTests();
            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(testMassTagIDs1);
            importer.Import(mtCollection);

            Assert.AreEqual(40, mtCollection.MassTagList.Count);
            Assert.AreEqual(24733, mtCollection.MassTagList[1].ID);
            Assert.AreEqual("ELLSEYDFPGDDLPVIQGSALK", mtCollection.MassTagList[1].PeptideSequence);
            Assert.AreEqual(2405.2001085m, (decimal)mtCollection.MassTagList[1].MonoIsotopicMass);
        }

        [Test]
        public void get8353MassTagsTest()
        {
            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(mt_sourceFile1, ',');
            mtidImporter.Import(massTagColl);

            Assert.AreEqual(8353, massTagColl.MassTagIDList.Count);
            Assert.AreEqual(5282, massTagColl.MassTagIDList.Distinct().Count());


            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(5282, massTagColl.MassTagList.Count);

            massTagColl.Display();

        }




    }
}
