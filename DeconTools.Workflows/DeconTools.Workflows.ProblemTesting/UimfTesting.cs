using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using DeconTools.UnitTesting2;
using NUnit.Framework;

namespace DeconTools.Workflows.ProblemTesting
{
    [TestFixture]
    public class UimfTesting
    {
        [Test]
        public void Test1()
        {
            Run run = new RunFactory().CreateRun(
                    @"\\pnl\projects\MSSHARE\Webb_Ian\no cid.UIMF");


            DeconToolsParameters parameters = new DeconToolsParameters();

            parameters.LoadFromOldDeconToolsParameterFile(
                @"\\pnl\projects\MSSHARE\Webb_Ian\IMS_UIMF_PeakBR2_PeptideBR3_SN3_SumScans3_NoLCSum_Sat50000_2012-02-27.xml");

            
            string expectedIsosFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_isos.csv";
            string expectedScansFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_scans.csv";
            string expectedPeaksFile = run.DataSetPath + Path.DirectorySeparatorChar + run.DatasetName + "_peaks.txt";

            if (File.Exists(expectedIsosFile)) File.Delete(expectedIsosFile);
            if (File.Exists(expectedScansFile)) File.Delete(expectedScansFile);
            if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.Execute();





        }

    }
}
