using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.ProblemTesting
{
    [TestFixture]
    public class WorkflowsInitialTesting
    {
        [Test]
        public void O16O18_onBruker15T()
        {
            string executorParametersFile =
                @"\\protoapps\DataPkgs\Public\2012\641_Alz_O16O18_dataprocessing2\Parameters\ExecutorParameters1 - Copy.xml";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParametersFile);

            executorParameters.TargetsFilePath =
                @"\\protoapps\DataPkgs\Public\2012\641_Alz_O16O18_dataprocessing2\Targets\MT_Human_ALZ_O18_P852\MassTags_PMT2.txt";


            string testDatasetPath = @"D:\Data\From_Vlad\2013_01_29_ALZ_CTRL_5_0p05_1_01_224.d";


            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);

            int testTarget = 7673789;
            //executor.Targets.TargetList =executor.Targets.TargetList.Where(p => p.ID == testTarget && p.ChargeState==3).ToList();

            executor.Targets.TargetList = executor.Targets.TargetList.Take(50).ToList();


            executor.Execute();

        }


        public void ProcessSangTaeStuff()
        {
            string executorParametersFile =
                @"\\protoapps\UserData\Sangtae\ToSlysz\IQ\ExecutorParameters1.xml";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParametersFile);
            executorParameters.TargetType = Globals.TargetType.LcmsFeature;

            executorParameters.TargetsFilePath =@"\\protoapps\UserData\Sangtae\ToSlysz\IQ\Target_TD.txt";
            
            string testDatasetPath = @"\\protoapps\UserData\Sangtae\ToSlysz\IQ\QC_Shew_12_02_2_1Aug12_Cougar_12-06-11.raw";
            
            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);

            int testTarget = 7673789;
            //executor.Targets.TargetList =executor.Targets.TargetList.Where(p => p.ID == testTarget && p.ChargeState==3).ToList();

            string testTargetSeq = "SEVIAVVSSDTFVRPIYAGNALATVQSHDAVK";
            testTargetSeq = "ILQGYGAGHQFAAGGDGTAINQGGIAEQVTSAALNYR";
            executor.Targets.TargetList =executor.Targets.TargetList.Where(p => p.Code == testTargetSeq).ToList();


            //executor.Targets.TargetList = executor.Targets.TargetList.Take(50).ToList();
            executor.Execute();

            TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);


        }


    }
}
