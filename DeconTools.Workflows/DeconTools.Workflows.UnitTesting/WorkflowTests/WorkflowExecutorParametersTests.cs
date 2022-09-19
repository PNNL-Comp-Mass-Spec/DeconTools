using System.IO;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    public class WorkflowExecutorParametersTests
    {
        [Test]
        public void exportParametersTest1()
        {
            var exportedParametersFile = Path.Combine(FileRefs.OutputFolderPath, "exportedBasicTargetedWorkflowExecutorParameters.xml");

            var parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.CopyRawFileLocal = true;
            parameters.DeleteLocalDatasetAfterProcessing = true;
            parameters.LocalDirectoryPathForCopiedRawDataset = @"d:\temp\rawdata";

            parameters.TargetsFilePath = @"d:\temp\MassTags\massTagsToBeTargeted.txt";
            parameters.TargetsBaseFolder = @"d:\temp";

            parameters.TargetedAlignmentWorkflowParameterFile = @"d:\temp\Parameters\targetedAlignmentParameters.xml";
            parameters.WorkflowParameterFile = @"d:\temp\Parameters\WorkflowParameters.xml";
            parameters.TargetsBaseFolder = @"d:\temp";

            parameters.SaveParametersToXML(exportedParametersFile);
        }

        [Test]
        public void exportParametersTest2()
        {
            var exportedParametersFile = Path.Combine(FileRefs.OutputFolderPath, "exportedLcmsTargetedWorkflowExecutorParameters.xml");

            var parameters = new LcmsFeatureTargetedWorkflowExecutorParameters();
            parameters.CopyRawFileLocal = true;
            parameters.DeleteLocalDatasetAfterProcessing = true;
            parameters.LocalDirectoryPathForCopiedRawDataset = @"d:\temp\rawdata";
            parameters.TargetsFilePath = @"d:\temp\MassTags\targets.txt";
            parameters.TargetsBaseFolder = @"d:\temp";

            parameters.MassTagsForReference = @"d:\temp\MassTags\MassTagsForReference.txt";
            parameters.TargetedAlignmentWorkflowParameterFile = @"d:\temp\Parameters\targetedAlignmentParameters.xml";
            parameters.WorkflowParameterFile = @"d:\temp\Parameters\WorkflowParameters.xml";

            parameters.SaveParametersToXML(exportedParametersFile);
        }

        [Test]
        public void importParametersTest1()
        {
            var importedParametersFile = Path.Combine(FileRefs.ImportedData, "importedBasicTargetedWorkflowExecutorParameters_defaults.xml");

            var parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.LoadParameters(importedParametersFile);

            Assert.AreEqual(true, parameters.CopyRawFileLocal);
            Assert.AreEqual(true, parameters.DeleteLocalDatasetAfterProcessing);

            Assert.AreEqual(@"d:\temp\rawdata", parameters.LocalDirectoryPathForCopiedRawDataset);
            Assert.AreEqual(@"d:\temp\MassTags\massTagsToBeTargeted.txt", parameters.TargetsFilePath);
            Assert.AreEqual(@"d:\temp\Parameters\targetedAlignmentParameters.xml", parameters.TargetedAlignmentWorkflowParameterFile);
            Assert.AreEqual(@"d:\temp\Parameters\WorkflowParameters.xml", parameters.WorkflowParameterFile);
            Assert.AreEqual(@"d:\temp", parameters.TargetsBaseFolder);
        }
    }
}
