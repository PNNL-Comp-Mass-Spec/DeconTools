using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data.Importers;
using DeconTools.Backend.Workflows;
using System.IO;
using DeconTools.Backend.FileIO;

namespace DeconTools.UnitTesting2.WorkflowTests
{
    [TestFixture]
    public class BasicTargetedWorkflowTests
    {
        [Test]
        public void findSingleMassTag_test1()
        {

            string testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_NETVals0.3-0.33.txt";


            Run run= RunUtilities.CreateAndAlignRun(testFile, peaksTestFile);


            MassTagCollection mtc = new MassTagCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 24707;
            run.CurrentMassTag = (from n in mtc.MassTagList where n.ID == testMassTagID && n.ChargeState == 4 select n).First();


            DeconToolsTargetedWorkflowParameters parameters= new BasicTargetedWorkflowParameters();
            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            MassTagResult result = run.ResultCollection.GetMassTagResult(run.CurrentMassTag) as MassTagResult;
            result.DisplayToConsole();

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.AreEqual(4, result.IsotopicProfile.ChargeState);
            Assert.AreEqual(610.81m, (decimal)Math.Round(result.IsotopicProfile.GetMZ(),2));
            Assert.AreEqual(6483, (decimal)Math.Round(result.ChromPeakSelected.XValue));

        }

        [Test]
        public void saveXMLParameters_test1()
        {
            string outfile = FileRefs.BasicTargetedWorkflowPARAMETERS_EXPORTED_TESTFILE1;

            if (File.Exists(outfile)) File.Delete(outfile);

            BasicTargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.SaveParametersToXML(outfile);
        }

    }
}
