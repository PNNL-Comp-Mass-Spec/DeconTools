using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;
using DeconTools.Backend.Runs;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class ScanBasedWorkflowTests
    {
        [Test]
        public void createScanSetTests()
        {
            int testScan1 = 6005;

            var run = new RunFactory().CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanBasedWorkflow workflow = new ScanBasedWorkflow(run);
            workflow.NumScansSummed = 3;

            workflow.SetScanSet(testScan1);
            
            Assert.AreEqual(3,workflow.scanSetSelected.IndexValues.Count);


        }

    }
}
