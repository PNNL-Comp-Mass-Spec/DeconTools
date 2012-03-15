using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class SipperExecutorTests
    {
        [Test]
        public void test1()
        {
            string paramFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            string exportedParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperExecutorParams1.xml";
            parameters.SaveParametersToXML(exportedParameterFile);


            string testDataset =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);


            Assert.IsTrue(executor.ExecutorParameters.WorkflowType ==
                          Globals.TargetedWorkflowTypes.SipperWorkflowExecutor1);

            executor.Execute();
        }


        [Test]
        public void test2()
        {
            string paramFile =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);
            parameters.FolderPathForCopiedRawDataset = @"D:\Data\Temp";

            string testDataset =
                @"\\proto-9\VOrbiETD01\2010_1\Yellow_C13_103_26Mar10_Griffin_10-01-26\Yellow_C13_103_26Mar10_Griffin_10-01-26.raw";

            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);


            Assert.IsTrue(executor.ExecutorParameters.WorkflowType ==
                          Globals.TargetedWorkflowTypes.SipperWorkflowExecutor1);

            executor.Execute();
        }





        [Test]
        public void dbTest1()
        {
            string[] datasetNames = new string[]
                                        {
                                            "Yellow_C13_068_30Mar10_Griffin_10-01-28",
                                            "Yellow_C13_064_30Mar10_Griffin_10-03-01",
                                            "Yellow_C13_063_30Mar10_Griffin_10-01-13"
                                        };

            DatasetUtilities utilities = new DatasetUtilities();
            var path=   utilities.GetDatasetPath(datasetNames[0]);

            Console.WriteLine(path);

        }


    }
}
