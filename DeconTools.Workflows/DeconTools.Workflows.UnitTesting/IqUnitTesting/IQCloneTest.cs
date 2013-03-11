using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.IqUnitTesting
{
    
    class IQCloneTest
    {
        [Test]
        public void PTMMassFromCodeMSGFTest()
        {
            string targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            string resultsFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results";
            string peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            //targetsFile = executorParameters.TargetsFilePath;

            executorParameters.ResultsFolder = resultsFolder;
            executorParameters.TargetsFilePath = targetsFile;

            var executor = new IqExecutor(executorParameters);
            executor.ChromSourceDataFilePath = peaksTestFile;

            executor.LoadAndInitializeTargets(targetsFile);

            executor.Targets = (from n in executor.Targets where n.ElutionTimeTheor > 0.305 && n.ElutionTimeTheor < 0.325 select n).Take(10).ToList();

            IqTarget sampleTarget = executor.Targets[0];

            

            Console.WriteLine("there are " + executor.Targets.Count + " targets");

            Console.WriteLine("We selected " + sampleTarget.EmpiricalFormula);
        }
    }
}
