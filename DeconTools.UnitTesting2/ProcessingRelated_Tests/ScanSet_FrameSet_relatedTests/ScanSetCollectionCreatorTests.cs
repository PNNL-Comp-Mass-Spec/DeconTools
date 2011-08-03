using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ScanSet_FrameSet_relatedTests
{
    [TestFixture]
    public class ScanSetCollectionCreatorTests
    {
        [Test]
        public void createScanSets_processMSMS_test1()
        {
            RunFactory rf=new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            bool processMSMS = true;
            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1, processMSMS);
            sscc.Create();
        }




        [Test]
        public void createScanSets_MS1_test1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int startScan = 6000;
            int stopScan = 6020;

            bool processMSMS = false;
            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, startScan, stopScan, 1, 1, processMSMS);
            sscc.Create();

            Assert.AreEqual(3, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(6005, run.ScanSetCollection.ScanSetList[0].PrimaryScanNumber);
        }

    }
}
