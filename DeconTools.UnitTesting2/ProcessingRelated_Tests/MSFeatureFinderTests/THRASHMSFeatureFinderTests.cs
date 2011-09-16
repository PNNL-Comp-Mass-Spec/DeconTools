using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSFeatureFinderTests
{
    [TestFixture]
    public class THRASHMSFeatureFinderTests
    {
        [Test]
        public void findMSFeaturesInOrbitrapData_Test1()
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

            //order and get the most intense msfeature
            run.ResultCollection.ResultList = run.ResultCollection.ResultList.OrderByDescending(p => p.IsotopicProfile.IntensityAggregate).ToList();
            IsosResult testIso = run.ResultCollection.ResultList[0];
            Assert.AreEqual(13084442, testIso.IsotopicProfile.IntensityAggregate);
            Assert.AreEqual(2, testIso.IsotopicProfile.ChargeState);
            Assert.AreEqual(0.0101245114907111m, (decimal)testIso.IsotopicProfile.Score);
            Assert.AreEqual(3, testIso.IsotopicProfile.Peaklist.Count);
            Assert.AreEqual(481.274105402604m, (decimal)testIso.IsotopicProfile.Peaklist[0].XValue);
            Assert.AreEqual(481.775412188198m, (decimal)testIso.IsotopicProfile.Peaklist[1].XValue);
            Assert.AreEqual(482.276820274024m, (decimal)testIso.IsotopicProfile.Peaklist[2].XValue);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

          


        }


    }
}
