using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
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
			string testFile = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Testing\NoPeaks\CPTAC_OT_Pep_JB_5427_0min_4May12_Legolas_11-07-64.raw";
			string targetsFile = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Testing\NoPeaks\no_peaks.txt";
			string resultsFolder = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Testing\NoPeaks";

			Backend.Utilities.SipperDataDump.DataDumpSetup(@"\\protoapps\UserData\Fujimoto\TopDownTesting\Testing\NoPeaks\sipper_results.txt");
			
			var stopwatch = Stopwatch.StartNew();

			WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
			executorBaseParameters.ChromGenSourceDataPeakBR = 3;
			executorBaseParameters.ChromGenSourceDataSigNoise = 2;
			executorBaseParameters.ResultsFolder = resultsFolder;
			executorBaseParameters.TargetsFilePath = targetsFile;

			Run run = new RunFactory().CreateRun(testFile);

			var executor = new TopDownMSAlignExecutor(executorBaseParameters, run);


			executor.LoadAndInitializeTargets(targetsFile);

			var targetedWorkflowParameters = new BasicTargetedWorkflowParameters();
			targetedWorkflowParameters.ChromNETTolerance = 0.05;
			targetedWorkflowParameters.ChromatogramCorrelationIsPerformed = true;
			targetedWorkflowParameters.ChromSmootherNumPointsInSmooth = 15;

			//targetedWorkflowParameters.SaveParametersToXML(@"C:\Users\fuji510\Desktop");

			//define workflows for parentTarget and childTargets
			//var parentWorkflow = new ParentLogicIqWorkflow(run, targetedWorkflowParameters);

			var parentWorkflow = new ChromPeakDeciderIqWorkflow(run, targetedWorkflowParameters);
			var childWorkflow = new ChargeStateChildTopDownIqWorkflow(run, targetedWorkflowParameters);

			IqWorkflowAssigner workflowAssigner = new IqWorkflowAssigner();
			workflowAssigner.AssignWorkflowToParent(parentWorkflow, executor.Targets);
			workflowAssigner.AssignWorkflowToChildren(childWorkflow, executor.Targets);


			//Main line for executing IQ:
			executor.Execute();

			stopwatch.Stop();
			var runtime = stopwatch.Elapsed;
			Console.WriteLine("Runtime: " + runtime);
		}
	}
}
