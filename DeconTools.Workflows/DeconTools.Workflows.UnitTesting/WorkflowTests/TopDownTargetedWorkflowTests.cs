using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
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

			// Output chrom data
			var wf = executor.TargetedWorkflow as TopDownTargetedWorkflow;
			Console.Write("***** chrom data *****\n");
			foreach (var resultData in wf.TargetResults)
			{
				int id = resultData.Key;
				TargetedResultBase result = resultData.Value;
				double chromPeakSelected = (result.ChromPeakSelected != null) ? result.ChromPeakSelected.XValue : -1;

				Console.Write("TargetID=" + id + "\n");
				Console.Write("ChromPeakSelected=" + chromPeakSelected + "\n");
				for (int i = 0; i < result.ChromValues.Xvalues.Length; i++)
				{
					Console.Write(result.ChromValues.Xvalues[i] + "\t" + result.ChromValues.Yvalues[i] + "\n");
				}
				Console.Write("\n");
			}
			Console.Write("**********************\n");
			
			Assert.IsTrue(File.Exists(expectedResultsFilename));

			var importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
			TargetedResultRepository repository = importer.Import();

			Assert.AreEqual(15, repository.Results.Count);

			// expected results as tuples in format: <target id, charge state, scan lc>
			var expectedResults = new HashSet<Tuple<long, int, int>>
			{
				new Tuple<long, int, int>(1, 7, 1583),
				new Tuple<long, int, int>(2, 8, 1583),
				new Tuple<long, int, int>(3, 9, 1583),
				new Tuple<long, int, int>(4, 22, 2643),
				new Tuple<long, int, int>(5, 23, 2643),
				new Tuple<long, int, int>(6, 24, 2652),
				new Tuple<long, int, int>(7, 20, 1853),
				new Tuple<long, int, int>(8, 21, 1853),
				new Tuple<long, int, int>(9, 22, 1853),
				new Tuple<long, int, int>(10, 13, 2303),
				new Tuple<long, int, int>(11, 14, 2303),
				new Tuple<long, int, int>(12, 15, 2312),
				new Tuple<long, int, int>(13, 16, 2348),
				new Tuple<long, int, int>(14, 17, 2339),
				new Tuple<long, int, int>(15, 18, 2348)
			};

			foreach (TargetedResultDTO result in repository.Results)
			{
				expectedResults.Remove(new Tuple<long, int, int>(result.TargetID, result.ChargeState, result.ScanLC));
			}

			Assert.IsEmpty(expectedResults);
		}

		[Test]
		public void TestFindMassTag()
		{
			const string testFile = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD.raw";
			const string peaksTestFile = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD_peaks.txt";
			const string massTagFile = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD_MSAlign_ResultTable.txt";

			Run run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);

			var mtc = new TargetCollection();
			var mtimporter = new MassTagFromMSAlignFileImporter(massTagFile);
			mtc = mtimporter.Import();

			const int testMassTagID = 14;
			run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID select n).First();

			TargetedWorkflowParameters parameters = new TopDownTargetedWorkflowParameters();

			var workflow = new TopDownTargetedWorkflow(run, parameters);
			workflow.Execute();

			var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;

			if (result.FailedResult) Console.WriteLine(result.ErrorDescription);

			Assert.IsFalse(result.FailedResult);

			result.DisplayToConsole();
		}
        
	}
}
