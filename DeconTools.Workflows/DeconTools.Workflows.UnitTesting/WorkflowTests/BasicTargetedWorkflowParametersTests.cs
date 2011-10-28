using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class BasicTargetedWorkflowParametersTests
    {
        [Test]
        public void exportParametersTest1()
        {
            string exportedParametersFile = FileRefs.OutputFolderPath + "\\" + "exportedBasicTargetedWorkflowParameters.xml";

            BasicTargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.SaveParametersToXML(exportedParametersFile);

        }

        [Test]
        public void importParametersTest1()
        {
            string importedParametersFile = FileRefs.ImportedData + "\\" + "importedBasicTargetedWorkflowParameters.xml";

            BasicTargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.LoadParameters(importedParametersFile);

            Assert.AreEqual("O16O18_TARGETED_RESULT", parameters.ResultType.ToString());
            

        }


        [Test]
        public void createParametersObjectTest1()
        {
            string importedParametersFile = FileRefs.ImportedData + "\\" + "importedBasicTargetedWorkflowParameters.xml";
            WorkflowParameters wp = WorkflowParameters.CreateParameters(importedParametersFile);

            Assert.AreEqual("UnlabelledTargeted1", wp.WorkflowType);
            Assert.IsTrue(wp is BasicTargetedWorkflowParameters);

            wp.LoadParameters(importedParametersFile);

        }

    }
}
