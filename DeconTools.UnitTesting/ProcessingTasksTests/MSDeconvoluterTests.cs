using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend;
using DeconTools.Backend.ProcessingTasks;
using System.Diagnostics;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.MSGenerators;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class HornDeconvoluterTests
    {

        string imfMSScanTextfile = "..\\..\\Testfiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt";
        string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";


        [Test]
        public void DeconvoluteTestXYData()
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


            DeconToolsV2.HornTransform.clsHornTransformParameters hornParams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            Task hornDeconvolutor = new HornDeconvolutor(hornParams);
            hornDeconvolutor.Execute(resultcollection);

            Task scanresultUpdater = new ScanResultUpdater();
            scanresultUpdater.Execute(resultcollection);

            int counter = 0;
            foreach (StandardIsosResult result in resultcollection.ResultList)
            {
                IsotopicProfile profile = result.IsotopicProfile;

                Console.Write("------------ profile  " + counter + "  ----------------\n");
                int peakcounter = 0;
                Console.Write("idx" + "\t" + "mz" + "\t" + "intensity" + "\t" + "SN" + "\t" + "FWHM" + "\n");
                foreach (MSPeak peak in profile.Peaklist)
                {
                    Console.Write(peakcounter + "\t" + peak.XValue + "\t" + peak.Height + "\t" + peak.SN + "\t" + peak.Width + "\n");
                    peakcounter++;
                }
                counter++;
            }

            Assert.AreEqual(12, resultcollection.ResultList.Count);
            Assert.AreEqual(682.346929789284, Convert.ToDecimal(resultcollection.ResultList[0].IsotopicProfile.Peaklist[0].XValue));
            Assert.AreEqual(682.683202478548, Convert.ToDecimal(resultcollection.ResultList[0].IsotopicProfile.Peaklist[1].XValue));
            Assert.AreEqual(10780, resultcollection.ResultList[0].IsotopicProfile.Peaklist[1].Height);
            Assert.AreEqual(2695, Convert.ToDecimal(resultcollection.ResultList[0].IsotopicProfile.Peaklist[1].SN));
            Assert.AreEqual(0.08720002, Convert.ToDecimal(resultcollection.ResultList[0].IsotopicProfile.Peaklist[1].Width));

            Assert.AreEqual(5, resultcollection.ResultList[4].IsotopicProfile.GetNumOfIsotopesInProfile());
            Assert.AreEqual(2488.71170791031, Convert.ToDecimal(resultcollection.ResultList[4].IsotopicProfile.AverageMass));
            Assert.AreEqual(3, resultcollection.ResultList[4].IsotopicProfile.ChargeState);
            Assert.AreEqual(6764, Convert.ToDecimal(resultcollection.ResultList[4].IsotopicProfile.IntensityAggregate));
            Assert.AreEqual(2487.10393391031, Convert.ToDecimal(resultcollection.ResultList[4].IsotopicProfile.MonoIsotopicMass));

            Assert.AreEqual(1, resultcollection.ScanResultList.Count);
            Assert.AreEqual(12, resultcollection.ScanResultList[0].NumIsotopicProfiles);




        }

        [Test]
        public void DeconvoluteIMFFileTest1()
        {
            Run run = new IMFRun(imfFilepath);
            run.CurrentScanSet = new ScanSet(233);

            ResultCollection results = new ResultCollection(run);

            
            Task msGen = new GenericMSGenerator(500, 600);
            msGen.Execute(results);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            Task peakDetector = new DeconToolsPeakDetector();
            peakDetector.Execute(results);

            DeconToolsV2.HornTransform.clsHornTransformParameters hornParams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            Task decon = new HornDeconvolutor(hornParams);
            decon.Execute(results);


        }

        [Test]
        public void DeconvoluteIMFFileTest2()
        {
            Run run = new IMFRun(imfFilepath);

            ScanSetCollection scanSetCollection = new ScanSetCollection();
            scanSetCollection.ScanSetList.Add(new ScanSet(232));
            scanSetCollection.ScanSetList.Add(new ScanSet(233));
            scanSetCollection.ScanSetList.Add(new ScanSet(234));

            ResultCollection results = new ResultCollection(run);

            foreach (ScanSet scanset in scanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanset;

                Task msGen = new GenericMSGenerator(500, 800);
                msGen.Execute(results);

                DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
                Task peakDetector = new DeconToolsPeakDetector();
                peakDetector.Execute(results);

                DeconToolsV2.HornTransform.clsHornTransformParameters hornParams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
                Task decon = new HornDeconvolutor(hornParams);
                decon.Execute(results);

                Task scanResultupdater = new ScanResultUpdater();
                scanResultupdater.Execute(results);
                
            }

            Assert.AreEqual(3, results.ScanResultList.Count);
            Assert.AreEqual(12, results.ScanResultList[0].NumIsotopicProfiles);
            Assert.AreEqual(10, results.ScanResultList[1].NumIsotopicProfiles);
            Assert.AreEqual(12, results.ScanResultList[2].NumIsotopicProfiles);


 


        }

    }
}
