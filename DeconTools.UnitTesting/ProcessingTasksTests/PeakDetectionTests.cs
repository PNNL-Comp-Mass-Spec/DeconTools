using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Utilities;



namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class PeakDetectionTests
    {
        string imfMSScanTextfile = "..\\..\\Testfiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt";
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private string xcaliburTestfile2 = "..\\..\\TestFiles\\QC_Shew_09_02-pt1-3_10Jun09_Sphinx_09-02-18.RAW";
        private string uimfTestfile1 = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";



        [Test]
        public void DeconToolsPeakDetectionTest1()
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

            Assert.AreEqual(82, resultcollection.Run.PeakList.Count);
            Assert.AreEqual(167.243318707619, Convert.ToDecimal(resultcollection.Run.CurrentScanSet.BackgroundIntensity));

            detectorParams.PeakBackgroundRatio = 2;
            peakdetector = new DeconToolsPeakDetector(detectorParams);
            peakdetector.Execute(resultcollection);
            Assert.AreEqual(154, resultcollection.Run.PeakList.Count);
            Assert.AreEqual(167.243318707619, Convert.ToDecimal(resultcollection.Run.CurrentScanSet.BackgroundIntensity));

            detectorParams.PeakBackgroundRatio = 2;
            detectorParams.SignalToNoiseThreshold = 1;
            peakdetector = new DeconToolsPeakDetector(detectorParams);
            peakdetector.Execute(resultcollection);
            Assert.AreEqual(156, resultcollection.Run.PeakList.Count);
            Assert.AreEqual(167.243318707619, Convert.ToDecimal(resultcollection.Run.CurrentScanSet.BackgroundIntensity));

            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = true;
            peakdetector = new DeconToolsPeakDetector(detectorParams);
            peakdetector.Execute(resultcollection);

            Task scanresultUpdater = new ScanResultUpdater();
            scanresultUpdater.Execute(resultcollection);

            Assert.AreEqual(82, resultcollection.Run.PeakList.Count);
            Assert.AreEqual(1, resultcollection.ScanResultList.Count);
            Assert.AreEqual(82, resultcollection.ScanResultList[0].NumPeaks);
            Assert.AreEqual(167.243318707619, Convert.ToDecimal(resultcollection.Run.CurrentScanSet.BackgroundIntensity));

        }

        [Test]
        public void peakDetectionOnXCaliburTest1()
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
                Task msgen = new GenericMSGenerator();
                msgen.Execute(results);

                Task peakDetector = new DeconToolsPeakDetector();
                peakDetector.Execute(results);

                Task scanResultUpdater = new ScanResultUpdater();
                scanResultUpdater.Execute(results);
            }

            Assert.AreEqual(2, results.ScanResultList.Count);
        }


        [Test]
        public void peakDetectionOnXCaliburTest2_effectOfThresholdingOnPeakDetector()
        {
            Run run = new XCaliburRun(xcaliburTestfile2);

            ResultCollection results = new ResultCollection(run);

            run.CurrentScanSet = new ScanSet(4003);
            
            Task msgen = new GenericMSGenerator(0,2000);
            msgen.Execute(results);

            run.IsDataThresholded = false;

            Task peakDetector = new DeconToolsPeakDetector();
            ((DeconToolsPeakDetector)(peakDetector)).PeakBackgroundRatio = 1.3;
            ((DeconToolsPeakDetector)(peakDetector)).PeakFitType = Globals.PeakFitType.QUADRATIC;
            ((DeconToolsPeakDetector)(peakDetector)).SigNoiseThreshold = 2;
            peakDetector.Execute(results);

            Assert.AreEqual(562, results.Run.PeakList.Count);

            run.IsDataThresholded = true;      //fyi... by default, xcalibur runs are set to be true

            peakDetector = new DeconToolsPeakDetector();
            ((DeconToolsPeakDetector)(peakDetector)).PeakBackgroundRatio = 1.3;
            ((DeconToolsPeakDetector)(peakDetector)).PeakFitType = Globals.PeakFitType.QUADRATIC;
            ((DeconToolsPeakDetector)(peakDetector)).SigNoiseThreshold = 2;
            peakDetector.Execute(results);

            Assert.AreEqual(570, results.Run.PeakList.Count);


        }


        [Test]
        public void uimfPeakDetectionErrorTest1()
        {
            UIMFRun run = new UIMFRun(uimfTestfile1);

            FrameSetCollectionCreator framecreator = new FrameSetCollectionCreator(run, 1, 10, 3, 1);
            framecreator.Create();

            ScanSetCollectionCreator scanCreator = new ScanSetCollectionCreator(run, 1, 1);
            scanCreator.Create();

            Task msgen = new UIMF_MSGenerator(10,5000);
            Task peakDet = new DeconToolsPeakDetector();


            int scanstop = 599;
            int scanstart = 0;

            for (int i = scanstart; i < scanstop; i++)
            {
                run.CurrentFrameSet = run.FrameSetCollection.GetFrameSet(1);
                run.CurrentScanSet = run.ScanSetCollection.GetScanSet(i);
                msgen.Execute(run.ResultCollection);
              

                try
                {
                    peakDet.Execute(run.ResultCollection);
                }
                catch (Exception)
                {
                    Console.WriteLine("--------Report of scan " + run.CurrentScanSet.PrimaryScanNumber + "-------------------------");
                    reportXYValues(run);
                }
            }
            
           


        }

        private void reportXYValues(Run run)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("m/z\tintens\n");
            TestUtilities.GetXYValuesToStringBuilder(sb, run.XYData.Xvalues, run.XYData.Yvalues);
            Console.Write(sb.ToString());
        }

        private void reportXYValues()
        {
            throw new NotImplementedException();
        }



    }
}
