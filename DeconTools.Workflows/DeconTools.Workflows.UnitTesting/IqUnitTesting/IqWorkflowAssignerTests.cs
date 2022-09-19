using System.Linq;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.IqUnitTesting
{
    [TestFixture]
    public class IqWorkflowAssignerTests
    {
        [Test]
        public void Test1()
        {
            TargetedWorkflowParameters o16o18workflowParameters = new O16O18WorkflowParameters();
            var parentWorkflow = new O16O18IqWorkflow(o16o18workflowParameters);

            TargetedWorkflowParameters workflowParameters = new BasicTargetedWorkflowParameters();
            var childWorkflow = new BasicIqWorkflow(workflowParameters);

            IqTarget parentTarget = new IqChargeStateTarget();

            IqTarget childTarget1 = new IqChargeStateTarget();
            IqTarget childTarget2 = new IqChargeStateTarget();
            IqTarget childTarget3 = new IqChargeStateTarget();

            parentTarget.AddTarget(childTarget1);
            parentTarget.AddTarget(childTarget2);
            parentTarget.AddTarget(childTarget3);

            var workflowAssigner = new IqWorkflowAssigner();

            workflowAssigner.AssignWorkflowToParent(parentWorkflow, parentTarget);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, parentTarget);

            Assert.IsNotNull(parentTarget.ChildTargets().First().Workflow);

            var childTargets = parentTarget.ChildTargets();

            foreach (var childTarget in childTargets)
            {
                Assert.IsTrue(childTarget.Workflow is BasicIqWorkflow);
            }

            Assert.IsTrue(parentTarget.Workflow is O16O18IqWorkflow);
        }
    }
}
