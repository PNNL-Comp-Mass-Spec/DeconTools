using System;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting2.ScanSetFrameSetRelatedTests
{
    [TestFixture]
    public class ScanSetFactory_tests
    {
        [Test]
        public void Test1()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var ssf = new ScanSetFactory();
            var scan = ssf.CreateScanSet(run, 6005, 11);

            Console.WriteLine(scan);
        }

        [Test]
        public void CreateScanSetBasedOnRangeOfScansTest1()
        {
            var startScan = 5970;
            var stopScan = 6035;

            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var ssf = new ScanSetFactory();
            var scan = ssf.CreateScanSet(run, 6005, startScan, stopScan);

            Assert.AreEqual("6005 {5970, 5977, 5984, 5991, 5998, 6005, 6012, 6019, 6026, 6033}", scan.ToString());
        }

        [Test]
        public void TrimScansTest1()
        {
            var startScan = 5970;
            var stopScan = 6035;

            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var ssf = new ScanSetFactory();
            var scan = ssf.CreateScanSet(run, 6005, startScan, stopScan);

            Assert.AreEqual("6005 {5970, 5977, 5984, 5991, 5998, 6005, 6012, 6019, 6026, 6033}", scan.ToString());

            var maxScansAllowed = 7;
            ssf.TrimScans(scan, maxScansAllowed);

            Assert.AreEqual("6005 {5984, 5991, 5998, 6005, 6012, 6019}", scan.ToString());
        }
    }
}
