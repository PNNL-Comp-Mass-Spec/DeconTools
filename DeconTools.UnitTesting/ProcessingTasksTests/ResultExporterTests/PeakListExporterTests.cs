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
using System.Diagnostics;
using System.Data.Common;


namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class PeakListExporterTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private string outputFilename = "..\\..\\TestFiles\\PeakListExporterOutputTest1.txt";
        private string sqliteOutfileName = "..\\..\\TestFiles\\PeakListExporterOutputTest2.sqlite";

        [Test]
        public void sqliteNET_test1()
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=" + sqliteOutfileName;
                cnn.Open();
                Assert.AreEqual(System.Data.ConnectionState.Open, cnn.State);
            }
           
        }

        [Test]
        public void exportToSqliteTest1()
        {
            if (File.Exists(sqliteOutfileName)) File.Delete(sqliteOutfileName);

            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, 6000, 6020, 1, 1);
            scansetCreator.Create();

            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            ((DeconToolsPeakDetector)peakDetector).StorePeakData = true;

            Task exporter = new SqlitePeakListExporter(sqliteOutfileName, 1000000);     //trigger of 1E5 = 310 sec (memory = 150 MB);    trigger of 1E6 =  231 Sec (memory = 250 MB); 

            foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);

                Stopwatch sw = new Stopwatch();
                sw.Start();
                exporter.Execute(run.ResultCollection);
                sw.Stop();
                if (sw.ElapsedMilliseconds > 5) Console.WriteLine("PeakListExporter execution time = " + sw.ElapsedMilliseconds);
            }

            exporter.Cleanup();
            Assert.AreEqual(true, File.Exists(sqliteOutfileName));

            FileInfo fi = new FileInfo(sqliteOutfileName);
            Assert.AreEqual(137216, fi.Length);
            Console.Write(fi.Length);


        }

        [Test]
        public void exportToTextFileTest1()
        {
            if (File.Exists(outputFilename)) File.Delete(outputFilename);

            Run run = new XCaliburRun(xcaliburTestfile);

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, 6000, 6020, 1, 1);
            scansetCreator.Create();

            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            ((DeconToolsPeakDetector)peakDetector).StorePeakData = true;

            Task exporter = new DeconTools.Backend.ProcessingTasks.PeakListExporters.PeakListTextExporter(outputFilename, 1000000);     //trigger of 1E5 = 310 sec (memory = 150 MB);    trigger of 1E6 =  231 Sec (memory = 250 MB); 

            foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);

                Stopwatch sw = new Stopwatch();
                sw.Start();
                exporter.Execute(run.ResultCollection);
                sw.Stop();
                if (sw.ElapsedMilliseconds > 5) Console.WriteLine("PeakListExporter execution time = " + sw.ElapsedMilliseconds);
            }

            exporter.Cleanup();
            Assert.AreEqual(true, File.Exists(outputFilename));

            FileInfo fi = new FileInfo(outputFilename);
            Assert.AreEqual(174337, fi.Length);
            Console.Write(fi.Length);


        }

    }
}
