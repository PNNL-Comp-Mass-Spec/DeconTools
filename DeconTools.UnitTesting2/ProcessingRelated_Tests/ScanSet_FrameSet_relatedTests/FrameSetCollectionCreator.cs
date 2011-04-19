using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ScanSet_FrameSet_relatedTests
{
    [TestFixture]
    public class FrameSetCollectionCreatorTest
    {
        [Test]
        public void creatorTest_sum3_test1()
        {

            RunFactory rf=new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile1);

            int frameStart=0;
            int frameStop = 10;

            int numFramesToSum = 3;

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, frameStart, frameStop, numFramesToSum, 1);

            fscc.Create();

            UIMFRun uimfrun = (UIMFRun)run;

            Assert.AreEqual(11, uimfrun.FrameSetCollection.FrameSetList.Count);


            foreach (var frameset in uimfrun.FrameSetCollection.FrameSetList)
            {
                Console.WriteLine(frameset);


            }


            FrameSet testFrame0 = uimfrun.FrameSetCollection.FrameSetList[1];
            Assert.AreEqual(3, testFrame0.IndexValues.Count);
            Assert.AreEqual(0, testFrame0.IndexValues[0]);





        }


        [Test]
        public void creatorTest_sum7_test1()
        {

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile1);

            int frameStart = 0;
            int frameStop = 10;

            int numFramesToSum = 7;

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, frameStart, frameStop, numFramesToSum, 1);

            fscc.Create();

            UIMFRun uimfrun = (UIMFRun)run;

            Assert.AreEqual(11, uimfrun.FrameSetCollection.FrameSetList.Count);


            foreach (var frameset in uimfrun.FrameSetCollection.FrameSetList)
            {
                Console.WriteLine(frameset);


            }

            FrameSet testFrame0 = uimfrun.FrameSetCollection.FrameSetList[3];
             Assert.AreEqual(0, testFrame0.IndexValues.First());
             Assert.AreEqual(7, testFrame0.IndexValues.Count);
             Assert.AreEqual(6, testFrame0.IndexValues.Last());




        }



        [Test]
        public void creator_endFramesTest1()
        {

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile1);

            
            int frameStop = ((UIMFRun)run).MaxFrame;
            int frameStart = frameStop - 10;

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, frameStart, frameStop, 3, 1);

            fscc.Create();

            UIMFRun uimfrun = (UIMFRun)run;

            Assert.AreEqual(11, uimfrun.FrameSetCollection.FrameSetList.Count);


            foreach (var frameset in uimfrun.FrameSetCollection.FrameSetList)
            {
                Console.WriteLine(frameset);
            }


            FrameSet testFrame0 = uimfrun.FrameSetCollection.FrameSetList[0];
            Assert.AreEqual(3, testFrame0.IndexValues.Count);
            Assert.AreEqual(1938, testFrame0.IndexValues[0]);
            Assert.AreEqual(1939, testFrame0.IndexValues[1]);
            Assert.AreEqual(1940, testFrame0.IndexValues[2]);





        }


    }
}
