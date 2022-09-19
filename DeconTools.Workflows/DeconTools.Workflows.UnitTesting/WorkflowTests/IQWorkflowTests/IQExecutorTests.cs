using System;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests.IQWorkflowTests
{
    [TestFixture]
    public class IQExecutorTests
    {
        [Test]
        public void Isotest1()
        {
            var inputMass = 15.567;
            var empForumla = IsotopicDistributionCalculator.Instance.GetAveragineFormulaAsString(inputMass, false);
            Console.WriteLine(empForumla);

            var formulaTable = IsotopicDistributionCalculator.Instance.GetAveragineFormulaAsTable(inputMass);
        }

        [Category("MustPass")]
        [Test]
        public void ExecutorCreatingTargetsTest1()
        {
            var util = new IqTargetUtilities();
            var testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

            var targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            var resultsFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results";

            var expectedResultsFilename = Path.Combine(resultsFolder, RunUtilities.GetDatasetName(testFile) + "_iqResults.txt");
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);

            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.TargetsFilePath = targetsFile;

            //create no more than two charge state targets per peptide
            executorBaseParameters.MaxNumberOfChargeStateTargetsToCreate = 2;

            var run = new RunFactory().CreateRun(testFile);

            var executor = new IqExecutor(executorBaseParameters, run);
            executor.ChromSourceDataFilePath = peaksTestFile;

            executor.LoadAndInitializeTargets(targetsFile);
            executor.Targets = (from n in executor.Targets where n.ElutionTimeTheor > 0.305 && n.ElutionTimeTheor < 0.325 select n).Take(10).ToList();

            foreach (var iqTarget in executor.Targets)
            {
                var numChildTargets = iqTarget.GetChildCount();

                Assert.IsTrue(numChildTargets <= 2);    //MaxNumberOfChargeStateTargetsToCreate = 2;

                Console.WriteLine(iqTarget + "\t" + numChildTargets);
            }
        }

        //"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Targets\QCShew_Formic_MassTags_Bin10_singleTest1.txt"

        [Category("MustPass")]
        [Test]
        public void ExecuteMultipleTargetsTest1()
        {
            var util = new IqTargetUtilities();
            var testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

            var targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.TargetsFilePath = targetsFile;
            executorBaseParameters.OutputDirectoryBase = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled";

            var expectedResultsFilename = Path.Combine(executorBaseParameters.OutputDirectoryBase,
                                                          "IqResults",
                                                          RunUtilities.GetDatasetName(testFile) + "_iqResults.txt");
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);

            var run = new RunFactory().CreateRun(testFile);

            var executor = new IqExecutor(executorBaseParameters, run);
            executor.ChromSourceDataFilePath = peaksTestFile;

            executor.LoadAndInitializeTargets(targetsFile);
            executor.Targets = (from n in executor.Targets where n.ElutionTimeTheor > 0.305 && n.ElutionTimeTheor < 0.325 select n).Take(10).ToList();

            var targetedWorkflowParameters = new BasicTargetedWorkflowParameters();
            targetedWorkflowParameters.ChromNETTolerance = 0.5;

            //define workflows for parentTarget and childTargets
            var parentWorkflow = new BasicIqWorkflow(run, targetedWorkflowParameters);
            var childWorkflow = new BasicIqWorkflow(run, targetedWorkflowParameters);

            var workflowAssigner = new IqWorkflowAssigner();
            workflowAssigner.AssignWorkflowToParent(parentWorkflow, executor.Targets);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, executor.Targets);

            //Main line for executing IQ:
            executor.Execute();

            //Test the results...
            Assert.IsTrue(File.Exists(expectedResultsFilename), "results file doesn't exist");
            var numResultsInResultsFile = 0;
            var outputToConsole = true;

            using (var reader = new StreamReader(expectedResultsFilename))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    numResultsInResultsFile++;

                    if (outputToConsole)
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            Assert.IsTrue(numResultsInResultsFile > 1, "No results in output file");

            //the Result Tree is flattened out in the results file.
            Assert.IsTrue(numResultsInResultsFile == 11);

            //the results in the Executor are in the a Result tree. So there should be just 10.
            Assert.AreEqual(10, executor.Results.Count);
        }

        [Category("MustPass")]
        [Test]
        public void ExecuteMultipleTargetsTest2()
        {
            var util = new IqTargetUtilities();
            var testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

            var targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.TargetsFilePath = targetsFile;
            executorBaseParameters.OutputDirectoryBase = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled";
            executorBaseParameters.IsMassAlignmentPerformed = true;
            executorBaseParameters.IsNetAlignmentPerformed = true;

            var expectedResultsFilename = Path.Combine(executorBaseParameters.OutputDirectoryBase,
                                                          "IqResults",
                                                          RunUtilities.GetDatasetName(testFile) + "_iqResults.txt");
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);

            var run = new RunFactory().CreateRun(testFile);

            var executor = new IqExecutor(executorBaseParameters, run);
            executor.ChromSourceDataFilePath = peaksTestFile;
            executor.LoadAndInitializeTargets(targetsFile);
            executor.Targets = (from n in executor.Targets where n.ElutionTimeTheor > 0.305 && n.ElutionTimeTheor < 0.325 select n).Take(10).ToList();

            var targetedWorkflowParameters = new BasicTargetedWorkflowParameters();
            targetedWorkflowParameters.ChromNETTolerance = 0.01;

            //define workflows for parentTarget and childTargets
            var parentWorkflow = new BasicIqWorkflow(run, targetedWorkflowParameters);
            var childWorkflow = new BasicIqWorkflow(run, targetedWorkflowParameters);

            var workflowAssigner = new IqWorkflowAssigner();
            workflowAssigner.AssignWorkflowToParent(parentWorkflow, executor.Targets);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, executor.Targets);

            executor.SetupMassAndNetAlignment();
            executor.DoAlignment();

            //Main line for executing IQ:
            executor.Execute();

            //Test the results...
            Assert.IsTrue(File.Exists(expectedResultsFilename), "results file doesn't exist");
            var numResultsInResultsFile = 0;
            var outputToConsole = true;

            using (var reader = new StreamReader(expectedResultsFilename))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    numResultsInResultsFile++;

                    if (outputToConsole)
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            Assert.IsTrue(numResultsInResultsFile > 1, "No results in output file");

            //the Result Tree is flattened out in the results file.
            Assert.IsTrue(numResultsInResultsFile == 11);

            //the results in the Executor are in the a Result tree. So there should be just 10.
            Assert.AreEqual(10, executor.Results.Count);
        }

        [Test]
        public void ExecutorTest1()
        {
            var util = new IqTargetUtilities();
            var testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";
            var targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.TargetsFilePath = targetsFile;
            executorBaseParameters.OutputDirectoryBase = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled";

            var expectedResultsFilename = Path.Combine(executorBaseParameters.OutputDirectoryBase,
                                                          "IqResults",
                                                          RunUtilities.GetDatasetName(testFile) + "_iqResults.txt");
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);

            var run = new RunFactory().CreateRun(testFile);

            var executor = new IqExecutor(executorBaseParameters, run);
            executor.ChromSourceDataFilePath = peaksTestFile;

            executor.LoadAndInitializeTargets(targetsFile);
            executor.Targets = (from n in executor.Targets where n.ElutionTimeTheor > 0.305 && n.ElutionTimeTheor < 0.325 select n).Take(10).ToList();

            executor.Targets = (from n in executor.Targets where n.ID == 27168 select n).ToList();

            var targetedWorkflowParameters = new BasicTargetedWorkflowParameters();
            targetedWorkflowParameters.ChromNETTolerance = 0.5;

            //define workflows for parentTarget and childTargets
            var parentWorkflow = new ChromPeakDeciderIqWorkflow(run, targetedWorkflowParameters);
            var childWorkflow = new ChargeStateChildIqWorkflow(run, targetedWorkflowParameters);

            var workflowAssigner = new IqWorkflowAssigner();
            workflowAssigner.AssignWorkflowToParent(parentWorkflow, executor.Targets);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, executor.Targets);

            //Main line for executing IQ:
            executor.Execute();

            //Test the results...
            Assert.IsTrue(File.Exists(expectedResultsFilename), "results file doesn't exist");
            var numResultsInResultsFile = 0;
            var outputToConsole = true;

            using (var reader = new StreamReader(expectedResultsFilename))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    numResultsInResultsFile++;

                    if (outputToConsole)
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            //Assert.IsTrue(numResultsInResultsFile > 1, "No results in output file");

            //the results in the Executor are in the a Result tree. So there should be just 10.
            //Assert.AreEqual(10, executor.Results.Count);
        }

        [Category("MustPass")]
        [Test]
        public void Executor_loadAlignment_Test1()
        {
            var util = new IqTargetUtilities();
            var testFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            var peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";
            var targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.TargetsFilePath = targetsFile;
            executorBaseParameters.IsMassAlignmentPerformed = true;
            executorBaseParameters.IsNetAlignmentPerformed = true;
            executorBaseParameters.OutputDirectoryBase = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled";
            var run = new RunFactory().CreateRun(testFile);

            var expectedResultsFilename = executorBaseParameters.OutputDirectoryBase  +"\\IqResults\\" + RunUtilities.GetDatasetName(testFile) + "_iqResults.txt";
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);

            var executor = new IqExecutor(executorBaseParameters, run);
            executor.ChromSourceDataFilePath = peaksTestFile;

            executor.LoadAndInitializeTargets(targetsFile);
            executor.Targets = (from n in executor.Targets where n.ElutionTimeTheor > 0.305 && n.ElutionTimeTheor < 0.325 select n).Take(10).ToList();

            executor.Targets = (from n in executor.Targets where n.ID == 27168 select n).ToList();

            executor.SetupMassAndNetAlignment();
            executor.DoAlignment();

            var targetedWorkflowParameters = new BasicTargetedWorkflowParameters();
            targetedWorkflowParameters.ChromNETTolerance = 0.1;

            //define workflows for parentTarget and childTargets
            var parentWorkflow = new ChromPeakDeciderIqWorkflow(run, targetedWorkflowParameters);
            var childWorkflow = new ChargeStateChildIqWorkflow(run, targetedWorkflowParameters);

            var workflowAssigner = new IqWorkflowAssigner();
            workflowAssigner.AssignWorkflowToParent(parentWorkflow, executor.Targets);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, executor.Targets);

            //Main line for executing IQ:
            executor.Execute();

            //Test the results...
            Assert.IsTrue(File.Exists(expectedResultsFilename), "results file doesn't exist");
            var numResultsInResultsFile = 0;
            var outputToConsole = true;

            using (var reader = new StreamReader(expectedResultsFilename))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    numResultsInResultsFile++;

                    if (outputToConsole)
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            Assert.IsTrue(numResultsInResultsFile > 1, "No results in output file");

            //the results in the Executor are in the a Result tree. So there should be just 10.
            Assert.AreEqual(10, executor.Results.Count);
        }
    }
}
