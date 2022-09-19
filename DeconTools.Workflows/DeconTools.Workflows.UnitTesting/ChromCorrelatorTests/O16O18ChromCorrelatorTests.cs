using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.ChromCorrelatorTests
{
    [TestFixture]
    public class O16O18ChromCorrelatorTests
    {
        [Test]
        public void ChromCorrelatorTest1()
        {
            var rawFilename = @"D:\Data\O16O18\Vlad_Mouse\mhp_plat_test_1_14April13_Frodo_12-12-04.raw";
            var run = new RunFactory().CreateRun(rawFilename);

            WorkflowExecutorBaseParameters executorBaseParameters = new BasicTargetedWorkflowExecutorParameters();
            var executor = new IqExecutor(executorBaseParameters, run);
            executor.LoadChromData(run);

            var workflowAssigner = new IqWorkflowAssigner();

            TargetedWorkflowParameters workflowParameters = new O16O18WorkflowParameters();
            IqWorkflow workflow = new O16O18IqWorkflow(run,workflowParameters);

            IqTarget iqTarget = new IqChargeStateTarget();
            iqTarget.EmpiricalFormula = "C58H100N18O20";
            iqTarget.Code = "GAAQNIIPASTGAAK";
            iqTarget.ID = 1093;
            iqTarget.ChargeState = 0;

            var iqTargetList = new List<IqTarget>();
            iqTargetList.Add(iqTarget);

            var utilities = new IqTargetUtilities();
            utilities.CreateChildTargets(iqTargetList);

            workflowAssigner.AssignWorkflowToParent(workflow, iqTargetList);
            workflowAssigner.AssignWorkflowToChildren(workflow, iqTargetList);

            var theorFeatureGenerator = new JoshTheorFeatureGenerator();
            foreach (var target in iqTarget.ChildTargets())
            {
                utilities.UpdateTargetMissingInfo(target);
                target.TheorIsotopicProfile = theorFeatureGenerator.GenerateTheorProfile(target.EmpiricalFormula, target.ChargeState);
            }

            foreach (var childTarget in iqTarget.ChildTargets())
            {
                var result = childTarget.CreateResult();
                var correlator = new O16O18ChromCorrelator(7, 0.1, 20, Globals.ToleranceUnit.PPM);
                var corrData = correlator.CorrelateData(run, result, 5700, 6500);

                var corrDataItem1 = corrData.CorrelationDataItems.First();
                Console.WriteLine("z= \t"+  childTarget.ChargeState + "\tCorrelationData (slope,intercept,rsquared)= \t" +  corrDataItem1);
            }
        }
    }
}
