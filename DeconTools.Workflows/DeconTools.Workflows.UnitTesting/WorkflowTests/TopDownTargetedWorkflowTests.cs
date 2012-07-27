using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using GWSGraphLibrary;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
	public class TopDownTargetedWorkflowTests
	{
		[Test]
		public void TestTargetedWorkflowExecutorFullDataset()
		{
			const string executorParameterFile = @"\\protoapps\UserData\Kaipo\TopDown\test2paramsfulldataset.xml";
			var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
			executorParameters.LoadParameters(executorParameterFile);

			string resultsFolderLocation = executorParameters.ResultsFolder;
			const string testDatasetPath = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD.raw";
			const string testDatasetName = "Proteus_Peri_intact_ETD";

			string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_quant.txt";
			if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);

			var executor = new TopDownTargetedWorkflowExecutor(executorParameters, testDatasetPath);
			executor.Execute();
			
			// Output chrom data
			var wf = executor.TargetedWorkflow as TopDownTargetedWorkflow;
			foreach (var resultData in wf.TargetResults)
			{
				int id = resultData.Key;
				TargetedResultBase result = resultData.Value;
				double chromPeakSelected = (result.ChromPeakSelected != null) ? result.ChromPeakSelected.XValue : -1;

				Console.Write("TargetID=" + id + "; ChromPeakSelected=" + chromPeakSelected + "\n");
			}
			
			Assert.IsTrue(File.Exists(expectedResultsFilename));

			var importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
			TargetedResultRepository repository = importer.Import();

			Assert.AreEqual(478, repository.Results.Count);
		}

		[Test]
		public void TestTargetedWorkflowExecutor()
		{
			const string executorParameterFile = @"\\protoapps\UserData\Kaipo\TopDown\test2params.xml";
			var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
			executorParameters.LoadParameters(executorParameterFile);

			string resultsFolderLocation = executorParameters.ResultsFolder;
			const string testDatasetPath = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD.raw";
			const string testDatasetName = "Proteus_Peri_intact_ETD";

			string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_quant.txt";
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

				Console.Write("TargetID=" + id + "; ChromPeakSelected=" + chromPeakSelected + "\n");
				for (int i = 0; i < result.ChromValues.Xvalues.Length; i++)
				{
					//Console.Write(result.ChromValues.Xvalues[i] + "\t" + result.ChromValues.Yvalues[i] + "\n");
				}
				//Console.Write("\n");
			}
			Console.Write("**********************\n");
			
			Assert.IsTrue(File.Exists(expectedResultsFilename));

			var importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
			TargetedResultRepository repository = importer.Import();

			Assert.AreEqual(9, repository.Results.Count);

            //// expected results as tuples in format: <target id, charge state, scan lc>
            //var expectedResults = new HashSet<Tuple<long, int, int>>
            //{
            //    new Tuple<long, int, int>(1, 8, 1583),
            //    new Tuple<long, int, int>(2, 23, 2643),
            //    new Tuple<long, int, int>(3, 21, 1853),
            //    new Tuple<long, int, int>(4, 14, 2303),
            //    new Tuple<long, int, int>(5, 17, 2339),
            //    new Tuple<long, int, int>(6, 26, 4630),
            //    new Tuple<long, int, int>(7, 26, 3583),
            //    new Tuple<long, int, int>(8, 7, 3709),
            //    new Tuple<long, int, int>(9, 42, 3439),
            //};

            //foreach (TargetedResultDTO result in repository.Results)
            //{
            //    expectedResults.Remove(new Tuple<long, int, int>(result.TargetID, result.ChargeState, result.ScanLC));
            //}

            //Assert.IsEmpty(expectedResults);
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

			var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag);

			if (result.FailedResult) Console.WriteLine(result.ErrorDescription);

			Assert.IsFalse(result.FailedResult);

			result.DisplayToConsole();
		}
        
	}
}
