using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Linq;

using System.Diagnostics;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.ProcessingTasks.MSGenerators;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class UIMF_TaskControllerTests
    {
        public string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";
        private string uimfParameters1 = "..\\..\\TestFiles\\uimfParameters_completeFIt_zeroFill.xml";
        private string imfFilePath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000.Accum_1202.IMF";
        private string imfParameters1 = "..\\..\\TestFiles\\imfParameters_completeFIt_zeroFill.xml";

        [Test]
        public void test1()
        {

            Project.Reset();
            Project project = Project.getInstance();

            UIMFRun run = new UIMFRun(uimfFilepath,1200, 1202);
            project.RunCollection.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 1, 1);
            sscc.Create();

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, run.MinFrame, run.MaxFrame, 1, 1);
            fscc.Create();

            Task msGen = new UIMF_MSGenerator(200,2000);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            Task rapidDecon = new RapidDeconvolutor();

            Task scanResultsUpdater = new ScanResultUpdater();

            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(rapidDecon);
            project.TaskCollection.TaskList.Add(scanResultsUpdater);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new UIMF_TaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);
            sw.Stop();

            Console.WriteLine("Time required (ms) = " + sw.ElapsedMilliseconds);
            Console.WriteLine("Scans analyzed = " + project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("Features found = " + project.RunCollection[0].ResultCollection.ResultList.Count);


            Assert.AreEqual(1, Project.getInstance().RunCollection.Count);
            Assert.AreEqual(4, project.TaskCollection.TaskList.Count);

            Assert.AreEqual(600, project.RunCollection[0].ScanSetCollection.ScanSetList.Count);

            UIMFRun uimfRun = (UIMFRun)(project.RunCollection[0]);
            Assert.AreEqual(3, uimfRun.FrameSetCollection.FrameSetList.Count);


            StringBuilder sb = new StringBuilder();

            foreach (IsosResult result in project.RunCollection[0].ResultCollection.ResultList)
            {
                Assert.IsInstanceOfType(typeof(UIMFIsosResult), result);
                UIMFIsosResult uimfResult = (UIMFIsosResult)result;
                sb.Append(uimfResult.FrameSet.PrimaryFrame);
                sb.Append("\t");
                sb.Append(uimfResult.ScanSet.PrimaryScanNumber);
                sb.Append("\t"); 
                sb.Append(uimfResult.IsotopicProfile.Peaklist[0].MZ);
                sb.Append("\t"); 
                sb.Append(uimfResult.IsotopicProfile.GetAbundance());
                sb.Append("\t");
                //sb.Append(uimfResult.IsotopicProfile.
                sb.Append(Environment.NewLine);
            }
            Console.Write(sb.ToString());

        }


        [Test]
        public void multipleFrames_horn_test1()
        {

            Project.Reset();
            Project project = Project.getInstance();
            project.LoadOldDecon2LSParameters(uimfParameters1);

            UIMFRun run = new UIMFRun(uimfFilepath, 1201, 1203);
            project.RunCollection.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 9, 1);
            sscc.Create();

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, run.MinFrame, run.MaxFrame, 1, 1);
            fscc.Create();

            Task msGen = new UIMF_MSGenerator(0, 5000);
            Task zeroFiller = new DeconToolsZeroFiller(3);
            
            Task peakDetector = new DeconToolsPeakDetector(project.Parameters.OldDecon2LSParameters.PeakProcessorParameters);

            Task decon = new HornDeconvolutor(project.Parameters.OldDecon2LSParameters.HornTransformParameters);

            Task scanResultsUpdater = new ScanResultUpdater();

            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(zeroFiller);

            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(decon);
            project.TaskCollection.TaskList.Add(scanResultsUpdater);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new UIMF_TaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);
            sw.Stop();

            Console.WriteLine("Time required (ms) = " + sw.ElapsedMilliseconds);
            Console.WriteLine("Scans analyzed = " + project.RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("Features found = " + project.RunCollection[0].ResultCollection.ResultList.Count);
            Console.WriteLine("Peaks found = " + project.RunCollection[0].ResultCollection.ScanResultList[0].NumPeaks);


           

            StringBuilder sb = new StringBuilder();


        }

        [Test]
        public void multipleFrames_horn_test2()
        {


            List<Run> runCollection = new List<Run>();

            UIMFRun run = new UIMFRun(uimfFilepath, 1201, 1203);
            runCollection.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 9, 1);
            sscc.Create();

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, run.MinFrame, run.MaxFrame, 1, 1);
            fscc.Create();

            Task msGen = new UIMF_MSGenerator(0, 5000);
            Task zeroFiller = new DeconToolsZeroFiller(3);


            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 4;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;


            Task peakDetector = new DeconToolsPeakDetector(detectorParams);


            DeconToolsV2.HornTransform.clsHornTransformParameters hornParams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            hornParams.CompleteFit = true;
            hornParams.LeftFitStringencyFactor = 2.5;
            hornParams.RightFitStringencyFactor = 0.5;
            hornParams.PeptideMinBackgroundRatio = 4;
            hornParams.MaxFit = 0.4;
            hornParams.UseMZRange = false;
            hornParams.UseMercuryCaching = true;
            
            Task decon = new HornDeconvolutor(hornParams);

            Task scanResultsUpdater = new ScanResultUpdater();

            TaskCollection tc = new TaskCollection();
            tc.TaskList.Add(msGen);
            tc.TaskList.Add(zeroFiller);

            tc.TaskList.Add(peakDetector);
            tc.TaskList.Add(decon);
            tc.TaskList.Add(scanResultsUpdater);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new UIMF_TaskController(tc);
            controller.Execute(runCollection);
            sw.Stop();

            Console.WriteLine("Time required (ms) = " + sw.ElapsedMilliseconds);
            Console.WriteLine("Scans analyzed = " + runCollection[0].ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("Features found = " + runCollection[0].ResultCollection.ResultList.Count);
            Console.WriteLine("Peaks found = " + runCollection[0].ResultCollection.ScanResultList[0].NumPeaks);




            StringBuilder sb = new StringBuilder();


        }

        [Test]
        public void imf_multipleFrames_horn_test2()
        {


            List<Run> runCollection = new List<Run>();

            Run run = new IMFRun(imfFilePath);
            runCollection.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 9, 1);
            sscc.Create();

        

            Task msGen = new GenericMSGenerator(0, 5000);
            Task zeroFiller = new DeconToolsZeroFiller(3);


            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 4;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;


            Task peakDetector = new DeconToolsPeakDetector(detectorParams);


            DeconToolsV2.HornTransform.clsHornTransformParameters hornParams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            hornParams.CompleteFit = true;
            hornParams.LeftFitStringencyFactor = 2.5;
            hornParams.RightFitStringencyFactor = 0.5;
            hornParams.PeptideMinBackgroundRatio = 4;
            hornParams.MaxFit = 0.4;
            

            Task decon = new HornDeconvolutor(hornParams);

            Task scanResultsUpdater = new ScanResultUpdater();

            TaskCollection tc = new TaskCollection();
            tc.TaskList.Add(msGen);
            tc.TaskList.Add(zeroFiller);

            tc.TaskList.Add(peakDetector);
            tc.TaskList.Add(decon);
            tc.TaskList.Add(scanResultsUpdater);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new BasicTaskController(tc);
            controller.Execute(runCollection);
            sw.Stop();

            Console.WriteLine("Time required (ms) = " + sw.ElapsedMilliseconds);
            Console.WriteLine("Scans analyzed = " + runCollection[0].ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("Features found = " + runCollection[0].ResultCollection.ResultList.Count);
            Console.WriteLine("Peaks found = " + runCollection[0].ResultCollection.ScanResultList.Sum(p => p.NumPeaks));




            StringBuilder sb = new StringBuilder();


        }

        [Test]
        public void imf_projectRef_horn_test2()
        {

            Project.Reset();
            //Project.getInstance().RunCollection = new List<Run>();


           

            Run run = new IMFRun(imfFilePath);
            Project.getInstance().RunCollection.Add(run);
            Project.getInstance().LoadOldDecon2LSParameters(imfParameters1);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 9, 1);
            sscc.Create();



            Task msGen = new GenericMSGenerator(0, 5000);
            Task zeroFiller = new DeconToolsZeroFiller(3);


            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 4;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;


            Task peakDetector = new DeconToolsPeakDetector(detectorParams);


            DeconToolsV2.HornTransform.clsHornTransformParameters hornParams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            hornParams.CompleteFit = true;
            hornParams.LeftFitStringencyFactor = 2.5;
            hornParams.RightFitStringencyFactor = 0.5;
            hornParams.PeptideMinBackgroundRatio = 4;
            hornParams.MaxFit = 0.4;
            hornParams.UseMZRange = false;
            hornParams.UseMercuryCaching = true;


            Task decon = new HornDeconvolutor(hornParams);

            HornDeconvolutor horn = (HornDeconvolutor)decon;



            Task scanResultsUpdater = new ScanResultUpdater();

            TaskCollection tc = new TaskCollection();
            tc.TaskList.Add(msGen);
            tc.TaskList.Add(zeroFiller);

            tc.TaskList.Add(peakDetector);
            tc.TaskList.Add(decon);
            tc.TaskList.Add(scanResultsUpdater);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new BasicTaskController(tc);
            controller.Execute(Project.getInstance().RunCollection);
            sw.Stop();

            Console.WriteLine("Time required (ms) = " + sw.ElapsedMilliseconds);
            Console.WriteLine("Scans analyzed = " + Project.getInstance().RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("Features found = " + Project.getInstance().RunCollection[0].ResultCollection.ResultList.Count);
            Console.WriteLine("Peaks found = " + Project.getInstance().RunCollection[0].ResultCollection.ScanResultList.Sum(p => p.NumPeaks));




            StringBuilder sb = new StringBuilder();


        }

        [Test]
        public void multipleFrames_horn_test3()
        {

            Project.Reset();

            Project.getInstance().LoadOldDecon2LSParameters(uimfParameters1);

            UIMFRun run = new UIMFRun(uimfFilepath, 1202, 1202);
            Project.getInstance().RunCollection.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 9, 1);
            sscc.Create();

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, run.MinFrame, run.MaxFrame, 1, 1);
            fscc.Create();

            Task msGen = new UIMF_MSGenerator(0, 5000);
            Task zeroFiller = new DeconToolsZeroFiller(3);

            Task peakDetector = new DeconToolsPeakDetector(Project.getInstance().Parameters.OldDecon2LSParameters.PeakProcessorParameters);

            Task decon = new HornDeconvolutor(Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters);

            Task scanResultsUpdater = new ScanResultUpdater();

            Project.getInstance().TaskCollection.TaskList.Add(msGen);
            Project.getInstance().TaskCollection.TaskList.Add(zeroFiller);

            Project.getInstance().TaskCollection.TaskList.Add(peakDetector);
            Project.getInstance().TaskCollection.TaskList.Add(decon);
            Project.getInstance().TaskCollection.TaskList.Add(scanResultsUpdater);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new UIMF_TaskController(Project.getInstance().TaskCollection);
            controller.Execute(Project.getInstance().RunCollection);
            sw.Stop();

            Console.WriteLine("Time required (ms) = " + sw.ElapsedMilliseconds);
            Console.WriteLine("Scans analyzed = " + Project.getInstance().RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("Features found = " + Project.getInstance().RunCollection[0].ResultCollection.ResultList.Count);
            Console.WriteLine("Peaks found = " + Project.getInstance().RunCollection[0].ResultCollection.ScanResultList[0].NumPeaks);




            StringBuilder sb = new StringBuilder();


        }


        [Test]
        public void multipleFrames_horn_test4()
        {
            Project.Reset();

            //Project.getInstance().LoadOldDecon2LSParameters(uimfParameters1);

            UIMFRun run = new UIMFRun(uimfFilepath, 1202, 1202);
            Project.getInstance().RunCollection.Add(run);


            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 9, 1);
            sscc.Create();

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, run.MinFrame, run.MaxFrame, 1, 1);
            fscc.Create();

            Task msGen = new UIMF_MSGenerator(0, 5000);
            Task zeroFiller = new DeconToolsZeroFiller(3);


            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 4;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;


            Task peakDetector = new DeconToolsPeakDetector(detectorParams);


            DeconToolsV2.HornTransform.clsHornTransformParameters hornParams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            hornParams.CompleteFit = true;
            hornParams.LeftFitStringencyFactor = 2.5;
            hornParams.RightFitStringencyFactor = 0.5;
            hornParams.PeptideMinBackgroundRatio = 4;
            hornParams.MaxFit = 0.4;
            hornParams.UseMZRange = false;
            hornParams.UseMercuryCaching = true;

            Task decon = new HornDeconvolutor(hornParams);

            Task scanResultsUpdater = new ScanResultUpdater();

            Project.getInstance().TaskCollection.TaskList.Add(msGen);
            Project.getInstance().TaskCollection.TaskList.Add(zeroFiller);

            Project.getInstance().TaskCollection.TaskList.Add(peakDetector);
            Project.getInstance().TaskCollection.TaskList.Add(decon);
            Project.getInstance().TaskCollection.TaskList.Add(scanResultsUpdater);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskController controller = new UIMF_TaskController(Project.getInstance().TaskCollection);
            controller.Execute(Project.getInstance().RunCollection);
            sw.Stop();

            Console.WriteLine("Time required (ms) = " + sw.ElapsedMilliseconds);
            Console.WriteLine("Scans analyzed = " + Project.getInstance().RunCollection[0].ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("Features found = " + Project.getInstance().RunCollection[0].ResultCollection.ResultList.Count);
            Console.WriteLine("Peaks found = " + Project.getInstance().RunCollection[0].ResultCollection.ScanResultList[0].NumPeaks);




            StringBuilder sb = new StringBuilder();


        }


    }
}
