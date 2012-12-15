using System.IO;
using DeconTools.Workflows.Backend;
using System.Linq;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class SpecialOrNewCasesWorkflowTests
    {
        [Test]
        public void Test1()
        {
            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.WorkflowParameterFile = @"D:\Data\From_Scott\BasicTargetedWorkflowParameters1.xml";
            executorParameters.TargetsFilePath = @"D:\Data\From_Scott\Glycan_targets.txt";
            //executorParameters.TargetType = Globals.TargetType.LcmsFeature;
            

            var testDatasetPath = @"D:\Data\From_Scott\Gly08_Velos4_Jaguar_200nL_Sp01_3X_7uL_1000A_31Aug12.raw";
            


            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);

            //executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == 18).ToList();
            executor.Execute();

        }

    }
}
