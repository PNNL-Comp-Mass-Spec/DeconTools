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
    public class UIMF_ScansExporterTests
    {
        private string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";
        private string uimfScansExporterTest1output = "..\\..\\Testfiles\\UIMFScansExporterTest1Output.csv";
        
        [Test]
        public void test1()
        {
            Project project = Project.getInstance();
            //TODO:  add project parameters  (for summing)

            Run run = new UIMFRun(uimfFilepath, 1200, 1202, 300, 400);

            project.RunCollection.Add(run);

            FrameSetCollectionCreator framesetCreator = new FrameSetCollectionCreator(run, ((UIMFRun)run).MinFrame, ((UIMFRun)run).MaxFrame, 1, 1);
            framesetCreator.Create();

            ScanSetCollectionCreator scanSetcreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1, false);
            scanSetcreator.Create();

            Task msGen = new UIMF_MSGenerator(200, 2000);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            Task rapidDecon = new RapidDeconvolutor();

            Task scanresultUpdater = new ScanResultUpdater();
            

            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(rapidDecon);
            project.TaskCollection.TaskList.Add(scanresultUpdater);

            TaskController controller = new UIMF_TaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);

            UIMFScansExporter scansExporter = new UIMFScansExporter(uimfScansExporterTest1output);
            scansExporter.Export(project.RunCollection[0].ResultCollection);



        }


    }
}
