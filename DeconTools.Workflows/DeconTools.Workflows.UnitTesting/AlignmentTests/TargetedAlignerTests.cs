﻿using System;
using System.IO;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting
{
    public class TargetedAlignerTests
    {
        [Test]
        public void featuresLoadedFromFile_test1()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_allMassTags.txt";
            var deconToolsResultFile = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_targetedFeatures.txt");

            var parameters = new TargetedAlignerWorkflowParameters();
            parameters.ImportedFeaturesFilename = deconToolsResultFile;

            var aligner = new TargetedAlignerWorkflow(run, parameters);
            aligner.SetMassTags(massTagFile);

            aligner.Execute();
        }

        [Test]
        public void featuresFoundByTargetedProcessing_thenAligned_test1()
        {
            var peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile1;
            var run = RunUtilities.CreateAndLoadPeaks(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1, peaksTestFile);
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_NETVals0.3-0.33.txt";

            var parameters = new TargetedAlignerWorkflowParameters();
            parameters.ChromNETTolerance = 0.2;
            parameters.ChromGenTolerance = 25;
            parameters.ChromGeneratorMode = Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            parameters.ChromPeakDetectorPeakBR = 2;
            parameters.ChromPeakDetectorSigNoise = 2;
            parameters.MSToleranceInPPM = 25;

            parameters.UpperFitScoreAllowedCriteria = 0.1;
            parameters.IScoreAllowedCriteria = 0.15;

            var aligner = new TargetedAlignerWorkflow(run, parameters);
            aligner.SetMassTags(massTagFile);
            aligner.Execute();

            Console.WriteLine(aligner.GetAlignmentReport1());

            Assert.IsNotNull(run.AlignmentInfo);
            Assert.AreEqual(-4.2m, (decimal)(Math.Round(run.AlignmentInfo.GetPPMShiftFromMZ(600.0f), 1)));
        }
    }
}
