using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using System.Diagnostics;

namespace DeconTools.UnitTesting.ProcessingTasksTests.ResultExporterTests
{
    [TestFixture]
    public class ScanResultExporterTests
    {
        private string xcaliburFile1 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private string uimfFile1 = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

        private string uimf_Sqlite_ScanResultOutputFile1 = "..\\..\\TestFiles\\UIMF_Sqlite_testOutput1_scans.sqlite";
        private string uimf_text_ScanResultOutputFile1 = "..\\..\\TestFiles\\UIMF_Text_testOutput1_scans.csv";

        private string xcalibur_sqlite_ScanResultOutputFile1 = "..\\..\\TestFiles\\XCalibur_Sqlite_testOutput1_scans.sqlite";
        private string xcalibur_text_ScanResultOutputFile1 = "..\\..\\TestFiles\\XCalibur_text_testOutput1_scans.csv";

        private string peakExporter1 = "..\\..\\TestFiles\\peakExporter_temp.sqlite";

        [Test]
        public void outputToSqlite_xcaliburData_Test1()
        {
            List<Run> runcoll = new List<Run>();
            Run run = new XCaliburRun(xcaliburFile1);
            runcoll.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 6000, 6100, 1, 1);
            sscc.Create();

            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new RapidDeconvolutor();
            Task sqliteScanResultExporter = new DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters.BasicScanResult_SqliteExporter(xcalibur_sqlite_ScanResultOutputFile1);

            Task peakExporter = new DeconTools.Backend.ProcessingTasks.PeakListExporters.PeakListSQLiteExporter(100000, peakExporter1);
            Task scanResultUpdater = new ScanResultUpdater();

            Stopwatch sw;

            foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);
                decon.Execute(run.ResultCollection);
                scanResultUpdater.Execute(run.ResultCollection);

                sw = new Stopwatch();
                sw.Start();

                sqliteScanResultExporter.Execute(run.ResultCollection);
                sw.Stop();


                if (sw.ElapsedMilliseconds > 10)
                {
                    Console.WriteLine("SqliteExporter execution time = \t" + sw.ElapsedMilliseconds);
                }

                peakExporter.Execute(run.ResultCollection);
            }

        }

        [Test]
        public void outputToText_xcaliburData_Test1()
        {
            List<Run> runcoll = new List<Run>();
            Run run = new XCaliburRun(xcaliburFile1);
            runcoll.Add(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 6000, 6100, 1, 1);
            sscc.Create();

            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new RapidDeconvolutor();
            Task sqliteScanResultExporter = new DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters.BasicScanResult_TextFileExporter(xcalibur_text_ScanResultOutputFile1);
            Task scanResultUpdater = new ScanResultUpdater();

            Stopwatch sw;

            foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);
                decon.Execute(run.ResultCollection);
                scanResultUpdater.Execute(run.ResultCollection);

                sw = new Stopwatch();
                sw.Start();

                sqliteScanResultExporter.Execute(run.ResultCollection);
                sw.Stop();
                if (sw.ElapsedMilliseconds > 10)
                {
                    Console.WriteLine("Exporter execution time = \t" + sw.ElapsedMilliseconds);
                }

            }



            //TaskCollection taskColl=new TaskCollection();
            //taskColl.TaskList.Add(msgen);
            //taskColl.TaskList.Add(peakDetector);
            //taskColl.TaskList.Add(decon);
            //taskColl.TaskList.Add(sqliteExporter);

            //TaskController controller = new BasicTaskController(taskColl);
            //controller.Execute(runcoll);
        }

        [Test]
        public void outputToSqlite_uimf_test1()
        {
            List<Run> runcoll = new List<Run>();
            Run run = new UIMFRun(uimfFile1);
            runcoll.Add(run);


            FrameSetCollectionCreator fsc = new FrameSetCollectionCreator(run, 800, 809, 3, 1);
            fsc.Create();

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 200, 220, 9, 1);
            sscc.Create();


            Task msgen = new UIMF_MSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new RapidDeconvolutor();
            Task driftTimeExtractor = new DeconTools.Backend.ProcessingTasks.UIMFDriftTimeExtractor();
            Task origIntensExtr = new DeconTools.Backend.ProcessingTasks.OriginalIntensitiesExtractor();
            Task ticExtractor = new DeconTools.Backend.ProcessingTasks.UIMF_TICExtractor();

            Task scanResultUpdater = new ScanResultUpdater();


            Task sqliteExporter = new DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters.UIMFScanResult_SqliteExporter(uimf_Sqlite_ScanResultOutputFile1);

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
                    ticExtractor.Execute(run.ResultCollection);
                    scanResultUpdater.Execute(run.ResultCollection);


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

        }

        [Test]
        public void outputToText_uimf_test1()
        {
            List<Run> runcoll = new List<Run>();
            Run run = new UIMFRun(uimfFile1);
            runcoll.Add(run);

            FrameSetCollectionCreator fsc = new FrameSetCollectionCreator(run, 800, 809, 3, 1);
            fsc.Create();

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 200, 220, 9, 1);
            sscc.Create();

            Task msgen = new UIMF_MSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new RapidDeconvolutor();
            Task driftTimeExtractor = new DeconTools.Backend.ProcessingTasks.UIMFDriftTimeExtractor();
            Task origIntensExtr = new DeconTools.Backend.ProcessingTasks.OriginalIntensitiesExtractor();
            Task ticExtractor = new DeconTools.Backend.ProcessingTasks.UIMF_TICExtractor();
            Task scanResultUpdater = new ScanResultUpdater();
            Task sqliteExporter = new DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters.UIMFScanResult_TextFileExporter(uimf_text_ScanResultOutputFile1);

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
                    ticExtractor.Execute(run.ResultCollection);
                    scanResultUpdater.Execute(run.ResultCollection);
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

        }

    }
}
