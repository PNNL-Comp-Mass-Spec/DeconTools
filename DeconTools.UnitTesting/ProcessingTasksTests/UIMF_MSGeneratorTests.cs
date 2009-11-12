using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class UIMF_MSGeneratorTests
    {
        private string uimfTestfile1 = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";

        [Test]
        public void GenerateUIMFSpectrumTest1()
        {

            Run run = new UIMFRun(uimfTestfile1);
            ResultCollection results = new ResultCollection(run);

            run.CurrentScanSet = new ScanSet(300);
            run.ScanSetCollection = new ScanSetCollection();
            run.ScanSetCollection.ScanSetList.Add(run.CurrentScanSet);

            ((UIMFRun)run).CurrentFrameSet = new FrameSet(1000, 999, 1001);

            Task msGen = new UIMF_MSGenerator();
            msGen.Execute(results);

            Assert.AreEqual(1010, results.Run.XYData.Xvalues.Length);
            Assert.AreEqual(1010, results.Run.XYData.Yvalues.Length);

        }

        [Test]
        public void GenerateUIMFSpectrumTest2()
        {

            Run run = new UIMFRun(uimfTestfile1);
            ResultCollection results = new ResultCollection(run);

            run.CurrentScanSet = new ScanSet(300);
            run.ScanSetCollection = new ScanSetCollection();
            run.ScanSetCollection.ScanSetList.Add(run.CurrentScanSet);

            ((UIMFRun)run).CurrentFrameSet = new FrameSet(1206);

            Task msGen = new UIMF_MSGenerator(200, 2000);
            msGen.Execute(results);

            Assert.AreEqual(578, results.Run.XYData.Xvalues.Length);
            Assert.AreEqual(578, results.Run.XYData.Yvalues.Length);

        }


        [Test]
        public void weirdZeroValuesForMZsAtEarlyFramesTest1()
        {
            UIMFRun run = new UIMFRun(uimfTestfile1);

            FrameSetCollectionCreator framecreator = new FrameSetCollectionCreator(run, 1, 10, 5, 1);
            framecreator.Create();

            ScanSetCollectionCreator scanCreator = new ScanSetCollectionCreator(run, 1, 1);
            scanCreator.Create();

            Task msgen = new UIMF_MSGenerator(0, 5000);
            Task peakDet = new DeconToolsPeakDetector();


            int scanstop = 599;
            int scanstart = 0;

            for (int i = scanstart; i < scanstop; i++)
            {
                run.CurrentFrameSet = run.FrameSetCollection.GetFrameSet(1);
                run.CurrentScanSet = run.ScanSetCollection.GetScanSet(i);
                msgen.Execute(run.ResultCollection);

                if (run.XYData != null && run.XYData.Xvalues != null && run.XYData.Xvalues[0] == 0)
                {
                    reportXYValues(run);
                }
            }


        }

       


       

        private void reportXYValues(Run run)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("--------------reporting scan " + run.CurrentScanSet.PrimaryScanNumber.ToString() + "-----------------\n");
            sb.Append("m/z\tintens\n");

            TestUtilities.GetXYValuesToStringBuilder(sb, run.XYData.Xvalues, run.XYData.Yvalues);
            Console.Write(sb.ToString());
        }


    }
}
