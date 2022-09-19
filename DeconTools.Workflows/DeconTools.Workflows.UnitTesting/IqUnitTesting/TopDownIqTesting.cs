using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.IqUnitTesting
{
    public class TopDownIqTesting
    {

        //Test runs full datasets through the entire Top Down IQ process.
        //Gain knowledge about cutoffs and other metrics etc.
        [Test]
        public void MSAlignTargetDataTest()
        {
            var testFile = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Charles_Data\SBEP_STM_001_02222012_Aragon.raw";
            var targetsFile = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Charles_Data\salmonella_top_target.txt";
            var resultsFolder = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Charles_Data\Results";

            //Backend.Utilities.SipperDataDump.DataDumpSetup(@"\\protoapps\UserData\Fujimoto\TopDownTesting\Charles_Data\Results\detailed_results.txt");



            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.TargetsFilePath = targetsFile;
            executorBaseParameters.OutputDirectoryBase = resultsFolder;

            var run = new RunFactory().CreateRun(testFile);

            var executor = new TopDownMSAlignExecutor(executorBaseParameters, run);

            var s = Stopwatch.StartNew();
            executor.LoadAndInitializeTargets(targetsFile);
            s.Stop();
            var rt = s.Elapsed;
            Console.WriteLine("import: " + rt);

            var targetedWorkflowParameters = new BasicTargetedWorkflowParameters();
            targetedWorkflowParameters.ChromNETTolerance = 0.05;
            targetedWorkflowParameters.ChromatogramCorrelationIsPerformed = true;
            targetedWorkflowParameters.ChromSmootherNumPointsInSmooth = 15;
            targetedWorkflowParameters.MSToleranceInPPM = 15;
            targetedWorkflowParameters.NumMSScansToSum = 5;

            //define workflows for parentTarget and childTargets

            var parentWorkflow = new ChromPeakDeciderTopDownIqWorkflow(run, targetedWorkflowParameters);
            var childWorkflow = new ChargeStateChildTopDownIqWorkflow(run, targetedWorkflowParameters);

            var workflowAssigner = new IqWorkflowAssigner();
            workflowAssigner.AssignWorkflowToParent(parentWorkflow, executor.Targets);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, executor.Targets);

            var stopwatch = Stopwatch.StartNew();

            //Main line for executing IQ:
            executor.Execute();

            stopwatch.Stop();
            var runtime = stopwatch.Elapsed;
            Console.WriteLine("Runtime: " + runtime);
        }


        [Test]
        public void Get3DElutionAndExportToFileTest1()
        {
            var rawFile = @"\\protoapps\UserData\Fujimoto\TopDownPaperData\FINAL_DATA\_004\SBEP_STM_004_02272012_Aragon.raw";
            var peaksFile = @"\\protoapps\UserData\Fujimoto\TopDownPaperData\FINAL_DATA\_004\SBEP_STM_004_02272012_Aragon_peaks.txt";

            var run = RunUtilities.CreateAndLoadPeaks(rawFile, peaksFile);
            //var run = new RunFactory().CreateRun(rawFile);

            var outputFile = @"\\protoapps\UserData\Fujimoto\TopDownPaperData\FINAL_DATA\_004\3D_PLOT\3DelutionProfile.txt";


            Assert.IsNotNull(run);
            Assert.IsTrue(run.ResultCollection.MSPeakResultList.Count > 0);

            var extractor = new IsotopicProfileElutionExtractor();

            var minScan = 3270;
            var maxScan = 3350;
            double minMZ = 700;
            double maxMZ = 1700;


            extractor.Get3DElutionProfileFromPeakLevelData(run, minScan, maxScan, minMZ, maxMZ, out var scans, out var mzBinVals, out var intensities);

            var intensities2D = extractor.GetIntensitiesAs2DArray();
            extractor.OutputElutionProfileToFile(outputFile);

        }


    }
}
