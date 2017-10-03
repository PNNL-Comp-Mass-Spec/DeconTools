using System;
using System.Diagnostics;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.IsotopicProfileQualityScoringTests
{
    [TestFixture]
    public class InterferenceScorerTests
    {

        [Test]
        public void rawinterference_weakFeature_test1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            var decon = new HornDeconvolutor();


            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            var testResult = run.ResultCollection.ResultList.Where(p => p.IsotopicProfile.MonoPeakMZ > 597 && p.IsotopicProfile.MonoPeakMZ < 598).First();

            var monoPeak = testResult.IsotopicProfile.getMonoPeak();
            var lastPeak = testResult.IsotopicProfile.Peaklist[testResult.IsotopicProfile.Peaklist.Count - 1];

            var startIndexOfXYData = MathUtils.BinarySearchWithTolerance(run.XYData.Xvalues, monoPeak.XValue - 3, 0, (run.XYData.Xvalues.Length - 1), 2);

            //interference scorer

            var interferenceScorer = new InterferenceScorer();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var interferenceScore = interferenceScorer.GetInterferenceScore(run.XYData, testResult.IsotopicProfile.Peaklist, monoPeak.XValue - 1.1,
                lastPeak.XValue + lastPeak.Width, startIndexOfXYData);
            stopwatch.Stop();

            Console.WriteLine("interference= " + interferenceScore);
            Console.WriteLine("Time taken = " + stopwatch.ElapsedMilliseconds);



        }

        [Test]
        public void raw_interference_strongFeature_test1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            Task msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);


            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            var decon = new HornDeconvolutor();


            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            var testResult = run.ResultCollection.ResultList.Where(p => p.IsotopicProfile.MonoPeakMZ > 903 && p.IsotopicProfile.MonoPeakMZ < 904).First();

            var monoPeak = testResult.IsotopicProfile.getMonoPeak();
            var lastPeak = testResult.IsotopicProfile.Peaklist[testResult.IsotopicProfile.Peaklist.Count - 1];

            var startIndexOfXYData = MathUtils.BinarySearchWithTolerance(run.XYData.Xvalues, monoPeak.XValue - 3, 0, (run.XYData.Xvalues.Length - 1), 2);

            //interference scorer

            var interferenceScorer = new InterferenceScorer();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var interferenceScore = interferenceScorer.GetInterferenceScore(run.XYData, testResult.IsotopicProfile.Peaklist, monoPeak.XValue - 1.1,
                lastPeak.XValue + lastPeak.Width, startIndexOfXYData);
            stopwatch.Stop();

            Console.WriteLine("interference= " + interferenceScore);
            Console.WriteLine("Time taken = " + stopwatch.ElapsedMilliseconds);



        }

        [Test]
        public void peak_interference_weakFeature_test1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            Task msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);


            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            var decon = new HornDeconvolutor();


            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            var testResult = run.ResultCollection.ResultList.Where(p => p.IsotopicProfile.MonoPeakMZ > 597 && p.IsotopicProfile.MonoPeakMZ < 598).First();

            var monoPeak = testResult.IsotopicProfile.getMonoPeak();
            var lastPeak = testResult.IsotopicProfile.Peaklist[testResult.IsotopicProfile.Peaklist.Count - 1];

            var startIndexOfXYData = MathUtils.BinarySearchWithTolerance(run.XYData.Xvalues, monoPeak.XValue - 3, 0, (run.XYData.Xvalues.Length - 1), 2);

            //interference scorer

            var interferenceScorer = new InterferenceScorer();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var scanPeaks = run.PeakList.Select<Peak, MSPeak>(i => (MSPeak)i).ToList();
            var interferenceScore = interferenceScorer.GetInterferenceScore(scanPeaks, testResult.IsotopicProfile.Peaklist, monoPeak.XValue - 1.1,
    lastPeak.XValue + lastPeak.Width);
            stopwatch.Stop();

            Console.WriteLine("interference= " + interferenceScore);
            Console.WriteLine("Time taken = " + stopwatch.ElapsedMilliseconds);



        }

        [Test]
        public void peak_interference_strongFeature_test1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;
            Task msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);


            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            var decon = new HornDeconvolutor();


            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            var testResult = run.ResultCollection.ResultList.Where(p => p.IsotopicProfile.MonoPeakMZ > 903 && p.IsotopicProfile.MonoPeakMZ < 904).First();

            var monoPeak = testResult.IsotopicProfile.getMonoPeak();
            var lastPeak = testResult.IsotopicProfile.Peaklist[testResult.IsotopicProfile.Peaklist.Count - 1];

            var startIndexOfXYData = MathUtils.BinarySearchWithTolerance(run.XYData.Xvalues, monoPeak.XValue - 3, 0, (run.XYData.Xvalues.Length - 1), 2);

            //interference scorer

            var interferenceScorer = new InterferenceScorer();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var scanPeaks = run.PeakList.Select<Peak, MSPeak>(i => (MSPeak)i).ToList();
            var interferenceScore = interferenceScorer.GetInterferenceScore(scanPeaks, testResult.IsotopicProfile.Peaklist, monoPeak.XValue - 1.1,
    lastPeak.XValue + lastPeak.Width); 
            stopwatch.Stop();

            Console.WriteLine("interference= " + interferenceScore);
            Console.WriteLine("Time taken = " + stopwatch.ElapsedMilliseconds);



        }


        [Test]
        public void peak_interference_UIMF_expectInterference_test1()
        {
            var uimfFrame1200_142 =  FileRefs.RawDataBasePath + @"\UIMF\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000_frame1200_scan142.txt";

            Run run = new DeconTools.Backend.Runs.MSScanFromTextFileRun(uimfFrame1200_142);

            var scanSet = new ScanSet(0);
            run.CurrentScanSet = scanSet;

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            msgen.MinMZ = 200;
            msgen.MaxMZ = 2000;

            var peakDetector = new DeconToolsPeakDetectorV2(4, 3, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            var decon = new HornDeconvolutor();
            decon.MinIntensityForScore = 10;
            decon.DeleteIntensityThreshold = 10;
            decon.MaxFitAllowed = 0.4;
            decon.MinMZ = 200;
            decon.MaxMZ = 2000;
            decon.IsMZRangeUsed = false;


            var zeroFiller = new DeconToolsZeroFiller();

            msgen.Execute(run.ResultCollection);
            zeroFiller.Execute(run.ResultCollection);

            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            //Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            var testResult = run.ResultCollection.ResultList.Where(p => p.IsotopicProfile.MonoPeakMZ > 428 && p.IsotopicProfile.MonoPeakMZ < 430).First();

            var monoPeak = testResult.IsotopicProfile.getMonoPeak();
            var lastPeak = testResult.IsotopicProfile.Peaklist[testResult.IsotopicProfile.Peaklist.Count - 1];

            var startIndexOfXYData = MathUtils.BinarySearchWithTolerance(run.XYData.Xvalues, monoPeak.XValue - 3, 0, (run.XYData.Xvalues.Length - 1), 2);

            //interference scorer

            var interferenceScorer = new InterferenceScorer();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var scanPeaks = run.PeakList.Select<Peak, MSPeak>(i => (MSPeak)i).ToList();
            var interferenceScore = interferenceScorer.GetInterferenceScore(scanPeaks, testResult.IsotopicProfile.Peaklist, monoPeak.XValue - 1.1,
    lastPeak.XValue + lastPeak.Width);
            stopwatch.Stop();

            Console.WriteLine("interference= " + interferenceScore);
            Console.WriteLine("Time taken = " + stopwatch.ElapsedMilliseconds);



        }


        [Test]
        public void interference_allFeaturesInScan_test1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            Task msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);


            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            var decon = new HornDeconvolutor();


            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            foreach (var isosResult in run.ResultCollection.ResultList)
            {

                var monoPeak = isosResult.IsotopicProfile.getMonoPeak();
                var lastPeak = isosResult.IsotopicProfile.Peaklist[isosResult.IsotopicProfile.Peaklist.Count - 1];

                var startIndexOfXYData = MathUtils.BinarySearchWithTolerance(run.XYData.Xvalues, monoPeak.XValue - 3, 0, (run.XYData.Xvalues.Length - 1), 2);

                //interference scorer

                var interferenceScorer = new InterferenceScorer();

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var interferenceScore = interferenceScorer.GetInterferenceScore(run.XYData, isosResult.IsotopicProfile.Peaklist, monoPeak.XValue - 1.1,
                    lastPeak.XValue + lastPeak.Width, startIndexOfXYData);
                stopwatch.Stop();

                Console.WriteLine("interference= \t" + interferenceScore);

            }

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);





        }



    }
}
