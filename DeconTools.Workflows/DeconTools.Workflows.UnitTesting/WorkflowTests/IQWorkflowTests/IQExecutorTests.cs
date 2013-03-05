using System;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests.IQWorkflowTests
{
    public class IQExecutorTests
    {

        [Test]
        public void Isotest1()
        {
            double inputMass = 15.567;
            var empForumla=  IsotopicDistributionCalculator.Instance.GetAveragineFormulaAsString(inputMass,false);
            Console.WriteLine(empForumla);

            var formulaTable = IsotopicDistributionCalculator.Instance.GetAveragineFormulaAsTable(inputMass);



        }


        [Category("MustPass")]
        [Test]
        public void ExecuteMultipleTargetsTest1()
        {
            var util = new IqTargetUtilities();
            string testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

            string targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            string resultsFolder=  @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results";

            string expectedResultsFilename = resultsFolder+ "\\"+ RunUtilities.GetDatasetName(testFile) + "_iqResults.txt";
            if (File.Exists(expectedResultsFilename)) File.Delete(expectedResultsFilename);

            LabelFreeIqTargetImporter importer = new LabelFreeIqTargetImporter(targetsFile);


            IqTargetUtilities targetUtilities = new IqTargetUtilities();

            var targets = importer.Import();

            //select distinct Target ID's 
            targets = (from n in targets
                       group n by new
                       {
                           n.ID,
                       }
                           into grp
                           select grp.First()).ToList();

            //Only use targets within this range (since _peaks file is small)
            targets = (from n in targets where n.ElutionTimeTheor > 0.305 && n.ElutionTimeTheor < 0.325 select n).Take(10).ToList();

            //Create child charge state targets
            foreach (IqTarget iqTarget in targets)
            {
                targetUtilities.UpdateTargetMissingInfo(iqTarget);

                var childTargets = targetUtilities.CreateChargeStateTargets(iqTarget);
                iqTarget.AddTargetRange(childTargets);
            }

            Run run = new RunFactory().CreateRun(testFile);
            var targetedWorkflowParameters = new BasicTargetedWorkflowParameters();
            targetedWorkflowParameters.ChromNETTolerance = 0.5;

            var workflow = new BasicIqWorkflow(run, targetedWorkflowParameters);
            foreach (IqTarget iqTarget in targets)
            {

                foreach (IqTarget childTarget in iqTarget.ChildTargets())
                {
                    childTarget.SetWorkflow(workflow);
                }

                iqTarget.SetWorkflow(workflow);
            }

            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            executorBaseParameters.ChromGenSourceDataPeakBR = 3;
            executorBaseParameters.ChromGenSourceDataSigNoise = 2;
            executorBaseParameters.ResultsFolder = resultsFolder;

            var executor = new IqExecutor(executorBaseParameters);
            executor.ChromSourceDataFilePath = peaksTestFile;
            executor.Execute(targets);

            Assert.IsTrue(File.Exists(expectedResultsFilename), "results file doesn't exist");
            int numLinesInResults = 0;
            bool outputToConsole = true;

            using (StreamReader reader=new StreamReader(expectedResultsFilename))
            {
                while (reader.Peek()!=-1)
                {
                    string line= reader.ReadLine();
                    numLinesInResults++;    

                    if (outputToConsole)
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            Assert.IsTrue(numLinesInResults > 1,"No results in output file");
            Assert.IsTrue(numLinesInResults == 35);


        }



    }
}
