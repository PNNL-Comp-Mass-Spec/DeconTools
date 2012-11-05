using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    public class PeakDetectAndExportWorkflowTests
    {
        [Test]
        public void test1()
        {

            RunFactory rf = new RunFactory();

            Run run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakDetectAndExportWorkflowParameters parameters = new PeakDetectAndExportWorkflowParameters();
            parameters.LCScanMin = 5500;
            parameters.LCScanMax = 6500;

            PeakDetectAndExportWorkflow workflow = new PeakDetectAndExportWorkflow(run,parameters);
            workflow.Execute();


        }


        [Test]
        public void UIMFTest1()
        {

            string uimffile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";
            RunFactory rf = new RunFactory();

            Run run = rf.CreateRun(uimffile);

            string expectedPeaksFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31_peaks.txt";
            if (File.Exists(expectedPeaksFile))
            {
                File.Delete(expectedPeaksFile);
            }

            PeakDetectAndExportWorkflowParameters parameters = new PeakDetectAndExportWorkflowParameters();
            parameters.LCScanMin = 500;
            parameters.LCScanMax = 510;
            parameters.NumIMSScansSummed = -1;

            PeakDetectAndExportWorkflow workflow = new PeakDetectAndExportWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsTrue(File.Exists(expectedPeaksFile));
        }
    
    }
}
