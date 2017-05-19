using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.IqUnitTesting
{
    class ChromPeakAnalyzerIqWorkflowTests
    {
        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void ChromPeakAnalyzerWorkflowWrongTargetTest()
        {
            //Reference JIRA: https://jira.pnnl.gov/jira/browse/OMCS-884

            var testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var run = new RunFactory().CreateRun(testFile);
            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            IqTarget testTarget = new IqChargeStateTarget();
            testTarget.SetWorkflow(new ChromPeakAnalyzerIqWorkflow(run, parameters));
            testTarget.DoWorkflow();
        }

        [Test]
        public void ChromPeakAnalyzerWorkflowTest()
        {
            //Reference JIRA: https://jira.pnnl.gov/jira/browse/OMCS-884

            var testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            var run = new RunFactory().CreateRun(testFile);
            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            var testTarget = new ChromPeakIqTarget();
            ITheorFeatureGenerator TheorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);

            var testPeak = new ChromPeak(5184, 840963, 50, 0);

            var executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            var executor = new TopDownMSAlignExecutor(executorBaseParameters, run);
            executor.ChromSourceDataFilePath = peaksTestFile;

            var workflow = new ChromPeakAnalyzerIqWorkflow(run, parameters);
            testTarget.SetWorkflow(workflow);

            testTarget.ID = 1;
            testTarget.Code = "NGIIMMENR";
            testTarget.EmpiricalFormula = "C43H76N14O14S2";
            testTarget.MonoMassTheor = 1076.510631;
            testTarget.ChromPeak = testPeak;
            testTarget.ChargeState = 1;
            testTarget.TheorIsotopicProfile = TheorFeatureGen.GenerateTheorProfile(testTarget.EmpiricalFormula, testTarget.ChargeState);

            var testTargetList = new List<IqTarget>();
            IqTarget testParent = new IqChargeStateTarget();
            testParent.AddTarget(testTarget);
            testTargetList.Add(testTarget);

            executor.Execute(testTargetList);

            var result = testTarget.GetResult();

            Assert.NotNull(result.LCScanSetSelected);
            Assert.AreNotEqual(1, result.FitScore);
            Assert.AreEqual(true, result.IsotopicProfileFound);
        }
    }
}
