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
			string testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
			string peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

			string targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

			string resultsFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results";


			WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
			executorBaseParameters.ChromGenSourceDataPeakBR = 3;
			executorBaseParameters.ChromGenSourceDataSigNoise = 2;
			executorBaseParameters.ResultsFolder = resultsFolder;
		    executorBaseParameters.LoggingFolder = resultsFolder;
			executorBaseParameters.TargetsFilePath = targetsFile;

			Run run = new RunFactory().CreateRun(testFile);

			string expectedResultsFilename = resultsFolder + "\\" + RunUtilities.GetDatasetName(testFile) + "_IqLog.txt";
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

			IqWorkflowAssigner workflowAssigner = new IqWorkflowAssigner();
			workflowAssigner.AssignWorkflowToParent(parentWorkflow, executor.Targets);
			workflowAssigner.AssignWorkflowToChildren(childWorkflow, executor.Targets);

			//Main line for executing IQ:
			executor.Execute();

			//Test the Log File
			Assert.IsTrue(File.Exists(expectedResultsFilename), "IqLog.txt file doesn't exist");
			Console.WriteLine("");
			Console.WriteLine("Log File");
			int numLogs = 0;
			bool outputToConsole = true;

			using (StreamReader reader = new StreamReader(expectedResultsFilename))
			{
				while (reader.Peek() != -1)
				{
					string line = reader.ReadLine();
					numLogs++;

					if (outputToConsole)
					{
						Console.WriteLine(line);
					}
				}
			}
			Assert.IsTrue(numLogs == 47, "No Logs in output file");
		}

	}
}
