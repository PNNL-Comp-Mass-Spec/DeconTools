using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.DTO;
using System.Diagnostics;


namespace DeconTools.UnitTesting.DTO_Tests
{
    [TestFixture]
    public class OriginalIntensitiesExtractorTests
    {
        private string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";



        //[Test]
        //public void test1_summing()
        //{

        //    Project.Reset();
        //    Project project = Project.getInstance();

        //    UIMFRun run = new UIMFRun(uimfFilepath, 1200, 1200);
        //    project.RunCollection.Add(run);

        //    int numFramesSummed = 3;
        //    int numScansSummed = 3;

        //    ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 250, 270, numScansSummed, 1);
        //    sscc.Create();

        //    FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, run.MinFrame, run.MaxFrame, numFramesSummed, 1);
        //    fscc.Create();

        //    Task msGen = new UIMF_MSGenerator(200, 2000);

        //    DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
        //    detectorParams.PeakBackgroundRatio = 3;
        //    detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
        //    detectorParams.SignalToNoiseThreshold = 3;
        //    Task peakDetector = new DeconToolsPeakDetector(detectorParams);

        //    Task decon = new HornDeconvolutor();

        //    Task scanResultsUpdater = new ScanResultUpdater();

        //    project.TaskCollection.TaskList.Add(msGen);
        //    project.TaskCollection.TaskList.Add(peakDetector);
        //    project.TaskCollection.TaskList.Add(decon);
        //    project.TaskCollection.TaskList.Add(scanResultsUpdater);
                
        //    TaskController controller = new UIMF_TaskController(project.TaskCollection);
        //    controller.Execute(project.RunCollection);

        //    ResultCollection results = project.RunCollection[0].ResultCollection;


        //    OriginalIntensitiesExtractor origIntensExtractor = new OriginalIntensitiesExtractor(results);

        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();
        //    List<OriginalIntensitiesDTO>data= origIntensExtractor.ExtractOriginalIntensities();
        //    sw.Stop();

        //    StringBuilder sb = new StringBuilder();
        //    sb.Append("\n\nOrigIntensityExtractor. Time taken = " + (sw.ElapsedMilliseconds/data.Count).ToString() + " milliseconds per result\n");

        //    Assert.AreEqual(202, results.ResultList.Count);
        //    Assert.AreEqual(202, data.Count);

        //    reportOriginalIntensityData(sb,results.ResultList, data);
        //    Console.Write(sb.ToString());

        //}

        [Test]
        public void test1_summing()
        {

            Project.Reset();
            Project project = Project.getInstance();

            UIMFRun run = new UIMFRun(uimfFilepath, 1201, 1201);
            project.RunCollection.Add(run);

            int numFramesSummed = 3;
            int numScansSummed = 3;

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 250, 270, numScansSummed, 1);
            sscc.Create();

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, run.MinFrame, run.MaxFrame, numFramesSummed, 1);
            fscc.Create();

            Task msGen = new UIMF_MSGenerator(200, 2000);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            Task decon = new HornDeconvolutor();

            Task scanResultsUpdater = new ScanResultUpdater();

            Task originalIntensitiesExtractor = new OriginalIntensitiesExtractor();


            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(decon);
            project.TaskCollection.TaskList.Add(scanResultsUpdater);
            project.TaskCollection.TaskList.Add(originalIntensitiesExtractor);

            TaskController controller = new UIMF_TaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);

            ResultCollection rc = project.RunCollection[0].ResultCollection;
            Assert.AreEqual(180, rc.ResultList.Count);

            StringBuilder sb = new StringBuilder();

            IsosResultUtilities.DisplayResults(sb, rc.ResultList);
            Console.Write(sb.ToString());
        }

        [Test]
        public void test1_Nosumming()
        {

            Project.Reset();
            Project project = Project.getInstance();

            UIMFRun run = new UIMFRun(uimfFilepath, 1201, 1201);
            project.RunCollection.Add(run);

            int numFramesSummed = 1;
            int numScansSummed = 1;

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 250, 270, numScansSummed, 1);
            sscc.Create();

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, run.MinFrame, run.MaxFrame, numFramesSummed, 1);
            fscc.Create();

            Task msGen = new UIMF_MSGenerator(200, 2000);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            Task decon = new HornDeconvolutor();

            Task scanResultsUpdater = new ScanResultUpdater();

            Task originalIntensitiesExtractor = new OriginalIntensitiesExtractor();


            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(decon);
            project.TaskCollection.TaskList.Add(scanResultsUpdater);
            project.TaskCollection.TaskList.Add(originalIntensitiesExtractor);

            TaskController controller = new UIMF_TaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);

            ResultCollection rc = project.RunCollection[0].ResultCollection;
            //Assert.AreEqual(20, rc.ResultList.Count);

            StringBuilder sb = new StringBuilder();

            IsosResultUtilities.DisplayResults(sb, rc.ResultList);
            Console.Write(sb.ToString());
        }


        /// <summary>
        /// This test ensures that the orig_intens data is the same as the data stored in the IsosResult when there is no summing
        /// </summary>
        //[Test]
        //public void test1_nosumming()
        //{

        //    Project.Reset();
        //    Project project = Project.getInstance();

        //    UIMFRun run = new UIMFRun(uimfFilepath, 1200, 1200);
        //    project.RunCollection.Add(run);

        //    ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 250, 270, 1, 1);
        //    sscc.Create();

        //    FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, run.MinFrame, run.MaxFrame, 1, 1);
        //    fscc.Create();

        //    Task msGen = new UIMF_MSGenerator(200, 2000);

        //    DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
        //    detectorParams.PeakBackgroundRatio = 3;
        //    detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
        //    detectorParams.SignalToNoiseThreshold = 3;
        //    detectorParams.ThresholdedData = false;
        //    Task peakDetector = new DeconToolsPeakDetector(detectorParams);

        //    Task decon = new HornDeconvolutor();

        //    Task scanResultsUpdater = new ScanResultUpdater();

        //    project.TaskCollection.TaskList.Add(msGen);
        //    project.TaskCollection.TaskList.Add(peakDetector);
        //    project.TaskCollection.TaskList.Add(decon);
        //    project.TaskCollection.TaskList.Add(scanResultsUpdater);

        //    TaskController controller = new UIMF_TaskController(project.TaskCollection);
        //    controller.Execute(project.RunCollection);

        //    ResultCollection results = project.RunCollection[0].ResultCollection;


        //    OriginalIntensitiesExtractor origIntensExtractor = new OriginalIntensitiesExtractor(results);
        //    List<OriginalIntensitiesDTO> data = origIntensExtractor.ExtractOriginalIntensities();

        //    Assert.AreEqual(20, results.ResultList.Count);
        //    Assert.AreEqual(20, data.Count);

        //    //for (int i = 0; i < data.Count; i++)
        //    //{
        //    //    Assert.AreEqual(results.ResultList[i].IsotopicProfile.GetAbundance(), data[i].originalIntensity);
        //    //    Assert.AreEqual(results.ResultList[i].IsotopicProfile.GetSummedIntensity(), data[i].totIsotopicOrginalIntens);
                
        //    //}

        //    StringBuilder sb = new StringBuilder();
        //    reportOriginalIntensityData(sb, results.ResultList, data);
        //    Console.Write(sb.ToString());

        //}


        private void reportOriginalIntensityData(StringBuilder sb, List<IsosResult> results, List<OriginalIntensitiesDTO> origIntensdata)
        {

            int counter=0;
            sb.Append("scan\tmz\tabundance\tTIA\torig_intens\torigIntenseTIA\n");

            foreach (OriginalIntensitiesDTO item in origIntensdata)
            {
                UIMFIsosResult result=(UIMFIsosResult)results[counter];
                sb.Append(result.ScanSet.PrimaryScanNumber);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.GetMZ());
                sb.Append("\t"); 
                sb.Append(result.IsotopicProfile.GetAbundance());
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.GetSummedIntensity());
                sb.Append("\t");
                sb.Append(item.originalIntensity);
                sb.Append("\t");
                sb.Append(item.totIsotopicOrginalIntens);
                sb.Append("\n");

                counter++;
            }
        }

        private void reportOriginalIntensityData(StringBuilder sb, List<OriginalIntensitiesDTO> data)
        {
           
        }


    }
}
