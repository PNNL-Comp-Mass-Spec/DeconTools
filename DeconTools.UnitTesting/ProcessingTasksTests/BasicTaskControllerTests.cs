using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using System.Diagnostics;
using DeconTools.Backend.Utilities;


namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class BasicTaskControllerTests
    {
        public string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        private List<string> imfFileList = new List<string>();




        [Test]
        public void imfFile100ScansTest1()
        {
            Project.Reset();
            Project project = Project.getInstance();

            Run run = new IMFRun(imfFilepath, 200, 299);

            project.RunCollection.Add(run);

            Task msGen = new GenericMSGenerator();

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            Task rapidDecon = new RapidDeconvolutor();

            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(rapidDecon);

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1);
            scanSetCreator.Create();




            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new BasicTaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);
            sw.Stop();

            Assert.AreEqual(1, Project.getInstance().RunCollection.Count);
            Assert.AreEqual(3, project.TaskCollection.TaskList.Count);

            Assert.AreEqual(100, project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(1070, project.RunCollection[0].ResultCollection.ResultList.Count);

            Console.WriteLine("Time required (ms) = " + sw.ElapsedMilliseconds);
            Console.WriteLine("Scans analyzed = " + project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("Features found = " + project.RunCollection[0].ResultCollection.ResultList.Count);
        }

        [Test]
        public void xcaliburFile100ScanRAPIDTest1()
        {
            Project.Reset();

            Run run = new XCaliburRun(xcaliburTestfile, 6000, 6999);

            Project project = Project.getInstance();
            project.RunCollection.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1);
            sscc.Create();



            Task msGen = new GenericMSGenerator();

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            Task rapidDecon = new RapidDeconvolutor();

            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(rapidDecon);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new BasicTaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);
            sw.Stop();

            Assert.AreEqual(1, Project.getInstance().RunCollection.Count);
            Assert.AreEqual(3, project.TaskCollection.TaskList.Count);

            Assert.AreEqual(1000, project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(12762, project.RunCollection[0].ResultCollection.ResultList.Count);

            Console.WriteLine("Time required (ms) = " + sw.ElapsedMilliseconds);
            Console.WriteLine("Scans analyzed = " + project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("Features found = " + project.RunCollection[0].ResultCollection.ResultList.Count);

        }


        [Test]
        public void xcaliburFile20ScanHornDeconTest1()
        {
            Project.Reset();

            Run run = new XCaliburRun(xcaliburTestfile, 6000, 6020);
            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1);
            sscc.Create();

            Project project = Project.getInstance();
            project.RunCollection.Add(run);

            Task msGen = new GenericMSGenerator();

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            DeconToolsV2.HornTransform.clsHornTransformParameters hornparams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            hornparams.PeptideMinBackgroundRatio = 5;
            Task hornDecon = new HornDeconvolutor(hornparams);

            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(hornDecon);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new BasicTaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);
            sw.Stop();

            Assert.AreEqual(1, Project.getInstance().RunCollection.Count);
            Assert.AreEqual(3, project.TaskCollection.TaskList.Count);

            Assert.AreEqual(21, project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(233, project.RunCollection[0].ResultCollection.ResultList.Count);

            Console.WriteLine("Time required (ms) = " + sw.ElapsedMilliseconds);
            Console.WriteLine("Scans analyzed = " + project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("Features found = " + project.RunCollection[0].ResultCollection.ResultList.Count);

        }


        [Test]
        public void xcaliburFileMSMSDataTest1()
        {
            //exploring how only MSMS data is processed...

            Project.Reset();

            Run run = new XCaliburRun(xcaliburTestfile, 6000, 6004);    //only MS/MS data here
            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1);
            sscc.Create();

            Project project = Project.getInstance();
            project.RunCollection.Add(run);

            Task msGen = new GenericMSGenerator();

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 1.3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 2;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            DeconToolsV2.HornTransform.clsHornTransformParameters hornparams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            hornparams.PeptideMinBackgroundRatio = 2;
            Task hornDecon = new HornDeconvolutor(hornparams);

            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(hornDecon);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new BasicTaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);
            sw.Stop();

            Assert.AreEqual(1, Project.getInstance().RunCollection.Count);
            Assert.AreEqual(3, project.TaskCollection.TaskList.Count);

//            Assert.AreEqual(21, project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(233, project.RunCollection[0].ResultCollection.ResultList.Count);

            Console.WriteLine("Time required (ms) = " + sw.ElapsedMilliseconds);
            Console.WriteLine("Scans analyzed = " + project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("Features found = " + project.RunCollection[0].ResultCollection.ResultList.Count);

        }


        [Test]
        public void multiIMFFileRAPIDDTest1()
        {
            imfFileList.Clear();
            imfFileList.Add("..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF");
            imfFileList.Add("..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_2.IMF");
            imfFileList.Add("..\\..\\TestFiles\\50ugpmlBSA_CID_SQ_14V_0000.Accum_1.IMF");

            Project.Reset();

            Project project = Project.getInstance();

            foreach (string strItem in imfFileList)
            {
                Run run = new IMFRun(strItem, 200, 299);
                
                project.RunCollection.Add(run);

                ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1);
                sscc.Create();
            }

            Task msGen = new GenericMSGenerator();

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            Task rapidDecon = new RapidDeconvolutor();

            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(rapidDecon);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new BasicTaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);
            sw.Stop();

            Assert.AreEqual(3, project.RunCollection.Count);
            Assert.AreEqual(3, project.TaskCollection.TaskList.Count);

            Assert.AreEqual(100, project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(1070, project.RunCollection[0].ResultCollection.ResultList.Count);


            int totalScans = 0;
            int totalFeatures = 0;
            for (int i = 0; i < project.RunCollection.Count; i++)
            {
                Console.WriteLine("Run name = " + project.RunCollection[i].Filename);
                Console.WriteLine("Scans analyzed = " + project.RunCollection[i].ScanSetCollection.ScanSetList.Count);
                Console.WriteLine("Features found = " + project.RunCollection[i].ResultCollection.ResultList.Count);

                totalFeatures += project.RunCollection[i].ResultCollection.ResultList.Count;
                totalScans += project.RunCollection[i].ScanSetCollection.ScanSetList.Count;

            }
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Total Features found = " + totalFeatures);
            Console.WriteLine("Total Scans = " + totalScans);
            Console.WriteLine("Total Time required (ms) = " + sw.ElapsedMilliseconds);

        }


        [Test]
        public void multiIMFFileDeconTest1()
        {
            imfFileList.Clear();
            imfFileList.Add("..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF");
            imfFileList.Add("..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_2.IMF");
            imfFileList.Add("..\\..\\TestFiles\\50ugpmlBSA_CID_SQ_14V_0000.Accum_1.IMF");


            Project.Reset();
            Project project = Project.getInstance();

            foreach (string strItem in imfFileList)
            {
                Run run = new IMFRun(strItem, 233, 233);
                project.RunCollection.Add(run);

                ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1);
                sscc.Create();
            }

            Task msGen = new GenericMSGenerator();

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            DeconToolsV2.HornTransform.clsHornTransformParameters hornparams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            hornparams.PeptideMinBackgroundRatio = 5;
            Task hornDecon = new HornDeconvolutor(hornparams);

            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(hornDecon);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new BasicTaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);
            sw.Stop();

            Assert.AreEqual(3, Project.getInstance().RunCollection.Count);
            Assert.AreEqual(3, project.TaskCollection.TaskList.Count);

            Assert.AreEqual(1, project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(12, project.RunCollection[0].ResultCollection.ResultList.Count);


            int totalScans = 0;
            int totalFeatures = 0;
            for (int i = 0; i < project.RunCollection.Count; i++)
            {
                Console.WriteLine("Run name = " + project.RunCollection[i].Filename);
                Console.WriteLine("Scans analyzed = " + project.RunCollection[i].ScanSetCollection.ScanSetList.Count);
                Console.WriteLine("Features found = " + project.RunCollection[i].ResultCollection.ResultList.Count);

                totalFeatures += project.RunCollection[i].ResultCollection.ResultList.Count;
                totalScans += project.RunCollection[i].ScanSetCollection.ScanSetList.Count;

            }
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Total Features found = " + totalFeatures);
            Console.WriteLine("Total Scans = " + totalScans);
            Console.WriteLine("Total Time required (ms) = " + sw.ElapsedMilliseconds);

        }

    }
}
