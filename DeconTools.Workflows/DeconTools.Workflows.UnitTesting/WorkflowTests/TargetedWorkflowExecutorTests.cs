using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class TargetedWorkflowExecutorTests
    {
        [Test]
        public void Test1()
        {

            string executorParameterFile = @"\\protoapps\UserData\Slysz\Data\WorkflowExecutor_Parameters\basicTargetedWorkflowExecutorParameters_TestCase1.xml";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters);
            executor.Execute();


        }

    }
}
