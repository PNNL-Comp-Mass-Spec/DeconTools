using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend;
using DeconTools.Backend.ProcessingTasks.Smoothers;

namespace DeconTools.UnitTesting2.ProcessingTasksTests
{
    [TestFixture]
    public class DeconToolsSavitzkyGolaySmootherTests
    {
   
        [Test]
        public void smootherTest1()
        {
            Run run = new MSScanFromTextFileRun(FileRefs.RawDataMSFiles.TextFileMS_std1);
      
            Task msgen = new GenericMSGenerator();
            msgen.Execute(run.ResultCollection);

            Task peakdetector = new DeconToolsPeakDetector(3, 3, Globals.PeakFitType.QUADRATIC, false);
            peakdetector.Execute(run.ResultCollection);

            Assert.AreEqual(82, run.PeakList.Count);

            Task smoother = new DeconToolsSavitzkyGolaySmoother(3, 3, 2);
            smoother.Execute(run.ResultCollection);

            peakdetector.Execute(run.ResultCollection);

            Assert.AreEqual(67, run.PeakList.Count);
        }


    }
}
