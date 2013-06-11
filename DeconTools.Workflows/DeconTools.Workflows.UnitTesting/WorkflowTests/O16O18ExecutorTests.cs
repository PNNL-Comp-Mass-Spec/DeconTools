using System;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    [Category("Functional")]
    public class O16O18ExecutorTests
    {

        [Test]
        [Category("MustPass")]
        public void IqExecutor_StandardO16O18Testing_VladAlz()
        {
            //see JIRA https://jira.pnnl.gov/jira/browse/OMCS-628

            string executorParametersFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Parameters\ExecutorParameters1.xml";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParametersFile);

            string testDatasetPath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\RawData\Alz_P01_A01_097_26Apr12_Roc_12-03-15.RAW";


            string resultsFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results";

            string expectedResultsFilename = resultsFolder + "\\" + RunUtilities.GetDatasetName(testDatasetPath) + "_iqResults.txt";
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);



            string autoSavedExecutorParametersFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Parameters\ExecutorParameters1_autosaved.xml";
            executorParameters.SaveParametersToXML(autoSavedExecutorParametersFile);

            Run run = new RunFactory().CreateRun(testDatasetPath);

            var executor = new IqExecutor(executorParameters, run);

            string targetsFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Targets\MT_Human_ALZ_O18_P836\MassTags_PMT2_First60.txt";

            executor.LoadAndInitializeTargets(targetsFile);


            int testTarget = 9282;
            executor.Targets = (from n in executor.Targets where n.ID ==testTarget select n).ToList();


            var targetedWorkflowParameters = new BasicTargetedWorkflowParameters();
            targetedWorkflowParameters.ChromNETTolerance = 0.5;

            //define workflows for parentTarget and childTargets
            var parentWorkflow = new ChromPeakDeciderIqWorkflow(run, targetedWorkflowParameters);
            var childWorkflow = new O16O18IqWorkflow(run, targetedWorkflowParameters);

            IqWorkflowAssigner workflowAssigner = new IqWorkflowAssigner();
            workflowAssigner.AssignWorkflowToParent(parentWorkflow, executor.Targets);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, executor.Targets);

            executor.Execute();




            //var importer = new O16O18TargetedResultFromTextImporter(expectedResultsFilename);
            //Backend.Results.TargetedResultRepository repository = importer.Import();

            //Assert.AreEqual(3, repository.Results.Count);
            //var result1 = repository.Results[1] as O16O18TargetedResultDTO;

            //Assert.AreEqual(9282, result1.TargetID);
            //Assert.AreEqual(2, result1.ChargeState);
            //Assert.AreEqual(4537, result1.ScanLC);
            //Assert.AreEqual(0.32514m, (decimal)Math.Round(result1.NET, 5));
            //Assert.AreEqual(-0.001662m, (decimal)Math.Round(result1.NETError, 6));

            //Assert.AreEqual(0.274m, (decimal)Math.Round(result1.Ratio, 3));
            //Assert.IsTrue(result1.ChromCorrO16O18DoubleLabel > 0);

            //Console.WriteLine(result1.ToStringWithDetailsAsRow());

        }





        [Test]
        [Category("MustPass")]
        public void StandardO16O18Testing_VladAlz()
        {
            //see JIRA https://jira.pnnl.gov/jira/browse/OMCS-628

            string executorParametersFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Parameters\ExecutorParameters1.xml";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParametersFile);

            string testDatasetPath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\RawData\Alz_P01_A01_097_26Apr12_Roc_12-03-15.RAW";

            string autoSavedExecutorParametersFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Parameters\ExecutorParameters1_autosaved.xml";
            executorParameters.SaveParametersToXML(autoSavedExecutorParametersFile);
            executorParameters.OutputFolderBase =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz";

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);

            int testTarget = 9282;
            executor.Targets.TargetList =
                executor.Targets.TargetList.Where(p => p.ID == testTarget).ToList();

            
            //executor.InitializeRun(testDatasetPath);
            //executor.TargetedWorkflow.Run = executor.Run;

            //foreach (var targetBase in executor.Targets.TargetList)
            //{
            //    executor.Run.CurrentMassTag = targetBase;
            //    var workflow = (O16O18Workflow)executor.TargetedWorkflow;

            //    workflow.Execute();
            //    var result = workflow.Result as DeconTools.Backend.Core.Results.LcmsFeatureTargetedResult;


            //}

            executor.Execute();

            string expectedResultsFilename = executorParameters.OutputFolderBase + "\\Results" + "\\" +
                                             executor.TargetedWorkflow.Run.DatasetName + "_Results.txt";

            var importer = new O16O18TargetedResultFromTextImporter(expectedResultsFilename);
            Backend.Results.TargetedResultRepository repository = importer.Import();

            Assert.AreEqual(3, repository.Results.Count);
            var result1 = repository.Results[1] as O16O18TargetedResultDTO;

            Assert.AreEqual(9282, result1.TargetID);
            Assert.AreEqual(2, result1.ChargeState);
            Assert.AreEqual(4537, result1.ScanLC);
            Assert.AreEqual(0.32514m, (decimal)Math.Round(result1.NET, 5));
            Assert.AreEqual(-0.001662m, (decimal)Math.Round(result1.NETError, 6));

            Assert.AreEqual(0.274m, (decimal)Math.Round(result1.Ratio, 3));
            Assert.IsTrue(result1.ChromCorrO16O18DoubleLabel > 0);

            Console.WriteLine(result1.ToStringWithDetailsAsRow());

        }


        [Test]
        public void tempTest1()
        {
            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.TargetsFilePath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\MT_Mouse_MHP_O18_Set1_P890_targets.txt";

            executorParameters.WorkflowParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Parameters\O16O18WorkflowParameters_2011_08_23_sum5.xml";

            executorParameters.TargetedAlignmentWorkflowParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Parameters\TargetedAlignmentWorkflowParameters1.xml";

            executorParameters.TargetsUsedForAlignmentFilePath = executorParameters.TargetsFilePath;

            //executorParameters.AlignmentInfoFolder = 

            //string testDatasetPath = @"D:\Data\O16O18\Vlad_Mouse\mhp_plat_test_1_14April13_Frodo_12-12-04.raw";

            //TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);
            //executor.Execute();

        }



        [Category("ProblemTesting")]
        [Test]
        public void O16O18Workflow_ProblemCaseTesting1()
        {

            //This is a nice case where the O16 is quite low and can be missed. In the current settings
            //the O16Chrom is null (by itself), so the chrom correlation fails.
            //Thus quant based on chrom corr fails, but quant based on O16O18 feature finding succeeds. 


            //7673789

            string executorParametersFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Parameters\ExecutorParameters1.xml";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParametersFile);

            string testDatasetPath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\RawData\Alz_P01_A01_097_26Apr12_Roc_12-03-15.RAW";

            string autoSavedExecutorParametersFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Parameters\ExecutorParameters1_autosaved.xml";
            executorParameters.SaveParametersToXML(autoSavedExecutorParametersFile);

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);

            int testTarget = 7415;
            executor.Targets.TargetList =
                executor.Targets.TargetList.Where(p => p.ID == testTarget).ToList();

            executor.Execute();

            string expectedResultsFilename = executorParameters.OutputFolderBase+ "\\Results" + "\\" +
                                             executor.TargetedWorkflow.Run.DatasetName + "_results.txt";

            var importer = new O16O18TargetedResultFromTextImporter(expectedResultsFilename);
            var results = importer.Import().Results;
            var result1 = results.First();

            Console.WriteLine(result1.ToStringWithDetailsAsRow());

        }


        [Category("ProblemTesting")]
        [Test]
        public void O16O18Workflow_ProblemCaseTesting2()
        {

            string executorParametersFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Parameters\ExecutorParameters1.xml";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParametersFile);

            executorParameters.TargetsFilePath =
                @"\\protoapps\DataPkgs\Public\2012\641_Alz_O16O18_dataprocessing2\Targets\MT_Human_ALZ_O18_P852\MassTags_PMT2.txt";


            string testDatasetPath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\RawData\Alz_P01_A01_097_26Apr12_Roc_12-03-15.RAW";

            string autoSavedExecutorParametersFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Parameters\ExecutorParameters1_autosaved.xml";
            executorParameters.SaveParametersToXML(autoSavedExecutorParametersFile);

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);

            int testTarget = 7673789;
            //executor.Targets.TargetList =executor.Targets.TargetList.Where(p => p.ID == testTarget && p.ChargeState==3).ToList();

            executor.Execute();

            string expectedResultsFilename = executorParameters.OutputFolderBase+ "\\Results"+   "\\" +
                                             executor.TargetedWorkflow.Run.DatasetName + "_results.txt";

            var importer = new O16O18TargetedResultFromTextImporter(expectedResultsFilename);
            var results = importer.Import().Results;
            var result1 = results.First();

            Console.WriteLine(result1.ToStringWithDetailsAsRow());

        }


    }
}
