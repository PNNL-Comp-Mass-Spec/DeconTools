using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Workflow_Tests
{
    [TestFixture]
    public class RunMergingPeakExportingWorkflowTests
    {
        [Test]
        public void Test1()
        {

            OldDecon2LSParameters parameters = new OldDecon2LSParameters();


            List<string> datasetList = new List<string>();
               datasetList.Add(@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\JuliaLaskinRunMergingWorkflow\b-LSOA_HPLC_ESIbox_pos_res60k_RT92-98.raw");
            datasetList.Add(@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\JuliaLaskinRunMergingWorkflow\b-LSOA_HPLC_ESIbox_pos_res60k_RT1255-1282.raw");
            datasetList.Add(@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\JuliaLaskinRunMergingWorkflow\b-LSOA_HPLC_ESIbox_pos_res60k_RT1400-1500.raw");
    
            RunMergingPeakExportingWorkflow workflow = new RunMergingPeakExportingWorkflow(parameters, datasetList);
            workflow.Execute();




        }

    }
}
