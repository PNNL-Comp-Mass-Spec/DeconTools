using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    public class WorkflowExecutorParametersTests
    {

        [Test]
        public void exportParametersTest1()
        {
            string exportedParametersFile = FileRefs.OutputFolderPath + "\\" + "exportedBasicTargetedWorkflowExecutorParameters.xml";

            BasicTargetedWorkflowExecutorParameters parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.CopyRawFileLocal = true;
            parameters.DeleteLocalDatasetAfterProcessing = true;
            parameters.FolderPathForCopiedRawDataset = @"d:\temp\rawdata";
            parameters.LoggingFolder = @"d:\temp\logs";
            parameters.MassTagsForAlignmentFilePath = @"d:\temp\MassTags\massTagsForAlignment.txt";
            parameters.TargetsFilePath = @"d:\temp\MassTags\massTagsToBeTargeted.txt";
            parameters.ResultsFolder = @"d:\temp\results";
            parameters.TargetedAlignmentWorkflowParameterFile = @"d:\temp\Parameters\targetedAlignmentParameters.xml";
            parameters.WorkflowParameterFile = @"d:\temp\Parameters\WorkflowParameters.xml";

            parameters.AlignmentInfoFolder = @"d:\temp\AlignmentInfo";

            parameters.SaveParametersToXML(exportedParametersFile);

        }


        [Test]
        public void exportParametersTest2()
        {
            string exportedParametersFile = FileRefs.OutputFolderPath + "\\" + "exportedLcmsTargetedWorkflowExecutorParameters.xml";

            LcmsFeatureTargetedWorkflowExecutorParameters parameters = new LcmsFeatureTargetedWorkflowExecutorParameters();
            parameters.CopyRawFileLocal = true;
            parameters.DeleteLocalDatasetAfterProcessing = true;
            parameters.FolderPathForCopiedRawDataset = @"d:\temp\rawdata";
            parameters.LoggingFolder = @"d:\temp\logs";
            parameters.MassTagsForAlignmentFilePath = @"d:\temp\MassTags\massTagsForAlignment.txt";
            parameters.TargetsFilePath = @"d:\temp\MassTags\targets.txt";
            parameters.MassTagsForReference = @"d:\temp\MassTags\MassTagsForReference.txt";
            parameters.ResultsFolder = @"d:\temp\results";
            parameters.TargetedAlignmentWorkflowParameterFile = @"d:\temp\Parameters\targetedAlignmentParameters.xml";
            parameters.WorkflowParameterFile = @"d:\temp\Parameters\WorkflowParameters.xml";

            parameters.AlignmentInfoFolder = @"d:\temp\AlignmentInfo";

            parameters.SaveParametersToXML(exportedParametersFile);

        }


        [Test]
        public void importParametersTest1()
        {
            string importedParametersFile = FileRefs.ImportedData + "\\" + "importedBasicTargetedWorkflowExecutorParameters_defaults.xml";

            BasicTargetedWorkflowExecutorParameters parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.LoadParameters(importedParametersFile);

            Assert.AreEqual(true, parameters.CopyRawFileLocal);
            Assert.AreEqual(true, parameters.DeleteLocalDatasetAfterProcessing);

            Assert.AreEqual(@"d:\temp\rawdata", parameters.FolderPathForCopiedRawDataset);
            Assert.AreEqual(@"d:\temp\logs", parameters.LoggingFolder);
            Assert.AreEqual(@"d:\temp\MassTags\massTagsForAlignment.txt", parameters.MassTagsForAlignmentFilePath);
            Assert.AreEqual(@"d:\temp\MassTags\massTagsToBeTargeted.txt", parameters.TargetsFilePath);
            Assert.AreEqual(@"d:\temp\results", parameters.ResultsFolder);
            Assert.AreEqual(@"d:\temp\Parameters\targetedAlignmentParameters.xml", parameters.TargetedAlignmentWorkflowParameterFile);
            Assert.AreEqual(@"d:\temp\Parameters\WorkflowParameters.xml", parameters.WorkflowParameterFile);

        }


    }
}
