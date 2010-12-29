using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using System.Diagnostics;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.IsotopicProfileQualityScoringTests
{
    [TestFixture]
    public class InterferenceScorerTests
    {

        [Test]
        public void interference_weakFeature_test1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            I_MSGenerator msgen = msgenFactory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            HornDeconvolutor decon = new HornDeconvolutor();


            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            IsosResult testResult = run.ResultCollection.ResultList.Where(p => p.IsotopicProfile.MonoPeakMZ > 597 && p.IsotopicProfile.MonoPeakMZ < 598).First();

            MSPeak monoPeak = testResult.IsotopicProfile.getMonoPeak();
            MSPeak lastPeak = testResult.IsotopicProfile.Peaklist[testResult.IsotopicProfile.Peaklist.Count-1];

            int startIndexOfXYData = MathUtils.BinarySearchWithTolerance(run.XYData.Xvalues, monoPeak.XValue - 3, 0, (run.XYData.Xvalues.Length - 1), 2);

            //interference scorer

            InterferenceScorer interferenceScorer = new InterferenceScorer();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            double interferenceScore = interferenceScorer.GetInterferenceScore(run.XYData,testResult.IsotopicProfile.Peaklist,monoPeak.XValue-1.1,
                lastPeak.XValue+lastPeak.Width,startIndexOfXYData);
            stopwatch.Stop();

            Console.WriteLine("interference= "+ interferenceScore);
            Console.WriteLine("Time taken = " + stopwatch.ElapsedMilliseconds);
            


        }

        [Test]
        public void interference_strongFeature_test1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            I_MSGenerator msgen = msgenFactory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            HornDeconvolutor decon = new HornDeconvolutor();


            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            IsosResult testResult = run.ResultCollection.ResultList.Where(p => p.IsotopicProfile.MonoPeakMZ > 903 && p.IsotopicProfile.MonoPeakMZ < 904).First();

            MSPeak monoPeak = testResult.IsotopicProfile.getMonoPeak();
            MSPeak lastPeak = testResult.IsotopicProfile.Peaklist[testResult.IsotopicProfile.Peaklist.Count - 1];

            int startIndexOfXYData = MathUtils.BinarySearchWithTolerance(run.XYData.Xvalues, monoPeak.XValue - 3, 0, (run.XYData.Xvalues.Length - 1), 2);

            //interference scorer

            InterferenceScorer interferenceScorer = new InterferenceScorer();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            double interferenceScore = interferenceScorer.GetInterferenceScore(run.XYData, testResult.IsotopicProfile.Peaklist, monoPeak.XValue - 1.1,
                lastPeak.XValue + lastPeak.Width, startIndexOfXYData);
            stopwatch.Stop();

            Console.WriteLine("interference= " + interferenceScore);
            Console.WriteLine("Time taken = " + stopwatch.ElapsedMilliseconds);



        }


        [Test]
        public void interference_allFeaturesInScan_test1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            I_MSGenerator msgen = msgenFactory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            HornDeconvolutor decon = new HornDeconvolutor();


            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            foreach (var isosResult in run.ResultCollection.ResultList)
            {

                MSPeak monoPeak = isosResult.IsotopicProfile.getMonoPeak();
                MSPeak lastPeak = isosResult.IsotopicProfile.Peaklist[isosResult.IsotopicProfile.Peaklist.Count - 1];

                int startIndexOfXYData = MathUtils.BinarySearchWithTolerance(run.XYData.Xvalues, monoPeak.XValue - 3, 0, (run.XYData.Xvalues.Length - 1), 2);

                //interference scorer

                InterferenceScorer interferenceScorer = new InterferenceScorer();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                double interferenceScore = interferenceScorer.GetInterferenceScore(run.XYData, isosResult.IsotopicProfile.Peaklist, monoPeak.XValue - 1.1,
                    lastPeak.XValue + lastPeak.Width, startIndexOfXYData);
                stopwatch.Stop();

                Console.WriteLine("interference= \t" + interferenceScore);

            }

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
            
         



        }



    }
}
