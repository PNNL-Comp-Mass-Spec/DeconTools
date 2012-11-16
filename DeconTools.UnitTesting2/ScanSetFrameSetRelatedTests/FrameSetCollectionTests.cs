using System;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ScanSetFrameSetRelatedTests
{
    [TestFixture]
    public class FrameSetCollectionTests
    {
        [Test]
        public void creatorTest_sum3_test1()
        {

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            int frameStart = 1;
            int frameStop = 11;

            int numFramesToSum = 3;

            UIMFRun uimfRun = (UIMFRun)run;

            uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, frameStart, frameStop, numFramesToSum, 1);

            Assert.AreEqual(11, uimfRun.FrameSetCollection.FrameSetList.Count);


            foreach (var frameset in uimfRun.FrameSetCollection.FrameSetList)
            {
                Console.WriteLine(frameset);


            }


            FrameSet testFrame0 = uimfRun.FrameSetCollection.FrameSetList[1];
            Assert.AreEqual(3, testFrame0.IndexValues.Count);
            Assert.AreEqual(1, testFrame0.IndexValues[0]);





        }


        [Test]
        public void creatorTest_ensureOnlyMS1_test1()
        {

            //note that frame 26 is a calibration frame. So we don't want to have this if we are summing frames

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile4);    //Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf

            int frameStart = 24;
            int frameStop = 28;

            int numFramesToSum = 3;

            UIMFRun uimfRun = (UIMFRun)run;

            uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, frameStart, frameStop, numFramesToSum, 1);

            Assert.AreEqual(4, uimfRun.FrameSetCollection.FrameSetList.Count);
            Assert.AreEqual("24,25,27", uimfRun.FrameSetCollection.FrameSetList[1].ToString());



            //FrameSet testFrame0 = uimfRun.FrameSetCollection.FrameSetList[1];
            //Assert.AreEqual(3, testFrame0.IndexValues.Count);
            //Assert.AreEqual(1, testFrame0.IndexValues[0]);





        }

        [Test]
        public void CreateFrameSetCollectionIncludeMSMSFrames()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFFileContainingMSMSLevelData);

            int numFramesToSum = 3;

            UIMFRun uimfRun = (UIMFRun)run;

            uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, numFramesToSum, 1, true);

            Assert.AreEqual(15, uimfRun.FrameSetCollection.FrameSetList.Count);
            Assert.AreEqual("1,6", uimfRun.FrameSetCollection.FrameSetList[0].ToString());
			Assert.AreEqual("2,7", uimfRun.FrameSetCollection.FrameSetList[1].ToString());
			Assert.AreEqual("1,6,11", uimfRun.FrameSetCollection.FrameSetList[5].ToString());
			Assert.AreEqual("3,8,13", uimfRun.FrameSetCollection.FrameSetList[7].ToString());
			Assert.AreEqual("6,11", uimfRun.FrameSetCollection.FrameSetList[10].ToString());
			Assert.AreEqual("10,15", uimfRun.FrameSetCollection.FrameSetList[14].ToString());
        }

        [Test]
        public void CreateFrameSetCollectionProcessMS1_Only()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFFileContainingMSMSLevelData);

            int numFramesToSum = 3;

            UIMFRun uimfRun = (UIMFRun)run;

            uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, numFramesToSum, 1, false);

            Assert.AreEqual(3, uimfRun.FrameSetCollection.FrameSetList.Count);
            Assert.AreEqual("1,6", uimfRun.FrameSetCollection.FrameSetList[0].ToString());
            Assert.AreEqual("1,6,11", uimfRun.FrameSetCollection.FrameSetList[1].ToString());
			Assert.AreEqual("6,11", uimfRun.FrameSetCollection.FrameSetList[2].ToString());
        }

        [Test]
        public void creatorTest_sum7_test1()
        {

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            int frameStart = 1;
            int frameStop = 11;

            int numFramesToSum = 7;


            UIMFRun uimfRun = (UIMFRun)run;

            uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, frameStart, frameStop, numFramesToSum, 1);

            Assert.AreEqual(11, uimfRun.FrameSetCollection.FrameSetList.Count);

            foreach (var frameset in uimfRun.FrameSetCollection.FrameSetList)
            {
                Console.WriteLine(frameset);


            }

            FrameSet testFrame0 = uimfRun.FrameSetCollection.FrameSetList[3];
            Assert.AreEqual(1, testFrame0.IndexValues.First());
            Assert.AreEqual(7, testFrame0.IndexValues.Count);
            Assert.AreEqual(7, testFrame0.IndexValues.Last());

        }



        [Test]
        public void creator_endFramesTest1()
        {

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);


            int frameStop = ((UIMFRun)run).MaxLCScan;
            int frameStart = frameStop - 10;



            UIMFRun uimfrun = (UIMFRun)run;
            uimfrun.FrameSetCollection = FrameSetCollection.Create(uimfrun, frameStart, frameStop, 3, 1);


            Assert.AreEqual(11, uimfrun.FrameSetCollection.FrameSetList.Count);


            foreach (var frameset in uimfrun.FrameSetCollection.FrameSetList)
            {
                Console.WriteLine(frameset);
            }


            FrameSet testFrame0 = uimfrun.FrameSetCollection.FrameSetList[0];
            Assert.AreEqual(3, testFrame0.IndexValues.Count);
            Assert.AreEqual(1164, testFrame0.IndexValues[0]);
            Assert.AreEqual(1165, testFrame0.IndexValues[1]);
            Assert.AreEqual(1166, testFrame0.IndexValues[2]);





        }


    }
}
