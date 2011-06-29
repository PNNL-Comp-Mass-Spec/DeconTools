using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Workflows;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.WorkflowTests
{
    public class PeakDetectAndExportWorkflowTests
    {
        [Test]
        public void test1()
        {

            RunFactory rf = new RunFactory();

            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakDetectAndExportWorkflowParameters parameters = new PeakDetectAndExportWorkflowParameters();
            parameters.ScanMin = 6000;
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
