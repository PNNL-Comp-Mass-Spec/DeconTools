using System;
using System.Linq;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ScanSetFrameSetRelatedTests
{
    [TestFixture]
    public class ScanSetCollectionForUIMF_Tests
    {
        [Test]
        public void creatorTest_sum3_test1()
        {

            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var frameStart = 1;
            var frameStop = 11;

            var numFramesToSum = 3;

            var uimfRun = (UIMFRun)run;

            uimfRun.ScanSetCollection.Create(uimfRun, frameStart, frameStop, numFramesToSum, 1);

            Assert.AreEqual(11, uimfRun.ScanSetCollection.ScanSetList.Count);


            foreach (var frameset in uimfRun.ScanSetCollection.ScanSetList)
            {
                Console.WriteLine(frameset);


            }


            var testFrame0 = uimfRun.ScanSetCollection.ScanSetList[1];
            Assert.AreEqual(3, testFrame0.IndexValues.Count);
            Assert.AreEqual(1, testFrame0.IndexValues[0]);





        }


        [Test]
        public void creatorTest_ensureOnlyMS1_test1()
        {

            //note that frame 26 is a calibration frame. So we don't want to have this if we are summing frames

            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile4);    //Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf

            var frameStart = 24;
            var frameStop = 28;

            var numFramesToSum = 3;

            var uimfRun = (UIMFRun)run;

            uimfRun.ScanSetCollection.Create(uimfRun, frameStart, frameStop, numFramesToSum, 1);

            Assert.AreEqual(4, uimfRun.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual("25 {24, 25, 27}", uimfRun.ScanSetCollection.ScanSetList[1].ToString());



            //FrameSet testFrame0 = uimfRun.ScanSetCollection.ScanSetList[1];
            //Assert.AreEqual(3, testFrame0.IndexValues.Count);
            //Assert.AreEqual(1, testFrame0.IndexValues[0]);





        }

        [Test]
        public void CreateFrameSetCollectionIncludeMSMSFrames()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFFileContainingMSMSLevelData);

            var numFramesToSum = 3;

            var uimfRun = (UIMFRun)run;

            uimfRun.ScanSetCollection.Create(uimfRun, numFramesToSum, 1, true);

            Assert.AreEqual(15, uimfRun.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual("1 {1, 6}", uimfRun.ScanSetCollection.ScanSetList[0].ToString());
            Assert.AreEqual("2 {2, 7}", uimfRun.ScanSetCollection.ScanSetList[1].ToString());
            Assert.AreEqual("6 {1, 6, 11}", uimfRun.ScanSetCollection.ScanSetList[5].ToString());
            Assert.AreEqual("8 {3, 8, 13}", uimfRun.ScanSetCollection.ScanSetList[7].ToString());
            Assert.AreEqual("11 {6, 11}", uimfRun.ScanSetCollection.ScanSetList[10].ToString());
            Assert.AreEqual("15 {10, 15}", uimfRun.ScanSetCollection.ScanSetList[14].ToString());
        }

        [Test]
        public void CreateFrameSetCollectionSumAllConsecutiveMSMSFrames()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFFileContainingMSMSLevelData);

            var numFramesToSum = 3;

            var uimfRun = (UIMFRun)run;

            uimfRun.ScanSetCollection.Create(uimfRun, numFramesToSum, 1, true);

            Assert.AreEqual(6, uimfRun.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual("1 {1, 6}", uimfRun.ScanSetCollection.ScanSetList[0].ToString());
            Assert.AreEqual("2 {2, 3, 4, 5}", uimfRun.ScanSetCollection.ScanSetList[1].ToString());
            Assert.AreEqual("6 {1, 6, 11}", uimfRun.ScanSetCollection.ScanSetList[2].ToString());
            Assert.AreEqual("7 {7, 8, 9, 10}", uimfRun.ScanSetCollection.ScanSetList[3].ToString());
            Assert.AreEqual("11 {6, 11}", uimfRun.ScanSetCollection.ScanSetList[4].ToString());
            Assert.AreEqual("12 {12, 13, 14, 15}", uimfRun.ScanSetCollection.ScanSetList[5].ToString());
        }

        [Test]
        public void CreateFrameSetCollectionProcessMS1_Only()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFFileContainingMSMSLevelData);

            var numFramesToSum = 3;

            var uimfRun = (UIMFRun)run;

            uimfRun.ScanSetCollection.Create(uimfRun, numFramesToSum, 1, false);

            Assert.AreEqual(3, uimfRun.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual("1 {1, 6}", uimfRun.ScanSetCollection.ScanSetList[0].ToString());
            Assert.AreEqual("6 {1, 6, 11}", uimfRun.ScanSetCollection.ScanSetList[1].ToString());
            Assert.AreEqual("11 {6, 11}", uimfRun.ScanSetCollection.ScanSetList[2].ToString());
        }

        [Test]
        public void creatorTest_sum7_test1()
        {

            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var frameStart = 1;
            var frameStop = 11;

            var numFramesToSum = 7;


            var uimfRun = (UIMFRun)run;

            uimfRun.ScanSetCollection.Create(uimfRun, frameStart, frameStop, numFramesToSum, 1);

            Assert.AreEqual(11, uimfRun.ScanSetCollection.ScanSetList.Count);

            foreach (var frameset in uimfRun.ScanSetCollection.ScanSetList)
            {
                Console.WriteLine(frameset);


            }

            var testFrame0 = uimfRun.ScanSetCollection.ScanSetList[3];
            Assert.AreEqual(1, testFrame0.IndexValues.First());
            Assert.AreEqual(7, testFrame0.IndexValues.Count);
            Assert.AreEqual(7, testFrame0.IndexValues.Last());

        }



        [Test]
        public void creator_endFramesTest1()
        {

            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);


            var frameStop = ((UIMFRun)run).MaxLCScan;
            var frameStart = frameStop - 10;



            var uimfrun = (UIMFRun)run;
            uimfrun.ScanSetCollection.Create(uimfrun, frameStart, frameStop, 3, 1);


            Assert.AreEqual(11, uimfrun.ScanSetCollection.ScanSetList.Count);


            foreach (var frameset in uimfrun.ScanSetCollection.ScanSetList)
            {
                Console.WriteLine(frameset);
            }


            var testFrame0 = uimfrun.ScanSetCollection.ScanSetList[0];
            Assert.AreEqual(3, testFrame0.IndexValues.Count);
            Assert.AreEqual(1164, testFrame0.IndexValues[0]);
            Assert.AreEqual(1165, testFrame0.IndexValues[1]);
            Assert.AreEqual(1166, testFrame0.IndexValues[2]);





        }



        [Test]
        public void createIMSScanSets1()
        {
            var factory = new RunFactory();
            var run = factory.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var startScan = 120;
            var stopScan = 200;

            var numScansSummed = 7;
            run.ScanSetCollection.Create(run, startScan, stopScan, numScansSummed, 1);

            Assert.IsNotNull(run.ScanSetCollection);
            Assert.AreEqual(81, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(7, run.ScanSetCollection.ScanSetList[0].IndexValues.Count);
            Assert.AreEqual(117, run.ScanSetCollection.ScanSetList[0].IndexValues[0]);
            Assert.AreEqual(123, run.ScanSetCollection.ScanSetList[0].IndexValues[6]);

        }


        [Test]
        public void createIMSScanSets2()
        {
            var factory = new RunFactory();
            var run = (UIMFRun)factory.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);


            var numScansSummed = 7;
            run.IMSScanSetCollection.Create(run, numScansSummed, 1);


            Assert.IsNotNull(run.IMSScanSetCollection);

            //tests the lowest ims_scans
            Assert.AreEqual(361, run.IMSScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(4, run.IMSScanSetCollection.ScanSetList[0].IndexValues.Count);
            Assert.AreEqual(0, run.IMSScanSetCollection.ScanSetList[0].IndexValues[0]);
            Assert.AreEqual(3, run.IMSScanSetCollection.ScanSetList[0].IndexValues[3]);

            var testIMSScan2 = 100;
            Assert.AreEqual(7, run.IMSScanSetCollection.ScanSetList[testIMSScan2].IndexValues.Count);
            Assert.AreEqual(100, run.IMSScanSetCollection.ScanSetList[testIMSScan2].IndexValues[3]);
            Assert.AreEqual(100, run.IMSScanSetCollection.ScanSetList[testIMSScan2].PrimaryScanNumber);


        }

    }
}
