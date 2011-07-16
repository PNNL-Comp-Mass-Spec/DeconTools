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
            string exportedParametersFile = FileRefs.OutputFolderPath + "\\" + "exportedTargetedAlignmentWorkflowParameters.xml";

            TargetedAlignerWorkflowParameters parameters = new TargetedAlignerWorkflowParameters();
            parameters.SaveParametersToXML(exportedParametersFile);
        }

        [Test]
        public void importParameterTests1()
        {
            string importedParametersFile = FileRefs.OutputFolderPath + "\\" + "exportedTargetedAlignmentWorkflowParameters.xml";
            TargetedAlignerWorkflowParameters parameters = new TargetedAlignerWorkflowParameters();
            parameters.LoadParameters(importedParametersFile);
        }

    }
}
