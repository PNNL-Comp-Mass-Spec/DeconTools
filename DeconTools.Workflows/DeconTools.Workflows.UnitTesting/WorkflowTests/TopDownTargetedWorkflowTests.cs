using System;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
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

			Assert.AreEqual(6, repository.Results.Count);

			TargetedResultDTO result1 = repository.Results[0];
			TargetedResultDTO result2 = repository.Results[4];
			
			// result1 should have a selected chrompeak
			Assert.AreEqual(1, result1.TargetID);
			Assert.AreEqual(19, result1.ChargeState);
			Assert.AreEqual(2422, result1.ScanLC);

			// result2 should not have a selected chrompeak
			Assert.AreEqual(5, result2.TargetID);
			Assert.AreEqual(23, result2.ChargeState);
			Assert.AreEqual(-1, result2.ScanLC);
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

        [Test]
        public void TestFindMassTag_byGord()
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


            TestUtilities.DisplayPeaks(workflow.ChromPeaksDetected.Select(p => (Peak)p).ToList());


            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;

            if (result.FailedResult) Console.WriteLine(result.ErrorDescription);

            //Assert.IsFalse(result.FailedResult);

            result.DisplayToConsole();
        }

	}
}
