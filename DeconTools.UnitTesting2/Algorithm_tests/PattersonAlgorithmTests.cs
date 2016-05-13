using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.AlgorithmTests
{



    [TestFixture]
    public class PattersonAlgorithmTests
    {
        private string xcaliburTestfile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

        [Test]
        public void EasyCase1()
        {

            // https://jira.pnnl.gov/jira/browse/OMCS-647

            Run run = new XCaliburRun2(xcaliburTestfile);

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 1.3;

            run.CurrentScanSet = new ScanSet(6005);

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

            var peak = run.PeakList.First(n => n.XValue > 903.94 && n.XValue < 903.95);

            int chargeState = PattersonChargeStateCalculator.GetChargeState(run.XYData, run.PeakList, peak as MSPeak);
            Assert.AreEqual(2, chargeState);
        }

        [Test]
        public void HardCase1()
        {

            // https://jira.pnnl.gov/jira/browse/OMCS-647

            Run run = new XCaliburRun2(xcaliburTestfile);

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 1.3;

            run.CurrentScanSet = new ScanSet(6005);

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

           
            //579.535
            //TestUtilities.DisplayPeaks(run.PeakList);

            var peak = run.PeakList.First(n => n.XValue > 579.53 && n.XValue < 579.54);
            int chargeState = PattersonChargeStateCalculator.GetChargeState(run.XYData, run.PeakList, peak as MSPeak);
            Assert.AreEqual(4, chargeState);
            Console.WriteLine("charge State: " + chargeState);
            List<long> times = new List<long>();

            // return;
            for (int i = 0; i < 500; i++)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                chargeState = PattersonChargeStateCalculator.GetChargeState(run.XYData, run.PeakList, peak as MSPeak);
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }

            Console.WriteLine("average time in ms= " + times.Average());
        }

    }
}
