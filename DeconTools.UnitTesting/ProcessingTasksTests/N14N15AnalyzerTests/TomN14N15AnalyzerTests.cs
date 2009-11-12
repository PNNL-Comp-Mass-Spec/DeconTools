using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Linq;
using DeconTools.Backend.ProcessingTasks.N14N15Analyzers.TomN14N15Analyzer;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.UnitTesting.ProcessingTasksTests.N14N15AnalyzerTests
{
    [TestFixture]
    public class TomN14N15AnalyzerTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private string icr2LSRawDatafile1 = "..\\..\\TestFiles\\ICR2LS_RawData\\ICR2LSRawData_Samplefile1_RSPH_PtoA_.02210";


        [Test]
        public void test1()
        {
            Run run = new ICR2LSRun(icr2LSRawDatafile1);
            run.CurrentScanSet = new ScanSet(2210);
           
            Task msGen = new GenericMSGenerator(732,746);
            msGen.Execute(run.ResultCollection);
         
            Task peakdetector = new DeconToolsPeakDetector();
            ((DeconToolsPeakDetector)peakdetector).PeakBackgroundRatio = 5;
            ((DeconToolsPeakDetector)peakdetector).SigNoiseThreshold = 2;

            peakdetector.Execute(run.ResultCollection);

            Console.WriteLine("Peaks detected = " + run.MSPeakList.Count);

            TomN14N15Analyzer analyzer = new TomN14N15Analyzer();
            analyzer.ExtractN14N15Values(run.ResultCollection);
            
            Console.WriteLine("Peaks detected = " + run.MSPeakList.Count);

            Assert.AreEqual(14, run.MSPeakList.Count);


        }


        [Test]
        public void test2()
        {

            Run run = new XCaliburRun(xcaliburTestfile);
            run.CurrentScanSet = new ScanSet(6005);


            Task msGen = new GenericMSGenerator(438, 500);
            msGen.Execute(run.ResultCollection);

            Task peakdetector = new DeconToolsPeakDetector();
            peakdetector.Execute(run.ResultCollection);

            Console.WriteLine("Peaks detected = " + run.MSPeakList.Count);

            TomN14N15Analyzer analyzer = new TomN14N15Analyzer();
            analyzer.ExtractN14N15Values(run.ResultCollection);

            Console.WriteLine("Peaks detected = " + run.MSPeakList.Count);

        }
    }
}
