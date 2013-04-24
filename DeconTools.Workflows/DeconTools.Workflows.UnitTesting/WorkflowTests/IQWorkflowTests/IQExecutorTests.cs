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
    public class IQExecutorTests
    {

        [Test]
        public void Isotest1()
        {
            double inputMass = 15.567;
            var empForumla = IsotopicDistributionCalculator.Instance.GetAveragineFormulaAsString(inputMass, false);
            Console.WriteLine(empForumla);

            var formulaTable = IsotopicDistributionCalculator.Instance.GetAveragineFormulaAsTable(inputMass);



        }

        [Category("MustPass")]
        [Test]
        public void ExecutorCreatingTargetsTest1()
        {
            var util = new IqTargetUtilities();
            string testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

            string targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            string resultsFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results";

            string expectedResultsFilename = resultsFolder + "\\" + RunUtilities.GetDatasetName(testFile) + "_iqResults.txt";
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);


            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.ResultsFolder = resultsFolder;
            executorBaseParameters.TargetsFilePath = targetsFile;
            
            
            //create no more than two charge state targets per peptide
            executorBaseParameters.MaxNumberOfChargeStateTargetsToCreate = 2;

            var executor = new IqExecutor(executorBaseParameters);
            executor.ChromSourceDataFilePath = peaksTestFile;

            executor.LoadAndInitializeTargets(targetsFile);
            executor.Targets = (from n in executor.Targets where n.ElutionTimeTheor > 0.305 && n.ElutionTimeTheor < 0.325 select n).Take(10).ToList();

            foreach (var iqTarget in executor.Targets)
            {
                int numChildTargets = iqTarget.GetChildCount();

                Assert.IsTrue(numChildTargets <= 2);    //MaxNumberOfChargeStateTargetsToCreate = 2;

                Console.WriteLine(iqTarget + "\t" + numChildTargets);
            }

          
        }


        //"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Targets\QCShew_Formic_MassTags_Bin10_singleTest1.txt"

        [Category("MustPass")]
        [Test]
        public void ExecutorIqTest1()
        {
            var util = new IqTargetUtilities();
            string testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

            string targetsFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Targets\QCShew_Formic_MassTags_Bin10_singleTest1.txt";

            string resultsFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results";

            string expectedResultsFilename = resultsFolder + "\\" + RunUtilities.GetDatasetName(testFile) + "_iqResults.txt";
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);


            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.ResultsFolder = resultsFolder;
            executorBaseParameters.TargetsFilePath = targetsFile;
            executorBaseParameters.MaxMzForDefiningChargeStateTargets = 2000;


            //create no more than two charge state targets per peptide
            //executorBaseParameters.MaxNumberOfChargeStateTargetsToCreate = 2;

            var executor = new IqExecutor(executorBaseParameters);
            executor.ChromSourceDataFilePath = peaksTestFile;

            executor.LoadAndInitializeTargets(targetsFile);
            

            foreach (var iqTarget in executor.Targets)
            {
                int numChildTargets = iqTarget.GetChildCount();

                
                Console.WriteLine(iqTarget + "\t" + numChildTargets);
            }


            Run run = new RunFactory().CreateRun(testFile);
            executor.SetRun(run);

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

            //Test the results...
            Assert.IsTrue(File.Exists(expectedResultsFilename), "results file doesn't exist");
            int numResultsInResultsFile = 0;
            bool outputToConsole = true;

            using (StreamReader reader = new StreamReader(expectedResultsFilename))
            {
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    numResultsInResultsFile++;

                    if (outputToConsole)
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            Assert.IsTrue(numResultsInResultsFile > 1, "No results in output file");

            //the results in the Executor are in the a Result tree. So there should be just 10. 
            Assert.AreEqual(1, executor.Results.Count);


        }



        [Category("MustPass")]
        [Test]
        public void ExecuteMultipleTargetsTest1()
        {
            var util = new IqTargetUtilities();
            string testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

            string targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            string resultsFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results";

            string expectedResultsFilename = resultsFolder + "\\" + RunUtilities.GetDatasetName(testFile) + "_iqResults.txt";
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);


            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.ResultsFolder = resultsFolder;
            executorBaseParameters.TargetsFilePath = targetsFile;


            var executor = new IqExecutor(executorBaseParameters);
            executor.ChromSourceDataFilePath = peaksTestFile;

            executor.LoadAndInitializeTargets(targetsFile);
            executor.Targets = (from n in executor.Targets where n.ElutionTimeTheor > 0.305 && n.ElutionTimeTheor < 0.325 select n).Take(10).ToList();

            Run run = new RunFactory().CreateRun(testFile);
            executor.SetRun(run);

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

            //Test the results...
            Assert.IsTrue(File.Exists(expectedResultsFilename), "results file doesn't exist");
            int numResultsInResultsFile = 0;
            bool outputToConsole = true;

            using (StreamReader reader = new StreamReader(expectedResultsFilename))
            {
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    numResultsInResultsFile++;

                    if (outputToConsole)
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            Assert.IsTrue(numResultsInResultsFile > 1, "No results in output file");

            //the Result Tree is flattened out in the results file.
            Assert.IsTrue(numResultsInResultsFile == 35);

            //the results in the Executor are in the a Result tree. So there should be just 10. 
            Assert.AreEqual(10, executor.Results.Count);

        }



    }
}
