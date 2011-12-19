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
        public void importParametersTest2()
        {
            string importedParametersFile = FileRefs.ImportedData + "\\" + "importedParameters_MostIntenseChromPeakSelection.xml";

            BasicTargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.LoadParameters(importedParametersFile);

            Assert.AreEqual("BASIC_TARGETED_RESULT", parameters.ResultType.ToString());
            Assert.AreEqual("MostIntense", parameters.ChromPeakSelectorMode.ToString());
        }

        [Test]
        public void createParametersObjectTest1()
        {
            string importedParametersFile = FileRefs.ImportedData + "\\" + "importedBasicTargetedWorkflowParameters.xml";
            WorkflowParameters wp = WorkflowParameters.CreateParameters(importedParametersFile);

            Assert.AreEqual("UnlabelledTargeted1", wp.WorkflowType.ToString());
            Assert.IsTrue(wp is BasicTargetedWorkflowParameters);

            wp.LoadParameters(importedParametersFile);

        }

    }
}
