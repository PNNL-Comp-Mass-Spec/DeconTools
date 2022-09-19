using System.IO;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class TargetedAlignmentWorkflowParametersTests
    {
        [Test]
        public void exportParameterTests1()
        {
            var exportedParametersFile = Path.Combine(FileRefs.OutputFolderPath, "exportedTargetedAlignmentWorkflowParameters.xml");

            var parameters = new TargetedAlignerWorkflowParameters();
            parameters.SaveParametersToXML(exportedParametersFile);
        }

        [Test]
        public void importParameterTests1()
        {
            var importedParametersFile = Path.Combine(FileRefs.OutputFolderPath, "exportedTargetedAlignmentWorkflowParameters.xml");
            var parameters = new TargetedAlignerWorkflowParameters();
            parameters.LoadParameters(importedParametersFile);
        }
    }
}
