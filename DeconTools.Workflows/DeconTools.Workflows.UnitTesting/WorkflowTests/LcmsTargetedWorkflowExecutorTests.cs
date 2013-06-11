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
            string executorParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\workflowExecutor_parameters.xml";
            WorkflowExecutorBaseParameters executorParameters = new LcmsFeatureTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            string testDatasetPath = FileRefs.SipperRawDataFile;
            string testDatasetName = "Yellow_C13_070_23Mar10_Griffin_10-01-28";

            string expectedResultsFilename = executorParameters.OutputFolderBase+ "\\Results" + "\\" + testDatasetName + "_results.txt";
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }


            TargetedWorkflowExecutor executor = new LcmsFeatureTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFilename));

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(expectedResultsFilename);
            Backend.Results.TargetedResultRepository repository = importer.Import();

            Assert.AreEqual(9, repository.Results.Count);

            TargetedResultDTO result1 = repository.Results[2];

            Assert.AreEqual(8586, result1.TargetID);
            Assert.AreEqual(2, result1.ChargeState);
            Assert.AreEqual(6512, result1.ScanLC);
            Assert.AreEqual(1543.82565m, (decimal)Math.Round(result1.MonoMass, 5));
          
        }


        [Test]
        public void executeWorkflow2()
        {
            string executorParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\workflowExecutor_parameters - Copy.xml";

            WorkflowExecutorBaseParameters executorParameters = new LcmsFeatureTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);
            
            string testDatasetPath = FileRefs.SipperRawDataFile;
           
            TargetedWorkflowExecutor executor = new LcmsFeatureTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();

          

        }


        [Test]
        public void executeWorkflow3()
        {
            string executorParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\workflowExecutor_parameters - Copy.xml";

            WorkflowExecutorBaseParameters executorParameters = new LcmsFeatureTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            string testDatasetPath = FileRefs.SipperRawDataFile;

            TargetedWorkflowExecutor executor = new LcmsFeatureTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            executor.Execute();



        }



    }
}
