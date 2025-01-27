﻿using System;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
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
            var workflow = new BasicTargetedWorkflow(parameters);

            //workflow.Execute();
        }

        [Test]
        public void parameterFileTest1()
        {
            var parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromGenSourceDataPeakBR = 2;
            parameters.ChromGenSourceDataSigNoise = 3;
            parameters.ChromGeneratorMode = Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            parameters.ChromNETTolerance = 0.025;
            parameters.ChromPeakDetectorPeakBR = 1;
            parameters.ChromPeakDetectorSigNoise = 1;
            parameters.ChromPeakSelectorMode = Globals.PeakSelectorMode.Smart;
            parameters.ChromSmootherNumPointsInSmooth = 9;
            parameters.ChromGenTolerance = 10;
            parameters.ChromatogramCorrelationIsPerformed = true;
            parameters.MSPeakDetectorPeakBR = 1.3;
            parameters.MSPeakDetectorSigNoise = 3;
            parameters.MSToleranceInPPM = 10;
            parameters.NumMSScansToSum = 5;
            parameters.SmartChromPeakSelectorNumMSSummed = 3;

            var exportedParameterFilename =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Parameters\BasicTargetedWorkflowParameters1_autoexported.xml";
            parameters.SaveParametersToXML(exportedParameterFilename);
        }

        [Test]
        public void parameterFileImportTest1()
        {
            var parameters = new BasicTargetedWorkflowParameters();

            var importTestFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Parameters\BasicTargetedWorkflowParameters1_forImportTesting.xml";

            parameters.LoadParameters(importTestFile);

            //TODO: complete testing of other values
            Assert.AreEqual(parameters.ChromGenToleranceUnit, Globals.ToleranceUnit.MZ);
            Assert.AreEqual(0.01, parameters.ChromGenTolerance);
        }

        [Category("MustPass")]
        [Test]
        public void findSingleMassTag_test1()
        {
            //see  https://jira.pnnl.gov/jira/browse/OMCS-709

            var testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            var run= RunUtilities.CreateAndAlignRun(testFile, peaksTestFile);

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var testMassTagID = 24800;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            TargetedWorkflowParameters parameters= new BasicTargetedWorkflowParameters();
            parameters.ChromatogramCorrelationIsPerformed = true;
            parameters.ChromSmootherNumPointsInSmooth = 9;
            parameters.ChromPeakDetectorPeakBR = 1;
            parameters.ChromPeakDetectorSigNoise = 1;

            var workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            Assert.IsNotNull(workflow.ChromatogramXYData, "Chrom XY data is empty");
            Assert.IsNotEmpty(workflow.ChromPeaksDetected, "Chrom peaks are empty");

            Console.WriteLine("Chrom peaks detected");
            foreach (var chromPeak in workflow.ChromPeaksDetected)
            {
                Console.WriteLine(chromPeak.XValue.ToString("0.0") + "\t" + chromPeak.Height.ToString("0") + "\t" +
                                  chromPeak.Width.ToString("0.0"));
            }

            Assert.AreEqual(3, workflow.ChromPeaksDetected.Count);

            Assert.IsNotNull(workflow.ChromPeakSelected, "No chrom peak was selected");
            Assert.IsNotNull(workflow.MassSpectrumXYData, "Mass spectrum for selected chrom peak was not generated");

            //TestUtilities.DisplayXYValues(workflow.MassSpectrumXYData);
            //TestUtilities.DisplayXYValues(workflow.ChromatogramXYData);

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;

            if (result.FailedResult)
            {
                Console.WriteLine(result.ErrorDescription);
            }

            Assert.IsFalse(result.FailedResult);

           // result.DisplayToConsole();

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.AreEqual(2, result.IsotopicProfile.ChargeState);
            Assert.AreEqual(718.41m, (decimal)Math.Round(result.IsotopicProfile.GetMZ(), 2));
            Assert.AreEqual(5939, (decimal)Math.Round(result.ChromPeakSelected.XValue));

            Assert.IsNotNull(result.ChromCorrelationData);

            foreach (var dataItem in result.ChromCorrelationData.CorrelationDataItems)
            {
                Console.WriteLine(dataItem);
            }
        }

        [Test]
        public void InvestigateIQFailures()
        {
            var executorParamFile =
              @"\\protoapps\DataPkgs\Public\2013\743_Mycobacterium_tuberculosis_Cys_and_Ser_ABP\IQ_Analysis\Parameters\ExecutorParameters1 - Copy.xml";

            var executorParameters =
                new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParamFile);

            var testFile =
                @"\\protoapps\DataPkgs\Public\2013\743_Mycobacterium_tuberculosis_Cys_and_Ser_ABP\IQ_Analysis\Testing\LNA_A_Expo_Sample_SC_9_LNA_ExpA_Expo_Stat_SeattleBioMed_15Feb13_Cougar_12-12-35.raw";
            var run = new RunFactory().CreateRun(testFile);

            var iqparameterFile =
                @"\\protoapps\DataPkgs\Public\2013\743_Mycobacterium_tuberculosis_Cys_and_Ser_ABP\IQ_Analysis\Testing\IQWorkflowParameters1.xml";

            var workflowParameters = new BasicTargetedWorkflowParameters();
            workflowParameters.LoadParameters(iqparameterFile);
            workflowParameters.MSToleranceInPPM = 10;
            workflowParameters.ChromPeakDetectorPeakBR = 0.25;
            workflowParameters.ChromPeakDetectorSigNoise = 2;

            var targetedWorkflow = new BasicTargetedWorkflow(run, workflowParameters);
            var executor = new BasicTargetedWorkflowExecutor(executorParameters, targetedWorkflow, testFile);

            //int[] testTargets = {349959971, 349951038,349954483 };    
            //int[] testTargets = { 349951038 };

            //int[] testTargets = { 349954483 };
            //int[] testTargets = { 2911730 };

            int[] testTargets = { 349946881 };
            var chargeState = 3;

            executor.Targets.TargetList = (from n in executor.Targets.TargetList where testTargets.Contains(n.ID) select n).ToList();
            executor.Targets.TargetList = (from n in executor.Targets.TargetList where n.ChargeState == chargeState select n).ToList();

            executor.Execute();

            //Results of investiga  tion!  -  349959971 was being missed because the MSTolerance was too narrow. When changed from 5 to 6, it was found. I think we can safely set this at 10. 
            //Results of investigation!  -  349951038 was being missed because it was being flagged (peak to the left) problem.  

            TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);
        }

        [Test]
        public void findSingleModifiedMassTagTest1()
        {
            var testFile = UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_NETVals0.3-0.33.txt";

            var run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);
            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var testMassTagID = 189685150;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            Assert.AreEqual(true,run.CurrentMassTag.ContainsMods);

            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromatogramCorrelationIsPerformed = true;

            var workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;
            Assert.AreEqual(false, result.FailedResult);

            result.DisplayToConsole();

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.AreEqual(2, result.IsotopicProfile.ChargeState);
            Assert.AreEqual(959.48m, (decimal)Math.Round(result.IsotopicProfile.GetMZ(), 2));
            //Assert.AreEqual(6070, (decimal)Math.Round(result.ChromPeakSelected.XValue));

            foreach (var dataItem in result.ChromCorrelationData.CorrelationDataItems)
            {
                Console.WriteLine(dataItem);
            }
        }

        [Test]
        public void findSingleMassTag_checkAlignmentData_test1()
        {
            var testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            var run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);
            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var testMassTagID = 24800;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            var workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;
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
            var testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            var run = RunUtilities.CreateAndAlignRun(testFile, peaksTestFile);

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var testMassTagID = 26523;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 1 select n).First();

            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            var workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;

            Assert.IsNull(result.IsotopicProfile);
            Assert.IsNull(result.ScanSet);
            Assert.IsNull(result.ChromPeakSelected);

            Assert.IsTrue(result.FailedResult);
            Assert.AreEqual(DeconTools.Backend.Globals.TargetedResultFailureType.ChrompeakNotFoundWithinTolerances, result.FailureType);
        }

        [Test]
        public void findSingleMassTag_alternateQCShew()
        {
            var testFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16.RAW";
            var peaksTestFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_peaks.txt";
            var massTagFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_peakMatchedFeatures.txt";

            var run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var testMassTagID = 24709;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 4 select n).First();

            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.2;
            parameters.MSToleranceInPPM = 25;
            parameters.ChromatogramCorrelationIsPerformed = true;

            var workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as MassTagResult;
            result.DisplayToConsole();

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.AreEqual(4, result.IsotopicProfile.ChargeState);

           // Assert.AreEqual(610.81m, (decimal)Math.Round(result.IsotopicProfile.GetMZ(), 2));
            //Assert.AreEqual(6483, (decimal)Math.Round(result.ChromPeakSelected.XValue));

            double maxIntensity = result.IsotopicProfile.Peaklist.Max(p => p.Height);

            for (var i = 0; i < result.IsotopicProfile.Peaklist.Count; i++)
            {
                double correctedRatio=0;
                if (i<result.ChromCorrelationData.CorrelationDataItems.Count)
                {
                    var correlationSlope = result.ChromCorrelationData.CorrelationDataItems[i].CorrelationSlope;
                    if (correlationSlope != null)
                    {
                        correctedRatio = (double) correlationSlope;
                    }
                }
                else
                {
                    correctedRatio = 0;
                }

                var observedRelIntensity = result.IsotopicProfile.Peaklist[i].Height/maxIntensity;

                Console.WriteLine(i + "\t" + observedRelIntensity + "\t" + correctedRatio);
            }
        }
    }
}
