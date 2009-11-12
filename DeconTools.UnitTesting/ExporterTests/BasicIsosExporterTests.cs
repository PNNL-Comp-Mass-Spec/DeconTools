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
    public class BasicIsosExporterTests
    {
        string exporterTest1OutputName = "..\\..\\Testfiles\\BasicIsosExporterTest1Output.csv";
        string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";
        
        [Test]
        public void exporterTest1()
        {
            Run run = new IMFRun(imfFilepath);
            run.CurrentScanSet = new ScanSet(233);

            ResultCollection results = new ResultCollection(run);
            Task msGen = new GenericMSGenerator();
            msGen.Execute(results);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);
            peakDetector.Execute(results);

            DeconToolsV2.HornTransform.clsHornTransformParameters hornParams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            Task decon = new HornDeconvolutor(hornParams);
            decon.Execute(results);

            Exporter<ResultCollection> exporter = new BasicIsosExporter(exporterTest1OutputName);
            exporter.Export(results);

        }



    }
}
