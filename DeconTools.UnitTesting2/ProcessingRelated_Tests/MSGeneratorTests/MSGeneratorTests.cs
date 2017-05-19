using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSGeneratorTests
{
    [TestFixture]
    public class MSGeneratorTests
    {

        [Test]
        public void MSGeneratorOnOrbiTest1()
        {
            var testfile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var run = new RunFactory().CreateRun(testfile);
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var testLCScan=6005;
            var scanSet = new ScanSetFactory().CreateScanSet(run, testLCScan, 1);

            var xydata= msgen.GenerateMS(run, scanSet);

            Assert.IsNotNull(xydata);
            Assert.IsTrue(xydata.Xvalues.Length > 1000);
        }

        [Test]
        public void MSGeneratorOnUIMFTest1()
        {
            var uimfFile = FileRefs.RawDataMSFiles.UIMFStdFile3;

            var run = new RunFactory().CreateRun(uimfFile);
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, Globals.PeakFitType.QUADRATIC, true);
            var zeroFiller = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);

            var thrashParameters = new ThrashParameters();
            thrashParameters.MinMSFeatureToBackgroundRatio = 1;
            thrashParameters.MaxFit = 0.4;

            var newDeconvolutor = new ThrashDeconvolutorV2(thrashParameters);

            var testLCScan = 500;
            var testIMSScan = 120;
            var numIMSScanSummed = 7;
            var lowerIMSScan = testIMSScan - (numIMSScanSummed - 1) / 2;
            var upperIMSScan = testIMSScan + (numIMSScanSummed - 1) / 2;

            var scanSet = new ScanSetFactory().CreateScanSet(run, testLCScan, 1);

            run.CurrentScanSet = scanSet;
            ((UIMFRun)run).CurrentIMSScanSet = new IMSScanSet(testIMSScan, lowerIMSScan, upperIMSScan);

            msgen.Execute(run.ResultCollection);

            Assert.IsTrue(run.XYData.Xvalues.Length > 100);

        }


    }
}
