using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;


namespace DeconTools.UnitTesting.DTO_Tests
{
    [TestFixture]
    public class IsoResultComparerTests
    {
        private string xcaliburThrashIsos1 = "..\\..\\TestFiles\\thrashTestIsos1.csv";
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        private string isosByThrash1 = "..\\..\\TestFiles\\isosSample_thrash_isos.csv";
        private string isosByRapid1 = "..\\..\\TestFiles\\isosSample_rapid_isos.csv";

        private string rawIsosCacheOnCompleteFitOFF = @"D:\Tickets\DeconTools\TicketXX_Thrash_Cache_CompleteFit_Issues\Data_Analysis\Test01_cacheON_completeFitOFF\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_isos.csv";
        private string rawIsosCacheOFFCompleteFitOFF = @"D:\Tickets\DeconTools\TicketXX_Thrash_Cache_CompleteFit_Issues\Data_Analysis\Test02_cacheOFF_completeFitOFF\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_isos.csv";


        [Test]
        public void test1()
        {

            IsosResultComparer comparer = new IsosResultComparer();

            IsosResultUtilities isosUtil = new IsosResultUtilities();
            isosUtil.LoadResults(xcaliburThrashIsos1, DeconTools.Backend.Globals.MSFileType.Finnigan);

            IsosResultUtilities isosUtil2 = new IsosResultUtilities();
            isosUtil2.LoadResults(xcaliburThrashIsos1, DeconTools.Backend.Globals.MSFileType.Finnigan);


            List<IsosResult> resultList1 = isosUtil.Results.Where(p => p.ScanSet.PrimaryScanNumber ==6005).ToList() ;
            List<IsosResult> resultList2 = isosUtil2.Results.Where(p => p.ScanSet.PrimaryScanNumber == 6005).ToList(); ;

            Assert.AreEqual(86, resultList1.Count);
            Assert.AreEqual(86, resultList2.Count);

            resultList1.RemoveAt(30);
            resultList1.RemoveAt(30);
            resultList1.RemoveAt(30);

            resultList2.RemoveAt(0);
            resultList2.RemoveAt(0);
            resultList2.RemoveAt(0);
            resultList2.RemoveAt(0);
            Assert.AreEqual(86 - 4, resultList2.Count);
            Assert.AreEqual(86 - 3, resultList1.Count);

            List<IsosResult>intersectedList =  resultList1.Intersect(resultList2, comparer).ToList();
            Assert.AreEqual(86 - 7, intersectedList.Count);

        }

        [Test]
        public void test2()
        {

            IsosResultComparer comparer = new IsosResultComparer();

            IsosResultUtilities isosUtil = new IsosResultUtilities();
            isosUtil.LoadResults(xcaliburThrashIsos1, DeconTools.Backend.Globals.MSFileType.Finnigan);

            IsosResultUtilities isosUtil2 = new IsosResultUtilities();
            isosUtil2.LoadResults(xcaliburThrashIsos1, DeconTools.Backend.Globals.MSFileType.Finnigan);


            List<IsosResult> resultList1 = isosUtil.Results.Where(p => p.ScanSet.PrimaryScanNumber == 6005).ToList();
            List<IsosResult> resultList2 = isosUtil2.Results.Where(p => p.ScanSet.PrimaryScanNumber == 6005).ToList(); ;

            Assert.AreEqual(86, resultList1.Count);
            Assert.AreEqual(86, resultList2.Count);

            resultList1.RemoveAt(30);
            resultList1.RemoveAt(30);
            resultList1.RemoveAt(30);

            resultList2.RemoveAt(0);
            resultList2.RemoveAt(0);
            resultList2.RemoveAt(0);
            resultList2.RemoveAt(0);
            Assert.AreEqual(86 - 4, resultList2.Count);
            Assert.AreEqual(86 - 3, resultList1.Count);

            List<IsosResult> intersectedList = comparer.GetIntersectionBetweenTwoIsosResultSets(resultList1, resultList2);
            Assert.AreEqual(86 - 7, intersectedList.Count);

        }

        [Test]
        public void compareThrashRapidTest1()
        {
            IsosResultComparer comparer = new IsosResultComparer();

            IsosResultUtilities isosUtil = new IsosResultUtilities();
            isosUtil.LoadResults(isosByThrash1, DeconTools.Backend.Globals.MSFileType.Finnigan);

            IsosResultUtilities isosUtil2 = new IsosResultUtilities();
            isosUtil2.LoadResults(isosByRapid1, DeconTools.Backend.Globals.MSFileType.Finnigan);


            List<IsosResult> rapidResults = isosUtil2.Results;
            List<IsosResult> thrashResults = isosUtil.Results;

            Console.WriteLine("Thrash result count = " + thrashResults.Count);
            Console.WriteLine("RAPID result count = " + rapidResults.Count);



            List<IsosResult>intersectedResults=  comparer.GetIntersectionBetweenTwoIsosResultSets(thrashResults, rapidResults);
            //List<IsosResult> intersectedResults2 = comparer.GetIntersectionTheManualWay(thrashResults, rapidResults);
            //List<IsosResult> intersectedResults3 = comparer.GetIntersectionTheManualWay(rapidResults, thrashResults);

            List<IsosResult> unmatchedResults1 = comparer.GetUniqueBetweenTwoIsosResultSets(rapidResults, thrashResults);
            List<IsosResult> unmatchedResults2 = comparer.GetUniqueBetweenTwoIsosResultSets(thrashResults, rapidResults);



            Console.WriteLine("Intersected result count = " + intersectedResults.Count);
            //Console.WriteLine("Manual Intersected result count = " + intersectedResults2.Count);
            //Console.WriteLine("Manual Intersected result count = " + intersectedResults3.Count);

            Console.WriteLine("unmatched result count = " + unmatchedResults1.Count);
            Console.WriteLine("unmatched result count2 = " + unmatchedResults2.Count);


        }


        [Test]
        public void compareThrashRapidTest2()
        {
            IsosResultComparer comparer = new IsosResultComparer();
            Run run = new XCaliburRun(xcaliburTestfile, 6000, 6050);
            Project.getInstance().RunCollection.Add(run);


            ResultCollection results = new ResultCollection(run);
            TaskCollection taskCollection = new TaskCollection();

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1);
            scansetCreator.Create();


            Task msgen = new GenericMSGenerator();

            DeconToolsV2.Peaks.clsPeakProcessorParameters peakDetParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            peakDetParams.SignalToNoiseThreshold = 3;
            peakDetParams.PeakBackgroundRatio = 0.5;
            Task peakDetector = new DeconToolsPeakDetector(peakDetParams);

            Task thrashDecon = new HornDeconvolutor();
            Task rapidDecon = new RapidDeconvolutor();

            taskCollection.TaskList.Add(msgen);
            taskCollection.TaskList.Add(peakDetector);
            taskCollection.TaskList.Add(thrashDecon);


            TaskController taskcontroller = new BasicTaskController(taskCollection);
            taskcontroller.Execute(Project.getInstance().RunCollection);

            Assert.AreEqual(644, Project.getInstance().RunCollection[0].ResultCollection.ResultList.Count);
            List<IsosResult> thrashResults = new List<IsosResult>(Project.getInstance().RunCollection[0].ResultCollection.ResultList);


            taskCollection.TaskList.Remove(thrashDecon);
            taskCollection.TaskList.Add(rapidDecon);

            Project.getInstance().RunCollection[0].ResultCollection.ResultList.Clear();

            taskcontroller.Execute(Project.getInstance().RunCollection);
            Assert.AreEqual(2472, Project.getInstance().RunCollection[0].ResultCollection.ResultList.Count);


          
            List<IsosResult> rapidResults = new List<IsosResult>(Project.getInstance().RunCollection[0].ResultCollection.ResultList);
            Console.WriteLine("Thrash result count = " + thrashResults.Count);
            Console.WriteLine("RAPID result count = " + rapidResults.Count);


            List<IsosResult> intersectedResults = comparer.GetIntersectionBetweenTwoIsosResultSets(thrashResults, rapidResults);
            List<IsosResult> intersectedResults2 = comparer.GetIntersectionTheManualWay(thrashResults, rapidResults);
            //List<IsosResult> intersectedResults3 = comparer.GetIntersectionTheManualWay(rapidResults, thrashResults);

            List<IsosResult> unmatchedResults1 = comparer.GetUniqueBetweenTwoIsosResultSets(rapidResults, thrashResults);
            List<IsosResult> unmatchedResults2 = comparer.GetUniqueBetweenTwoIsosResultSets(thrashResults, rapidResults);

            Console.WriteLine("Intersected result count = " + intersectedResults.Count);
            Console.WriteLine("Manual Intersected result count = " + intersectedResults2.Count);
            //Console.WriteLine("Manual Intersected result count = " + intersectedResults3.Count);

            Console.WriteLine("unmatched result count = " + unmatchedResults1.Count);
            Console.WriteLine("unmatched result count2 = " + unmatchedResults2.Count);

        }


        [Test]
        public void compareRAWData_cacheONvs_cacheOffTest1()
        {
            IsosResultComparer comparer = new IsosResultComparer();

            IsosResultUtilities isosUtil = new IsosResultUtilities();
            isosUtil.LoadResults(rawIsosCacheOnCompleteFitOFF, DeconTools.Backend.Globals.MSFileType.Finnigan);

            IsosResultUtilities isosUtil2 = new IsosResultUtilities();
            isosUtil2.LoadResults(rawIsosCacheOFFCompleteFitOFF, DeconTools.Backend.Globals.MSFileType.Finnigan);


            List<IsosResult> cacheOnResults = isosUtil.Results;
            List<IsosResult> cacheOffResults = isosUtil2.Results;

            Console.WriteLine("cache on count = " + cacheOnResults.Count);
            Console.WriteLine("cache off count = " + cacheOffResults.Count);



            List<IsosResult> intersectedResults = comparer.GetIntersectionBetweenTwoIsosResultSets(cacheOffResults, cacheOnResults);

            List<IsosResult> unmatchedResults1 = comparer.GetUniqueBetweenTwoIsosResultSets(cacheOnResults, cacheOffResults);
            List<IsosResult> unmatchedResults2 = comparer.GetUniqueBetweenTwoIsosResultSets(cacheOffResults, cacheOnResults);

            List<IsosResult> intersectedResults2 = comparer.GetIntersectionTheManualWay(cacheOnResults, cacheOffResults);
            //List<IsosResult> intersectedResults3 = comparer.GetIntersectionTheManualWay(rapidResults, thrashResults);

            Console.WriteLine("Intersected result count = " + intersectedResults.Count);
            Console.WriteLine("unmatched result count = " + unmatchedResults1.Count);
            Console.WriteLine("unmatched result count2 = " + unmatchedResults2.Count);
            Console.WriteLine("Manual Intersected result count = " + intersectedResults2.Count);
            //Console.WriteLine("Manual Intersected result count = " + intersectedResults3.Count);

          
        }


        [Test]
        public void getUniqueTest1()
        {


        }


    }
}
