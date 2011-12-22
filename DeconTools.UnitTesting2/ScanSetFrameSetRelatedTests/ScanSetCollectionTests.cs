using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ScanSetFrameSetRelatedTests
{
    [TestFixture]
    public class ScanSetCollectionTests
    {
        [Test]
        public void createScanSets_processMSMS_test1()
        {
            RunFactory rf=new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int startScan = 6000;
            int stopScan = 6020;

            bool processMSMS = true;

            run.ScanSetCollection = ScanSetCollection.Create(run, startScan, stopScan, 1, 1, processMSMS);

            Assert.AreEqual(21, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(6000, run.ScanSetCollection.ScanSetList[0].PrimaryScanNumber);

        }

        [Test]
        public void createScanSets_MS1_test1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int startScan = 6000;
            int stopScan = 6020;

            bool processMSMS = false;

            run.ScanSetCollection = ScanSetCollection.Create(run, startScan, stopScan, 1, 1, processMSMS);
            
            Assert.AreEqual(3, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(6005, run.ScanSetCollection.ScanSetList[0].PrimaryScanNumber);
        }


        [Test]
        public void createScanSets_summed_MS1_test1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int startScan = 6000;
            int stopScan = 6020;

            int numSummed = 5;

            bool processMSMS = false;
            run.ScanSetCollection = ScanSetCollection.Create(run, startScan, stopScan,numSummed, 1, processMSMS);

            Assert.AreEqual(3, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(6005, run.ScanSetCollection.ScanSetList[0].PrimaryScanNumber);
            Assert.AreEqual(5, run.ScanSetCollection.ScanSetList[0].IndexValues.Count);
            Assert.AreEqual(5991, run.ScanSetCollection.ScanSetList[0].IndexValues[0]);
            Assert.AreEqual(6019, run.ScanSetCollection.ScanSetList[0].IndexValues[4]);

            //Console.WriteLine(run.ScanSetCollection.ScanSetList[0]);
        }


        [Test]
        public void createIMSScanSets1()
        {
            RunFactory factory=new RunFactory();
            Run run = factory.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            int startScan = 120;
            int stopScan = 200;

            int numScansSummed = 7;
            run.ScanSetCollection = ScanSetCollection.Create(run, startScan, stopScan, numScansSummed, 1);

            Assert.IsNotNull(run.ScanSetCollection);
            Assert.AreEqual(81, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(7, run.ScanSetCollection.ScanSetList[0].IndexValues.Count);
            Assert.AreEqual(117,run.ScanSetCollection.ScanSetList[0].IndexValues[0]);
            Assert.AreEqual(123, run.ScanSetCollection.ScanSetList[0].IndexValues[6]);

        }


        [Test]
        public void createIMSScanSets2()
        {
            RunFactory factory = new RunFactory();
            Run run = factory.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

           
            int numScansSummed = 7;
            run.ScanSetCollection = ScanSetCollection.Create(run, numScansSummed, 1);

           
            Assert.IsNotNull(run.ScanSetCollection);

            //tests the lowest ims_scans
            Assert.AreEqual(360, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(4, run.ScanSetCollection.ScanSetList[0].IndexValues.Count);
            Assert.AreEqual(0, run.ScanSetCollection.ScanSetList[0].IndexValues[0]);
            Assert.AreEqual(3, run.ScanSetCollection.ScanSetList[0].IndexValues[3]);

        }


    }
}
