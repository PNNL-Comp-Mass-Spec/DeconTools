using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class ScanSetCollectionCreatorTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        public string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000.uimf";


        [Test]
        public void basicCreatorTest1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 3, 1);
            creator.Create();

            Assert.AreEqual(18505, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(new int[] { 0, 1, 2 }, run.ScanSetCollection.ScanSetList[0].IndexValues.ToArray());
            Assert.AreEqual(new int[] { 18504, 18505 }, run.ScanSetCollection.ScanSetList[18504].IndexValues.ToArray());
        }

        [Test]
        public void basicCreatorTestWithAlternateConstructor()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 1000, 2000, 3, 1);
            creator.Create();

            Assert.AreEqual(1001, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(new int[] { 999, 1000, 1001 }, run.ScanSetCollection.ScanSetList[0].IndexValues.ToArray());
        }

        [Test]
        public void incrementTest1()
        {

            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 1000, 2000, 3, 3);
            creator.Create();

            Assert.AreEqual(334, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(new int[] { 999, 1000, 1001 }, run.ScanSetCollection.ScanSetList[0].IndexValues.ToArray());
            Assert.AreEqual(new int[] { 1002, 1003, 1004 }, run.ScanSetCollection.ScanSetList[1].IndexValues.ToArray());

        }


        [Test]
        public void uimfTest1()       //ScanSets are created based on the number of scans per frame
        {
            Run run = new UIMFRun(uimfFilepath);

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 3, 1);
            creator.Create();

            Assert.AreEqual(600, run.ScanSetCollection.ScanSetList.Count);
        }

        [ExpectedException(ExpectedMessage = "Either the Start Scan or Stop Scan value exceeds the maximum possible value")]
        [Test]
        public void uimfTest2()       //ScanSets are created based on the number of scans per frame
        {
            Run run = new UIMFRun(uimfFilepath);

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 700, 800, 3, 1);
            creator.Create();
        }

    }
}
