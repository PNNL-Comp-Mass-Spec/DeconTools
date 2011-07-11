using System;
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
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_allMassTags.txt";
            string deconToolsResultFile = FileRefs.ImportedData + "\\" + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_targetedFeatures.txt";

            TargetedAlignerWorkflowParameters parameters = new TargetedAlignerWorkflowParameters();
            parameters.ImportedFeaturesFilename = deconToolsResultFile;

            TargetedAlignerWorkflow aligner = new TargetedAlignerWorkflow(run, parameters);
            aligner.SetMassTags(massTagFile);

            aligner.Execute();

        }

        [Test]
        public void featuresFoundByTargetedProcessing_thenAligned_test1()
        {
            string peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            Run run = RunUtilities.CreateAndLoadPeaks(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1, peaksTestFile);
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_NETVals0.3-0.33.txt";

            TargetedAlignerWorkflowParameters parameters = new TargetedAlignerWorkflowParameters();
            parameters.ChromNETTolerance = 0.2;
            parameters.ChromToleranceInPPM = 25;
            parameters.ChromGeneratorMode = ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            parameters.ChromPeakDetectorPeakBR = 2;
            parameters.ChromPeakDetectorSigNoise = 2;
            parameters.MSToleranceInPPM = 25;

            parameters.UpperFitScoreAllowedCriteria = 0.1;
            parameters.IScoreAllowedCriteria = 0.15;

            TargetedAlignerWorkflow aligner = new TargetedAlignerWorkflow(run, parameters);
            aligner.SetMassTags(massTagFile);
            aligner.Execute();

            Assert.IsNotNull(run.AlignmentInfo);
            Assert.AreEqual(-3.6m, (decimal)(Math.Round(run.AlignmentInfo.GetPPMShiftFromMZ(600.0f), 1)));

        }




    }
}
