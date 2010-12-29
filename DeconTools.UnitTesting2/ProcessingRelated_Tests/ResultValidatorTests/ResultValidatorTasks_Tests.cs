﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ResultValidators;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ResultValidatorTests
{
    [TestFixture]
    public class ResultValidatorTasks_Tests
    {

        [Test]
        public void deconWithTHRASH_Then_ValidateTest1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            ScanSet scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            I_MSGenerator msgen = msgenFactory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            HornDeconvolutor decon = new HornDeconvolutor();
            ResultValidatorTask validator = new ResultValidatorTask();

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);
            validator.Execute(run.ResultCollection);

            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);
            IsosResult testResult = run.ResultCollection.ResultList[0];
            Assert.AreEqual(0.89647500236721m, (decimal)testResult.InterferenceScore);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
        }

        [Test]
        public void deconWithRAPID_Then_ValidateTest1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            ScanSet scanSet = new ScanSet(6005);
            run.CurrentScanSet = scanSet;

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            I_MSGenerator msgen = msgenFactory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            RapidDeconvolutor decon = new RapidDeconvolutor();
            ResultValidatorTask validator = new ResultValidatorTask();

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);
            validator.Execute(run.ResultCollection);

            Assert.AreEqual(190, run.ResultCollection.ResultList.Count);
            IsosResult testResult = run.ResultCollection.ResultList[0];
            Assert.AreEqual(0.934351538973281m, (decimal)testResult.InterferenceScore);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
        }


    }
}
