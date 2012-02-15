using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class OMCS_0149_MSMSNoScans
    {
        [Test]
        public void scanBasedWorkflowTest1()
        {
            string testFile = @"D:\Data\JIRA_issues\OMCS-149_MS2_not_in_Scans\DsrC_Black_02_6Feb12_Cougar_11-10-11.raw";
            string parameterFile =
                @"D:\Data\JIRA_issues\OMCS-149_MS2_not_in_Scans\LTQ_Orb_USTags_MS2_THRASH_WithPeaks_Relaxed.xml";


            Run run = new RunFactory().CreateRun(testFile);
            var parameters = new OldDecon2LSParameters();
            parameters.Load(parameterFile);


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.OldDecon2LsParameters.HornTransformParameters.UseScanRange = true;
            workflow.OldDecon2LsParameters.HornTransformParameters.MinScan = 25;
            workflow.OldDecon2LsParameters.HornTransformParameters.MaxScan = 35;

            workflow.ExportData = true;
            workflow.Execute();

            // foreach (var isosResult in run.ResultCollection.ResultList)
            // {
            //     isosResult.Display();
            // }



        }


    }
}
