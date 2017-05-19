using System;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    [Category("Functional")]
    public class N14N15ExecutorTests
    {
        [Test]
        [Category("Standard")]
        public void saveExecutorParameterFileTest1()
        {
            var targetsFilePath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Targets\POnly_MassTagsMatchingInHalfOfDatasets_Filtered0.45-0.47NET_first18.txt";
            var workflowFilePath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\N14N15WorkflowParameters1.xml";
            var targetedAlignmentParameterFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\TargetedAlignmentWorkflowParameters1.xml";
            var exportedExecutorParametersFilePath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\ExecutorParameters1.xml";


            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.CopyRawFileLocal = false;
            executorParameters.DeleteLocalDatasetAfterProcessing = false;
            executorParameters.TargetType = Globals.TargetType.DatabaseTarget;
            executorParameters.TargetedAlignmentIsPerformed = true;
            executorParameters.TargetedAlignmentWorkflowParameterFile = targetedAlignmentParameterFile;
            executorParameters.TargetsFilePath = targetsFilePath;
            executorParameters.WorkflowParameterFile = workflowFilePath;
            executorParameters.SaveParametersToXML(exportedExecutorParametersFilePath);

        }

        [Test]
        [Category("LongRunning")]
        public void Bruker9T_N14N15_executorTest1()
        {
            var testfile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\RawData\RSPH_PtoA_L_28_rep1_28Feb08_Raptor_08-01-01";
            var parameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\ExecutorParameters1 - forTesting.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(parameterFile);
            executorParameters.CopyRawFileLocal = false;

            var executor = new BasicTargetedWorkflowExecutor(executorParameters, testfile);
            executor.Execute();

        }


        [Test]
        [Category("LongRunning")]
        public void orbitrap_N14N15_executorTest2()
        {
            var testfile =
                @"\\protoapps\UserData\Slysz\Data\N14N15\2012_07_09_VelosOrbi Ponly datasets\RawData\RSPH_Ponly_28_A_8May12_Earth_12-03-11.raw";
            var parameterFile =
                @"\\protoapps\UserData\Slysz\Data\N14N15\2012_07_09_VelosOrbi Ponly datasets\Parameters\ExecutorParameters1 - forTesting.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(parameterFile);
            executorParameters.CopyRawFileLocal = false;

            var executor = new BasicTargetedWorkflowExecutor(executorParameters, testfile);
            executor.Execute();

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);

            foreach (var chromPeak in executor.TargetedWorkflow.ChromPeaksDetected)
            {
                Console.WriteLine(chromPeak.XValue + "\t" + chromPeak.Height + "\t" + chromPeak.Width);
            }

        }





    }
}
