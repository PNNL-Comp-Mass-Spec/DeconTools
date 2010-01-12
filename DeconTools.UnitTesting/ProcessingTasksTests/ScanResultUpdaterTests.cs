using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using System.IO;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class ScanResultUpdaterTests
    {
        string imfMSScanTextfile = "..\\..\\Testfiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt";
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";
        private string peakListoutputPath = "..\\..\\TestFiles\\peakList.txt";

        [Test]
        public void updateScanResultsOnXCaliburFileTest1()
        {           
            Run run = new XCaliburRun(xcaliburTestfile, 6000, 7000);

            ScanSetCollection scansetCollection = new ScanSetCollection();
            for (int i = 6000; i < 6015; i++)
            {
                scansetCollection.ScanSetList.Add(new ScanSet(i));
            }

            ResultCollection results = new ResultCollection(run);
            foreach (ScanSet scanset in scansetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanset;
                Task msgen = new GenericMSGenerator(0,2000);
                msgen.Execute(results);

                Task peakDetector = new DeconToolsPeakDetector();
                peakDetector.Execute(results);

                Task rapid = new RapidDeconvolutor();
                rapid.Execute(results);

                Task scanResultUpdater = new ScanResultUpdater();
                scanResultUpdater.Execute(results);
            }

            Assert.AreEqual(2, results.ScanResultList.Count);
            Assert.AreEqual(1, results.ScanResultList[0].SpectrumType);
            Assert.AreEqual(481.274105402604, (decimal)results.ScanResultList[0].BasePeak.XValue);
            Assert.AreEqual(353, results.ScanResultList[0].NumIsotopicProfiles);

            Assert.AreEqual(2052, results.ScanResultList[0].NumPeaks);
            //Assert.AreEqual(2132, results.ScanResultList[0].NumPeaks);
            Assert.AreEqual(32.6941466666667, (decimal)results.ScanResultList[0].ScanTime);
            Assert.AreEqual(6005, results.ScanResultList[0].ScanSet.PrimaryScanNumber);

            Assert.AreEqual(362, results.ScanResultList[1].NumIsotopicProfiles);
            //Assert.AreEqual(370, results.ScanResultList[1].NumIsotopicProfiles);
            Assert.AreEqual(715, results.ResultList.Count);

           
        }

        
        /// <summary>
        /// ScanResultUpdater should create one scan result for each Frame
        /// </summary>
        [Test]
        public void updateScanResultsUIMFFileTest1()
        {
            UIMFRun run = new UIMFRun(uimfFilepath);

            run.FrameSetCollection = new FrameSetCollection();
            for (int i = 1200; i < 1203; i++)
            {
                run.FrameSetCollection.FrameSetList.Add(new FrameSet(i));
            }

            run.ScanSetCollection = new ScanSetCollection();
            for (int i = 200; i < 304; i++)
            {
                run.ScanSetCollection.ScanSetList.Add(new ScanSet(i));
            }
            StreamWriter sw;
            ResultCollection results = new ResultCollection(run);

            Task msgen = new UIMF_MSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task rapid = new RapidDeconvolutor();
            Task scanResultUpdater = new ScanResultUpdater();
            Task peakListExporter = new PeakListTextExporter(run.MSFileType, peakListoutputPath);


            foreach (FrameSet frameset in run.FrameSetCollection.FrameSetList)
            {
                ((UIMFRun)run).CurrentFrameSet = frameset;

                foreach (ScanSet scanset in run.ScanSetCollection.ScanSetList)
                {
                    run.CurrentScanSet = scanset;
                    msgen.Execute(results);

                    peakDetector.Execute(results);

                    rapid.Execute(results);

                    scanResultUpdater.Execute(results);

                    peakListExporter.Execute(results);
                }
                
            }
            peakListExporter.Cleanup();

            Assert.AreEqual(3, results.ScanResultList.Count);
            Assert.AreEqual(1, results.ScanResultList[0].SpectrumType);
            Assert.AreEqual(670.990710325132, (decimal)results.ScanResultList[0].BasePeak.XValue);
            Assert.AreEqual(183, results.ScanResultList[0].NumIsotopicProfiles);

            Assert.AreEqual(7097, results.ScanResultList[0].NumPeaks);
            Assert.AreEqual(-1, (decimal)results.ScanResultList[0].ScanTime);

            UIMFScanResult uimfScanresult1 = (UIMFScanResult)(results.ScanResultList[0]);


            Assert.AreEqual(4.0, uimfScanresult1.FramePressureFront);
            Assert.AreEqual(4.016, uimfScanresult1.FramePressureBack);
            Assert.AreEqual(1200, uimfScanresult1.Frameset.PrimaryFrame);

            Assert.AreEqual(504, results.ResultList.Count);


        }

    }
}
