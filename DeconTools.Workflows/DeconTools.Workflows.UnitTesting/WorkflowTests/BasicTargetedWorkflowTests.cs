using System;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class BasicTargetedWorkflowTests
    {

        [Test]
        public void constructor_noRun_test1()
        {
            DeconToolsTargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(parameters);

            
            //workflow.Execute();
        }



        [Test]
        public void findSingleMassTag_test1()
        {

            string testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
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
        public void findSingleMassTag_test2()
        {

            string testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_NETVals0.3-0.33.txt";


            Run run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);


            MassTagCollection mtc = new MassTagCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 24707;
            run.CurrentMassTag = (from n in mtc.MassTagList where n.ID == testMassTagID && n.ChargeState == 4 select n).First();


            DeconToolsTargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            MassTagResult result = run.ResultCollection.GetMassTagResult(run.CurrentMassTag) as MassTagResult;
            result.DisplayToConsole();

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.AreEqual(4, result.IsotopicProfile.ChargeState);
            Assert.AreEqual(610.81m, (decimal)Math.Round(result.IsotopicProfile.GetMZ(), 2));
            Assert.AreEqual(6483, (decimal)Math.Round(result.ChromPeakSelected.XValue));

        }


        [Test]
        public void findSingleMassTag_alternateQCShew()
        {
            string testFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16.RAW";
            string peaksTestFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_peaks.txt";
            string massTagFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_peakMatchedFeatures.txt";


            Run run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);


            MassTagCollection mtc = new MassTagCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 24709;
            run.CurrentMassTag = (from n in mtc.MassTagList where n.ID == testMassTagID && n.ChargeState == 4 select n).First();


            DeconToolsTargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.2;
            parameters.MSToleranceInPPM = 25;

            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            MassTagResult result = run.ResultCollection.GetMassTagResult(run.CurrentMassTag) as MassTagResult;
            result.DisplayToConsole();

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.AreEqual(4, result.IsotopicProfile.ChargeState);
           // Assert.AreEqual(610.81m, (decimal)Math.Round(result.IsotopicProfile.GetMZ(), 2));
            //Assert.AreEqual(6483, (decimal)Math.Round(result.ChromPeakSelected.XValue));


        }

    }
}
