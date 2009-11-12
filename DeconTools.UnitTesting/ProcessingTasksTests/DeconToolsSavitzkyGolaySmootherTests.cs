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

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class DeconToolsSavitzkyGolaySmootherTests
    {
        string imfMSScanTextfile = "..\\..\\Testfiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt";
        string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";
 

        [Test]
        public void test1()
        {
            Run run = new MSScanFromTextFileRun(imfMSScanTextfile, Globals.XYDataFileType.Textfile);
            ResultCollection resultcollection = new ResultCollection(run);

            Task msgen = new GenericMSGenerator();
            msgen.Execute(resultcollection);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;

            Task peakdetector = new DeconToolsPeakDetector(detectorParams);
            peakdetector.Execute(resultcollection);

            Assert.AreEqual(82, resultcollection.Run.MSPeakList.Count);

            Task smoother = new DeconToolsSavitzkyGolaySmoother(3, 3, 2);
            smoother.Execute(resultcollection);

            peakdetector.Execute(resultcollection);

            Assert.AreEqual(67, resultcollection.Run.MSPeakList.Count);
        }


    }
}
