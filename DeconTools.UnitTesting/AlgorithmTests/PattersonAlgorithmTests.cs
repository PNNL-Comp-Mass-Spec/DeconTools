using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.UnitTesting.AlgorithmTests
{



    [TestFixture]
    public class PattersonAlgorithmTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        [Test]
        public void test1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();

            run.CurrentScanSet = new ScanSet(6005);

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

            MSPeak testPeak = (MSPeak)run.PeakList[500];



            PattersonChargeStateCalculator chargeCalc = new PattersonChargeStateCalculator();
            short chargeState = chargeCalc.GetChargeState(run.XYData, run.PeakList, testPeak);

            Console.WriteLine(testPeak.XValue + "; Charge = "+ chargeState);


        }


    }
}
