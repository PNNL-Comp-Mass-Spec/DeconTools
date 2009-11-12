using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Data;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting.ExporterTests
{
    [TestFixture]
    public class UIMF_IsosExporterTests
    {
        private string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";
        private string uimfIsosExporterTest1output = "..\\..\\Testfiles\\UIMFIsosExporterTest1Output.csv";
        private string uimfIsosExporterTest2output = "..\\..\\Testfiles\\UIMFIsosExporterTest2Output.csv";

        private string binaryUIMFData = "..\\..\\TestFiles\\deserializer_UIMF_Test1.bin";

        [Test]
        public void smallExportTest1()
        {
            Project project = Project.getInstance();
            //TODO:  add project parameters  (for summing)

            project.RunCollection.Add(new UIMFRun(uimfFilepath, 1200, 1202,250,300));

            Task msGen = new UIMF_MSGenerator(200, 2000);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            Task decon = new HornDeconvolutor();

            Task driftTimeExtractor = new UIMFDriftTimeExtractor();


            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(decon);
            project.TaskCollection.TaskList.Add(driftTimeExtractor);


            TaskController controller = new UIMF_TaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);

            UIMFIsosExporter isosExporter = new UIMFIsosExporter(uimfIsosExporterTest1output);
            isosExporter.Export(project.RunCollection[0].ResultCollection);

        }


        [Test]
        public void readInBinaryDataAndExport()
        {
            UIMFIsosExporter isosExporter = new UIMFIsosExporter(uimfIsosExporterTest2output);
            isosExporter.Export(binaryUIMFData, false);
        }

        //[Test]
        //public void readInBinaryDataAndExport2()
        //{
        //    UIMFIsosExporter isosExporter = new UIMFIsosExporter(@"D:\MSData_Examples\ReFitting_tests\UIMF\Test06_thrash_frames400-900_refit\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000_isos.csv");

        //    string inputBinaryData = @"D:\MSData_Examples\ReFitting_tests\UIMF\Test06_thrash_frames400-900_refit\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf_tmp.bin";

        //    isosExporter.Export(inputBinaryData,false);
        //}


        //[Test]
        //public void largeTest1()
        //{
        //    Project project = Project.getInstance();
        //    //TODO:  add project parameters  (for summing)

        //    project.RunCollection.Add(new UIMFRun(uimfFilepath,1,500));

        //    Task msGen = new UIMF_MSGenerator(200, 2000);

        //    DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
        //    detectorParams.PeakBackgroundRatio = 3;
        //    detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
        //    detectorParams.SignalToNoiseThreshold = 3;
        //    detectorParams.ThresholdedData = false;
        //    Task peakDetector = new DeconToolsPeakDetector(detectorParams);

        //    Task rapidDecon = new RapidDeconvolutor();

        //    Task driftTimeExtractor = new UIMFDriftTimeExtractor();

        //    project.TaskCollection.TaskList.Add(msGen);
        //    project.TaskCollection.TaskList.Add(peakDetector);
        //    project.TaskCollection.TaskList.Add(rapidDecon);
        //    project.TaskCollection.TaskList.Add(driftTimeExtractor);

        //    TaskController controller = new UIMF_TaskController(project.TaskCollection);
        //    controller.Execute();

        //    UIMFIsosExporter isosExporter = new UIMFIsosExporter(uimfIsosExporterTest1output);
        //    isosExporter.Export(project.RunCollection[0].ResultCollection);

        //}



    }
}
