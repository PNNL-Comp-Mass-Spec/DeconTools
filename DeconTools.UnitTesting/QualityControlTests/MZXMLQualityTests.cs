using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;


namespace DeconTools.UnitTesting.QualityControlTests
{
    [TestFixture]
    public class MZXMLQualityTests
    {
        private string mzxmlfile1 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_Scans6000-7000.mzXML";
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        [Test]
        public void test1()
        {

            Run xcalRun = new XCaliburRun(xcaliburTestfile, 6000, 6020);
            Run mzxmlRun = new MZXMLRun(mzxmlfile1, 1, 21);

            List<Run> runCollection = new List<Run>();
            runCollection.Add(xcalRun);
            runCollection.Add(mzxmlRun);

            int numScansSummed = 1;

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(xcalRun, xcalRun.MinScan, xcalRun.MaxScan, numScansSummed, 1,false);
            scanSetCreator.Create();

            scanSetCreator = new ScanSetCollectionCreator(mzxmlRun, mzxmlRun.MinScan, mzxmlRun.MaxScan, numScansSummed, 1);
            scanSetCreator.Create();

            Task msgen = new GenericMSGenerator();
            DeconToolsV2.Peaks.clsPeakProcessorParameters detParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detParams.PeakBackgroundRatio = 3;
            detParams.SignalToNoiseThreshold = 3;

            Task peakDet = new DeconToolsPeakDetector(detParams);
            Task horn = new HornDeconvolutor();

            TaskCollection taskColl = new TaskCollection();
            taskColl.TaskList.Add(msgen);
            taskColl.TaskList.Add(peakDet);
            taskColl.TaskList.Add(horn);

            TaskController taskController = new BasicTaskController(taskColl);
            taskController.Execute(runCollection);

            Assert.AreEqual(true, TestUtilities.AreIsosResultsTheSame(xcalRun.ResultCollection.ResultList, mzxmlRun.ResultCollection.ResultList));

        }


    }
}
