using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Workflow_Tests
{
    [TestFixture]
    public class DeconMSnWorkflowTests
    {
        private string testFile1 = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

        [Test]
        public void WorkflowTest1()
        {
            var parameters = new DeconToolsParameters();
            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 6005;
            parameters.MSGeneratorParameters.MaxLCScan = 6012;

            parameters.PeakDetectorParameters.PeakToBackgroundRatio = 1.3;
            parameters.PeakDetectorParameters.SignalToNoiseThreshold = 2;
            parameters.PeakDetectorParameters.IsDataThresholded = true;

            parameters.ThrashParameters.MinIntensityForScore = 1;
            parameters.ThrashParameters.MinMSFeatureToBackgroundRatio = 1;

            var run = new RunFactory().CreateRun(testFile1);

            var workflow = new DeconMSnWorkflow(parameters, run);
            workflow.Execute();


        }

        [Test]
        public void WorkflowTestUsingParameterFile1()
        {
            string parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\DeconMSn\ParameterFiles\DeconMSn_LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_scans6000_6050.xml";

            string rawdataFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            string outputFolder = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\DeconMSn\Output";


            var workflow = ScanBasedWorkflow.CreateWorkflow(rawdataFile, parameterFile, outputFolder);
            workflow.Execute();

        }


        [Test]
        public void WorkflowTestUsingParameterFile_Sum5_Test1()
        {
            string parameterFile =
                @"C:\Users\d3x720\Documents\Data\QCShew\DeconMSn_LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_LCSum5.xml";



            string rawdataFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            string localTestFile =
                @"C:\Users\d3x720\Documents\Data\QCShew\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";



            string outputFolder = @"C:\Users\d3x720\Documents\Data\QCShew\Output";


            var workflow = ScanBasedWorkflow.CreateWorkflow(localTestFile, parameterFile, outputFolder);
            workflow.Execute();

        }



        [Test]
        public void WorkflowTesting1_NoLCSum()
        {

            string localTestFile =
                @"C:\Users\d3x720\Documents\Data\QCShew\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";


            var parameters = new DeconToolsParameters();
            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 6012;
            parameters.MSGeneratorParameters.MaxLCScan = 6019;

            parameters.PeakDetectorParameters.PeakToBackgroundRatio = 1.3;
            parameters.PeakDetectorParameters.SignalToNoiseThreshold = 2;
            parameters.PeakDetectorParameters.IsDataThresholded = true;

            parameters.ThrashParameters.MinIntensityForScore = 1;
            parameters.ThrashParameters.MinMSFeatureToBackgroundRatio = 1;

            var run = new RunFactory().CreateRun(localTestFile);

            var workflow = new DeconMSnWorkflow(parameters, run);
            workflow.Execute();


        }


        [Test]
        public void WorkflowTesting2_SumLC5()
        {

            string localTestFile =
                @"C:\Users\d3x720\Documents\Data\QCShew\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";


            var parameters = new DeconToolsParameters();
            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 6012;
            parameters.MSGeneratorParameters.MaxLCScan = 6019;

            parameters.MSGeneratorParameters.SumSpectraAcrossLC = true;
            parameters.MSGeneratorParameters.NumLCScansToSum = 5;

            parameters.PeakDetectorParameters.PeakToBackgroundRatio = 1.3;
            parameters.PeakDetectorParameters.SignalToNoiseThreshold = 2;
            parameters.PeakDetectorParameters.IsDataThresholded = true;

            parameters.ThrashParameters.MinIntensityForScore = 1;
            parameters.ThrashParameters.MinMSFeatureToBackgroundRatio = 1;

            var run = new RunFactory().CreateRun(localTestFile);

            var workflow = new DeconMSnWorkflow(parameters, run);
            workflow.Execute();


        }

    }
}
