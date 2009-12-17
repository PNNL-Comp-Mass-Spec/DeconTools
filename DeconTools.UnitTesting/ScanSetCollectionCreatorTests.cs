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

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 6000, 7000, 3, 1,false);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            creator.Create();
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            Assert.AreEqual(145, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(new int[] { 5998, 6005, 6012 }, run.ScanSetCollection.ScanSetList[0].IndexValues.ToArray());
        }

        [Test]
        public void basicCreatorTestWithAlternateConstructor_MSMSIncludedTest1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 6000, 7000, 3, 1, true);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            creator.Create();
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            //Assert.AreEqual(1001, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(new int[] { 5999, 6000, 6001 }, run.ScanSetCollection.ScanSetList[0].IndexValues.ToArray());
        }



        [Test]
        public void basicCreatorTestWithAlternateConstructorTest2()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 6000, 7000, 1, 1, false);
            creator.Create();

            Assert.AreEqual(145, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(new int[] { 6005 }, run.ScanSetCollection.ScanSetList[0].IndexValues.ToArray());     //first scan is 6005, a MS-level scan
        }

        [Test]
        public void basicCreatorTestWithAlternateConstructorTest3()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 1, 19000, 1, 1, true);    //Get all MS and MS/MS scans
            creator.Create();

            Assert.AreEqual(18505, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(new int[] { 1 }, run.ScanSetCollection.ScanSetList[0].IndexValues.ToArray());
        }


        [Test]
        public void basicCreatorTestWithAlternateConstructorTest4()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 1, 19000, 1, 1, false);     //get all MS-Level scans
            creator.Create();

            Assert.AreEqual(2695, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(new int[] { 1 }, run.ScanSetCollection.ScanSetList[0].IndexValues.ToArray());
        }


        [Test]
        public void incrementTest1()
        {

            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 1000, 2000, 3, 3,false);
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

            ScanSetCollectionCreator creator = new ScanSetCollectionCreator(run, 700, 800, 3, 1,false);
            creator.Create();
        }

    }
}
