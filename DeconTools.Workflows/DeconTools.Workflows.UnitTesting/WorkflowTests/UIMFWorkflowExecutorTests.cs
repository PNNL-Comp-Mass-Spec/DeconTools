using System.IO;
using System.Linq;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
using NUnit.Framework;
using Globals = DeconTools.Backend.Globals;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class UIMFWorkflowExecutorTests
    {
        private string uimfTestfile1 =
            @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\UIMF_O16O18Testing\RawData\Alz_O18_Run03_7Sep12_Cheetah_11-12-23.uimf";

        private string uimfMsMsFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\UIMF_Targeted_MSMS_Testing\RawData\SarcCtrl_P21_1mgml_IMS6_AgTOF07_210min_CID_01_05Oct12_Frodo.UIMF";


        [Test]
        public void Test1()
        {
            var testDatasetPath = uimfTestfile1;

            var executorParameterFilename =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\UIMF_O16O18Testing\Parameters\WorkflowExecutorParameters.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();

            executorParameters.LoadParameters(executorParameterFilename);

            var outputExecutorParameterFilename = executorParameterFilename.Replace(".xml", "_autoGen.xml");
            executorParameters.SaveParametersToXML(outputExecutorParameterFilename);


            var testTarget = 7563580;

            var executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTarget).ToList();

            executor.Execute();

        }

        [Test]
        public void TestUIMFTargetedMSMSWorkflowSingleTarget()
        {
            var datasetPath = uimfMsMsFile;

            var executorParameterFilename =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\UIMF_Targeted_MSMS_Testing\Parameters\UIMFTargetedMSMSWorkflowExecutorParameters.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFilename);

            var executor = new BasicTargetedWorkflowExecutor(executorParameters, datasetPath);
            executor.Execute();

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);
        }

        [Test]
        public void TestUIMFTargetedMSMSWorkflow()
        {
            var datasetPath = uimfMsMsFile;

            var executorParameterFilename =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\UIMF_Targeted_MSMS_Testing\Parameters\UIMFTargetedMSMSWorkflowExecutorParametersMostConfidentMassTags.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFilename);

            var executor = new BasicTargetedWorkflowExecutor(executorParameters, datasetPath);
            executor.Execute();

            //List<TargetedResultBase> resultList = executor.TargetedWorkflow.Run.ResultCollection.GetMassTagResults();
            //foreach (var targetedResultBase in resultList)
            //{
            //    Console.WriteLine("*******************************************************");
            //    Console.WriteLine(targetedResultBase.Target.Code + "\t" + targetedResultBase.Target.ChargeState);

            //    foreach (var peakQualityData in targetedResultBase.ChromPeakQualityList)
            //    {
            //        peakQualityData.Display();
            //    }
            //}

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);
        }

        [Test]
        public void TestUIMFTargetedMSMSWorkflowLotsOfTargets()
        {
            var datasetPath = uimfMsMsFile;

            var executorParameterFilename =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\UIMF_Targeted_MSMS_Testing\Parameters\UIMFTargetedMSMSWorkflowExecutorParametersLotsOfMassTags.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFilename);

            var executor = new BasicTargetedWorkflowExecutor(executorParameters, datasetPath);
            executor.Execute();
        }

        [Test]
        public void TestUIMFTargetedMSMSWorkflowCreateParametersManually()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var datasetPath = uimfMsMsFile;

            var executorParameters = new BasicTargetedWorkflowExecutorParameters
            {
                CopyRawFileLocal = false,
                DeleteLocalDatasetAfterProcessing = false,
                TargetsFilePath =
                    @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\UIMF_Targeted_MSMS_Testing\Targets\ConfidentTargets.txt",
#pragma warning disable 618
                TargetedAlignmentIsPerformed = false
#pragma warning restore 618
            };

            var workflowParameters = new UIMFTargetedMSMSWorkflowCollapseIMSParameters
            {
                AreaOfPeakToSumInDynamicSumming = 2,
                ChromatogramCorrelationIsPerformed = false,
                ChromGeneratorMode = Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK,
                ChromGenSourceDataPeakBR = 2,
                ChromGenSourceDataSigNoise = 3,
                ChromNETTolerance = 0.1,
                ChromPeakDetectorPeakBR = 1,
                ChromPeakDetectorSigNoise = 1,
                ChromPeakSelectorMode = Globals.PeakSelectorMode.SmartUIMF,
                ChromSmootherNumPointsInSmooth = 9,
                ChromGenTolerance = 25,
                MaxScansSummedInDynamicSumming = 100,
                MSPeakDetectorPeakBR = 1.3,
                MSPeakDetectorSigNoise = 3,
                MSToleranceInPPM = 25,
                MultipleHighQualityMatchesAreAllowed = true,
                NumMSScansToSum = 1,
                NumChromPeaksAllowedDuringSelection = int.MaxValue,
                ProcessMsMs = true,
                ResultType = Globals.ResultType.BASIC_TARGETED_RESULT,
                SummingMode = SummingModeEnum.SUMMINGMODE_STATIC
            };

            var executor = new UIMFTargetedWorkflowExecutor(executorParameters, workflowParameters, datasetPath);
            executor.Execute();
        }

    }
}
