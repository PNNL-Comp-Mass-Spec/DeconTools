using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;
using Globals = DeconTools.Workflows.Backend.Globals;
using Globals1 = DeconTools.Backend.Globals;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    public class TopDownTargetedWorkflowTests
    {
        [Category("MustPass")]
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
            executorParameters.ExportChromatogramData = false;
            executorParameters.OutputFolderBase =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Topdown_standard_testing\Test1_MSAlign_ProteusPeriIntact";


            const string testDatasetPath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Topdown_standard_testing\Test1_MSAlign_ProteusPeriIntact\RawData\Proteus_Peri_intact_ETD.raw";


            string testDatasetName = RunUtilities.GetDatasetName(testDatasetPath);

            string expectedResultsFilename = executorParameters.OutputFolderBase +"\\IqResults" + "\\" + testDatasetName + "_quant.txt";
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

            Assert.IsTrue(File.Exists(exportedParameterFile), "Exported parameter file doesn't exist: " + exportedParameterFile);

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

            Assert.IsTrue(File.Exists(exportedParameterFile), "Exported parameter file doesn't exist: " + exportedParameterFile);

            var executorParameters2 = new TopDownTargetedWorkflowParameters();
            executorParameters2.LoadParameters(exportedParameterFile);

            Assert.AreEqual(true, executorParameters2.SaveChromatogramData);


        }


        [Ignore("")]
        [Test]
        public void TestTargetedWorkflowExecutor()
        {
            const string executorParameterFile = @"\\protoapps\UserData\Kaipo\TopDown\test2params.xml";
            var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParameterFile);

            string resultsFolderLocation = Path.Combine(executorParameters.OutputFolderBase, "IqResults");
            const string testDatasetPath = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD.raw";
            const string testDatasetName = "Proteus_Peri_intact_ETD";

            string expectedResultsFilename = Path.Combine(resultsFolderLocation, testDatasetName + "_quant.txt");
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

            string resultsFolderLocation = Path.Combine(executorParameters.OutputFolderBase, "Results");
            const string testDatasetPath = @"\\protoapps\UserData\Kaipo\TopDown\test2\Proteus_Peri_intact_ETD.raw";
            const string testDatasetName = "Proteus_Peri_intact_ETD";

            string expectedResultsFilename = Path.Combine(resultsFolderLocation, testDatasetName + "_quant.txt");
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

        //    string expectedResultsFilename = Path.Combine(resultsFolderLocation, testDatasetName + "_quant.txt");
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


        [Test]
        public void TopDownWorkflowTestRepeatedChargeState()
        {

            //see https://jira.pnnl.gov/jira/browse/OMCR-100

            var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters.CopyRawFileLocal = false;
            executorParameters.DeleteLocalDatasetAfterProcessing = false;
            executorParameters.TargetsFilePath = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Targets\repeatedChargeState\Proteus_Peri_intact_ETD_MSAlign_ResultTable.txt";
            executorParameters.TargetType = Globals.TargetType.LcmsFeature;
            executorParameters.ExportChromatogramData = true;

            var workflowParameters = new TopDownTargetedWorkflowParameters();
            workflowParameters.AreaOfPeakToSumInDynamicSumming = 2;
            workflowParameters.ChromatogramCorrelationIsPerformed = false;
            workflowParameters.ChromGeneratorMode = Globals1.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            workflowParameters.ChromGenSourceDataPeakBR = 4;
            workflowParameters.ChromGenSourceDataSigNoise = 3;
            workflowParameters.ChromNETTolerance = 0.025;
            workflowParameters.ChromPeakDetectorPeakBR = 0.1;
            workflowParameters.ChromPeakDetectorSigNoise = 0.1;
            workflowParameters.ChromPeakSelectorMode = Globals1.PeakSelectorMode.Smart;
            workflowParameters.ChromSmootherNumPointsInSmooth = 9;
            workflowParameters.MaxScansSummedInDynamicSumming = 100;
            workflowParameters.MSPeakDetectorPeakBR = 1.3;
            workflowParameters.MSPeakDetectorSigNoise = 2;
            workflowParameters.ChromGenTolerance = 15;
            workflowParameters.MSToleranceInPPM = 25;
            workflowParameters.MultipleHighQualityMatchesAreAllowed = true;
            workflowParameters.NumChromPeaksAllowedDuringSelection = 10;
            workflowParameters.NumMSScansToSum = 5;
            workflowParameters.ResultType = Globals1.ResultType.TOPDOWN_TARGETED_RESULT;
            workflowParameters.SummingMode = SummingModeEnum.SUMMINGMODE_STATIC;

            const string testDatasetPath = @"\\protoapps\UserData\Fujimoto\TopDownTesting\RawData\Proteus_Peri_intact_ETD.raw";

            const string resultsFolderLocation = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Results\repeatedChargeState";

            string testDatasetName = RunUtilities.GetDatasetName(testDatasetPath);

            string expectedResultsFilename = Path.Combine(resultsFolderLocation, testDatasetName + "_quant.txt");
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }

            var executor = new TopDownTargetedWorkflowExecutor(executorParameters, workflowParameters, testDatasetPath);

            string proteinSeq1 =
                @"M.SDKMKGQVKWFNESKGFGFITPADGSKDVFVHFSAIQGNGFKTLAEGQNVEFTIENGAKGPAAANVTAL.";
            string proteinSeq2 =
                @"A.AENVVHHKLDGMPISEAVEINAGNNLVFLSGKVPTKKSADAPEGELASYGNTEEQTINVLEQIKTNLNNLGLDMKDVVKMQVFLVGGEENNGTMDFKGFMNGYSKFYDASKTNQLPARSAFQVAKLANPAWRVEIEVIAVRPAK.";

            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => (p.Code == proteinSeq1) || (p.Code == proteinSeq2)).ToList();
            executor.Execute();

            Assert.IsNotNull(executor.TargetedWorkflow.Run);

            Console.WriteLine("Num targetedResults in Run = " + executor.TargetedWorkflow.Run.ResultCollection.MassTagResultList.Count);

            Assert.IsTrue(File.Exists(expectedResultsFilename), "Results file does not exist!");

            var resultsfile = new StreamReader(@"\\protoapps\UserData\Fujimoto\TopDownTesting\Results\repeatedChargeState\Proteus_Peri_intact_ETD_quant.txt");
            string line = resultsfile.ReadLine();
            line = resultsfile.ReadLine();
            string[] chargestatelist = line.Split('\t');
            Assert.AreEqual("5, 6, 7, 8, 9, 10, 11", chargestatelist[2]);
            resultsfile.Close();
        }

        [Test]
        public void TopDownWorkflowTestMissingChargeState()
        {

            //see https://jira.pnnl.gov/jira/browse/OMCR-102

            var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters.CopyRawFileLocal = false;
            executorParameters.DeleteLocalDatasetAfterProcessing = false;
            executorParameters.TargetsFilePath = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Targets\missingChargeState\Proteus_Peri_intact_ETD_MSAlign_ResultTable.txt";
            executorParameters.TargetType = Globals.TargetType.LcmsFeature;
            executorParameters.ExportChromatogramData = true;

            var workflowParameters = new TopDownTargetedWorkflowParameters();
            workflowParameters.AreaOfPeakToSumInDynamicSumming = 2;
            workflowParameters.ChromatogramCorrelationIsPerformed = false;
            workflowParameters.ChromGeneratorMode = Globals1.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            workflowParameters.ChromGenSourceDataPeakBR = 4;
            workflowParameters.ChromGenSourceDataSigNoise = 3;
            workflowParameters.ChromNETTolerance = 0.025;
            workflowParameters.ChromPeakDetectorPeakBR = 0.1;
            workflowParameters.ChromPeakDetectorSigNoise = 0.1;
            workflowParameters.ChromPeakSelectorMode = Globals1.PeakSelectorMode.Smart;
            workflowParameters.ChromSmootherNumPointsInSmooth = 9;
            workflowParameters.MaxScansSummedInDynamicSumming = 100;
            workflowParameters.MSPeakDetectorPeakBR = 1.3;
            workflowParameters.MSPeakDetectorSigNoise = 2;
            workflowParameters.ChromGenTolerance = 15;
            workflowParameters.MSToleranceInPPM = 25;
            workflowParameters.MultipleHighQualityMatchesAreAllowed = true;
            workflowParameters.NumChromPeaksAllowedDuringSelection = 10;
            workflowParameters.NumMSScansToSum = 5;
            workflowParameters.ResultType = Globals1.ResultType.TOPDOWN_TARGETED_RESULT;
            workflowParameters.SummingMode = SummingModeEnum.SUMMINGMODE_STATIC;

            const string testDatasetPath =
                @"\\protoapps\UserData\Fujimoto\TopDownTesting\RawData\Proteus_Peri_intact_ETD.raw";


            string resultsFolderLocation = executorParameters.OutputFolderBase+ "\\Results";

            string testDatasetName = RunUtilities.GetDatasetName(testDatasetPath);

            string expectedResultsFilename = Path.Combine(resultsFolderLocation, testDatasetName + "_quant.txt");
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }

            var executor = new TopDownTargetedWorkflowExecutor(executorParameters, workflowParameters, testDatasetPath);

            string proteinSeq1 =
                @"A.AENVVHHKLDGMPISEAVEINAGNNLVFLSGKVPTKKSADAPEGELASYGNTEEQTINVLEQIKTNLNNLGLDMKDVVKMQVFLVGGEENNGTMDFKGFMNGYSKFYDASKTNQLPARSAFQVAKLANPAWRVEIEVIAVRPAK.";

            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => (p.Code == proteinSeq1)).ToList();
            executor.Execute();

            Assert.IsNotNull(executor.TargetedWorkflow.Run);

            Console.WriteLine("Num targetedResults in Run = " + executor.TargetedWorkflow.Run.ResultCollection.MassTagResultList.Count);

            Assert.IsTrue(File.Exists(expectedResultsFilename), "Results file does not exist!");

            var resultsfile = new StreamReader(@"\\protoapps\UserData\Fujimoto\TopDownTesting\Results\missingChargeState\Proteus_Peri_intact_ETD_quant.txt");
            string line = resultsfile.ReadLine();
            line = resultsfile.ReadLine();
            string[] chargestatelist = line.Split('\t');
            Assert.AreEqual("10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22", chargestatelist[2]);
            resultsfile.Close();
        }

        [Test]
        public void TopDownWorkflowTestNoChargeStateList()
        {

            //see https://jira.pnnl.gov/jira/browse/OMCR-101

            var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters.CopyRawFileLocal = false;
            executorParameters.DeleteLocalDatasetAfterProcessing = false;
            executorParameters.TargetsFilePath = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Targets\noChargeStateList\Proteus_Peri_intact_ETD_MSAlign_ResultTable.txt";
            executorParameters.TargetType = Globals.TargetType.LcmsFeature;
            executorParameters.ExportChromatogramData = true;

            var workflowParameters = new TopDownTargetedWorkflowParameters();
            workflowParameters.AreaOfPeakToSumInDynamicSumming = 2;
            workflowParameters.ChromatogramCorrelationIsPerformed = false;
            workflowParameters.ChromGeneratorMode = Globals1.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            workflowParameters.ChromGenSourceDataPeakBR = 4;
            workflowParameters.ChromGenSourceDataSigNoise = 3;
            workflowParameters.ChromNETTolerance = 0.025;
            workflowParameters.ChromPeakDetectorPeakBR = 0.1;
            workflowParameters.ChromPeakDetectorSigNoise = 0.1;
            workflowParameters.ChromPeakSelectorMode = Globals1.PeakSelectorMode.Smart;
            workflowParameters.ChromSmootherNumPointsInSmooth = 9;
            workflowParameters.MaxScansSummedInDynamicSumming = 100;
            workflowParameters.MSPeakDetectorPeakBR = 1.3;
            workflowParameters.MSPeakDetectorSigNoise = 2;
            workflowParameters.ChromGenTolerance = 15;
            workflowParameters.MSToleranceInPPM = 25;
            workflowParameters.MultipleHighQualityMatchesAreAllowed = true;
            workflowParameters.NumChromPeaksAllowedDuringSelection = 10;
            workflowParameters.NumMSScansToSum = 5;
            workflowParameters.ResultType = Globals1.ResultType.TOPDOWN_TARGETED_RESULT;
            workflowParameters.SummingMode = SummingModeEnum.SUMMINGMODE_STATIC;

            const string testDatasetPath =
                @"\\protoapps\UserData\Fujimoto\TopDownTesting\RawData\Proteus_Peri_intact_ETD.raw";


            string resultsFolderLocation = Path.Combine(executorParameters.OutputFolderBase, "Results");

            string testDatasetName = RunUtilities.GetDatasetName(testDatasetPath);

            string expectedResultsFilename = Path.Combine(resultsFolderLocation, testDatasetName + "_quant.txt");
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }

            var executor = new TopDownTargetedWorkflowExecutor(executorParameters, workflowParameters, testDatasetPath);
            executor.Execute();

            /*
            var currentTarget = executor.TargetedWorkflow.Result.Target;
            Console.WriteLine("Target info:");
            Console.WriteLine(currentTarget.ID + "\t" + currentTarget.MonoIsotopicMass + "\t" + currentTarget.MZ + "\t" + currentTarget.ChargeState + "\t"+ currentTarget.EmpiricalFormula);
            Console.WriteLine("Theor profile");
            TestUtilities.DisplayIsotopicProfileData(executor.TargetedWorkflow.Result.Target.IsotopicProfile);
            TestUtilities.DisplayXYValues(executor.TargetedWorkflow.MassSpectrumXYData);
            TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);
            TestUtilities.DisplayPeaks(executor.TargetedWorkflow.ChromPeaksDetected.Select(p=>(Peak)p).ToList());
            */

            Assert.IsNotNull(executor.TargetedWorkflow.Run);

            Console.WriteLine("Num targetedResults in Run = " + executor.TargetedWorkflow.Run.ResultCollection.MassTagResultList.Count);

            Assert.IsTrue(File.Exists(expectedResultsFilename), "Results file does not exist!");

            //var resultsfile = new StreamReader(@"\\protoapps\UserData\Fujimoto\TopDownTesting\Results\noChargeStateList\Proteus_Peri_intact_ETD_quant.txt");
            //string line = resultsfile.ReadLine();
            //line = resultsfile.ReadLine();
            //string[] chargestatelist = line.Split('\t');
            //Assert.AreEqual("10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22", chargestatelist[2]);
            //resultsfile.Close();
        }

        [Test]
        public void TopDownWorkflowTestBestHit()
        {

            var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters.CopyRawFileLocal = false;
            executorParameters.DeleteLocalDatasetAfterProcessing = false;
            executorParameters.TargetsFilePath = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Targets\bestHit\Proteus_Peri_intact_ETD_MSAlign_ResultTable.txt";
            executorParameters.TargetType = Globals.TargetType.LcmsFeature;
            executorParameters.ExportChromatogramData = true;	

            var workflowParameters = new TopDownTargetedWorkflowParameters();
            workflowParameters.AreaOfPeakToSumInDynamicSumming = 2;
            workflowParameters.ChromatogramCorrelationIsPerformed = false;
            workflowParameters.ChromGeneratorMode = Globals1.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            workflowParameters.ChromGenSourceDataPeakBR = 4;
            workflowParameters.ChromGenSourceDataSigNoise = 3;
            workflowParameters.ChromNETTolerance = 0.025;
            workflowParameters.ChromPeakDetectorPeakBR = 0.1;
            workflowParameters.ChromPeakDetectorSigNoise = 0.1;
            workflowParameters.ChromPeakSelectorMode = Globals1.PeakSelectorMode.Smart;
            workflowParameters.ChromSmootherNumPointsInSmooth = 9; 
            workflowParameters.MaxScansSummedInDynamicSumming = 100;
            workflowParameters.MSPeakDetectorPeakBR = 1.3;
            workflowParameters.MSPeakDetectorSigNoise = 2;
            workflowParameters.ChromGenTolerance = 15;
            workflowParameters.MSToleranceInPPM = 25;
            workflowParameters.MultipleHighQualityMatchesAreAllowed = true;
            workflowParameters.NumChromPeaksAllowedDuringSelection = 10;
            workflowParameters.NumMSScansToSum = 5; 
            workflowParameters.ResultType = Globals1.ResultType.TOPDOWN_TARGETED_RESULT;
            workflowParameters.SummingMode = SummingModeEnum.SUMMINGMODE_STATIC;

            const string testDatasetPath =
                @"\\protoapps\UserData\Fujimoto\TopDownTesting\RawData\Proteus_Peri_intact_ETD.raw";


            string resultsFolderLocation = Path.Combine(executorParameters.OutputFolderBase, "Results");

            string testDatasetName = RunUtilities.GetDatasetName(testDatasetPath);

            string expectedResultsFilename = Path.Combine(resultsFolderLocation, testDatasetName + "_quant.txt");
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }

            var executor = new TopDownTargetedWorkflowExecutor(executorParameters, workflowParameters, testDatasetPath);

            executor.Execute();

            Assert.IsNotNull(executor.TargetedWorkflow.Run);
            Assert.IsTrue(File.Exists(expectedResultsFilename), "Results file does not exist!");

        }

        [Test]
        public void TopDownWorkflowFullTargetSet()
        {

            var executorParameters = new TopDownTargetedWorkflowExecutorParameters();
            executorParameters.CopyRawFileLocal = false;
            executorParameters.DeleteLocalDatasetAfterProcessing = false;
            executorParameters.TargetsFilePath = @"\\protoapps\UserData\Fujimoto\TopDownTesting\Targets\fullTargetSet\Proteus_Peri_intact_ETD_MSAlign_ResultTable.Filtered05FDR.txt";
            executorParameters.TargetType = Globals.TargetType.LcmsFeature;
            executorParameters.ExportChromatogramData = true;

            var workflowParameters = new TopDownTargetedWorkflowParameters();
            workflowParameters.AreaOfPeakToSumInDynamicSumming = 2;
            workflowParameters.ChromatogramCorrelationIsPerformed = false;
            workflowParameters.ChromGeneratorMode = Globals1.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            workflowParameters.ChromGenSourceDataPeakBR = 4;
            workflowParameters.ChromGenSourceDataSigNoise = 3;
            workflowParameters.ChromNETTolerance = 0.025;
            workflowParameters.ChromPeakDetectorPeakBR = 0.1;
            workflowParameters.ChromPeakDetectorSigNoise = 0.1;
            workflowParameters.ChromPeakSelectorMode = Globals1.PeakSelectorMode.Smart;
            workflowParameters.ChromSmootherNumPointsInSmooth = 9;
            workflowParameters.MaxScansSummedInDynamicSumming = 100;
            workflowParameters.MSPeakDetectorPeakBR = 1.3;
            workflowParameters.MSPeakDetectorSigNoise = 2;
            workflowParameters.ChromGenTolerance = 15;
            workflowParameters.MSToleranceInPPM = 25;
            workflowParameters.MultipleHighQualityMatchesAreAllowed = true;
            workflowParameters.NumChromPeaksAllowedDuringSelection = 10;
            workflowParameters.NumMSScansToSum = 5;
            workflowParameters.ResultType = Globals1.ResultType.TOPDOWN_TARGETED_RESULT;
            workflowParameters.SummingMode = SummingModeEnum.SUMMINGMODE_STATIC;

            const string testDatasetPath =
                @"\\protoapps\UserData\Fujimoto\TopDownTesting\RawData\Proteus_Peri_intact_ETD.raw";


            string resultsFolderLocation = Path.Combine(executorParameters.OutputFolderBase, "Results");

            string testDatasetName = RunUtilities.GetDatasetName(testDatasetPath);

            string expectedResultsFilename = Path.Combine(resultsFolderLocation, testDatasetName + "_quant.txt");
            if (File.Exists(expectedResultsFilename))
            {
                File.Delete(expectedResultsFilename);
            }

            var executor = new TopDownTargetedWorkflowExecutor(executorParameters, workflowParameters, testDatasetPath);

            executor.Execute();

            Assert.IsNotNull(executor.TargetedWorkflow.Run);
            Assert.IsTrue(File.Exists(expectedResultsFilename), "Results file does not exist!");

        }

    }

}
