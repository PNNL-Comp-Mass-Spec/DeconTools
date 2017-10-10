using System;
using System.Linq;
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
            var executorParameters =
                new BasicTargetedWorkflowExecutorParameters
                {
                    WorkflowParameterFile = @"D:\Data\From_Scott\BasicTargetedWorkflowParameters1.xml",
                    TargetsFilePath = @"D:\Data\From_Scott\Glycan_targets.txt"
                };
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
            var executorParameters = new BasicTargetedWorkflowExecutorParameters
            {
                TargetsFilePath = @"\\protoapps\DataPkgs\Public\2013\686_IQ_analysis_of_heme_peptides\Targets\SL-MtoA_peptides_formulas.txt"
            };


            var testDatasetPath = @"D:\Data\From_EricMerkley\HisHemeSL-MtrA_002_2Feb11_Sphinx_10-12-01.RAW";



            var workflowParameters = new BasicTargetedWorkflowParameters
            {
                ChromNETTolerance = 0.5,
                ChromSmootherNumPointsInSmooth = 9
            };

            var workflow = new BasicTargetedWorkflow(workflowParameters);

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters,workflow, testDatasetPath);

            var testTargetID = 750;
            var testTargetZ = 5;


            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTargetID
                && p.ChargeState == testTargetZ).ToList();

            //executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTargetID).ToList();

            executor.InitializeRun(testDatasetPath);
            executor.Run.CurrentMassTag = executor.Targets.TargetList.First();
            double[] chromParamValues = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10,15,25};

            //chromPeakBRValues =new double[] {10,15,25};

            foreach (var value in chromParamValues)
            {
                if (executor.TargetedWorkflow.WorkflowParameters is BasicTargetedWorkflowParameters parameters)
                {
                    parameters.ChromPeakDetectorPeakBR = 5;
                    parameters.ChromPeakDetectorSigNoise = value;

                    executor.TargetedWorkflow = new BasicTargetedWorkflow(executor.Run, parameters);
                }

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

            var parameterFileName = @"C:\Users\d3x720\Documents\Data\QCShew\IQ\IQExecutorParameterFile1.xml";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters
            {
                TargetsFilePath = @"C:\Users\d3x720\Documents\Data\QCShew\IQ\QCShew_Formic_MassTags_Bin10_first10.txt"
            };

            executorParameters.SaveParametersToXML(parameterFileName);


            var workflowParameters=new BasicTargetedWorkflowParameters();


            var workflow = new BasicTargetedWorkflow(workflowParameters);

            var testDatasetPath =
                @"C:\Users\d3x720\Documents\Data\QCShew\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, workflow,
                                                                                  testDatasetPath);
            var testTargetID = 24749;
            var testTargetZ = 3;



            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTargetID
                && p.ChargeState == testTargetZ).ToList();

            executor.Execute();


            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);

            foreach (var chrompeak in executor.TargetedWorkflow.ChromPeaksDetected)
            {
                Console.WriteLine(chrompeak.XValue.ToString("0.0000") + "\t"+ chrompeak.Height.ToString("0") + "\t"+chrompeak.Width.ToString("0.000") + "\n");
            }


        }


        [Test]
        public void ChaoChaoPeptidomicsTesting1()
        {
            var executorParameters =
                @"\\protoapps\DataPkgs\Public\2013\727_IQ_analysis_of_Peptidomics_data_First_attempts\Parameters\ExecutorParameters1.xml";

            var alignmentParametersFile =
                @"\\protoapps\DataPkgs\Public\2013\727_IQ_analysis_of_Peptidomics_data_First_attempts\Parameters\TargetedAlignmentWorkflowParameters1.xml";

            var rawDatafile = @"D:\Data\From_ChaoChao\CPTAC_OT_Pep_JB_5439_60min_4May12_Legolas_11-07-64.raw";

            var parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.LoadParameters(executorParameters);

#pragma warning disable 618
            parameters.TargetedAlignmentIsPerformed = true;
#pragma warning restore 618

            var targetedAlignerWorkflowParameters = new TargetedAlignerWorkflowParameters();
            targetedAlignerWorkflowParameters.LoadParameters(alignmentParametersFile);

            var alignmentParametersOutputfile = alignmentParametersFile.Replace(".xml", "_autoGenerated.xml");
            targetedAlignerWorkflowParameters.SaveParametersToXML(alignmentParametersOutputfile);

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(parameters, rawDatafile);
            executor.Execute();






        }


    }
}
