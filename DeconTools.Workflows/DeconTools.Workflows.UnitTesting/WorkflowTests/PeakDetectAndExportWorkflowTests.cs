using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    public class PeakDetectAndExportWorkflowTests
    {
        [Test]
        public void peakexporterTest1()
        {

            var rf = new RunFactory();

            var run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var parameters = new PeakDetectAndExportWorkflowParameters();
            parameters.LCScanMin = 5500;
            parameters.LCScanMax = 6500;

            parameters.OutputDirectory = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TempOutput";


            var expectedPeaksFile = Path.Combine(parameters.OutputDirectory, run.DatasetName + "_peaks.txt");
            if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);


            var workflow = new PeakDetectAndExportWorkflow(run,parameters);
            workflow.Execute();

            var fileinfo = new FileInfo(expectedPeaksFile);
            Assert.IsTrue(fileinfo.Exists);
            Assert.IsTrue(fileinfo.Length > 1000000);

        }

    
        [Test]
        public void UIMFTest1()
        {

            var uimffile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";
            var rf = new RunFactory();

            var run = rf.CreateRun(uimffile);

            var expectedPeaksFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31_peaks.txt";
            if (File.Exists(expectedPeaksFile))
            {
                File.Delete(expectedPeaksFile);
            }

            var parameters = new PeakDetectAndExportWorkflowParameters();
            parameters.LCScanMin = 500;
            parameters.LCScanMax = 510;
            parameters.NumIMSScansSummed = -1;

            var workflow = new PeakDetectAndExportWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedPeaksFile));
        }


        [Test]
        public void exportPeaksFromMixedProfileAndCentroidMS2Data()
        {

            //TODO:  finish off this test

            var testFile = @"D:\Data\From_Matt\XGA121_lipid_pt5uM_1.raw";

            var rf = new RunFactory();

            var run = rf.CreateRun(testFile);

            var parameters = new PeakDetectAndExportWorkflowParameters();
            parameters.LCScanMin = 1;
            parameters.LCScanMax = 20;
            parameters.ProcessMSMS = true;
            parameters.IsDataThresholded = true;
            
            parameters.MS2PeakDetectorDataIsThresholded = true;
            


            var expectedPeaksFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");
            if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);


            var workflow = new PeakDetectAndExportWorkflow(run, parameters);
            workflow.Execute();

            var fileinfo = new FileInfo(expectedPeaksFile);
            Assert.IsTrue(fileinfo.Exists);
            

        }

    }
}
