using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Data;
using DeconTools.Backend.Workflows;
using DeconTools.UnitTesting2;
using System.Linq;

namespace TestConsole1
{
    class Program
    {

        static void Main(string[] args)
        {
            WholisticChromBasedLCMSFeatureFinderWorkflow workflow = new WholisticChromBasedLCMSFeatureFinderWorkflow();
            int minScan = 200;
            int maxScan = 18500;

            //int minScan = 5500;
            //int maxScan = 6500;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan).ToList();
            //run.ResultCollection.MSPeakResultList = run.ResultCollection.MSPeakResultList.Where(p => p.Scan_num > minScan && p.Scan_num < maxScan && p.MSPeak.XValue > 771 && p.MSPeak.XValue < 775).ToList();


            workflow.ExecuteWorkflow(run);


        }
    }
}
