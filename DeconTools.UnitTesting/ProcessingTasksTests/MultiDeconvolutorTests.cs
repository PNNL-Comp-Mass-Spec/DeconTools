using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class MultiDeconvolutorTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        [Test]
        public void simpleAddTest1()
        {


            //first deconvolute with THRASH
            //then deconvolute with RAPID, using comboMode = 'simplyAddIt'
            //so there will be duplicate results
            Project.Reset();

            Run run = new XCaliburRun(xcaliburTestfile, 6000, 6050);
            Project.getInstance().RunCollection.Add(run);
            

            ResultCollection results = new ResultCollection(run);
            TaskCollection taskCollection = new TaskCollection();

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1, false);
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


            taskCollection.TaskList.Remove(thrashDecon);
            taskCollection.TaskList.Add(rapidDecon);

            Project.getInstance().RunCollection[0].ResultCollection.ResultList.Clear();

            taskcontroller.Execute(Project.getInstance().RunCollection);
            Assert.AreEqual(2472, Project.getInstance().RunCollection[0].ResultCollection.ResultList.Count);

            Project.getInstance().RunCollection[0].ResultCollection.ResultList.Clear();
            taskCollection.TaskList.Remove(rapidDecon);
            taskCollection.TaskList.Add(thrashDecon);
            taskCollection.TaskList.Add(rapidDecon);

            taskcontroller.Execute(Project.getInstance().RunCollection);
            Assert.AreEqual(2472 + 644, Project.getInstance().RunCollection[0].ResultCollection.ResultList.Count);

        }

        [Test]
        public void simpleAddTest2()
        {


            //first deconvolute with THRASH
            //then deconvolute with RAPID, using comboMode = 'simplyAddIt'
            //so there will be duplicate results

            Project.Reset();

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
            Task rapidDecon = new RapidDeconvolutor(DeconTools.Backend.ProcessingTasks.IDeconvolutor.DeconResultComboMode.simplyAddIt);

            taskCollection.TaskList.Add(msgen);
            taskCollection.TaskList.Add(peakDetector);
            taskCollection.TaskList.Add(thrashDecon);
            taskCollection.TaskList.Add(rapidDecon);


            TaskController taskcontroller = new BasicTaskController(taskCollection);
            taskcontroller.Execute(Project.getInstance().RunCollection);


            Assert.AreEqual(3116, Project.getInstance().RunCollection[0].ResultCollection.ResultList.Count);

        }

        [Test]
        public void addOnlyUniqueRAPIDResultsTest1()
        {


            //first deconvolute with THRASH
            //then deconvolute with RAPID, using comboMode = 'AddIfUnique'
            //so there will be duplicate results

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
            Task rapidDecon = new RapidDeconvolutor(DeconTools.Backend.ProcessingTasks.IDeconvolutor.DeconResultComboMode.addItIfUnique);

            taskCollection.TaskList.Add(msgen);
            taskCollection.TaskList.Add(peakDetector);
            taskCollection.TaskList.Add(thrashDecon);
            taskCollection.TaskList.Add(rapidDecon);


            TaskController taskcontroller = new BasicTaskController(taskCollection);
            taskcontroller.Execute(Project.getInstance().RunCollection);


            Assert.AreEqual(2525, Project.getInstance().RunCollection[0].ResultCollection.ResultList.Count);

        }


        [Test]
        public void THRASHonly()
        {

            Project.Reset();
            //first deconvolute with THRASH
            //then deconvolute with RAPID, using comboMode = 'simplyAddIt'
            //so there will be duplicate results

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


        }


    }
}
