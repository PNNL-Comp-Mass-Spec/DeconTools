using System;
using System.IO;
using System.Linq;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;
using PRISM.Logging;

namespace DeconTools.Workflows.UnitTesting.IqUnitTesting
{
    public class IqLoggerUnitTests
    {

        [Test]
        public void IqLoggerUnitTest1()
        {
            // var util = new IqTargetUtilities();
            var testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            const string peaksTestFile = @"\\proto-2\unitTest_Files\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

            const string targetsFile = @"\\proto-2\unitTest_Files\DeconTools_TestFiles\Targeted_FeatureFinding\SIPPER_standard_testing\Targets\refID22508_massTags.txt";

            const string resultsFolder = @"\\proto-2\unitTest_Files\DeconTools_TestFiles\Targeted_FeatureFinding\Results_IqLoggerUnitTest";

            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters
            {
                ChromGenSourceDataPeakBR = 3,
                ChromGenSourceDataSigNoise = 2,
                TargetsFilePath = targetsFile,
                OutputFolderBase = resultsFolder
            };

            var run = new RunFactory().CreateRun(testFile);
            var datasetName = RunUtilities.GetDatasetName(testFile);

            var expectedLogFile = new FileInfo(Path.Combine(resultsFolder, "IqLogs", datasetName + "_IqLog.txt"));

            if (expectedLogFile.Exists)
                expectedLogFile.Delete();

            // FileLogger.ChangeLogFileBaseName(expectedLogFile.FullName, false);

            var executor = new IqExecutor(executorBaseParameters, run)
            {
                ChromSourceDataFilePath = peaksTestFile
            };

            executor.LoadAndInitializeTargets(targetsFile);
            executor.Targets = (from n in executor.Targets where n.ElutionTimeTheor > 0.1 && n.ElutionTimeTheor < 0.9 select n).Take(10).ToList();

            Assert.AreEqual(10, executor.Targets.Count, "Unexpected number of targets");

            var targetedWorkflowParameters = new BasicTargetedWorkflowParameters
            {
                ChromNETTolerance = 0.5
            };

            //define workflows for parentTarget and childTargets
            var parentWorkflow = new BasicIqWorkflow(run, targetedWorkflowParameters);
            var childWorkflow = new BasicIqWorkflow(run, targetedWorkflowParameters);

            var workflowAssigner = new IqWorkflowAssigner();
            workflowAssigner.AssignWorkflowToParent(parentWorkflow, executor.Targets);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, executor.Targets);

            // Main line for executing IQ:
            executor.Execute();

            FileLogger.FlushPendingMessages();

            // Test the Log File
            expectedLogFile.Refresh();

            if (!expectedLogFile.Exists)
                return;

            Console.WriteLine("");
            Console.WriteLine("Log File");
            var numLogs = 0;

            using (var reader = new StreamReader(expectedLogFile.FullName))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    numLogs++;
                    Console.WriteLine(line);
                }
            }
            Console.WriteLine("Log file lines read: " + numLogs);

            const int ExpectedLineCount = 33;
            Assert.AreEqual(ExpectedLineCount, numLogs, "Log file has {0} entries; expecting {1}", numLogs, ExpectedLineCount);
        }

    }
}
