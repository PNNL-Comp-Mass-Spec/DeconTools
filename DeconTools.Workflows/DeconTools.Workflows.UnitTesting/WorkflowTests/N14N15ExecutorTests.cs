using System;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class N14N15ExecutorTests
    {
          private string bruker9t_samplefile1 =
            @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\RawData\RSPH_PtoA_L_28_rep1_28Feb08_Raptor_08-01-01";


        [Test]
        public void saveExecutorParameterFileTest1()
        {
               string resultsFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Results";
            string alignmentInfoFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\AlignmentInfo";
            string targetsFilePath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Targets\POnly_MassTagsMatchingInHalfOfDatasets_Filtered0.45-0.47NET_first18.txt";
            string targetsForAlignmentFilePath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Targets\POnly_MassTagsMatchingInHalfOfDatasets.txt";
            string workflowFilePath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\N14N15WorkflowParameters1.xml";
            string targetedAlignmentParameterFile =@"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\TargetedAlignmentWorkflowParameters1.xml";
            string loggingFolder =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Logs";
            string exportedExecutorParametersFilePath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\ExecutorParameters1.xml";


            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.ResultsFolder = resultsFolder;
            executorParameters.AlignmentInfoFolder = alignmentInfoFolder;
            executorParameters.CopyRawFileLocal = false;
            executorParameters.LoggingFolder = loggingFolder;
            executorParameters.DeleteLocalDatasetAfterProcessing = false;
            executorParameters.TargetType = Globals.TargetType.DatabaseTarget;
            executorParameters.TargetedAlignmentIsPerformed = true;
            executorParameters.TargetedAlignmentWorkflowParameterFile = targetedAlignmentParameterFile;
            executorParameters.TargetsFilePath = targetsFilePath;
            executorParameters.TargetsUsedForAlignmentFilePath = targetsForAlignmentFilePath;
            executorParameters.WorkflowParameterFile = workflowFilePath;

            executorParameters.SaveParametersToXML(exportedExecutorParametersFilePath);

        }

        [Test]
        public void executorTest1()
        {
            string testfile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\RawData\RSPH_PtoA_L_28_rep1_28Feb08_Raptor_08-01-01";
            string parameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\ExecutorParameters1 - forTesting.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(parameterFile);

            BasicTargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testfile);
            executor.Execute();

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);

            foreach (var chromPeak in executor.TargetedWorkflow.ChromPeaksDetected)
            {
                Console.WriteLine(chromPeak.XValue + "\t" + chromPeak.Height + "\t" + chromPeak.Width);
            }

        }


        [Test]
        public void executorTest2()
        {
            string testfile =
                @"\\protoapps\UserData\Slysz\Data\N14N15\2012_07_09_VelosOrbi Ponly datasets\RawData\RSPH_Ponly_28_A_8May12_Earth_12-03-11.raw";
            string parameterFile =
                @"\\protoapps\UserData\Slysz\Data\N14N15\2012_07_09_VelosOrbi Ponly datasets\Parameters\ExecutorParameters1 - forTesting.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(parameterFile);

            BasicTargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testfile);
            executor.Execute();

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);

            foreach (var chromPeak in executor.TargetedWorkflow.ChromPeaksDetected)
            {
                Console.WriteLine(chromPeak.XValue + "\t" + chromPeak.Height + "\t" + chromPeak.Width);
            }

        }


        [Test]
        public void executorTest3_NewDatasets_lowN15Label()
        {
            string testfile =
                @"\\proto-7\VOrbi05\2012_2\RSPH_Ponly_25_A_9May12_Earth_12-03-13\RSPH_Ponly_25_A_9May12_Earth_12-03-13.raw";
            string parameterFile =
                @"\\protoapps\UserData\Slysz\Data\N14N15\2012_07_09_VelosOrbi Ponly datasets\Parameters\ExecutorParameters1 - forTesting.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(parameterFile);

            BasicTargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testfile);
            executor.Execute();

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);

            foreach (var chromPeak in executor.TargetedWorkflow.ChromPeaksDetected)
            {
                Console.WriteLine(chromPeak.XValue + "\t" + chromPeak.Height + "\t" + chromPeak.Width);
            }

        }


        [Test]
        public void executorTest4_OldDatasets_lowN15Label()
        {
            string testfile = @"\\protoapps\UserData\Slysz\Data\N14N15\RawData\RSPH_Ponly_25_run1_20Jan08_Raptor_07-11-11";
            string parameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\ExecutorParameters1 - forTesting.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(parameterFile);

            BasicTargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testfile);
            executor.Execute();

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);

            foreach (var chromPeak in executor.TargetedWorkflow.ChromPeaksDetected)
            {
                Console.WriteLine(chromPeak.XValue + "\t" + chromPeak.Height + "\t" + chromPeak.Width);
            }

        }


        [Test]
        public void executorTest5_NewDatasets_lowN15Label()
        {
            string testfile =
                @"\\proto-7\VOrbi05\2012_2\RSPH_Ponly_24_A_8May12_Earth_12-03-13\RSPH_Ponly_24_A_8May12_Earth_12-03-13.raw";
            string parameterFile =
                @"\\protoapps\UserData\Slysz\Data\N14N15\2012_07_09_VelosOrbi Ponly datasets\Parameters\ExecutorParameters1 - forTesting.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(parameterFile);

            BasicTargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testfile);
            executor.Execute();

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);

            foreach (var chromPeak in executor.TargetedWorkflow.ChromPeaksDetected)
            {
                Console.WriteLine(chromPeak.XValue + "\t" + chromPeak.Height + "\t" + chromPeak.Width);
            }

        }


        [Test]
        public void executorTest6_OldDatasets_lowN15Label()
        {
            string testfile =
                @"\\protoapps\UserData\Slysz\Data\N14N15\RawData\RSPH_Ponly_24_run1_30Jan08_Raptor_07-11-11";
            string parameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\ExecutorParameters1 - forTesting.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(parameterFile);

            BasicTargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testfile);
            executor.Execute();

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);

            foreach (var chromPeak in executor.TargetedWorkflow.ChromPeaksDetected)
            {
                Console.WriteLine(chromPeak.XValue + "\t" + chromPeak.Height + "\t" + chromPeak.Width);
            }

        }

        [Test]
        public void managerTest1()
        {
            string datasetList =
                @"\\protoapps\UserData\Slysz\Data\N14N15\2012_07_09_VelosOrbi Ponly datasets\Dataset_info\pub70_cpu1_p1.txt";
            string parameterFile =
                @"\\protoapps\UserData\Slysz\Data\N14N15\2012_07_09_VelosOrbi Ponly datasets\Parameters\ExecutorParameters1 - forTesting.xml";

            string[] args = {datasetList, parameterFile};

            TargetedWorkflowManagerConsole.Program.Main(args);

        }


        [Test]
        public void managerTest2()
        {
            string datasetList =
                @"\\protoapps\UserData\Slysz\Data\N14N15\2012_07_09_VelosOrbi Ponly datasets\Dataset_info\pub70_cpu1_p1.txt";
            string parameterFile =
                @"\\protoapps\UserData\Slysz\Data\N14N15\2012_07_09_VelosOrbi Ponly datasets\Parameters\ExecutorParameters1 - forTesting.xml";

            string[] args = { datasetList, parameterFile };

            TargetedWorkflowManagerConsole.Program.Main(args);

        }



    }
}
