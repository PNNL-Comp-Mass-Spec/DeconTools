using System;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
using NUnit.Framework;
using System.Collections.Generic;
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
            string testDatasetPath = uimfTestfile1;

            string executorParameterFilename =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\UIMF_O16O18Testing\Parameters\WorkflowExecutorParameters.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();

            executorParameters.LoadParameters(executorParameterFilename);

            string outputExecutorParameterFilename = executorParameterFilename.Replace(".xml", "_autoGen.xml");
            executorParameters.SaveParametersToXML(outputExecutorParameterFilename);


            int testTarget = 7563580;

            var executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTarget).ToList();

            executor.Execute();

        }

		[Test]
		public void TestUIMFTargetedMSMSWorkflowSingleTarget()
		{
			string datasetPath = uimfMsMsFile;

			string executorParameterFilename =
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
			string datasetPath = uimfMsMsFile;

			string executorParameterFilename =
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
			string datasetPath = uimfMsMsFile;

			string executorParameterFilename =
				@"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\UIMF_Targeted_MSMS_Testing\Parameters\UIMFTargetedMSMSWorkflowExecutorParametersLotsOfMassTags.xml";

			var executorParameters = new BasicTargetedWorkflowExecutorParameters();
			executorParameters.LoadParameters(executorParameterFilename);

			var executor = new BasicTargetedWorkflowExecutor(executorParameters, datasetPath);
			executor.Execute();
		}

		[Test]
		public void TestUIMFTargetedMSMSWorkflowCreateParametersManually()
		{
			string currentDirectory = Directory.GetCurrentDirectory();

			string datasetPath = uimfMsMsFile;

			var executorParameters = new BasicTargetedWorkflowExecutorParameters();
			executorParameters.CopyRawFileLocal = false;
			executorParameters.DeleteLocalDatasetAfterProcessing = false;
			executorParameters.TargetsFilePath = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\UIMF_Targeted_MSMS_Testing\Targets\ConfidentTargets.txt";
			executorParameters.TargetedAlignmentIsPerformed = false;

			var workflowParameters = new UIMFTargetedMSMSWorkflowCollapseIMSParameters();
			workflowParameters.AreaOfPeakToSumInDynamicSumming = 2;
			workflowParameters.ChromatogramCorrelationIsPerformed = false;
			workflowParameters.ChromGeneratorMode = Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
			workflowParameters.ChromGenSourceDataPeakBR = 2;
			workflowParameters.ChromGenSourceDataSigNoise = 3;
			workflowParameters.ChromNETTolerance = 0.1;
			workflowParameters.ChromPeakDetectorPeakBR = 1;
			workflowParameters.ChromPeakDetectorSigNoise = 1;
			workflowParameters.ChromPeakSelectorMode = Globals.PeakSelectorMode.SmartUIMF;
			workflowParameters.ChromSmootherNumPointsInSmooth = 9;
			workflowParameters.ChromGenTolerance = 25;
			workflowParameters.MaxScansSummedInDynamicSumming = 100;
			workflowParameters.MSPeakDetectorPeakBR = 1.3;
			workflowParameters.MSPeakDetectorSigNoise = 3;
			workflowParameters.MSToleranceInPPM = 25;
			workflowParameters.MultipleHighQualityMatchesAreAllowed = true;
			workflowParameters.NumMSScansToSum = 1;
			workflowParameters.NumChromPeaksAllowedDuringSelection = int.MaxValue;
			workflowParameters.ProcessMsMs = true;
			workflowParameters.ResultType = Globals.ResultType.BASIC_TARGETED_RESULT;
			workflowParameters.SummingMode = SummingModeEnum.SUMMINGMODE_STATIC;

			var executor = new UIMFTargetedWorkflowExecutor(executorParameters, workflowParameters, datasetPath);
			executor.Execute();
		}

    }
}
