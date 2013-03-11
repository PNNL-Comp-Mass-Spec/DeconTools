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

            sampleTarget.AddTarget(executor.Targets[1]);

            sampleTarget.ParentTarget = executor.Targets[2];
            
            sampleTarget.HasParent = true;

            Console.WriteLine("there are " + executor.Targets.Count + " targets");

            Console.WriteLine("We selected " + sampleTarget.EmpiricalFormula);

            IqTarget clonedTarget = new IqTargetBasic(sampleTarget);

            Assert.AreEqual(sampleTarget.ID, clonedTarget.ID);
            Assert.AreEqual(sampleTarget.MZTheor, clonedTarget.MZTheor);
            Assert.AreEqual(sampleTarget.MonoMassTheor, clonedTarget.MonoMassTheor);
            //Assert.AreEqual(sampleTarget.NodeLevel, clonedTarget.NodeLevel);
            Assert.AreEqual(sampleTarget.ChargeState, clonedTarget.ChargeState);
            Assert.AreEqual(sampleTarget.Code, clonedTarget.Code);
            Assert.AreEqual(sampleTarget.ElutionTimeTheor, clonedTarget.ElutionTimeTheor);
            Assert.AreEqual(sampleTarget.EmpiricalFormula, clonedTarget.EmpiricalFormula);
            //Assert.AreEqual(sampleTarget.HasParent, clonedTarget.HasParent);

            Assert.AreEqual(sampleTarget._childTargets.Count, clonedTarget._childTargets.Count);
            Assert.AreEqual(sampleTarget._childTargets[0].ChargeState, clonedTarget._childTargets[0].ChargeState);
            Assert.AreEqual(sampleTarget._childTargets[1].ChargeState, clonedTarget._childTargets[1].ChargeState);
            Assert.AreEqual(sampleTarget._childTargets[2].ChargeState, clonedTarget._childTargets[2].ChargeState);
            Assert.AreEqual(sampleTarget._childTargets[3].ChargeState, clonedTarget._childTargets[3].ChargeState);

            Assert.AreEqual(sampleTarget._childTargets[3]._childTargets.Count, clonedTarget._childTargets[3]._childTargets.Count);
            Assert.AreEqual(sampleTarget._childTargets[3]._childTargets[0].ChargeState, clonedTarget._childTargets[3]._childTargets[0].ChargeState);
            Assert.AreEqual(sampleTarget._childTargets[3]._childTargets[1].ChargeState, clonedTarget._childTargets[3]._childTargets[1].ChargeState);

            //Assert.AreEqual(sampleTarget.ParentTarget, clonedTarget.ParentTarget);

            if (sampleTarget.TheorIsotopicProfile != null)
            {
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.AverageMass, clonedTarget.TheorIsotopicProfile.AverageMass);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.ChargeState, clonedTarget.TheorIsotopicProfile.ChargeState);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.IntensityAggregateAdjusted, clonedTarget.TheorIsotopicProfile.IntensityAggregateAdjusted);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.IntensityMostAbundant, clonedTarget.TheorIsotopicProfile.IntensityMostAbundant);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.IntensityMostAbundantTheor, clonedTarget.TheorIsotopicProfile.IntensityMostAbundantTheor);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.IsSaturated, clonedTarget.TheorIsotopicProfile.IsSaturated);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.MonoIsotopicMass, clonedTarget.TheorIsotopicProfile.MonoIsotopicMass);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.MonoIsotopicPeakIndex, clonedTarget.TheorIsotopicProfile.MonoIsotopicPeakIndex);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.MonoPeakMZ, clonedTarget.TheorIsotopicProfile.MonoPeakMZ);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.MonoPlusTwoAbundance, clonedTarget.TheorIsotopicProfile.MonoPlusTwoAbundance);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.MostAbundantIsotopeMass, clonedTarget.TheorIsotopicProfile.MostAbundantIsotopeMass);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.OriginalIntensity, clonedTarget.TheorIsotopicProfile.OriginalIntensity);
                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.Score, clonedTarget.TheorIsotopicProfile.Score);

                Assert.AreEqual(sampleTarget.TheorIsotopicProfile.Peaklist.Count, clonedTarget.TheorIsotopicProfile.Peaklist.Count);
            }


        }
    }
}
