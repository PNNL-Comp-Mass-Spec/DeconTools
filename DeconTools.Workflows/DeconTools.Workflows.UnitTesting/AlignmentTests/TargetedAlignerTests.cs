using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
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
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_allMassTags.txt";
            string deconToolsResultFile = @"D:\Temp\output7\targetedFeatures.txt";

            DeconToolsTargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();

            TargetedAlignerWorkflow aligner = new TargetedAlignerWorkflow(run, parameters);
            aligner.SetMassTags(massTagFile);
            aligner.ImportedFeaturesFilename = deconToolsResultFile;

            aligner.Execute();

        }

        [Test]
        public void featuresFoundByTargetedProcessing_thenAligned_test1()
        {
            string peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile1;

            Console.WriteLine("loading peaks.");
            Run run = RunUtilities.CreateAndLoadPeaks(FileRefs.RawDataMSFiles.OrbitrapStdFile1, peaksTestFile);

            Console.WriteLine("done.");
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_allMassTags.txt";

            DeconToolsTargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.2;
            parameters.ChromToleranceInPPM = 25;
            parameters.ChromGeneratorMode = ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            parameters.ChromPeakDetectorPeakBR = 2;
            parameters.ChromPeakDetectorSigNoise = 2;
            parameters.MSToleranceInPPM = 25;

            TargetedAlignerWorkflow aligner = new TargetedAlignerWorkflow(run, parameters);
            aligner.UpperFitScoreAllowedCriteria = 0.1;
            aligner.IScoreAllowedCriteria = 0.15;
            

            Console.WriteLine("Loading Mass tags...");
            aligner.SetMassTags(massTagFile);

            Console.WriteLine("Done.");
            aligner.Execute();
        }




    }
}
