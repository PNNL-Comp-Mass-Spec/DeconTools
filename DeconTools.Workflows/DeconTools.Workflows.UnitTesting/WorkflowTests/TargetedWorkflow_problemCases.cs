using System;
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
            string rawdataFile = @"D:\Data\Orbitrap\QC_Shew_09_05-pt5-6_4Jan10_Doc_09-11-08.RAW";
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            string parameterFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\WorkflowParameterFiles\UnlabelledTargeted_WorkflowParameters_noSum.xml";

            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);

            MassTagCollection mtc = new MassTagCollection();
            mtc = mtimporter.Import();


            Run run = new RunFactory().CreateRun(rawdataFile);
            //RunUtilities.AlignRunUsingAlignmentInfoInFiles(run);

            string expectedPeaksFilename = run.DataSetPath + "\\" + run.DatasetName + "_peaks.txt";

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(expectedPeaksFilename, null);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            int testMassTagID = 3513677;
            int massTagChargeState = 2;

            run.CurrentMassTag = (from n in mtc.MassTagList where n.ID == testMassTagID && n.ChargeState == massTagChargeState select n).First();

            WorkflowParameters parameters = WorkflowParameters.CreateParameters(parameterFile);

            TargetedWorkflow workflow= TargetedWorkflow.CreateWorkflow(parameters);
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
            string rawdataFile = @"D:\Data\Orbitrap\QC_Shew_09_05-pt5-6_4Jan10_Doc_09-11-08.RAW";
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            string parameterFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\WorkflowParameterFiles\UnlabelledTargeted_WorkflowParameters_noSum.xml";

            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);

            MassTagCollection mtc = new MassTagCollection();
            mtc = mtimporter.Import();


            Run run = new RunFactory().CreateRun(rawdataFile);
            //RunUtilities.AlignRunUsingAlignmentInfoInFiles(run);

            string expectedPeaksFilename = run.DataSetPath + "\\" + run.DatasetName + "_peaks.txt";

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(expectedPeaksFilename, null);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            int testMassTagID = 25517;
            int massTagChargeState = 4;

            run.CurrentMassTag = (from n in mtc.MassTagList where n.ID == testMassTagID && n.ChargeState == massTagChargeState select n).First();

            WorkflowParameters parameters = WorkflowParameters.CreateParameters(parameterFile);

            TargetedWorkflow workflow = TargetedWorkflow.CreateWorkflow(parameters);
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
     

            string executorParameterFile = @"\\protoapps\UserData\Slysz\Data\Redmine_Issues\Issue0705_failedAlignment\workflowExecutorParameters.xml";
            string datasetPath = @"\\proto-3\LTQ_Orb_3\2010_1\QC_Shew_10_01-pt5-4_12Feb10_Doc_09-12-26\QC_Shew_10_01-pt5-4_12Feb10_Doc_09-12-26.raw";

             BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
             executorParameters.LoadParameters(executorParameterFile);

             TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, datasetPath);
             executor.Execute();
        }


    }
}
