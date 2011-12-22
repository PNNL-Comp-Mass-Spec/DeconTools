using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Workflow_Tests
{
    [TestFixture]
    public class ScanBasedWorkflowTests
    {
        [Test]
        public void TraditionalWorkflowTest1()
        {
            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var parameters=new OldDecon2LSParameters();
            parameters.HornTransformParameters.UseScanRange = true;
            parameters.HornTransformParameters.MinScan = 6005;
            parameters.HornTransformParameters.MaxScan = 6005;
            parameters.PeakProcessorParameters.WritePeaksToTextFile = true;


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();

            
        }


        [Test]
        public void UIMFWorkflowTest1()
        {
            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var parameters = new OldDecon2LSParameters();
            parameters.HornTransformParameters.UseScanRange = true;
            parameters.HornTransformParameters.MinScan = 800;       //refers to LCScan which is 'Frame' in the UIMF world
            parameters.HornTransformParameters.MaxScan = 801;
            parameters.PeakProcessorParameters.WritePeaksToTextFile = true;
            parameters.PeakProcessorParameters.PeakBackgroundRatio = 5;

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();
        }


        [Test]
        public void UIMFWorkflowTest2()
        {
            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            string parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_Frame_500-510.xml";

            var parameters = new OldDecon2LSParameters();
            parameters.Load(parameterFile);
            parameters.HornTransformParameters.ScanBasedWorkflowType = "uimf_saturation_repair";
           
            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();
        }


    }
}
