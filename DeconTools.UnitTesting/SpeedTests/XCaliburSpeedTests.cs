using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.UnitTesting.SpeedTests
{
    [TestFixture]
    public class XCaliburSpeedTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        [Test]
        public void xcaliburSpeed_RAPID_Test1()
        {
            Run run = new XCaliburRun(xcaliburTestfile );
            int startScan = 6005;
            int stopScan = 7000;

            int numScansSummed = 1;

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(run,startScan,stopScan, numScansSummed, 1);
            scanSetCreator.Create();

            ResultCollection results = new ResultCollection(run);
            List<timingResult> timingResults = new List<timingResult>();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            foreach (ScanSet scanset in run.ScanSetCollection.ScanSetList)
            {
                if (results.Run.GetMSLevel(scanset.PrimaryScanNumber) != 1) continue;

                sw.Reset();
                timingResult timeresult = new timingResult();
                timeresult.frameNum = -1;
                timeresult.scanNum = scanset.PrimaryScanNumber;
                sw.Start();

                run.CurrentScanSet = scanset;
                Task msgen = new GenericMSGenerator();
                msgen.Execute(results);
               
                timeresult.msGenTime = sw.ElapsedMilliseconds;

                sw.Reset();
                sw.Start();
                Task peakDetector = new DeconToolsPeakDetector();
                peakDetector.Execute(results);
                timeresult.peakDetectorTime = sw.ElapsedMilliseconds;

                sw.Reset();
                sw.Start();
                Task rapid = new RapidDeconvolutor();
                rapid.Execute(results);
                timeresult.deconTime = sw.ElapsedMilliseconds;

                sw.Reset();
                sw.Start();
                Task scanResultUpdater = new ScanResultUpdater();
                scanResultUpdater.Execute(results);
                timeresult.resultUpdaterTime = sw.ElapsedMilliseconds;

                timingResults.Add(timeresult);

            }
            reportTimingEachScan(timingResults);


        }

        [Test]
        public void xcaliburSpeed_THRASH_Test1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);
            int startScan = 6005;
            int stopScan = 6050;

            int numScansSummed = 1;

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(run, startScan, stopScan, numScansSummed, 1);
            scanSetCreator.Create();

            ResultCollection results = new ResultCollection(run);
            List<timingResult> timingResults = new List<timingResult>();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            foreach (ScanSet scanset in run.ScanSetCollection.ScanSetList)
            {
                if (results.Run.GetMSLevel(scanset.PrimaryScanNumber) != 1) continue;

                sw.Reset();
                timingResult timeresult = new timingResult();
                timeresult.frameNum = -1;
                timeresult.scanNum = scanset.PrimaryScanNumber;
                sw.Start();

                run.CurrentScanSet = scanset;
                Task msgen = new GenericMSGenerator();
                msgen.Execute(results);

                timeresult.msGenTime = sw.ElapsedMilliseconds;

                sw.Reset();
                sw.Start();
                Task peakDetector = new DeconToolsPeakDetector();
                peakDetector.Execute(results);
                timeresult.peakDetectorTime = sw.ElapsedMilliseconds;

                sw.Reset();
                sw.Start();
                DeconToolsV2.HornTransform.clsHornTransformParameters hornParams = new DeconToolsV2.HornTransform.clsHornTransformParameters();
                Task decon = new HornDeconvolutor(hornParams);
                decon.Execute(results);
                timeresult.deconTime = sw.ElapsedMilliseconds;

                sw.Reset();
                sw.Start();
                Task scanResultUpdater = new ScanResultUpdater();
                scanResultUpdater.Execute(results);
                timeresult.resultUpdaterTime = sw.ElapsedMilliseconds;

                timingResults.Add(timeresult);

            }
            reportTimingEachScan(timingResults);


        }

        private void reportTimingEachScan(List<timingResult> timingResults)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("scanNum\tmsgen\tpeakDet\tdecon\tupdater\n");
            foreach (timingResult timeresult in timingResults)
            {
 
                sb.Append(timeresult.scanNum);
                sb.Append("\t");
                sb.Append(timeresult.msGenTime);
                sb.Append("\t");
                sb.Append(timeresult.peakDetectorTime);
                sb.Append("\t");
                sb.Append(timeresult.deconTime);
                sb.Append("\t");
                sb.Append(timeresult.resultUpdaterTime);
                sb.Append("\n");

            }

            double avgMSGen = timingResults.Average(p => p.msGenTime);
            double avgPeakDet = timingResults.Average(p => p.peakDetectorTime);
            double avgDecon = timingResults.Average(p => p.deconTime);
            double avgUpdater = timingResults.Average(p => p.resultUpdaterTime);

            sb.Append("\n");
            sb.Append("Avg:\t" + avgMSGen.ToString("0.0") + "\t" + avgPeakDet.ToString("0.0") + "\t" + 
                avgDecon.ToString("0.0") + "\t" + avgUpdater.ToString("0.0") + "\n");
            Console.WriteLine(sb.ToString());
        }


    }
}
