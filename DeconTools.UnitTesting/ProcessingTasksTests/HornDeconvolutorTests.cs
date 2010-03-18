using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks.MSGenerators;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class HornDeconvolutorTests
    {

        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private string agilentDTestfile = @"F:\Gord\Data\AgilentD\BSA_TOF4.d";

       
        [Test]
        public void simpleTest1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, 6000, 6020, 1, 1);
            scansetCreator.Create();

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();

            foreach (var scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);
                decon.Execute(run.ResultCollection);
            }

            Assert.AreEqual(285, run.ResultCollection.ResultList.Count);
        }

        [Test]
        public void deconvoluteAgilentTest1()
        {
            Run run = new AgilentD_Run(agilentDTestfile);

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, 25, 27, 1, 1);
            scansetCreator.Create();

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();

            foreach (var scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);
                decon.Execute(run.ResultCollection);
            }

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
        }




        [Test]
        public void numPeaksUsedInAbundanceTest1()
        {
            List<Run>runColl=new List<Run>();
            Run run = new XCaliburRun(xcaliburTestfile, 6005,6005);
            runColl.Add(run);

            ScanSetCollectionCreator scanCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1);
            scanCreator.Create();

            MSGeneratorFactory msgenFactory=new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();
            ((HornDeconvolutor)decon).NumPeaksUsedInAbundance = 1;

            List<Task> taskList = new List<Task>();
            taskList.Add(msgen);
            taskList.Add(peakDetector);
            taskList.Add(decon);

            TaskCollection taskColl=new TaskCollection(taskList);

            TaskControllerFactory taskfactory=new TaskControllerFactory();
            TaskController controller = taskfactory.CreateTaskController(run.MSFileType, taskColl);

            controller.Execute(runColl);

            List<IsosResult> results = run.ResultCollection.ResultList;
            Assert.AreEqual(93, results.Count);
            Assert.AreEqual(639.822102339465m, (decimal)results[1].IsotopicProfile.MonoPeakMZ);
            Assert.AreEqual(9063344, results[1].IsotopicProfile.IntensityAggregate);
            Assert.AreEqual(9063344, results[1].IsotopicProfile.GetAbundance());

            run.ResultCollection.ClearAllResults();

            ((HornDeconvolutor)decon).NumPeaksUsedInAbundance = 2;
            controller.Execute(runColl);
            Assert.AreEqual(93, results.Count);
            Assert.AreEqual(639.822102339465m, (decimal)results[1].IsotopicProfile.MonoPeakMZ);
            Assert.AreEqual(14725979, results[1].IsotopicProfile.IntensityAggregate);
            Assert.AreEqual(14725979, results[1].IsotopicProfile.GetAbundance());


            run.ResultCollection.ClearAllResults();

            ((HornDeconvolutor)decon).NumPeaksUsedInAbundance = 3;
            controller.Execute(runColl);
            Assert.AreEqual(93, results.Count);
            Assert.AreEqual(639.822102339465m, (decimal)results[1].IsotopicProfile.MonoPeakMZ);
            Assert.AreEqual(16666043, results[1].IsotopicProfile.IntensityAggregate);
            Assert.AreEqual(16666043, results[1].IsotopicProfile.GetAbundance());
        }
    }
}