using System.IO;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    [Category("Standard")]
    public class BasicTargetedWorkflowParametersTests
    {
        [Test]

        public void exportParametersTest1()
        {
            var exportedParametersFile = Path.Combine(FileRefs.OutputFolderPath, "exportedBasicTargetedWorkflowParameters.xml");

            if (File.Exists(exportedParametersFile))
            {
                File.Delete(exportedParametersFile);
            }

            var parameters = new BasicTargetedWorkflowParameters();
            parameters.SaveParametersToXML(exportedParametersFile);

            Assert.That(File.Exists(exportedParametersFile), "Parameter file doesn't exist: " + exportedParametersFile);
        }

        [Test]
        public void importParametersTest1()
        {
            var importedParametersFile = Path.Combine(FileRefs.ImportedData, "importedBasicTargetedWorkflowParameters.xml");

            var parameters = new BasicTargetedWorkflowParameters();
            parameters.LoadParameters(importedParametersFile);

            Assert.AreEqual("O16O18_TARGETED_RESULT", parameters.ResultType.ToString());
        }

        [Test]
        public void importParametersTest2()
        {
            var importedParametersFile = Path.Combine(FileRefs.ImportedData, "importedParameters_MostIntenseChromPeakSelection.xml");

            var parameters = new BasicTargetedWorkflowParameters();
            parameters.LoadParameters(importedParametersFile);

            Assert.AreEqual("BASIC_TARGETED_RESULT", parameters.ResultType.ToString());
            Assert.AreEqual("MostIntense", parameters.ChromPeakSelectorMode.ToString());
        }

        [Test]
        public void createParametersObjectTest1()
        {
            var importedParametersFile = Path.Combine(FileRefs.ImportedData, "importedBasicTargetedWorkflowParameters.xml");
            var wp = WorkflowParameters.CreateParameters(importedParametersFile);

            Assert.AreEqual("UnlabeledTargeted1", wp.WorkflowType.ToString());
            Assert.IsTrue(wp is BasicTargetedWorkflowParameters);

            wp.LoadParameters(importedParametersFile);
        }
    }
}
