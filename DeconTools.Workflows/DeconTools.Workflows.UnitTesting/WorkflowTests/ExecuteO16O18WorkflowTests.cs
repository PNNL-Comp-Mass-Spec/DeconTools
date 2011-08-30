using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Workflows.Backend.Core;
using System.IO;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class ExecuteO16O18WorkflowTests
    {
        [Test]
        public void Test1()
        {
            string executorParameterFile = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2011\O16O18_TargetedProcessing\WorkflowParameters\workflow_executor.xml";
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            string resultsFolderLocation = executorParameters.ResultsFolder;
            string testDatasetPath = @"D:\Data\O16O18\Weijun\TechTest_O18_02\TechTest_O18_02_RunA_10Dec09_Doc_09-11-08.RAW";
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
