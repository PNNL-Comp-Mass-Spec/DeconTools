using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
	public class TopDownTargetedWorkflowTests
	{
		[Test]
		public void TestTargetedWorkflowExecutor()
		{
			const string executorParameterFile = @"\\protoapps\UserData\Kaipo\TopDown\test1params.xml";
			var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
			executorParameters.LoadParameters(executorParameterFile);

			string resultsFolderLocation = executorParameters.ResultsFolder;
			const string testDatasetPath = @"\\protoapps\UserData\Kaipo\TopDown\test1\BW_20_1_111104210637.raw";
			const string testDatasetName = "BW_20_1_111104210637";

			string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_results.txt";
			if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);

			var executor = new TopDownTargetedWorkflowExecutor(executorParameters, testDatasetPath);
			executor.Execute();
			
			Assert.IsTrue(File.Exists(expectedResultsFilename));

			var importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
			TargetedResultRepository repository = importer.Import();

			Assert.AreEqual(10, repository.Results.Count);

			TargetedResultDTO result1 = repository.Results[2];

			Assert.AreEqual(24702, result1.TargetID);
			Assert.AreEqual(3, result1.ChargeState);
			Assert.AreEqual(8119, result1.ScanLC);
			Assert.AreEqual(0.41724m, (decimal)Math.Round(result1.NET, 5));
			Assert.AreEqual(0.002534m, (decimal)Math.Round(result1.NETError, 6));
			//Assert.AreEqual(2920.53082m, (decimal)Math.Round(result1.MonoMass, 5));
			//Assert.AreEqual(2920.53733m, (decimal)Math.Round(result1.MonoMassCalibrated, 5));
			//Assert.AreEqual(-1.83m, (decimal)Math.Round(result1.MassErrorInPPM, 2));
		}

		[Test]
		public void TestFindSingleMassTag()
		{
			const string testFile = @"\\protoapps\UserData\Kaipo\TopDown\test1\BW_20_1_111104210637.raw";
			const string massTagFile = @"\\protoapps\UserData\Kaipo\TopDown\test1\BW_20_1_111104210637_MSAlign_ResultTable.txt";

			var runFactory = new RunFactory();
			Run run = runFactory.CreateRun(testFile);

			var mtimporter = new MassTagFromMSAlignFileImporter(massTagFile);
			TargetCollection mtc = mtimporter.Import();

			Console.Write(mtc.TargetList.Count + "..\n");

			const int testMassTagID = 5;
			run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID select n).First();

			TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
			parameters.ChromatogramCorrelationIsPerformed = true;

			var workflow = new TopDownTargetedWorkflow(run, parameters);
			workflow.Execute();

			var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;

			if (result.FailedResult)
			{
				Console.WriteLine(result.ErrorDescription);
			}

			Assert.IsFalse(result.FailedResult);

			result.DisplayToConsole();

			Assert.IsNotNull(result.IsotopicProfile);
			Assert.IsNotNull(result.ScanSet);
			Assert.IsNotNull(result.ChromPeakSelected);
			Assert.AreEqual(2, result.IsotopicProfile.ChargeState);
			Assert.AreEqual(718.41m, (decimal)Math.Round(result.IsotopicProfile.GetMZ(), 2));
			Assert.AreEqual(5947m, (decimal)Math.Round(result.ChromPeakSelected.XValue));

			Assert.IsNotNull(result.ChromCorrelationData);

			foreach (var dataItem in result.ChromCorrelationData.CorrelationDataItems)
			{
				Console.WriteLine(dataItem);
			}
		}
	}
}
