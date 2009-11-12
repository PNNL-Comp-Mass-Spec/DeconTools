using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class FrameSetCollectionCreatorTests
    {
        public string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000.uimf";

        [Test]
        public void basicCreatorTest1()
        {
            Run run = new UIMFRun(uimfFilepath);

            FrameSetCollectionCreator creator = new FrameSetCollectionCreator(run, 3, 1);
            creator.Create();

            UIMFRun uimfRun = (UIMFRun)run;
            Assert.AreEqual(2400, uimfRun.FrameSetCollection.FrameSetList.Count);
            Assert.AreEqual(2, uimfRun.FrameSetCollection.FrameSetList[2].PrimaryFrame);
            Assert.AreEqual(new int[] { 1, 2, 3 }, uimfRun.FrameSetCollection.FrameSetList[2].IndexValues.ToArray());
            Assert.AreEqual(new int[] { 0, 1 }, uimfRun.FrameSetCollection.FrameSetList[0].IndexValues.ToArray());
            Assert.AreEqual(new int[] { 2398, 2399 }, uimfRun.FrameSetCollection.FrameSetList[2399].IndexValues.ToArray());

        }

        [Test]
        public void basicCreatorTest2()
        {
            Run run = new UIMFRun(uimfFilepath);

            FrameSetCollectionCreator creator = new FrameSetCollectionCreator(run,1200,1300, 3, 1);
            creator.Create();

            UIMFRun uimfRun = (UIMFRun)run;
            Assert.AreEqual(101, uimfRun.FrameSetCollection.FrameSetList.Count);
            Assert.AreEqual(1200, uimfRun.FrameSetCollection.FrameSetList[0].PrimaryFrame);
            Assert.AreEqual(new int[] { 1199, 1200, 1201 }, uimfRun.FrameSetCollection.FrameSetList[0].IndexValues.ToArray());
        }

        [Test]
        public void incrementorTest1()
        {
            Run run = new UIMFRun(uimfFilepath);

            FrameSetCollectionCreator creator = new FrameSetCollectionCreator(run, 1200, 1300, 3, 3);
            creator.Create();

            UIMFRun uimfRun = (UIMFRun)run;
            Assert.AreEqual(34, uimfRun.FrameSetCollection.FrameSetList.Count);
            Assert.AreEqual(1200, uimfRun.FrameSetCollection.FrameSetList[0].PrimaryFrame);
            Assert.AreEqual(new int[] { 1199, 1200, 1201 }, uimfRun.FrameSetCollection.FrameSetList[0].IndexValues.ToArray());
            Assert.AreEqual(new int[] { 1202, 1203, 1204 }, uimfRun.FrameSetCollection.FrameSetList[1].IndexValues.ToArray());

        }



        [ExpectedException(ExpectedMessage = "Range value must be an odd number")]
        [Test]
        public void rangeIsNotOddTest1()
        {
            Run run = new UIMFRun(uimfFilepath);
            FrameSetCollectionCreator creator = new FrameSetCollectionCreator(run, 1200, 1300, 4, 1);
            creator.Create();
        }





    }
}
