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
    public class UIMF_SQLiteIsosExporterTests
    {

        string inputUIMFFile1 = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";
        string outputFile1 = "..\\..\\TestFiles\\UIMF_SQLiteIsosExporterTestOutput1.sqlite"; 
        

        [Test]
        public void test1()
        {

            Run uimfrun = new UIMFRun(inputUIMFFile1);
            int startFrame = 800;
            int stopFrame = 800;

            int numFramesSummed = 3;
            int numScansSummed = 9;

            FrameSetCollectionCreator framesetCreator = new FrameSetCollectionCreator(uimfrun, startFrame, stopFrame, numFramesSummed, 1);
            framesetCreator.Create();

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(uimfrun, 230, 240, numScansSummed, 1);
            scansetCreator.Create();

            Project project = Project.getInstance();

            project.RunCollection.Add(uimfrun);

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

            Assert.AreEqual(252, project.RunCollection[0].ResultCollection.ResultList.Count);

            UIMFSQLiteIsosExporter isosExporter = new UIMFSQLiteIsosExporter(outputFile1);
            isosExporter.Export(project.RunCollection[0].ResultCollection);
        }


    }
}
