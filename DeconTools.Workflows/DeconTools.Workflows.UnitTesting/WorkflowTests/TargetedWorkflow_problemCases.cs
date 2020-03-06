using System;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class TargetedWorkflow_problemCases
    {




        [Test]
        public void Test1()
        {
            var rawdataFile = @"D:\Data\Orbitrap\QC_Shew_09_05-pt5-6_4Jan10_Doc_09-11-08.RAW";
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            var parameterFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\WorkflowParameterFiles\UnlabeledTargeted_WorkflowParameters_noSum.xml";

            var mtimporter = new MassTagFromTextFileImporter(massTagFile);

            var mtc = new TargetCollection();
            mtc = mtimporter.Import();


            var run = new RunFactory().CreateRun(rawdataFile);
            //RunUtilities.AlignRunUsingAlignmentInfoInFiles(run);

            var expectedPeaksFilename = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");

            var peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(expectedPeaksFilename, null);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            var testMassTagID = 3513677;
            var massTagChargeState = 2;

            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == massTagChargeState select n).First();

            var parameters = WorkflowParameters.CreateParameters(parameterFile);

            var workflow= TargetedWorkflow.CreateWorkflow(parameters);
            workflow.Run = run;

            workflow.Execute();



            workflow.Result.DisplayToConsole();


            RunUtilities.AlignRunUsingAlignmentInfoInFiles(run);
            workflow.Execute();
            workflow.Result.DisplayToConsole();

        }

        [Test]
        public void issue0690_tooManyChrompeaksWithinTolerance()
        {
            var rawdataFile = @"D:\Data\Orbitrap\QC_Shew_09_05-pt5-6_4Jan10_Doc_09-11-08.RAW";
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            var parameterFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\WorkflowParameterFiles\UnlabeledTargeted_WorkflowParameters_noSum.xml";

            var mtimporter = new MassTagFromTextFileImporter(massTagFile);

            var mtc = new TargetCollection();
            mtc = mtimporter.Import();


            var run = new RunFactory().CreateRun(rawdataFile);
            //RunUtilities.AlignRunUsingAlignmentInfoInFiles(run);

            var expectedPeaksFilename = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");

            var peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(expectedPeaksFilename, null);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            var testMassTagID = 25517;
            var massTagChargeState = 4;

            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == massTagChargeState select n).First();

            var parameters = WorkflowParameters.CreateParameters(parameterFile);

            var workflow = TargetedWorkflow.CreateWorkflow(parameters);
            workflow.Run = run;

            RunUtilities.AlignRunUsingAlignmentInfoInFiles(run);
            workflow.Execute();

            Console.WriteLine("NumChromPeaksWithinTol = " + workflow.Result.NumChromPeaksWithinTolerance);
            Console.WriteLine("NumQualityChromPeaksWithinTol = " + workflow.Result.NumQualityChromPeaks);

            workflow.Result.DisplayToConsole();


        }



        [Test]
        public void issue0705_completelyFailedAlignment_multialignErrors()
        {


            var executorParameterFile = @"\\protoapps\UserData\Slysz\Data\Redmine_Issues\Issue0705_failedAlignment\workflowExecutorParameters.xml";
            var datasetPath = @"\\proto-3\LTQ_Orb_3\2010_1\QC_Shew_10_01-pt5-4_12Feb10_Doc_09-12-26\QC_Shew_10_01-pt5-4_12Feb10_Doc_09-12-26.raw";

             var executorParameters = new BasicTargetedWorkflowExecutorParameters();
             executorParameters.LoadParameters(executorParameterFile);

             TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, datasetPath);
             executor.Execute();
        }


        [Test]
        public void issue0725_targetedAlignmentProblems()
        {

            var executorParameterFile = @"D:\Data\Orbitrap\Issue0725_badAlignment\Issue0725_executorWorkflow.xml";
            var datasetPath = @"D:\Data\Orbitrap\Issue0725_badAlignment\QC_Shew_10_03-2_100min_06May10_Tiger_10-04-08.RAW";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, datasetPath);
            executor.Execute();


        }


    }
}
