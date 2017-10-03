using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSFeatureFinderTests
{
    [TestFixture]
    public class RAPIDMSFeatureFinderTests
    {
        // To include support for Rapid, you must add a reference to DeconEngine.dll, which was compiled with Visual Studio 2003 and uses MSVCP71.dll
        // Note that DeconEngine.dll also depends on xerces-c_2_7.dll while DeconEngineV2.dll depends on xerces-c_2_8.dll
#if INCLUDE_RAPID

        [Test]
        public void findMSFeaturesInOrbitrapData_Test1()
        {

            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            peakDetector.IsDataThresholded = true;

            RapidDeconvolutor decon = new RapidDeconvolutor();
            decon.IsNewFitCalculationPerformed = true;


            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            Assert.AreEqual(192, run.ResultCollection.ResultList.Count);

            //order and get the most intense msfeature
            run.ResultCollection.ResultList=  run.ResultCollection.ResultList.OrderByDescending(p => p.IntensityAggregate).ToList();
            IsosResult testIso = run.ResultCollection.ResultList[0];

            Assert.AreEqual(20986588.375, testIso.IntensityAggregate);
            Assert.AreEqual(2, testIso.IsotopicProfile.ChargeState);
            Assert.AreEqual(0.00548633562231413m, (decimal)testIso.IsotopicProfile.Score);
            Assert.AreEqual(3, testIso.IsotopicProfile.Peaklist.Count);
            Assert.AreEqual(481.274105402604m, (decimal)testIso.IsotopicProfile.Peaklist[0].XValue);
            Assert.AreEqual(481.775412188198m, (decimal)testIso.IsotopicProfile.Peaklist[1].XValue);
            Assert.AreEqual(482.276820274024m, (decimal)testIso.IsotopicProfile.Peaklist[2].XValue);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);





        }

#endif
    }
}
