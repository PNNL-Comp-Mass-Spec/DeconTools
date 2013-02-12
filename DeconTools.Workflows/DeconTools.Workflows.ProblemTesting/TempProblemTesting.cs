using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.ProblemTesting
{
    [TestFixture]
    public class TempProblemTesting
    {
        [Test]
        public void Test1()
        {
            string executorParameterFileName = @"D:\Data\IQ_testing\From_SangTae\ExecutorParameters1.xml";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFileName);

            string testDataFile = @"D:\Data\IQ_testing\From_SangTae\QC_Shew_12_02_2_1Aug12_Cougar_12-06-11.raw";

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDataFile);

            executor.Targets.TargetList = executor.Targets.TargetList.Take(10).ToList();
            
            executor.Execute();



        }

    }
}
