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
            //set data files
            string targetsFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            string resultsFolder = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Results";
            string peaksTestFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

            var executorParameters = new BasicTargetedWorkflowExecutorParameters {ResultsFolder = resultsFolder, TargetsFilePath = targetsFile};

            var executor = new IqExecutor(executorParameters) {ChromSourceDataFilePath = peaksTestFile};

            //load targets
            executor.LoadAndInitializeTargets(targetsFile);

            //select some targets
            executor.Targets = (from n in executor.Targets where n.ElutionTimeTheor > 0.305 && n.ElutionTimeTheor < 0.325 select n).Take(10).ToList();

            //make composite target for testing
            IqTarget rootTarget = executor.Targets[0];

            IqTarget node1TargetAddition = executor.Targets[1];
            rootTarget.AddTarget(node1TargetAddition);

            IqTarget node2TargetAddition = executor.Targets[2];
            node1TargetAddition.AddTarget(node2TargetAddition);

            //set entry point
            IqTarget sampleTarget = node2TargetAddition;

            Console.WriteLine("there are " + executor.Targets.Count + " targets");

            Console.WriteLine("We selected " + sampleTarget.EmpiricalFormula + Environment.NewLine);

            //clone target
            IqTarget clonedTarget = IqTarget.CloneIqTarget(sampleTarget);

            //unit test
            Console.WriteLine("Check Basics");
            Assert.AreEqual(sampleTarget.ID, clonedTarget.ID);
            Assert.AreEqual(sampleTarget.MZTheor, clonedTarget.MZTheor);
            Assert.AreEqual(sampleTarget.MonoMassTheor, clonedTarget.MonoMassTheor);
            Assert.AreEqual(sampleTarget.ChargeState, clonedTarget.ChargeState);
            Assert.AreEqual(sampleTarget.Code, clonedTarget.Code);
            Assert.AreEqual(sampleTarget.ElutionTimeTheor, clonedTarget.ElutionTimeTheor);
            Assert.AreEqual(sampleTarget.EmpiricalFormula, clonedTarget.EmpiricalFormula);
            Console.WriteLine("--Success" + Environment.NewLine);

            Console.WriteLine("Check Children");
            Assert.AreEqual(sampleTarget.HasChildren(), clonedTarget.HasChildren());
            List<IqTarget> sampleTargetChildren = sampleTarget.ChildTargets().ToList();
            List<IqTarget> clonedTargetChildren = clonedTarget.ChildTargets().ToList();
            Assert.AreEqual(sampleTargetChildren.Count, clonedTargetChildren.Count);
            Assert.AreEqual(sampleTargetChildren[0].ChargeState, clonedTargetChildren[0].ChargeState);
            Assert.AreEqual(sampleTargetChildren[1].ChargeState, clonedTargetChildren[1].ChargeState);
            Assert.AreEqual(sampleTargetChildren[2].ChargeState, clonedTargetChildren[2].ChargeState);
            Console.WriteLine("--Success" + Environment.NewLine);

            Console.WriteLine("Check Parent");
            Assert.AreEqual(sampleTarget.HasParent, clonedTarget.HasParent);
            Assert.AreEqual(sampleTarget.NodeLevel, clonedTarget.NodeLevel);
            Assert.AreEqual(sampleTarget.ParentTarget.ChargeState, clonedTarget.ParentTarget.ChargeState);
            List<IqTarget> sampleTargetChildrenParent = sampleTarget.ParentTarget.ChildTargets().ToList();
            List<IqTarget> clonedTargetChildrenParent = clonedTarget.ParentTarget.ChildTargets().ToList();
            Assert.AreEqual(sampleTargetChildrenParent.Count, clonedTargetChildrenParent.Count);
            Assert.AreEqual(sampleTargetChildrenParent[0].ChargeState, clonedTargetChildrenParent[0].ChargeState);
            Assert.AreEqual(sampleTargetChildrenParent[1].ChargeState, clonedTargetChildrenParent[1].ChargeState);
            Assert.AreEqual(sampleTargetChildrenParent[2].ChargeState, clonedTargetChildrenParent[2].ChargeState);
            Console.WriteLine("--Success" + Environment.NewLine);

            Console.WriteLine("Check Root");
            Assert.AreEqual(sampleTarget.RootTarget.ChargeState, clonedTarget.RootTarget.ChargeState);
            Assert.AreEqual(sampleTarget.RootTarget.EmpiricalFormula, clonedTarget.RootTarget.EmpiricalFormula);
            Assert.AreEqual(sampleTarget.RootTarget.HasChildren(), clonedTarget.RootTarget.HasChildren());

            List<IqTarget> sampleTargetChildrenRoot = sampleTarget.RootTarget.ChildTargets().ToList();
            List<IqTarget> clonedTargetChildrenRoot = clonedTarget.RootTarget.ChildTargets().ToList();
            Assert.AreEqual(sampleTargetChildrenRoot[0].ChargeState, clonedTargetChildrenRoot[0].ChargeState);
            Assert.AreEqual(sampleTargetChildrenRoot[1].ChargeState, clonedTargetChildrenRoot[1].ChargeState);
            Assert.AreEqual(sampleTargetChildrenRoot[2].ChargeState, clonedTargetChildrenRoot[2].ChargeState);
            Assert.AreEqual(sampleTargetChildrenRoot[3].ChargeState, clonedTargetChildrenRoot[3].ChargeState);
            Console.WriteLine("--Success" + Environment.NewLine);


            //not set up yet
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
