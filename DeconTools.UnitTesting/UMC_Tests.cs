using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Data;
using DeconTools.Backend.Core;
using System.Diagnostics;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class UMC_Tests
    {

        string umcTestfile1 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_UMCs.txt";
        string umcO16O18TestFile1 = "..\\..\\TestFiles\\TechTest_O18_02_RunA_10Dec09_Doc_09-11-08_UMCs.txt";

        [Test]
        public void importUMCsTest1()
        {
            UMCCollection umcs = new UMCCollection();
            UMCFileImporter importer = new UMCFileImporter(umcTestfile1, '\t');
            importer.Import(umcs);

            Assert.AreEqual(15381, umcs.UMCList.Count);

        }

        [Test]
        public void umcTest2()
        {
            UMCCollection umcs = new UMCCollection();
            UMCFileImporter importer = new UMCFileImporter(umcTestfile1, '\t');
            importer.Import(umcs);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Dictionary<int, double> lookupTable = umcs.GetScanNETLookupTable();

            sw.Stop();

            double netVal = lookupTable[6005];
            netVal = lookupTable[6004];
            Console.Write(sw.ElapsedMilliseconds);
            Assert.AreEqual(0.3216, netVal);

        }


        [Test]
        public void importUMCsWithO16O18PairedDataTest1()
        {
            UMCCollection umcs = new UMCCollection();
            UMCFileImporter importer = new UMCFileImporter(umcO16O18TestFile1, '\t');
            importer.Import(umcs);

            Assert.AreEqual(23073, umcs.UMCList.Count);




        }


        [Test]
        public void filterUMCSByMassTagsTest1()
        {
            UMCCollection umcs = new UMCCollection();
            UMCFileImporter importer = new UMCFileImporter(umcO16O18TestFile1, '\t');
            importer.Import(umcs);


            UMCCollection filteredUMCs = new UMCCollection();
            filteredUMCs.UMCList = umcs.FilterUMCsByMassTagMatch(new List<int> { 22807265, 22580887, 20791942, 20791939, 20750857, 20908613, 20842966, 22598396, 174124103 });

            Assert.AreEqual(8, filteredUMCs.UMCList.Count);

            filteredUMCs.DisplayUMCExpressionInfo();
        }



        [Test]
        public void filterUMCsByMassTagsTest2()
        {
            UMCCollection umcs = new UMCCollection();
            UMCFileImporter importer = new UMCFileImporter(umcO16O18TestFile1, '\t');
            importer.Import(umcs);


            UMCCollection filteredUMCs = new UMCCollection();
            filteredUMCs.UMCList = umcs.FilterUMCsByMassTagMatch(new List<int> { 22807265, 22580887, 20791942, 20791939, 20750857, 20908613, 20842966, 22598396, 174124103 });

            Assert.AreEqual(8, filteredUMCs.UMCList.Count);

            filteredUMCs.DisplayUMCExpressionInfo();

        }



        [Test]
        public void filterOutPairedUMCsTest1()
        {
            UMCCollection umcs = new UMCCollection();
            UMCFileImporter importer = new UMCFileImporter(umcO16O18TestFile1, '\t');
            importer.Import(umcs);


            UMCCollection filteredUMCs = new UMCCollection();
            filteredUMCs.UMCList = umcs.FilterOutPairedData();




            Assert.AreEqual(1734, filteredUMCs.UMCList.Count);

            


            filteredUMCs.DisplayUMCExpressionInfo();

        }



       
    }
}
