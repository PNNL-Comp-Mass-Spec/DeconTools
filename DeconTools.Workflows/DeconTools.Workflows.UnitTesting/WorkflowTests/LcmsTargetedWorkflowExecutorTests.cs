using System;
using System.IO;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    public class LcmsTargetedWorkflowExecutorTests
    {

        [Test]
        public void executeWorkflow1()
        {
            var executorParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\workflowExecutor_parameters.xml";
            WorkflowExecutorBaseParameters executorParameters = new LcmsFeatureTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            var testDatasetPath = FileRefs.SipperRawDataFile;
            var testDatasetName = "Yellow_C13_070_23Mar10_Griffin_10-01-28";

            var expectedResultsFilename = executorParameters.OutputDirectoryBase+ "\\Results" + "\\" + testDatasetName + "_results.txt";
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }


            TargetedWorkflowExecutor executor = new LcmsFeatureTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFilename));

            var importer = new SipperResultFromTextImporter(expectedResultsFilename);
            var repository = importer.Import();

            Assert.AreEqual(9, repository.Results.Count);

            var result1 = repository.Results[2];

            Assert.AreEqual(8586, result1.TargetID);
            Assert.AreEqual(2, result1.ChargeState);
            Assert.AreEqual(6512, result1.ScanLC);
            Assert.AreEqual(1543.82565m, (decimal)Math.Round(result1.MonoMass, 5));
          
        }


        [Test]
        public void executeWorkflow2()
        {
            var executorParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\workflowExecutor_parameters - Copy.xml";

            WorkflowExecutorBaseParameters executorParameters = new LcmsFeatureTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);
            
            var testDatasetPath = FileRefs.SipperRawDataFile;
           
            TargetedWorkflowExecutor executor = new LcmsFeatureTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

          

        }


        [Test]
        public void executeWorkflow3()
        {
            var executorParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\workflowExecutor_parameters - Copy.xml";

            WorkflowExecutorBaseParameters executorParameters = new LcmsFeatureTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            var testDatasetPath = FileRefs.SipperRawDataFile;

            TargetedWorkflowExecutor executor = new LcmsFeatureTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();



        }



    }
}
