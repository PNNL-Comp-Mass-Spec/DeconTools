using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            var parameters = new SipperWorkflowExecutorParameters();
            parameters.DeleteLocalDatasetAfterProcessing = false;
            parameters.FolderPathForCopiedRawDataset = string.Empty;
            parameters.ReferenceDataForTargets = @"C:\Sipper\SipperDemo\SipperInputs\Sample_ReferenceDataForTargets.txt";
            parameters.TargetedAlignmentIsPerformed = false;
            parameters.TargetedAlignmentWorkflowParameterFile = string.Empty;
            parameters.TargetsBaseFolder = @"C:\Sipper\SipperDemo\SipperInputs\Targets";
            parameters.TargetsFilePath = @"C:\Sipper\SipperDemo\SipperInputs\Targets\sample_targets.txt";
            parameters.TargetsToFilterOn = string.Empty;
            parameters.WorkflowParameterFile = @"C:\Sipper\SipperDemo\SipperInputs\SipperWorkflowParameters.xml";

            parameters.SaveParametersToXML(outputFile);

        }

    }
}
