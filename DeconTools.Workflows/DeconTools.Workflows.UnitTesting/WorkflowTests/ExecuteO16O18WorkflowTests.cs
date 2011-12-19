using System.IO;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class ExecuteO16O18WorkflowTests
    {

   



        [Test]
        public void Test1()
        {
            string executorParameterFile =
                @"D:\Data\O16O18\Vlad_ALZ\2011_09_28_Vlad_ALZ_reps\Workflow_Parameters\workflow_executor.xml";
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            string resultsFolderLocation = executorParameters.ResultsFolder;
            string testDatasetPath = @"D:\Data\O16O18\Vlad_ALZ\Alz_CV_O18_01_31Aug11_Falcon_11-06-05.RAW";
            string testDatasetName = Path.GetFileName(testDatasetPath).Replace(".RAW", "");

            string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_results.txt";
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }



            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

        }

    }
}
