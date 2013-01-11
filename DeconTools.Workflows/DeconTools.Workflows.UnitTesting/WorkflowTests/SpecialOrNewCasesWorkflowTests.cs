using System.Linq;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class SpecialOrNewCasesWorkflowTests
    {
        [Test]
        public void GlycanProcessingTest1()
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

        [Test]
        public void HemePeptidesProcessingTest1()
        {
            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.WorkflowParameterFile =
                @"\\protoapps\DataPkgs\Public\2013\686_IQ_analysis_of_heme_peptides\Parameters\BasicTargetedWorkflowParameters1.xml";
            executorParameters.TargetsFilePath =
                @"\\protoapps\DataPkgs\Public\2013\686_IQ_analysis_of_heme_peptides\Targets\SL-MtoA_peptides_formulas.txt";



            var testDatasetPath = @"D:\Data\From_EricMerkley\HisHemeSL-MtrA_002_2Feb11_Sphinx_10-12-01.RAW";
            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);


            int testTargetID = 1950;
            int testTargetZ = 3;

            testTargetID = 240;
            testTargetZ = 4;

            testTargetID = 359;
            testTargetZ = 3;

            testTargetID = 750;
            testTargetZ = 5;


            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTargetID
                && p.ChargeState == testTargetZ).ToList();

            //executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTargetID).ToList();

            executor.Execute();

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);
            //TestUtilities.DisplayIsotopicProfileData(executor.TargetedWorkflow.Result.Target.IsotopicProfile);

           // Console.WriteLine(executor.TargetedWorkflow.Result.Target.EmpiricalFormula);

        }

    }
}
