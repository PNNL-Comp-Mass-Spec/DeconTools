using System.IO;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class N14N15WorkflowParametersTests
    {
        [Test]
        public void exportParametersTest1()
        {
            var exportedParametersFile = Path.Combine(FileRefs.OutputFolderPath, "exportedN14N15WorkflowParameters.xml");

            var parameters = new N14N15Workflow2Parameters();
            parameters.SaveParametersToXML(exportedParametersFile);

        }


        [Test]
        public void importParametersTest1()
        {
            var importedParametersFile = Path.Combine(FileRefs.ImportedData, "importedN14N15WorkflowParameters.xml");

            var wp = WorkflowParameters.CreateParameters(importedParametersFile);

            Assert.AreEqual("N14N15Targeted1", wp.WorkflowType.ToString());
            Assert.IsTrue(wp is N14N15Workflow2Parameters);

        }

    }
}
