﻿using System.Collections.Generic;
using DeconTools.Backend.Parameters;
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
            var parameters = new DeconToolsParameters();

            var datasetList = new List<string>
            {
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\JuliaLaskinRunMergingWorkflow\b-LSOA_HPLC_ESIbox_pos_res60k_RT92-98.raw",
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\JuliaLaskinRunMergingWorkflow\b-LSOA_HPLC_ESIbox_pos_res60k_RT1255-1282.raw",
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\JuliaLaskinRunMergingWorkflow\b-LSOA_HPLC_ESIbox_pos_res60k_RT1400-1500.raw"
            };

            var workflow = new RunMergingPeakExportingWorkflow(parameters, datasetList);
            workflow.Execute();
        }
    }
}
