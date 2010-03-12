using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using System.Diagnostics;
using System.IO;
using DeconTools.Backend.ProcessingTasks.ResultValidators;

namespace DeconTools.UnitTesting.ProcessingTasksTests.ResultExporterTests
{
    [TestFixture]
    public class IsosResultExporterTests
    {

        private string xcaliburFile1 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private string uimfFile1 = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

        private string uimf_Sqlite_IsosResultOutputFile1 = "..\\..\\TestFiles\\UIMF_Sqlite_testOutput1_isos.sqlite";
        private string uimf_text_IsosResultOutputFile1 = "..\\..\\TestFiles\\UIMF_Text_testOutput1_isos.csv";

        private string xcalibur_sqlite_IsosResultOutputFile1 = "..\\..\\TestFiles\\XCalibur_Sqlite_testOutput1_isos.sqlite";
        private string xcalibur_text_IsosResultOutputFile1 = "..\\..\\TestFiles\\XCalibur_text_testOutput1_isos.csv";

        private string peakExporter1 = "..\\..\\TestFiles\\peakExporter_temp.sqlite";

        [Test]
        public void outputToSqlite_xcaliburData_Test1()
        {
            string testFile = xcalibur_sqlite_IsosResultOutputFile1;

            List<Run> runcoll = new List<Run>();
            Run run = new XCaliburRun(xcaliburFile1);
            runcoll.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 6000, 6020, 1, 1,false);
            sscc.Create();

            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();
            Task sqliteExporter = new DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters.BasicIsosResultSqliteExporter(testFile, 1000000);
            Task peakExporter = new DeconTools.Backend.ProcessingTasks.PeakListExporters.PeakListSQLiteExporter(100000, peakExporter1);
            Task flagger = new ResultValidatorTask();

            Stopwatch sw;

            foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
            {

                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);

                decon.Execute(run.ResultCollection);
                flagger.Execute(run.ResultCollection);


                sw = new Stopwatch();
                sw.Start();

                sqliteExporter.Execute(run.ResultCollection);
                sw.Stop();
                if (sw.ElapsedMilliseconds > 10)
                {
                    Console.WriteLine("SqliteExporter execution time = \t" + sw.ElapsedMilliseconds);
                }

                peakExporter.Execute(run.ResultCollection);
            }

            Assert.AreEqual(true, File.Exists(testFile));

            FileInfo fi = new FileInfo(testFile);
            Assert.AreEqual(28672, fi.Length);
            Console.Write(fi.Length);
        }

        [Test]
        public void outputToText_xcaliburData_Test1()
        {
            string testFile = xcalibur_text_IsosResultOutputFile1;
            
            List<Run> runcoll = new List<Run>();
            Run run = new XCaliburRun(xcaliburFile1);
            runcoll.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 6000, 6020, 1, 1,false);
            sscc.Create();

            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();
            Task flagger = new ResultValidatorTask();
            Task isosExporter = new DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters.BasicIsosResultTextFileExporter(testFile);
            Task peakExporter = new DeconTools.Backend.ProcessingTasks.PeakListExporters.PeakListSQLiteExporter(100000, peakExporter1);


            Stopwatch sw;

            foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
            {

                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);

                decon.Execute(run.ResultCollection);
                flagger.Execute(run.ResultCollection);

                sw = new Stopwatch();
                sw.Start();

                isosExporter.Execute(run.ResultCollection);
                sw.Stop();
                if (sw.ElapsedMilliseconds > 10)
                {
                    Console.WriteLine("Exporter execution time = \t" + sw.ElapsedMilliseconds);
                }

                peakExporter.Execute(run.ResultCollection);
            }

            Assert.AreEqual(true, File.Exists(testFile));

            FileInfo fi = new FileInfo(testFile);
            //Assert.AreEqual(25881, fi.Length);
            Console.Write(fi.Length);

        }

        [Test]
        public void outputToSqlite_uimf_test1()
        {
            if (File.Exists(uimf_Sqlite_IsosResultOutputFile1)) File.Delete(uimf_Sqlite_IsosResultOutputFile1);


            List<Run> runcoll = new List<Run>();
            Run run = new UIMFRun(uimfFile1);
            runcoll.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 200, 240, 9, 1,false);
            sscc.Create();

            FrameSetCollectionCreator fsc = new FrameSetCollectionCreator(run, 800, 801, 3, 1);
            fsc.Create();

            Task msgen = new UIMF_MSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();
            Task driftTimeExtractor = new DeconTools.Backend.ProcessingTasks.UIMFDriftTimeExtractor();
            Task origIntensExtr = new DeconTools.Backend.ProcessingTasks.OriginalIntensitiesExtractor();
            Task sqliteExporter = new DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters.UIMFIsosResultSqliteExporter(uimf_Sqlite_IsosResultOutputFile1, 1000000);
            Task flagger = new ResultValidatorTask();

            Stopwatch sw;

            foreach (FrameSet frame in ((UIMFRun)run).FrameSetCollection.FrameSetList)
            {
                ((UIMFRun)run).CurrentFrameSet = frame;
                foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
                {

                    run.CurrentScanSet = scan;
                    msgen.Execute(run.ResultCollection);
                    peakDetector.Execute(run.ResultCollection);

                    decon.Execute(run.ResultCollection);
                    flagger.Execute(run.ResultCollection);
                    driftTimeExtractor.Execute(run.ResultCollection);
                    origIntensExtr.Execute(run.ResultCollection);

                    sw = new Stopwatch();
                    sw.Start();

                    sqliteExporter.Execute(run.ResultCollection);
                    sw.Stop();
                    if (sw.ElapsedMilliseconds > 10)
                    {
                        Console.WriteLine("SqliteExporter execution time = \t" + sw.ElapsedMilliseconds);
                    }
                }
            }

            Assert.AreEqual(true, File.Exists(uimf_Sqlite_IsosResultOutputFile1));

            FileInfo fi = new FileInfo(uimf_Sqlite_IsosResultOutputFile1);
            Assert.AreEqual(103424, fi.Length);

        }

        [Test]
        public void outputToText_uimf_test1()
        {
            if (File.Exists(uimf_text_IsosResultOutputFile1)) File.Delete(uimf_text_IsosResultOutputFile1);

            List<Run> runcoll = new List<Run>();
            Run run = new UIMFRun(uimfFile1);
            runcoll.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 200, 240, 9, 1,false);
            sscc.Create();

            FrameSetCollectionCreator fsc = new FrameSetCollectionCreator(run, 800, 801, 3, 1);
            fsc.Create();


            Task msgen = new UIMF_MSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();
            Task driftTimeExtractor = new DeconTools.Backend.ProcessingTasks.UIMFDriftTimeExtractor();
            Task origIntensExtr = new DeconTools.Backend.ProcessingTasks.OriginalIntensitiesExtractor();
            Task sqliteExporter = new DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters.UIMFIsosResultTextFileExporter(uimf_text_IsosResultOutputFile1, 1000000);
            Task flagger = new ResultValidatorTask();

            Stopwatch sw;

            foreach (FrameSet frame in ((UIMFRun)run).FrameSetCollection.FrameSetList)
            {
                ((UIMFRun)run).CurrentFrameSet = frame;
                foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
                {

                    run.CurrentScanSet = scan;
                    msgen.Execute(run.ResultCollection);
                    peakDetector.Execute(run.ResultCollection);

                    decon.Execute(run.ResultCollection);
                    flagger.Execute(run.ResultCollection);
                    driftTimeExtractor.Execute(run.ResultCollection);
                    origIntensExtr.Execute(run.ResultCollection);

                    sw = new Stopwatch();
                    sw.Start();

                    sqliteExporter.Execute(run.ResultCollection);
                    sw.Stop();
                    if (sw.ElapsedMilliseconds > 10)
                    {
                        Console.WriteLine("SqliteExporter execution time = \t" + sw.ElapsedMilliseconds);
                    }

                }


            }
            Assert.AreEqual(true, File.Exists(uimf_text_IsosResultOutputFile1));

            FileInfo fi = new FileInfo(uimf_text_IsosResultOutputFile1);
            Assert.AreEqual(98362, fi.Length);
            Console.Write(fi.Length);

        }


    }
}
