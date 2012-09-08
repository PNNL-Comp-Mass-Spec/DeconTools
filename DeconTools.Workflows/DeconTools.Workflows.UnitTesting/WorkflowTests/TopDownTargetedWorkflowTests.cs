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
        public void TopDownWorkflowTest1()
        {

            //see https://jira.pnnl.gov/jira/browse/OMCR-98

            string baseFolder =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Topdown_standard_testing\Test1_MSAlign_ProteusPeriIntact";

            const string executorParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Topdown_standard_testing\Test1_MSAlign_ProteusPeriIntact\Parameters\topdownExecutorParameters1.xml";
            var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);
            executorParameters.ExportChromatogramData = true;

            const string testDatasetPath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Topdown_standard_testing\Test1_MSAlign_ProteusPeriIntact\RawData\Proteus_Peri_intact_ETD.raw";


            string resultsFolderLocation = executorParameters.ResultsFolder;

            string testDatasetName = RunUtilities.GetDatasetName(testDatasetPath);

            string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_quant.txt";
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }

            var executor = new TopDownTargetedWorkflowExecutor(executorParameters, testDatasetPath);

            string proteinSeq =
                @"A.VDKTNPYALMEDAAQKTFDKLKTEQPEIRKNPELLREIVQQELLPYVHIKYAGALVLGPYYRNATPAQRDAYFAAFKDYLAQVYGQALAMYEGQEYRIEPAKPFADKSNLTIRVTIIDKNGRPPVRLDFQWRKNSKTGEWQAYDMIAEGVSMITTKQNEWSDILSAKGVDGLTKQLEISAKTPITLDEKK.";

            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.Code == proteinSeq).ToList();
            executor.Execute();

            Assert.IsNotNull(executor.TargetedWorkflow.Run);

            Console.WriteLine("Num targetedResults in Run = " + executor.TargetedWorkflow.Run.ResultCollection.MassTagResultList.Count);

            Assert.IsTrue(File.Exists(expectedResultsFilename), "Results file does not exist!");


            //TODO: fix _quant.txt output column headers
            TopDownTargetedResultFromTextImporter importer = new TopDownTargetedResultFromTextImporter(expectedResultsFilename);
            var results=   importer.Import();

            Assert.IsNotNull(results);
            Assert.IsTrue(results.HasResults);
            Assert.AreEqual(1, results.Results.Count);

            //foreach (var r in results.Results)
            //{
            //    Console.WriteLine(r.TargetID + "\t" + r.ScanLC + "\t" + r.Intensity);
            //}


        }


        //TODO: add simple unit test for various mods


        [Test]
        public void WorkflowExecutorParametersTest()
        {
            const string executorParameterFile =
               @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Topdown_standard_testing\Test1_MSAlign_ProteusPeriIntact\Parameters\topdownExecutorParameters1.xml";
            var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            Assert.AreEqual(false, executorParameters.ExportChromatogramData);

            executorParameters.ExportChromatogramData = true;

            string exportedParameterFile = executorParameterFile.Replace("1.xml", "_exportTest1.xml");
            executorParameters.SaveParametersToXML(exportedParameterFile);

            Assert.IsTrue(File.Exists(exportedParameterFile), "Exported parameter file doesn't exist");

            var executorParameters2 = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters2.LoadParameters(exportedParameterFile);

            Assert.AreEqual(true, executorParameters2.ExportChromatogramData);


        }

        [Test]
        public void WorkflowParametersTest()
        {
            const string executorParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Topdown_standard_testing\Test1_MSAlign_ProteusPeriIntact\Parameters\topdownWorkflowParameters1.xml";
            var workflowParameters = new TopDownTargetedWorkflowParameters();
            workflowParameters.LoadParameters(executorParameterFile);

            Assert.AreEqual(false, workflowParameters.SaveChromatogramData);

            workflowParameters.SaveChromatogramData = true;

            string exportedParameterFile = executorParameterFile.Replace("1.xml", "_exportTest1.xml");
            workflowParameters.SaveParametersToXML(exportedParameterFile);

            Assert.IsTrue(File.Exists(exportedParameterFile), "Exported parameter file doesn't exist");

            var executorParameters2 = new TopDownTargetedWorkflowParameters();
            executorParameters2.LoadParameters(exportedParameterFile);

            Assert.AreEqual(true, executorParameters2.SaveChromatogramData);


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

            string proteinSeq =
                @"A.VDKTNPYALMEDAAQKTFDKLKTEQPEIRKNPELLREIVQQELLPYVHIKYAGALVLGPYYRNATPAQRDAYFAAFKDYLAQVYGQALAMYEGQEYRIEPAKPFADKSNLTIRVTIIDKNGRPPVRLDFQWRKNSKTGEWQAYDMIAEGVSMITTKQNEWSDILSAKGVDGLTKQLEISAKTPITLDEKK.";

            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.Code == proteinSeq).ToList();
            executor.Execute();

            // Output chrom data
            //var wf = executor.TargetedWorkflow as TopDownTargetedWorkflow;
            //Console.Write("***** chrom data *****\n");
            //foreach (var resultData in wf.TargetResults)
            //{
            //    int id = resultData.Key;
            //    TargetedResultBase result = resultData.Value;
            //    double chromPeakSelected = (result.ChromPeakSelected != null) ? result.ChromPeakSelected.XValue : -1;

            //    Console.Write("TargetID=" + id + "; ChromPeakSelected=" + chromPeakSelected + "\n");
            //    for (int i = 0; i < result.ChromValues.Xvalues.Length; i++)
            //    {
            //        //Console.Write(result.ChromValues.Xvalues[i] + "\t" + result.ChromValues.Yvalues[i] + "\n");
            //    }
            //    //Console.Write("\n");
            //}
            Console.Write("**********************\n");

            Assert.IsTrue(File.Exists(expectedResultsFilename));

            var importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
            TargetedResultRepository repository = importer.Import();

            Assert.AreEqual(1, repository.Results.Count);

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
		public void TestTargetedWorkflowExecutorMod()
		{
			const string executorParameterFile = @"\\protoapps\UserData\Kaipo\TopDown\test2paramsmod.xml";
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
					Console.Write(result.ChromValues.Xvalues[i] + "\t" + result.ChromValues.Yvalues[i] + "\n");
				}
				Console.Write("\n");
			}
			Console.Write("**********************\n");

			Assert.IsTrue(File.Exists(expectedResultsFilename));

			var importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
			TargetedResultRepository repository = importer.Import();

			Assert.AreEqual(9, repository.Results.Count);
		}


        [Test]
        public void TestGetEmpiricalFormulaForModifiedPeptideSequences()
        {
            var pyroglutamateCodes = new Dictionary<string, string>
			{
				{
					"M.QVYAMRRLKQWLVGSYQTDNSAFVPYDRTLLWFTFGLAVVGFVMVTSASMPVGQRLAEDPFLFAKRDGIYMIVALCLALVTMRVPMAVWQRYSSLMLFGSILLLLVVLAVGSSVNGASRWIAFGPLRIQPAELSKLALFCYLSSYLVRKVEEVRNNFWGFCKPMGVMLILAVLLLLQPDLGTVVVLFVTTLALLFLAGAKIWQFLAIIGTGIAAVVMLIIVEPYRVRRITSFLEPWEDPFGSGYQLTQSLMAFGRGDLLGQGLGNSVQKLEYLPEAHTDFIFSILAEELGYIGVVLVLLMVFFIAFRAMQIGRRALLLDQRFSGFLACSIGIWFTFQTLVNVGAAAGML.P",
					"M.(Q)[-17.03]VYAMRRLKQWLVGSYQTDNSAFVPYDRTLLWFTFGLAVVGFVMVTSASMPVGQRLAEDPFLFAKRDGIYMIVALCLALVTMRVPMAVWQRYSSLMLFGSILLLLVVLAVGSSVNGASRWIAFGPLRIQPAELSKLALFCYLSSYLVRKVEEVRNNFWGFCKPMGVMLILAVLLLLQPDLGTVVVLFVTTLALLFLAGAKIWQFLAIIGTGIAAVVMLIIVEPYRVRRITSFLEPWEDPFGSGYQLTQSLMAFGRGDLLGQGLGNSVQKLEYLPEAHTDFIFSILAEELGYIGVVLVLLMVFFIAFRAMQIGRRALLLDQRFSGFLACSIGIWFTFQTLVNVGAAAGML.P"
				},
				{
					".QFHIYHSNQLSLLKSLMVHFMQNRPLSSPFEQEVILVQSPGMSQWLQIQLAESLGIAANIRYPLPATFIWEMFTRVLSGIPKESAFSKDAMTWKLMALLPNYLDDPAFKPLRHYLKDDEDKRKLHQLAGRVADLFDQYLVYRPDWLSAWENDQLIDGLSDNQYWQKTLWLALQRYTEDLAQPKWHRANLYQQFISTLNDAPVGALAHCFPSRIFICGISALPQVYLQALQAIGRHTEIYLLFTNPCRYYWGDIQDPKFLARLNSRKPRHYQQLHELPWFKDEQNASTLFNEEGEQNVGNPLLASWGKLGKDNLYFLSELEYSDVLDAFVDIPRD.N",
				   	".(Q)[-17.03]FHIYHSNQLSLLKSLMVHFMQNRPLSSPFEQEVILVQSPGMSQWLQIQLAESLGIAANIRYPLPATFIWEMFTRVLSGIPKESAFSKDAMTWKLMALLPNYLDDPAFKPLRHYLKDDEDKRKLHQLAGRVADLFDQYLVYRPDWLSAWENDQLIDGLSDNQYWQKTLWLALQRYTEDLAQPKWHRANLYQQFISTLNDAPVGALAHCFPSRIFICGISALPQVYLQALQAIGRHTEIYLLFTNPCRYYWGDIQDPKFLARLNSRKPRHYQQLHELPWFKDEQNASTLFNEEGEQNVGNPLLASWGKLGKDNLYFLSELEYSDVLDAFVDIPRD.N"
				},
				{
					".QRFKLWVFISLCLHASLVAAAILYVVEDKPIAPEPISIQMLAFAADEPVGEPEPVVEEVTPPEPEPVVEPEPEPEPEPIPDVKPVIEKPIEKKPEPKPKPKPKPVEKPKPPVERPQQQPLA.L",
					".(QRFKLW)[-17.03]VFISLCLHASLVAAAILYVVEDKPIAPEPISIQMLAFAADEPVGEPEPVVEEVTPPEPEPVVEPEPEPEPEPIPDVKPVIEKPIEKKPEPKPKPKPKPVEKPKPPVERPQQQPLA.L"
				}
			};

            var acetylationCodes = new Dictionary<string, string>
			{
				{
					"M.SVYAMRRLKQWLVGSYQTDNSAFVPYDRTLLWFTFGLAVVGFVMVTSASMPVGQRLAEDPFLFAKRDGIYMIVALCLALVTMRVPMAVWQRYSSLMLFGSILLLLVVLAVGSSVNGASRWIAFGPLRIQPAELSKLALFCYLSSYLVRKVEEVRNNFWGFCKPMGVMLILAVLLLLQPDLGTVVVLFVTTLALLFLAGAKIWQFLAIIGTGIAAVVMLIIVEPYRVRRITSFLEPWEDPFGSGYQLTQSLMAFGRGDLLGQGLGNSVQKLEYLPEAHTDFIFSILAEELGYIGVVLVLLMVFFIAFRAMQIGRRALLLDQRFSGFLACSIGIWFTFQTLVNVGAAAGML.P",
					"M.(S)[42.01]VYAMRRLKQWLVGSYQTDNSAFVPYDRTLLWFTFGLAVVGFVMVTSASMPVGQRLAEDPFLFAKRDGIYMIVALCLALVTMRVPMAVWQRYSSLMLFGSILLLLVVLAVGSSVNGASRWIAFGPLRIQPAELSKLALFCYLSSYLVRKVEEVRNNFWGFCKPMGVMLILAVLLLLQPDLGTVVVLFVTTLALLFLAGAKIWQFLAIIGTGIAAVVMLIIVEPYRVRRITSFLEPWEDPFGSGYQLTQSLMAFGRGDLLGQGLGNSVQKLEYLPEAHTDFIFSILAEELGYIGVVLVLLMVFFIAFRAMQIGRRALLLDQRFSGFLACSIGIWFTFQTLVNVGAAAGML.P"
				},
				{
					".MFHIYHSNQLSLLKSLMVHFMQNRPLSSPFEQEVILVQSPGMSQWLQIQLAESLGIAANIRYPLPATFIWEMFTRVLSGIPKESAFSKDAMTWKLMALLPNYLDDPAFKPLRHYLKDDEDKRKLHQLAGRVADLFDQYLVYRPDWLSAWENDQLIDGLSDNQYWQKTLWLALQRYTEDLAQPKWHRANLYQQFISTLNDAPVGALAHCFPSRIFICGISALPQVYLQALQAIGRHTEIYLLFTNPCRYYWGDIQDPKFLARLNSRKPRHYQQLHELPWFKDEQNASTLFNEEGEQNVGNPLLASWGKLGKDNLYFLSELEYSDVLDAFVDIPRD.N",
				   	".(M)[42.01]FHIYHSNQLSLLKSLMVHFMQNRPLSSPFEQEVILVQSPGMSQWLQIQLAESLGIAANIRYPLPATFIWEMFTRVLSGIPKESAFSKDAMTWKLMALLPNYLDDPAFKPLRHYLKDDEDKRKLHQLAGRVADLFDQYLVYRPDWLSAWENDQLIDGLSDNQYWQKTLWLALQRYTEDLAQPKWHRANLYQQFISTLNDAPVGALAHCFPSRIFICGISALPQVYLQALQAIGRHTEIYLLFTNPCRYYWGDIQDPKFLARLNSRKPRHYQQLHELPWFKDEQNASTLFNEEGEQNVGNPLLASWGKLGKDNLYFLSELEYSDVLDAFVDIPRD.N"
				},
				{
					".MRFKLWVFISLCLHASLVAAAILYVVEDKPIAPEPISIQMLAFAADEPVGEPEPVVEEVTPPEPEPVVEPEPEPEPEPIPDVKPVIEKPIEKKPEPKPKPKPKPVEKPKPPVERPQQQPLA.L",
					".(MRFKLW)[42.01]VFISLCLHASLVAAAILYVVEDKPIAPEPISIQMLAFAADEPVGEPEPVVEEVTPPEPEPVVEPEPEPEPEPIPDVKPVIEKPIEKKPEPKPKPKPKPVEKPKPPVERPQQQPLA.L"
				}
			};

            var msAlignImporter = new MassTagFromMSAlignFileImporter(String.Empty);
            var peptideUtils = new PeptideUtils();

            Console.WriteLine("pyroglutamate");
            foreach (var pair in pyroglutamateCodes)
            {
                string empiricalFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(pair.Key);
                string empiricalFormulaWithMod = msAlignImporter.GetEmpiricalFormulaForSequenceWithMods(pair.Value);

                double monoMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormula);
                double monoMassWithMod = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormulaWithMod);
                double diff = monoMassWithMod - monoMass;

                Console.WriteLine(empiricalFormula + "(" + monoMass + ")\t" + empiricalFormulaWithMod + "(" + monoMassWithMod + "); diff=" + diff);
                Assert.AreEqual(-17, Math.Round(diff, 0, MidpointRounding.AwayFromZero));
            }

            Console.WriteLine("acetylation");
            foreach (var pair in acetylationCodes)
            {
                string empiricalFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(pair.Key);
                string empiricalFormulaWithMod = msAlignImporter.GetEmpiricalFormulaForSequenceWithMods(pair.Value);

                double monoMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormula);
                double monoMassWithMod = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormulaWithMod);
                double diff = monoMassWithMod - monoMass;

                Console.WriteLine(empiricalFormula + "(" + monoMass + ")\t" + empiricalFormulaWithMod + "(" + monoMassWithMod + "); diff=" + diff);
                Assert.AreEqual(42, Math.Round(diff, 0, MidpointRounding.AwayFromZero));
            }
        }


	

		[Test]
		public void TestFindMassTag()
		{
			const string testFile = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD.raw";
			const string peaksTestFile = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD_peaks.txt";
			const string massTagFile = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD_MSAlign_ResultTable.txt";

			Run run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);

			TargetCollection mtc = new TargetCollection();
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

        //[Test]
        //public void TestTargetedWorkflowExecutorFullDataset()
        //{
        //    const string executorParameterFile = @"\\protoapps\UserData\Kaipo\TopDown\test2paramsfulldataset.xml";
        //    var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
        //    executorParameters.LoadParameters(executorParameterFile);

        //    string resultsFolderLocation = executorParameters.ResultsFolder;
        //    const string testDatasetPath = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD.raw";
        //    const string testDatasetName = "Proteus_Peri_intact_ETD";

        //    string expectedResultsFilename = resultsFolderLocation + "\\" + testDatasetName + "_quant.txt";
        //    if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);

        //    var executor = new TopDownTargetedWorkflowExecutor(executorParameters, testDatasetPath);
        //    executor.Execute();

        //    // Output chrom data
        //    var wf = executor.TargetedWorkflow as TopDownTargetedWorkflow;
        //    foreach (var resultData in wf.TargetResults)
        //    {
        //        int id = resultData.Key;
        //        TargetedResultBase result = resultData.Value;
        //        double chromPeakSelected = (result.ChromPeakSelected != null) ? result.ChromPeakSelected.XValue : -1;

        //        Console.Write("TargetID=" + id + "; ChromPeakSelected=" + chromPeakSelected + "\n");
        //    }

        //    Assert.IsTrue(File.Exists(expectedResultsFilename));

        //    var importer = new UnlabelledTargetedResultFromTextImporter(expectedResultsFilename);
        //    TargetedResultRepository repository = importer.Import();

        //    Assert.AreEqual(478, repository.Results.Count);
        //}

        
	}
}
