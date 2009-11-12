using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Data;

namespace DeconTools.UnitTesting.ExporterTests
{
    [TestFixture]
    public class BasicScansExporterTests
    {
        string scansExporterTest1output = "..\\..\\Testfiles\\BasicScansExporterTest1Output.csv";
        string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";
 
        [Test]
        public void ExportMultipleScansTest1()
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

                Task msGen = new GenericMSGenerator();
                msGen.Execute(results);

                DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
                detectorParams.PeakBackgroundRatio = 3;
                detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
                detectorParams.SignalToNoiseThreshold = 3;
                detectorParams.ThresholdedData = false;

                Task peakDetector = new DeconToolsPeakDetector(detectorParams);
                peakDetector.Execute(results);

                Task decon = new RapidDeconvolutor();
                decon.Execute(results);

                Task scanResultUpdater = new ScanResultUpdater();
                scanResultUpdater.Execute(results);

            }

            Assert.AreEqual(3, results.ScanResultList.Count);

            Assert.AreEqual(92, results.ScanResultList[0].NumPeaks);
            Assert.AreEqual(82, results.ScanResultList[1].NumPeaks);
            Assert.AreEqual(72, results.ScanResultList[2].NumPeaks);

            Assert.AreEqual(11, results.ScanResultList[0].NumIsotopicProfiles);
            Assert.AreEqual(9, results.ScanResultList[1].NumIsotopicProfiles);
            Assert.AreEqual(9, results.ScanResultList[2].NumIsotopicProfiles);

            Assert.AreEqual(830.045752112968, (Decimal)results.ScanResultList[0].BasePeak.MZ);
            Assert.AreEqual(10438, results.ScanResultList[0].BasePeak.Intensity);
            Assert.AreEqual(0.09454554, (Decimal)results.ScanResultList[0].BasePeak.FWHM);
            Assert.AreEqual(434.9167, (Decimal)results.ScanResultList[0].BasePeak.SN);

            Exporter<ResultCollection> exporter = new BasicScansExporter(scansExporterTest1output);
            exporter.Export(results);
        }


    }
}
