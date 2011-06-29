#define peaksAreFilteredToNarrowMZ

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Workflows;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Data;
using NUnit.Framework;
using DeconTools.Backend.DTO;
using System.Diagnostics;

namespace DeconTools.UnitTesting2.WorkflowTests
{
    public class WholisticChromBasedLCMSFeatureFinderWorkflow_tests
    {

        [Test]
        public void processOrbiWithNewWorkflowTest1()
        {

            int minScan = 200;
            int maxScan = 18400;
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1, minScan, maxScan);

            WholisticChromBasedLCMSFeatureFinderWorkflow workflow = new WholisticChromBasedLCMSFeatureFinderWorkflow(run);



            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan).ToList();
            //run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan && p.MSPeak.XValue > 771 && p.MSPeak.XValue < 775).ToList();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            workflow.ExecuteWorkflow2(run);
            sw.Stop();

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            Console.WriteLine("workflow time = " + sw.ElapsedMilliseconds);



        }

        [Test]
        public void processOrbiWithNewWorkflowTest2()
        {
            int minScan = 5500;
            int maxScan = 6500;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1, minScan, maxScan);
            WholisticChromBasedLCMSFeatureFinderWorkflow workflow = new WholisticChromBasedLCMSFeatureFinderWorkflow(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            //run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan).ToList();
            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan && p.MSPeak.XValue > 771 && p.MSPeak.XValue < 775).ToList();


            Stopwatch sw = new Stopwatch();
            sw.Start();
            workflow.Execute();
            sw.Stop();

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            Console.WriteLine("workflow time = " + sw.ElapsedMilliseconds);

        }




        [Test]
        public void processOrbi_Scans3000_6000_Test1()
        {
            int minScan = 5500;
            int maxScan = 6500;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1, minScan, maxScan);
            WholisticChromBasedLCMSFeatureFinderWorkflow workflow = new WholisticChromBasedLCMSFeatureFinderWorkflow(run);
           
            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            //run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan).ToList();

            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan && p.MSPeak.XValue > 771 && p.MSPeak.XValue < 775).ToList();


            workflow.ExecuteWorkflow2(run);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

        }




        [Test]
        public void processOrbi_Scans5500_6500_Test1()
        {
            int minScan = 5500;
            int maxScan = 6500;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1, minScan, maxScan);
            WholisticChromBasedLCMSFeatureFinderWorkflow workflow = new WholisticChromBasedLCMSFeatureFinderWorkflow(run);
           
            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan).ToList();

            Console.WriteLine(run.ResultCollection.MSPeakResultList.Count);




            // workflow.ExecuteWorkflow(run);

            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

        }






        [Test]
        public void processOrbi_Refining_Case01_whatHappensTo_PeakID_396293()
        {
            int minScan = 200;
            int maxScan = 18000;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            WholisticChromBasedLCMSFeatureFinderWorkflow workflow = new WholisticChromBasedLCMSFeatureFinderWorkflow(run);
           
            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


#if peaksAreFilteredToNarrowMZ

            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan && p.MSPeak.XValue > 771 && p.MSPeak.XValue < 775).ToList();
#else
            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan).ToList();
#endif

            workflow.Execute();

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);


            List<MSPeakResult> filteredPeakResults = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan && p.MSPeak.XValue > 771 && p.MSPeak.XValue < 775).ToList();

            //TestUtilities.DisplayMSPeakResults(filteredPeakResults);

        }



    }
}
