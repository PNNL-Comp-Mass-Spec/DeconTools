using System.IO;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Workflow_Tests
{
    [TestFixture]
    public class DeconMSnWorkflowTests
    {
        private string testFile1 = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

        [Category("Standard")]
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

            var expectedResultsFile1 = Path.Combine(run.DataSetPath, run.DatasetName + ".mgf");
			var expectedResultsFile2 = Path.Combine(run.DataSetPath, run.DatasetName + "_DeconMSn_log.txt");
            var expectedResultsFile3 = Path.Combine(run.DataSetPath, run.DatasetName + "_profile.txt");

            if (File.Exists(expectedResultsFile1)) File.Delete(expectedResultsFile1);
            if (File.Exists(expectedResultsFile2)) File.Delete(expectedResultsFile2);
            if (File.Exists(expectedResultsFile3)) File.Delete(expectedResultsFile3);

            var workflow = new DeconMSnWorkflow(parameters, run);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFile1));
            Assert.IsTrue(File.Exists(expectedResultsFile2));
            Assert.IsTrue(File.Exists(expectedResultsFile3));
        }

        [Category("Standard")]
        [Test]
        public void WorkflowTestUsingParameterFile1()
        {
            string parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\DeconMSn\ParameterFiles\DeconMSn_LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_scans6000_6050.xml";

            string rawdataFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            string outputFolder = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\DeconMSn\Output";

            string datasetName= RunUtilities.GetDatasetName(rawdataFile);

            var expectedResultsFile1 = Path.Combine(outputFolder, datasetName + ".mgf");
			var expectedResultsFile2 = Path.Combine(outputFolder, datasetName + "_DeconMSn_log.txt");
            var expectedResultsFile3 = Path.Combine(outputFolder, datasetName + "_profile.txt");

            if (File.Exists(expectedResultsFile1)) File.Delete(expectedResultsFile1);
            if (File.Exists(expectedResultsFile2)) File.Delete(expectedResultsFile2);
            if (File.Exists(expectedResultsFile3)) File.Delete(expectedResultsFile3);

            var workflow = ScanBasedWorkflow.CreateWorkflow(rawdataFile, parameterFile, outputFolder);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedResultsFile1));
            Assert.IsTrue(File.Exists(expectedResultsFile2));
            Assert.IsTrue(File.Exists(expectedResultsFile3));
        }


        [Category("ProblemTesting")]
        [Test]
        public void ProblemTesting()
        {
            string parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\DeconMSn\ParameterFiles\DeconMSn_LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_scans6000_6050.xml";

            string rawdataFile = @"D:\Data\DeconMSn_testing\CPTAC_CompRef_test_iTRAQ_NiNTA_08_17Jan13_Frodo_12-12-50.raw";
            
            var workflow = ScanBasedWorkflow.CreateWorkflow(rawdataFile, parameterFile);
            workflow.Execute();
        }


        [Category("ProblemTesting")]
        [Test]
        public void LowIntensityButGoodFeatureTest1()
        {
            string rawDataFile = @"D:\Data\DeconMSn_testing\QC_Shew_13_01_pt5_b_23Jan13_Cougar_12-02-27.raw";


            var parameters = new DeconToolsParameters();
            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 20593;
            parameters.MSGeneratorParameters.MaxLCScan = 20598;

            parameters.PeakDetectorParameters.PeakToBackgroundRatio = 1.3;
            parameters.PeakDetectorParameters.SignalToNoiseThreshold = 2;
            parameters.PeakDetectorParameters.IsDataThresholded = true;

            parameters.ThrashParameters.MinIntensityForScore = 1;
            parameters.ThrashParameters.MinMSFeatureToBackgroundRatio = 1;

            var run = new RunFactory().CreateRun(rawDataFile);

            var workflow = new DeconMSnWorkflow(parameters, run);
            workflow.NumMaxAttemptsAtLowIntensitySpecies = 4;
            workflow.Execute();
        }





        [Ignore("Local testing only")]
        [Test]
        public void WorkflowTestUsingParameterFile_Sum5_Test1()
        {
            string parameterFile =
                @"C:\Users\d3x720\Documents\Data\QCShew\DeconMSn_LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_LCSum5.xml";

            string localTestFile =
                @"C:\Users\d3x720\Documents\Data\QCShew\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";



            string outputFolder = @"C:\Users\d3x720\Documents\Data\QCShew\Output";


            var workflow = ScanBasedWorkflow.CreateWorkflow(localTestFile, parameterFile, outputFolder);
            workflow.Execute();

        }


        [Ignore("Local testing only")]
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
