using System;
using DeconTools.Backend.Core;
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
            var parameters = new OldDecon2LSParameters();
            parameters.Load(parameterFile);


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.OldDecon2LsParameters.HornTransformParameters.UseScanRange = true;
            workflow.OldDecon2LsParameters.HornTransformParameters.MinScan = 1;
            workflow.OldDecon2LsParameters.HornTransformParameters.MaxScan = 5;

            workflow.ExportData = true;
            workflow.Execute();

            // foreach (var isosResult in run.ResultCollection.ResultList)
            // {
            //     isosResult.Display();
            // }



        }


        [Test]
        public void checkScanSetTargetsTest1()
        {
            string testFile = @"D:\Data\UIMF\Problem_datasets\DsrC_Black_02_6Feb12_Cougar_11-10-11.raw";
            string parameterFile = @"D:\Data\UIMF\Problem_datasets\LTQ_Orb_USTags_MS2_THRASH_WithPeaks_Relaxed.xml";

            Run run = new RunFactory().CreateRun(testFile);

            OldDecon2LSParameters parameters = new OldDecon2LSParameters();
            parameters.Load(parameterFile);



            run.ScanSetCollection.Create(run, 1, 10,
                          parameters.HornTransformParameters.NumScansToSumOver * 2 + 1,
                          parameters.HornTransformParameters.NumScansToAdvance,
                          parameters.HornTransformParameters.ProcessMSMS);

            foreach (var scanSet in run.ScanSetCollection.ScanSetList)
            {
                Console.WriteLine(scanSet);
            }


        }

    }
}
