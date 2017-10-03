using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ResultValidatorTests
{
    [TestFixture]
    public class ResultValidatorTasks_Tests
    {

        [Test]
        public void deconWithTHRASH_Then_ValidateTest1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            var scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            var decon = new HornDeconvolutor();
            var validator = new ResultValidatorTask();

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);
            validator.Execute(run.ResultCollection);

            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);
            var testResult = run.ResultCollection.ResultList[0];
            Assert.AreEqual(0.0532199478712594m, (decimal)testResult.InterferenceScore);

            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
        }

        // To include support for Rapid, you must add a reference to DeconEngine.dll, which was compiled with Visual Studio 2003 and uses MSVCP71.dll
        // Note that DeconEngine.dll also depends on xerces-c_2_7.dll while DeconEngineV2.dll depends on xerces-c_2_8.dll
#if INCLUDE_RAPID

        [Test]
        public void deconWithRAPID_Then_ValidateTest1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            ScanSet scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            
            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            RapidDeconvolutor decon = new RapidDeconvolutor();
            ResultValidatorTask validator = new ResultValidatorTask();

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);
            validator.Execute(run.ResultCollection);

            Assert.AreEqual(190, run.ResultCollection.ResultList.Count);
            IsosResult testResult = run.ResultCollection.ResultList[0];
            Assert.AreEqual(0.0213668719893332m, (decimal)testResult.InterferenceScore);

            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
        }

#endif

    }
}
