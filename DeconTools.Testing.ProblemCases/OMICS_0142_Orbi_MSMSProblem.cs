using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class OMICS_0142_Orbi_MSMSProblem
    {
        [Test]
        public void general_msReading()
        {
            string testFile = @"D:\Data\UIMF\Problem_datasets\DsrC_Black_02_6Feb12_Cougar_11-10-11.raw";

            Run run = new RunFactory().CreateRun(testFile);

            int startScan = 1;
            int stopScan = 10;


            run.ScanSetCollection.Create(run, startScan, stopScan, 1, 1, true);
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            foreach (var scanSet in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanSet;
                msgen.Execute(run.ResultCollection);
            }

        }

        [Test]
        public void scanBasedWorkflowTest1()
        {
            string testFile = @"D:\Data\UIMF\Problem_datasets\DsrC_Black_02_6Feb12_Cougar_11-10-11.raw";
            string parameterFile = @"D:\Data\UIMF\Problem_datasets\LTQ_Orb_USTags_MS2_THRASH_WithPeaks_Relaxed.xml";


            Run run = new RunFactory().CreateRun(testFile);
            var parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.NewDeconToolsParameters.MSGeneratorParameters.UseLCScanRange = true;
            workflow.NewDeconToolsParameters.MSGeneratorParameters.MinLCScan = 1;
            workflow.NewDeconToolsParameters.MSGeneratorParameters.MaxLCScan = 5;

            workflow.ExportData = true;
            workflow.Execute();

            // foreach (var isosResult in run.ResultCollection.ResultList)
            // {
            //     isosResult.Display();
            // }



        }

    }
}
