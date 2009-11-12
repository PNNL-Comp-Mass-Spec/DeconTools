using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.Deconvoluters;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.IsosMergerExporters;


namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class IsosMergerExporterTests
    {
        string c2_blankfilePath = "..\\..\\TestFiles\\MergeTestFiles\\Blank C2 MeOH 50-1000 pos FTMS.raw";
        string c2_6FilePath = "..\\..\\TestFiles\\MergeTestFiles\\C2-6 Stock MeOH 50-1000 pos FTMS.raw";
        string c2_7FilePath = "..\\..\\TestFiles\\MergeTestFiles\\C2-7 Stock MeOH 50-1000 pos FTMS.raw";
        string c2_8FilePath = "..\\..\\TestFiles\\MergeTestFiles\\C2-8 Stock MeOH 50-1000 pos FTMS.raw";
        string c2_9FilePath = "..\\..\\TestFiles\\MergeTestFiles\\C2-9 Stock MeOH 50-1000 pos FTMS.raw";
        string c2_10FilePath = "..\\..\\TestFiles\\MergeTestFiles\\C2-10 Stock MeOH 50-1000 pos FTMS.raw";


        [Test]
        public void test1()
        {


            Run run0 = new XCaliburRun(c2_blankfilePath);
            Run run1 = new XCaliburRun(c2_6FilePath);
            Run run2 = new XCaliburRun(c2_7FilePath);
            Run run3 = new XCaliburRun(c2_8FilePath);
            Run run4 = new XCaliburRun(c2_9FilePath);
            Run run5 = new XCaliburRun(c2_10FilePath);

          


            Project.getInstance().RunCollection.Add(run0);
            Project.getInstance().RunCollection.Add(run1);
            Project.getInstance().RunCollection.Add(run2);
            Project.getInstance().RunCollection.Add(run3);
            Project.getInstance().RunCollection.Add(run4);
            Project.getInstance().RunCollection.Add(run5);

            foreach (Run run in Project.getInstance().RunCollection)
            {
                ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, 1, 1);
                scansetCreator.Create();
            }

            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new SimpleDecon();
            Task isosMergerExporter = new BasicIsosMergerExporter("..\\..\\TestFiles\\MergeTestFiles\\BasicIsosMergerExporterTest1Output_isos.csv");
            Task scansupdater = new ScanResultUpdater();
            Task scansMergerExporter = new BasicScansMergerExporter("..\\..\\TestFiles\\MergeTestFiles\\BasicIsosMergerExporterTest1Output_scans.csv");



            Project.getInstance().TaskCollection.TaskList.Add(msgen);
            Project.getInstance().TaskCollection.TaskList.Add(peakDetector);
            Project.getInstance().TaskCollection.TaskList.Add(decon);
            Project.getInstance().TaskCollection.TaskList.Add(isosMergerExporter);
            Project.getInstance().TaskCollection.TaskList.Add(scansupdater);
            Project.getInstance().TaskCollection.TaskList.Add(scansMergerExporter);

            BasicTaskController taskController = new BasicTaskController(Project.getInstance().TaskCollection);
            taskController.Execute(Project.getInstance().RunCollection);

            TaskCleaner cleaner = new TaskCleaner(Project.getInstance().TaskCollection);
            cleaner.CleanTasks();
        }


    }
}
