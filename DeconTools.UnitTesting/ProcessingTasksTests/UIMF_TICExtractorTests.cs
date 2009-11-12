using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class UIMF_TICExtractorTests
    {
        string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";


        [Test]
        public void test1()
        {
            Run run = new UIMFRun(uimfFilepath);
            ResultCollection results = new ResultCollection(run);

            run.CurrentScanSet = new ScanSet(300);
            ((UIMFRun)run).CurrentFrameSet = new FrameSet(1000, 999, 1001);
            
            Task ticExtractor = new UIMF_TICExtractor();
            ticExtractor.Execute(results);

            Assert.AreEqual(47973, results.Run.CurrentScanSet.TICValue);
        }


    }
}
