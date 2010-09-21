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

namespace DeconTools.UnitTesting2.WorkflowTests
{
    public class WholisticChromBasedLCMSFeatureFinderWorkflow_tests
    {

        [Test]
        public void processOrbi_Scans3000_6000_Test1()
        {
            WholisticChromBasedLCMSFeatureFinderWorkflow workflow = new WholisticChromBasedLCMSFeatureFinderWorkflow();
            int minScan = 5500;
            int maxScan = 6500;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1, minScan, maxScan);
            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan).ToList();

            

            workflow.ExecuteWorkflow(run);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

        }




        [Test]
        public void processOrbi_Scans5500_6500_Test1()
        {
            WholisticChromBasedLCMSFeatureFinderWorkflow workflow = new WholisticChromBasedLCMSFeatureFinderWorkflow();
            int minScan = 5500;
            int maxScan = 6500;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1, minScan, maxScan);
            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan).ToList();


            workflow.ExecuteWorkflow(run);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

        }



        [Test]
        public void processOrbi_Refining_Case01_whatHappensTo_PeakID_396293()
        {
            WholisticChromBasedLCMSFeatureFinderWorkflow workflow = new WholisticChromBasedLCMSFeatureFinderWorkflow();
            int minScan = 5500;
            int maxScan = 6500;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1, minScan, maxScan);
            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


#if peaksAreFilteredToNarrowMZ

            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan && p.MSPeak.XValue>771 && p.MSPeak.XValue<775).ToList();
#else
            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan).ToList();
#endif
            
            workflow.ExecuteWorkflow(run);

            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
            
            
            List<MSPeakResult>filteredPeakResults = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan && p.MSPeak.XValue>771 && p.MSPeak.XValue<775).ToList();
            
            TestUtilities.DisplayMSPeakResults(filteredPeakResults);

        }



    }
}
