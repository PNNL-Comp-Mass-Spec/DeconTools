using System;
using System.Collections.Generic;
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

            string outputFile=   FileRefs.OutputFolderPath + "\\" + "SipperExecutorParameters1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.DeleteLocalDatasetAfterProcessing = false;
            parameters.FolderPathForCopiedRawDataset = String.Empty;
            parameters.LoggingFolder = @"C:\Sipper\SipperDemo\SipperOutputs";
            parameters.ReferenceDataForTargets = @"C:\Sipper\SipperDemo\SipperInputs\Sample_ReferenceDataForTargets.txt";
            parameters.ResultsFolder = @"C:\Sipper\SipperDemo\SipperOutputs";
            parameters.TargetedAlignmentIsPerformed = false;
            parameters.TargetedAlignmentWorkflowParameterFile = String.Empty;
            parameters.TargetsBaseFolder = @"C:\Sipper\SipperDemo\SipperInputs\Targets";
            parameters.TargetsFilePath = @"C:\Sipper\SipperDemo\SipperInputs\Targets\sample_targets.txt";
            parameters.TargetsToFilterOn = String.Empty;
            parameters.TargetsUsedForAlignmentFilePath = String.Empty;
            parameters.WorkflowParameterFile = @"C:\Sipper\SipperDemo\SipperInputs\SipperWorkflowParameters.xml";

            parameters.SaveParametersToXML(outputFile);

        }

    }
}
