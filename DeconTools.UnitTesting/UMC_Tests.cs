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
       
    }
}
