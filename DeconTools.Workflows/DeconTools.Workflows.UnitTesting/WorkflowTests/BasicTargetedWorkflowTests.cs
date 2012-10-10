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
            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(parameters);

            
            //workflow.Execute();
        }



        [Test]
        public void findSingleMassTag_test1()
        {
            string testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            Run run= RunUtilities.CreateAndAlignRun(testFile, peaksTestFile);

            TargetCollection mtc = new TargetCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 24800;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            TargetedWorkflowParameters parameters= new BasicTargetedWorkflowParameters();
            parameters.ChromatogramCorrelationIsPerformed = true;

            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();



            MassTagResult result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;

            if (result.FailedResult)
            {
                Console.WriteLine(result.ErrorDescription);
            }            

            Assert.IsFalse(result.FailedResult);


            result.DisplayToConsole();

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.AreEqual(2, result.IsotopicProfile.ChargeState);
            Assert.AreEqual(718.41m, (decimal)Math.Round(result.IsotopicProfile.GetMZ(), 2));
            Assert.AreEqual(5947m, (decimal)Math.Round(result.ChromPeakSelected.XValue));

            Assert.IsNotNull(result.ChromCorrelationData);

            foreach (var dataItem in result.ChromCorrelationData.CorrelationDataItems)
            {
                Console.WriteLine(dataItem);
            }


        }


        [Test]
        public void findSingleModifiedMassTagTest1()
        {
            string testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_NETVals0.3-0.33.txt";

            Run run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);
            TargetCollection mtc = new TargetCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 189685150;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            Assert.AreEqual(true,run.CurrentMassTag.ContainsMods);

            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromatogramCorrelationIsPerformed = true;

            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            MassTagResult result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;
            Assert.AreEqual(false, result.FailedResult);

            result.DisplayToConsole();

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.AreEqual(2, result.IsotopicProfile.ChargeState);
            Assert.AreEqual(959.48m, (decimal)Math.Round(result.IsotopicProfile.GetMZ(), 2));
            Assert.AreEqual(6070, (decimal)Math.Round(result.ChromPeakSelected.XValue));

            foreach (var dataItem in result.ChromCorrelationData.CorrelationDataItems)
            {
                Console.WriteLine(dataItem);
            }

        }


        [Test]
        public void findSingleMassTag_checkAlignmentData_test1()
        {
            string testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            Run run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);
            TargetCollection mtc = new TargetCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 24800;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();


            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            MassTagResult result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;
            Assert.AreEqual(false, result.FailedResult);
            
            result.DisplayToConsole();

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.AreEqual(2, result.IsotopicProfile.ChargeState);
            Assert.AreEqual(718.41m, (decimal)Math.Round(result.IsotopicProfile.GetMZ(), 2));
            Assert.AreEqual(5947m, (decimal)Math.Round(result.ChromPeakSelected.XValue));


            Assert.AreEqual(5.91, (decimal)(Math.Round(result.GetMassErrorAfterAlignmentInPPM(),2)));
            Assert.AreEqual(0.0001585m, (decimal)(Math.Round(result.GetNETAlignmentError(), 7)));

            RunUtilities.AlignRunUsingAlignmentInfoInFiles(run);

            //these might change due to unit tests elsewhere. Need a better way of doing this
            //Assert.AreEqual(1.99290383722318m, (decimal)result.GetMassErrorAfterAlignmentInPPM());
            //Assert.AreEqual(0.00076708197593689m, (decimal)result.GetNETAlignmentError());

        }

  



   

        [Test]
        public void cannotFindMassTag_test1()
        {
            //
            string testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            string peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";


            Run run = RunUtilities.CreateAndAlignRun(testFile, peaksTestFile);


            TargetCollection mtc = new TargetCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 26523;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 1 select n).First();


            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            MassTagResult result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;
           
            Assert.IsNull(result.IsotopicProfile);
            Assert.IsNull(result.ScanSet);
            Assert.IsNull(result.ChromPeakSelected);

            Assert.IsTrue(result.FailedResult);
            Assert.AreEqual(DeconTools.Backend.Globals.TargetedResultFailureType.ChrompeakNotFoundWithinTolerances, result.FailureType);

          
        }




        [Test]
        public void findSingleMassTag_alternateQCShew()
        {
            string testFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16.RAW";
            string peaksTestFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_peaks.txt";
            string massTagFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_peakMatchedFeatures.txt";


            Run run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);


            TargetCollection mtc = new TargetCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 24709;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 4 select n).First();


            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.2;
            parameters.MSToleranceInPPM = 25;
            parameters.ChromatogramCorrelationIsPerformed = true;

            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            MassTagResult result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;
            result.DisplayToConsole();

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.AreEqual(4, result.IsotopicProfile.ChargeState);


            
           // Assert.AreEqual(610.81m, (decimal)Math.Round(result.IsotopicProfile.GetMZ(), 2));
            //Assert.AreEqual(6483, (decimal)Math.Round(result.ChromPeakSelected.XValue));


            double maxIntensity = result.IsotopicProfile.Peaklist.Max(p => p.Height);

            for (int i = 0; i < result.IsotopicProfile.Peaklist.Count; i++)
            {
                
                double correctedRatio=0;
                if (i<result.ChromCorrelationData.CorrelationDataItems.Count)
                {
                    var correlationSlope = result.ChromCorrelationData.CorrelationDataItems[i].CorrelationSlope;
                    if (correlationSlope != null)
                        correctedRatio = (double) correlationSlope;
                }
                else
                {
                    correctedRatio = 0;
                }

                double observedRelIntensity = result.IsotopicProfile.Peaklist[i].Height/maxIntensity;

                Console.WriteLine(i + "\t" + observedRelIntensity + "\t" + correctedRatio);


            }

        }

    }
}
