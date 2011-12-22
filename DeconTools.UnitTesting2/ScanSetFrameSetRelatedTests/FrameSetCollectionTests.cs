using System;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting2.ScanSetFrameSetRelatedTests
{
    [TestFixture]
    public class FrameSetCollectionTests
    {
        [Test]
        public void creatorTest_sum3_test1()
        {

            RunFactory rf=new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            int frameStart=1;
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

            
            int frameStop = ((UIMFRun)run).MaxFrame;
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
