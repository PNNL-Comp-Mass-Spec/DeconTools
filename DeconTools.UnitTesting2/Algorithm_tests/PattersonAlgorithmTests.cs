using DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.AlgorithmTests
{



    [TestFixture]
    public class PattersonAlgorithmTests
    {
        private string xcaliburTestfile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

        [Test]
        public void test1()
        {
            Run run = new XCaliburRun2(xcaliburTestfile);

            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();

            run.CurrentScanSet = new ScanSet(6005);
          
            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

            MSPeak testPeak1 = (MSPeak)run.PeakList[384];
            MSPeak testPeak2 = (MSPeak)run.PeakList[543];
            MSPeak testPeak3 = (MSPeak)run.PeakList[59];
            MSPeak testPeak4 = (MSPeak)run.PeakList[454];
            MSPeak testPeak5 = (MSPeak)run.PeakList[455];
            MSPeak testPeak6 = (MSPeak)run.PeakList[4];

            //TestUtilities.DisplayPeaks(run.PeakList);

            PattersonChargeStateCalculator chargeCalc = new PattersonChargeStateCalculator();
            int testpeak1CS = chargeCalc.GetChargeState(run.XYData, run.PeakList, testPeak1);
            int testpeak2CS = chargeCalc.GetChargeState(run.XYData, run.PeakList, testPeak2);
            int testpeak3CS = chargeCalc.GetChargeState(run.XYData, run.PeakList, testPeak3);
            int testpeak4CS = chargeCalc.GetChargeState(run.XYData, run.PeakList, testPeak4);
            int testpeak5CS = chargeCalc.GetChargeState(run.XYData, run.PeakList, testPeak5);
            int testpeak6CS = chargeCalc.GetChargeState(run.XYData, run.PeakList, testPeak6);



            Assert.AreEqual(2, testpeak1CS);
            Assert.AreEqual(3, testpeak2CS);
            Assert.AreEqual(5, testpeak3CS);
            //Assert.AreEqual(4, testpeak4CS);
            Assert.AreEqual(4, testpeak5CS);
            //Assert.AreEqual(1, testpeak6CS);
        }


    }
}
