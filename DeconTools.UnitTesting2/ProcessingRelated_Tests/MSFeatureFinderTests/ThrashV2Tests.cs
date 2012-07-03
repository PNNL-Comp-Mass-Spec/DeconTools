using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSFeatureFinderTests
{
    [TestFixture]
    public class ThrashV2Tests
    {
        [Test]
        public void Test1()
        {
            ThrashDeconvolutorV2 deconvolutor = new ThrashDeconvolutorV2();

             Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            
            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
          

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var isosResults=   deconvolutor.PerformThrash(run.XYData, run.PeakList, scanSet.BackgroundIntensity,
                                       scanSet.BackgroundIntensity);
            stopwatch.Stop();

            Console.WriteLine("Time for decon= " + stopwatch.ElapsedMilliseconds);

            IsosResult testResult = isosResults[0];

            TestUtilities.DisplayIsotopicProfileData(testResult.IsotopicProfile);

            //TestUtilities.DisplayMSFeatures(isosResults);


        }


        [Test]
        public void Test2()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            HornDeconvolutor decon = new HornDeconvolutor();

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

             Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            decon.Execute(run.ResultCollection);
            stopwatch.Stop();

            Console.WriteLine("Time for decon= " + stopwatch.ElapsedMilliseconds);
            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            //order and get the most intense msfeature
            run.ResultCollection.ResultList = run.ResultCollection.ResultList.OrderByDescending(p => p.IsotopicProfile.IntensityAggregate).ToList();
            IsosResult testIso = run.ResultCollection.ResultList[0];
            Assert.AreEqual(13084442, testIso.IsotopicProfile.IntensityAggregate);
            Assert.AreEqual(2, testIso.IsotopicProfile.ChargeState);
            Assert.AreEqual(0.01012m, (decimal)Math.Round(testIso.IsotopicProfile.Score, 5));
            Assert.AreEqual(3, testIso.IsotopicProfile.Peaklist.Count);
            Assert.AreEqual(481.274105402604m, (decimal)testIso.IsotopicProfile.Peaklist[0].XValue);
            Assert.AreEqual(481.775412188198m, (decimal)testIso.IsotopicProfile.Peaklist[1].XValue);
            Assert.AreEqual(482.276820274024m, (decimal)testIso.IsotopicProfile.Peaklist[2].XValue);

            TestUtilities.DisplayIsotopicProfileData(testIso.IsotopicProfile);

            // TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
            //TestUtilities.DisplayPeaks(run.PeakList);

        }
    }
}
