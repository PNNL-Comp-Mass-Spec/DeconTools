using System;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSFeatureFinderTests
{
    [TestFixture]
    public class THRASHMSFeatureFinderTests
    {
        [Test]
        public void findMSFeaturesInOrbitrapData_Test1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            
            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            HornDeconvolutor decon = new HornDeconvolutor();

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            //order and get the most intense msfeature
            run.ResultCollection.ResultList = run.ResultCollection.ResultList.OrderByDescending(p => p.IntensityAggregate).ToList();
            IsosResult testIso = run.ResultCollection.ResultList[0];
            Assert.AreEqual(13084442, testIso.IntensityAggregate);
            Assert.AreEqual(2, testIso.IsotopicProfile.ChargeState);
            Assert.AreEqual(0.01012m, (decimal)Math.Round(testIso.IsotopicProfile.Score,5));
            Assert.AreEqual(3, testIso.IsotopicProfile.Peaklist.Count);
            Assert.AreEqual(481.274105402604m, (decimal)testIso.IsotopicProfile.Peaklist[0].XValue);
            Assert.AreEqual(481.775412188198m, (decimal)testIso.IsotopicProfile.Peaklist[1].XValue);
            Assert.AreEqual(482.276820274024m, (decimal)testIso.IsotopicProfile.Peaklist[2].XValue);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
           // TestUtilities.DisplayPeaks(run.PeakList);



        }


    }
}
