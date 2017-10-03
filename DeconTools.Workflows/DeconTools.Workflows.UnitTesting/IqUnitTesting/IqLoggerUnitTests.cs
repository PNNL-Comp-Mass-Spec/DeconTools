using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;
using log4net;

namespace DeconTools.Workflows.UnitTesting.IqUnitTesting
{
    public class IqLoggerUnitTests
    {

        [Test]
        public void IqLoggerUnitTest1()
        {
            var util = new IqTargetUtilities();
            var testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

            var targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            var resultsFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results";


            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.TargetsFilePath = targetsFile;

            var run = new RunFactory().CreateRun(testFile);

            var expectedResultsFilename = Path.Combine(resultsFolder, RunUtilities.GetDatasetName(testFile) + "_IqLog.txt");
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);

            var executor = new IqExecutor(executorBaseParameters, run);
            executor.ChromSourceDataFilePath = peaksTestFile;

            executor.LoadAndInitializeTargets(targetsFile);
            executor.Targets = (from n in executor.Targets where n.ElutionTimeTheor > 0.305 && n.ElutionTimeTheor < 0.325 select n).Take(10).ToList();

            var targetedWorkflowParameters = new BasicTargetedWorkflowParameters();
            targetedWorkflowParameters.ChromNETTolerance = 0.5;

            //define workflows for parentTarget and childTargets
            var parentWorkflow = new BasicIqWorkflow(run, targetedWorkflowParameters);
            var childWorkflow = new BasicIqWorkflow(run, targetedWorkflowParameters);

            var workflowAssigner = new IqWorkflowAssigner();
            workflowAssigner.AssignWorkflowToParent(parentWorkflow, executor.Targets);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, executor.Targets);

            //Main line for executing IQ:
            executor.Execute();

            //Test the Log File
            Assert.IsTrue(File.Exists(expectedResultsFilename), "IqLog.txt file doesn't exist");
            Console.WriteLine("");
            Console.WriteLine("Log File");
            var numLogs = 0;
            var outputToConsole = true;

            using (var reader = new StreamReader(expectedResultsFilename))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    numLogs++;

                    if (outputToConsole)
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            Console.WriteLine(numLogs);
            Assert.IsTrue(numLogs == 37, "No Logs in output file");
        }

    }
}
