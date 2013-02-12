using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class TargetedAlignmentOnDifficultDatasetsTests
    {
        [Test]
        public void steepGradientAtEndDatasetTest1()
        {

            string datasetFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            string massTagFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\MassTags\QCShew_Formic_MassTags_for_alignment.txt";
            string workflowParameterFile = @"D:\Data\Orbitrap\Issue0725_badAlignment\TargetedAlignmentWorkflowParameters1 - Copy.xml";


            Run run = RunUtilities.CreateAndLoadPeaks(datasetFile, datasetFile.Replace(".RAW", "_peaks.txt"));

            TargetedAlignerWorkflowParameters parameters = new TargetedAlignerWorkflowParameters();
            parameters.LoadParameters(workflowParameterFile);
            parameters.ChromNETTolerance = 0.3;
            parameters.ChromGenTolerance = 60;
            parameters.MSToleranceInPPM = 60;

            Console.WriteLine(parameters.ToStringWithDetails());


            TargetedAlignerWorkflow aligner = new TargetedAlignerWorkflow(run, parameters);
            aligner.SetMassTags(massTagFile);
            aligner.outputToConsole = true;
            aligner.Execute();
        }


        [Test]
        public void standardQCShewDatasetTest1()
        {
            string datasetFile =
                           @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\MassTags\QCShew_Formic_MassTags_for_alignment.txt";
            string workflowParameterFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\WorkflowParameterFiles\TargetedAlignmentWorkflowParameters1.xml";

            Run run = RunUtilities.CreateAndLoadPeaks(datasetFile, datasetFile.Replace(".RAW", "_peaks.txt"));

            TargetedAlignerWorkflowParameters parameters = new TargetedAlignerWorkflowParameters();
            parameters.LoadParameters(workflowParameterFile);
            parameters.ChromNETTolerance = 0.3;
            parameters.ChromGenTolerance = 60;
            parameters.MSToleranceInPPM = 60;

            Console.WriteLine(parameters.ToStringWithDetails());


            TargetedAlignerWorkflow aligner = new TargetedAlignerWorkflow(run, parameters);
            aligner.outputToConsole = true;
            aligner.SetMassTags(massTagFile);
            aligner.Execute();

        }


        [Test]
        public void problemVladDatasetTest1()
        {
            string datasetFile =
                @"\\protoapps\UserData\Slysz\Data\O16O18\Vlad_O16O18\RawData\test.RAW";
            string massTagFile =
                @"\\protoapps\UserData\Slysz\Data\O16O18\Vlad_O16O18\Targets\massTags_found_across_all_5_datasets.txt";
            string workflowParameterFile =
                @"\\protoapps\UserData\Slysz\Data\O16O18\Vlad_O16O18\Workflow_Parameters\TargetedAlignmentWorkflowParameters1.xml";

            Run run = RunUtilities.CreateAndLoadPeaks(datasetFile, datasetFile.Replace(".RAW", "_peaks.txt"));

            TargetedAlignerWorkflowParameters parameters = new TargetedAlignerWorkflowParameters();
            parameters.LoadParameters(workflowParameterFile);
            parameters.ChromNETTolerance = 0.3;
            parameters.ChromGenTolerance = 60;
            parameters.MSToleranceInPPM = 60;

            Console.WriteLine(parameters.ToStringWithDetails());


            TargetedAlignerWorkflow aligner = new TargetedAlignerWorkflow(run, parameters);
            aligner.outputToConsole = true;
            aligner.SetMassTags(massTagFile);
            aligner.Execute();

        }




        [Test]
        public void highMassError_DatasetTest1()
        {
            string datasetFile = @"D:\Data\Orbitrap\Subissue01\QC_Shew_10_01-pt5-1_8Feb10_Doc_09-12-24.RAW";
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\MassTags\QCShew_Formic_MassTags_for_alignment.txt";
            string workflowParameterFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\WorkflowParameterFiles\TargetedAlignmentWorkflowParameters1.xml";

            Run run = RunUtilities.CreateAndLoadPeaks(datasetFile, datasetFile.Replace(".RAW", "_peaks.txt"));

            TargetedAlignerWorkflowParameters parameters = new TargetedAlignerWorkflowParameters();
            parameters.LoadParameters(workflowParameterFile);
            parameters.ChromNETTolerance = 0.3;
            parameters.ChromGenTolerance = 60;
            parameters.MSToleranceInPPM = 60;

            Console.WriteLine(parameters.ToStringWithDetails());


            TargetedAlignerWorkflow aligner = new TargetedAlignerWorkflow(run, parameters);
            aligner.outputToConsole = true;
            aligner.SetMassTags(massTagFile);
            aligner.Execute();

        }

    }
}
