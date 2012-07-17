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
			const string executorParameterFile = @"\\protoapps\UserData\Kaipo\TopDown\test2params.xml";
			var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
			executorParameters.LoadParameters(executorParameterFile);

			string resultsFolderLocation = executorParameters.ResultsFolder;
			const string testDatasetPath = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD.raw";
			const string testDatasetName = "Proteus_Peri_intact_ETD";

			string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_results.txt";
			if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);

			var executor = new TopDownTargetedWorkflowExecutor(executorParameters, testDatasetPath);
			executor.Execute();
			
			Assert.IsTrue(File.Exists(expectedResultsFilename));

			var importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
			TargetedResultRepository repository = importer.Import();

			Assert.AreEqual(6, repository.Results.Count);

			TargetedResultDTO result1 = repository.Results[0];
			TargetedResultDTO result2 = repository.Results[3];
			
			// result1 should have a selected chrompeak
			Assert.AreEqual(1, result1.TargetID);
			Assert.AreEqual(20, result1.ChargeState);
			Assert.AreEqual(2422, result1.ScanLC);

			// result2 should not have a selected chrompeak
			Assert.AreEqual(4, result2.TargetID);
			Assert.AreEqual(23, result2.ChargeState);
			Assert.AreEqual(-1, result2.ScanLC);
		}
	}
}
