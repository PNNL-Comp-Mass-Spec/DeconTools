using System.IO;
using System.Linq;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class O16O18ExecutorTests
    {


        [Test]
        public void StandardO16O18Testing_VladAlz()
        {
            string executorParametersFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Parameters\ExecutorParameters1.xml";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParametersFile);

            string testDatasetPath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\RawData\Alz_P01_A01_097_26Apr12_Roc_12-03-15.RAW";

            int testTarget = 24653;

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTarget).ToList();

            //executor.InitializeRun(testDatasetPath);
            //executor.TargetedWorkflow.Run = executor.Run;

            //foreach (var targetBase in executor.Targets.TargetList)
            //{
            //    executor.Run.CurrentMassTag = targetBase;
            //    var workflow = (O16O18Workflow)executor.TargetedWorkflow;

            //    workflow.Execute();
            //    var result = workflow.Result as DeconTools.Backend.Core.Results.LcmsFeatureTargetedResult;


            //}

            executor.Execute();
        }


        [Test]
        public void TestErnestosData1()
        {

            string baseFolder = @"\\protoapps\DataPkgs\Public\2012\573_O16O18_Data_analysis_of_ubiquintated_peptides";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.TargetsBaseFolder = baseFolder + Path.DirectorySeparatorChar + "Targets";
            executorParameters.WorkflowParameterFile = baseFolder + Path.DirectorySeparatorChar + "Parameters" + Path.DirectorySeparatorChar + "O16O18WorkflowParameters_2011_08_23_sum5.xml";

            executorParameters.TargetType = Globals.TargetType.LcmsFeature;
            executorParameters.ResultsFolder = baseFolder + Path.DirectorySeparatorChar + "Results\\Testing";
            executorParameters.LoggingFolder = baseFolder + Path.DirectorySeparatorChar + "Logs";

            string testDatasetPath = @"\\protoapps\UserData\Slysz\Data\O16O18\Ernesto\PSI_LRW_18O_02A_18Jun12_Falcon_12-03-34.RAW";
            testDatasetPath = @"D:\Data\O16O18\Ernesto\PSI_LRW_1to1_03A_6Jul12_Falcon_12-06-04.raw";

            string outputtedParameterFile =
                @"\\protoapps\DataPkgs\Public\2012\573_O16O18_Data_analysis_of_ubiquintated_peptides\Parameters\ExecutorParameters1.xml";

            //executorParameters.SaveParametersToXML(outputtedParameterFile);


            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();


        }

        [Test]
        public void TestErnestosData2_createExecutorFile()
        {
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.TargetsBaseFolder = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\O16O18_Targeted\2012_06_27_Ernesto";
            executorParameters.WorkflowParameterFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\O16O18_Targeted\2012_06_27_Ernesto\O16O18WorkflowParameters_2011_08_23_sum5.xml";

            executorParameters.TargetType = Globals.TargetType.LcmsFeature;


            string testDatasetPath = @"\\protoapps\UserData\Slysz\Data\O16O18\Ernesto\PSI_URW_1to1_01A_18Jun12_Falcon_12-03-37.RAW";

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();


        }

    


    }
}
