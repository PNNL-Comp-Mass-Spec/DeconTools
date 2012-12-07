using System.Linq;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

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

			//TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);
		}

    }
}
