using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.UnitTesting2;
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
            parameters.ScanMin = 5500;
            parameters.ScanMax = 6500;

            PeakDetectAndExportWorkflow workflow = new PeakDetectAndExportWorkflow(run,parameters);
            workflow.Execute();


        }
        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
