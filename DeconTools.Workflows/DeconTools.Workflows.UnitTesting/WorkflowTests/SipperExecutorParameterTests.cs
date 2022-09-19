using System.IO;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class SipperExecutorParameterTests
    {
        [Test]
        public void Test1()
        {
            var outputFile = Path.Combine(FileRefs.OutputFolderPath, "SipperExecutorParameters1.xml");

            var parameters = new SipperWorkflowExecutorParameters
            {
                DeleteLocalDatasetAfterProcessing = false,
                LocalDirectoryPathForCopiedRawDataset = string.Empty,
                ReferenceDataForTargets = @"C:\Sipper\SipperDemo\SipperInputs\Sample_ReferenceDataForTargets.txt",
#pragma warning disable 618
                TargetedAlignmentIsPerformed = false,
#pragma warning restore 618
                TargetedAlignmentWorkflowParameterFile = string.Empty,
                TargetsBaseFolder = @"C:\Sipper\SipperDemo\SipperInputs\Targets",
                TargetsFilePath = @"C:\Sipper\SipperDemo\SipperInputs\Targets\sample_targets.txt",
                TargetsToFilterOn = string.Empty,
                WorkflowParameterFile = @"C:\Sipper\SipperDemo\SipperInputs\SipperWorkflowParameters.xml"
            };

            parameters.SaveParametersToXML(outputFile);
        }
    }
}
