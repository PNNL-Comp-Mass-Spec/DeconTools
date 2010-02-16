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
        string mt_sourceFile1 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_SMART_Probs.csv";
        string mt_sourceFile2 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_SMART_Probs_ALL.csv";


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
            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.Import(massTagColl);

            Assert.AreEqual(43, massTagColl.MassTagList.Count);
            Assert.AreEqual(24917, massTagColl.MassTagList[1].ID);
            Assert.AreEqual("TQLKEFIDAQI", massTagColl.MassTagList[1].PeptideSequence);
            Assert.AreEqual(1304.6975526m, (decimal)massTagColl.MassTagList[1].MonoIsotopicMass);
            Assert.AreEqual(2, massTagColl.MassTagList[1].ChargeState);
            Assert.AreEqual(3192, massTagColl.MassTagList[1].ObsCount);
            Assert.AreEqual(653.35605279, (decimal)massTagColl.MassTagList[1].MZ);
            Assert.AreEqual(0.3989965, (decimal)massTagColl.MassTagList[1].NETVal);
            Assert.AreEqual(359, massTagColl.MassTagList[1].RefID);
        }

        [Test]
        public void import100MassTagIDs_andLookUpInDMSTest1()
        {
            this.massTagColl = new MassTagCollection();

            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(mt_sourceFile1, ',');
            mtidImporter.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);
            Assert.AreEqual(85, massTagColl.MassTagIDList.Distinct().Count());


            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.Import(massTagColl);
            massTagColl.Display();

            Assert.AreEqual(114, massTagColl.MassTagList.Count);

        }

#if test_is_big
        [Test]
        public void get14864MassTagsTest()     // this takes about 1 minute on Gord's machine    
        {
            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(mt_sourceFile2, ',');
            mtidImporter.Import(massTagColl);

            Assert.AreEqual(21047, massTagColl.MassTagIDList.Count);
            Assert.AreEqual(14864, massTagColl.MassTagIDList.Distinct().Count());

            massTagColl.MassTagIDList = massTagColl.MassTagIDList.Distinct().ToList();

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);
            massTagColl.Display();

        Assert.AreEqual(14864, massTagColl.MassTagList.Count);
        }       
#endif
 




    }
}
