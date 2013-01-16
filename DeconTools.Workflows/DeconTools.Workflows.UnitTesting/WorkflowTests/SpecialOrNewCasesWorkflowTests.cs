using System;
using System.Linq;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class SpecialOrNewCasesWorkflowTests
    {
        [Ignore("For dev only")]
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

    //    [Ignore("For dev only")]
        [Test]
        public void HemePeptidesProcessingTest1()
        {
            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
             executorParameters.TargetsFilePath =
                @"\\protoapps\DataPkgs\Public\2013\686_IQ_analysis_of_heme_peptides\Targets\SL-MtoA_peptides_formulas.txt";

            
            var testDatasetPath = @"D:\Data\From_EricMerkley\HisHemeSL-MtrA_002_2Feb11_Sphinx_10-12-01.RAW";



            BasicTargetedWorkflowParameters workflowParameters = new BasicTargetedWorkflowParameters();
            workflowParameters.ChromNETTolerance = 0.5;
            workflowParameters.ChromSmootherNumPointsInSmooth = 9;

            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(workflowParameters);

            
            
            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters,workflow, testDatasetPath);


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

            executor.InitializeRun(testDatasetPath);
            executor.Run.CurrentMassTag = executor.Targets.TargetList.First();
            double[] chromParamValues = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10,15,25};

            //chromPeakBRValues =new double[] {10,15,25};

            foreach (var value in chromParamValues)
            {
                var parameters = executor.TargetedWorkflow.WorkflowParameters as BasicTargetedWorkflowParameters;
                parameters.ChromPeakDetectorPeakBR = 5;
                parameters.ChromPeakDetectorSigNoise = value;

                executor.TargetedWorkflow = new BasicTargetedWorkflow(executor.Run, parameters);
               
                executor.TargetedWorkflow.Execute();

                Console.WriteLine("PeakBR=" + value + " num chrom peaks= " +   executor.TargetedWorkflow.ChromPeaksDetected.Count);
                foreach (var chrompeak in executor.TargetedWorkflow.ChromPeaksDetected)
                {
                    Console.WriteLine(chrompeak.XValue.ToString("0.0000") + "\t" + chrompeak.Height.ToString("0") + "\t" + chrompeak.Width.ToString("0.000"));
                }

            }

            



            foreach (var chrompeak in executor.TargetedWorkflow.ChromPeaksDetected)
            {
                Console.WriteLine(chrompeak.XValue.ToString("0.0000") + "\t" + chrompeak.Height.ToString("0") + "\t" + chrompeak.Width.ToString("0.000"));
            }

            Console.WriteLine();
            Console.WriteLine();

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);
            //TestUtilities.DisplayIsotopicProfileData(executor.TargetedWorkflow.Result.Target.IsotopicProfile);

           // Console.WriteLine(executor.TargetedWorkflow.Result.Target.EmpiricalFormula);

        }


  //      [Ignore("For dev only")]
        [Test]
        public void LocalQCShewProcessingTest1()
        {

            string parameterFileName = @"C:\Users\d3x720\Documents\Data\QCShew\IQ\IQExecutorParameterFile1.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.TargetsFilePath =
                @"C:\Users\d3x720\Documents\Data\QCShew\IQ\QCShew_Formic_MassTags_Bin10_first10.txt";

            executorParameters.SaveParametersToXML(parameterFileName);


            BasicTargetedWorkflowParameters workflowParameters=new BasicTargetedWorkflowParameters();


            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(workflowParameters);

            var testDatasetPath =
                @"C:\Users\d3x720\Documents\Data\QCShew\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, workflow,
                                                                                  testDatasetPath);
            int testTargetID = 24749;
            int testTargetZ = 3;



            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTargetID
                && p.ChargeState == testTargetZ).ToList();

            executor.Execute();
            

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);

            foreach (var chrompeak in executor.TargetedWorkflow.ChromPeaksDetected)
            {
                Console.WriteLine(chrompeak.XValue.ToString("0.0000") + "\t"+ chrompeak.Height.ToString("0") + "\t"+chrompeak.Width.ToString("0.000") + "\n");
            }


        }


    }
}
