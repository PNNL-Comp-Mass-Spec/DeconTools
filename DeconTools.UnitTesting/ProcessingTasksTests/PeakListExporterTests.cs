using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using System.IO;


namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class PeakListExporterTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private string outputFilename = "..\\..\\TestFiles\\PeakListExporterOutputTest1.txt";
        private string anujRequestFile = @"H:\MS_Data\042509_His_Pikaard_mut1_020909.raw";
        string anujOutputfilename = @"H:\MS_Data\042509_His_Pikaard_mut1_020909_peaks.txt";

        [Test]
        public void test1()
        {
            if (File.Exists(outputFilename)) File.Delete(outputFilename);

            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, 4000, 5000, 1, 1);
            scansetCreator.Create();

            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();

            StreamWriter sw = new StreamWriter(outputFilename);
            Task peakListExporter = new PeakListTextExporter(sw);

            foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);
                peakListExporter.Execute(run.ResultCollection);
            }

            peakListExporter.Cleanup();
            Assert.AreEqual(true, File.Exists(outputFilename));

            FileInfo fi = new FileInfo(outputFilename);
            Assert.AreEqual(8751480, fi.Length);

        }

        [Test]
        public void anujRequest1()
        {
            Run run = new XCaliburRun(anujRequestFile);

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, 110, 140, 1, 1);
            scansetCreator.Create();

            for (int i = 7350; i < 7380; i++)
            {
                run.ScanSetCollection.ScanSetList.Add(new ScanSet(i));

            }

            Task msgen = new GenericMSGenerator();
            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector();
            peakDetector.PeakBackgroundRatio = 1.3;
            peakDetector.SigNoiseThreshold = 2;
            peakDetector.IsDataThresholded = true;

            StreamWriter sw = new StreamWriter(anujOutputfilename);
            Task peakListExporter = new PeakListTextExporter(sw, new int[] { 1, 2 });

            foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);
                peakListExporter.Execute(run.ResultCollection);
            }
        }




    }
}
