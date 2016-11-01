using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.IqUnitTesting
{
    public class BottomUpIqTesting
    {

        [Test]
        public void ExecutorTest1()
        {
            Console.WriteLine(Environment.CurrentDirectory);
            var util = new IqTargetUtilities();
            string testFile = @"\\proto-5\External_Waters_TOF_Xfer\MzML_Files\130716_iPRG14_004.mzML";
            //string peaksTestFile =
                //@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";
            string targetsFile = @"\\protoapps\UserData\Fujimoto\SangtaeBottomUp\msgfPlus\C~~data~iPRG 2014~130716_iPRG14_004.raw.-1_Filtered.tsv";

            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.TargetsFilePath = targetsFile;
            executorBaseParameters.OutputFolderBase =
                @"\\protoapps\UserData\Fujimoto\SangtaeBottomUp\Results";


            string expectedResultsFilename = Path.Combine(executorBaseParameters.OutputFolderBase,
                                                          "IqResults",
                                                          RunUtilities.GetDatasetName(testFile) + "_iqResults.txt");
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);


            Run run = new RunFactory().CreateRun(testFile);

            var executor = new IqExecutor(executorBaseParameters, run);
            //executor.ChromSourceDataFilePath = peaksTestFile;

            executor.LoadAndInitializeTargets(targetsFile);

            var targetedWorkflowParameters = new BasicTargetedWorkflowParameters();
            targetedWorkflowParameters.ChromNETTolerance = 0.05;

            //define workflows for parentTarget and childTargets
            var parentWorkflow = new ChromPeakDeciderIqWorkflow(run, targetedWorkflowParameters);
            var childWorkflow = new ChargeStateChildIqWorkflow(run, targetedWorkflowParameters);

            IqWorkflowAssigner workflowAssigner = new IqWorkflowAssigner();
            workflowAssigner.AssignWorkflowToParent(parentWorkflow, executor.Targets);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, executor.Targets);

            //SipperDataDump.DataDumpSetup(@"\\pnl\projects\MSSHARE\Merkley_Eric\For_Grant\IqResults\EXP6B_F1_CSCL_LIGHT_130520020056\EXP6B_F1_CSCL_LIGHT_FULLRESULTS.txt");

            //Main line for executing IQ:
            executor.Execute();

            //Test the results...
            int numResultsInResultsFile = 0;
            bool outputToConsole = true;

            using (StreamReader reader = new StreamReader(expectedResultsFilename))
            {
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    numResultsInResultsFile++;

                    if (outputToConsole)
                    {
                        Console.WriteLine(line);
                    }
                }
            }
        }

    }
}
