using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.ProcessingTasks.NETAlignment;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.Workflows;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting2.NET_Mass_AlignmentTests
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

            TargetedAligner aligner = new TargetedAligner(run, parameters);
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
            parameters.ChromGeneratorMode = Backend.ProcessingTasks.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            parameters.ChromPeakDetectorPeakBR = 2;
            parameters.ChromPeakDetectorSigNoise = 2;
            parameters.MSToleranceInPPM = 25;

            TargetedAligner aligner = new TargetedAligner(run, parameters);
            aligner.UpperFitScoreAllowedCriteria = 0.1;
            aligner.IScoreAllowedCriteria = 0.15;
            

            Console.WriteLine("Loading Mass tags...");
            aligner.SetMassTags(massTagFile);

            Console.WriteLine("Done.");
            aligner.Execute();
        }




    }
}
