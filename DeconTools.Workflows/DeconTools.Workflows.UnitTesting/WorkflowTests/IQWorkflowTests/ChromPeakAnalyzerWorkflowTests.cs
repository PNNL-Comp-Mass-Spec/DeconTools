using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests.IQWorkflowTests
{
    class ChromPeakAnalyzerWorkflowTests
    {
        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void ChromPeakAnalyzerWorkflowWrongTargetTest()
        {
            //Reference JIRA: https://jira.pnnl.gov/jira/browse/OMCS-884

            string testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            Run run = new RunFactory().CreateRun(testFile);
            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            IqTarget testTarget = new IqChargeStateTarget();
            testTarget.SetWorkflow(new ChromPeakAnalyzerIqWorkflow(run, parameters));
            testTarget.DoWorkflow();
        }

        [Test]
        public void ChromPeakAnalyzerWorkflowTest()
        {
            //Reference JIRA: https://jira.pnnl.gov/jira/browse/OMCS-884

            string testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            Run run = new RunFactory().CreateRun(testFile);
            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            ChromPeakIqTarget testTarget = new ChromPeakIqTarget();
            ITheorFeatureGenerator TheorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);

            ChromPeak testPeak = new ChromPeak(5184, 840963, 50, 0);

            testTarget.SetWorkflow(new ChromPeakAnalyzerIqWorkflow(run, parameters));

            testTarget.Code = "NGIIMMENR";
            testTarget.EmpiricalFormula = "C43H76N14O14S2";
            testTarget.MonoMassTheor = 1076.510631;
            testTarget.ChromPeak = testPeak;
            testTarget.ChargeState = 1;
            testTarget.TheorIsotopicProfile = TheorFeatureGen.GenerateTheorProfile(testTarget.EmpiricalFormula, testTarget.ChargeState);

            testTarget.DoWorkflow();

            IqResult result = testTarget.GetResult();

            Console.WriteLine("Fit Score: " + result.FitScore + " Flagged: " + result.IsIsotopicProfileFlagged);
        }
    }
}
