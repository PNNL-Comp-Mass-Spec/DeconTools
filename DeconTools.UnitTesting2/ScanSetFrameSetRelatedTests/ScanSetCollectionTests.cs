using System;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ScanSetFrameSetRelatedTests
{
    [TestFixture]
    public class ScanSetCollectionTests
    {
        [Test]
        public void createScanSets_processMSMS_test1()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var startScan = 6000;
            var stopScan = 6020;

            var processMSMS = true;

            run.ScanSetCollection.Create(run, startScan, stopScan, 1, 1, processMSMS);

            Assert.AreEqual(21, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(6000, run.ScanSetCollection.ScanSetList[0].PrimaryScanNumber);
        }

        [Test]
        public void createScanSets_MS1_test1()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var startScan = 6000;
            var stopScan = 6020;

            var processMSMS = false;

            run.ScanSetCollection.Create(run, startScan, stopScan, 1, 1, processMSMS);

            Assert.AreEqual(3, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(6005, run.ScanSetCollection.ScanSetList[0].PrimaryScanNumber);
        }

        [Test]
        public void createScanSets_summed_MS1_test1()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var startScan = 6000;
            var stopScan = 6020;

            var numSummed = 5;

            var processMSMS = false;
            run.ScanSetCollection.Create(run, startScan, stopScan, numSummed, 1, processMSMS);

            Assert.AreEqual(3, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(6005, run.ScanSetCollection.ScanSetList[0].PrimaryScanNumber);
            Assert.AreEqual(5, run.ScanSetCollection.ScanSetList[0].IndexValues.Count);
            Assert.AreEqual(5991, run.ScanSetCollection.ScanSetList[0].IndexValues[0]);
            Assert.AreEqual(6019, run.ScanSetCollection.ScanSetList[0].IndexValues[4]);

            //Console.WriteLine(run.ScanSetCollection.ScanSetList[0]);
        }

        [Test]
        public void CreateScanSetsSummedMS1UnsummedMS2Test2()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var startScan = 6000;
            var stopScan = 6020;

            var numSummed = 5;

            var processMSMS = true;
            run.ScanSetCollection.Create(run, startScan, stopScan, numSummed, 1, processMSMS);

            Assert.AreEqual(21, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(6000, run.ScanSetCollection.ScanSetList[0].PrimaryScanNumber);
            Assert.AreEqual(1, run.ScanSetCollection.ScanSetList[0].IndexValues.Count);
            Assert.AreEqual(6005, run.ScanSetCollection.ScanSetList[5].PrimaryScanNumber);
            Assert.AreEqual(5, run.ScanSetCollection.ScanSetList[5].IndexValues.Count);
        }

        [Test]
        public void CreateSingleScansetThatSumsAll()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            run.ScanSetCollection.Create(run, true, true);

            Console.WriteLine(run.ScanSetCollection.ScanSetList[0]);

            Assert.AreEqual(2695, run.ScanSetCollection.ScanSetList[0].IndexValues.Count);
        }
    }
}
