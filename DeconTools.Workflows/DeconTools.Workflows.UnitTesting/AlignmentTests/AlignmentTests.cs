﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting
{
    [TestFixture]
    public class AlignmentTests
    {
        [Category("MustPass")]
        [Test]
        public void GetCalibratedMass1()
        {
            //see  https://jira.pnnl.gov/jira/browse/OMCS-870

            var testFile =
                @"\\protoapps\DataPkgs\Public\2013\743_Mycobacterium_tuberculosis_Cys_and_Ser_ABP\IQ_Analysis\Testing\LNA_A_Stat_Sample_SC_28_LNA_ExpA_Expo_Stat_SeattleBioMed_15Feb13_Cougar_12-12-36.raw";
            var run = new RunFactory().CreateRun(testFile);

            var calibrationData = new ViperMassCalibrationData();
            calibrationData.MassError = 4.8;

            var massAlignmentInfo = new MassAlignmentInfoLcmsWarp();
            massAlignmentInfo.SetMassAlignmentData(calibrationData);

            run.MassAlignmentInfo = massAlignmentInfo;

            var observedMZ = 440.5887078;
            var theorMZ = 440.5858315;

            var calibratedMZ = run.GetAlignedMZ(observedMZ);

            Assert.AreEqual(440.5866m, (decimal)Math.Round(calibratedMZ, 4));

            var ppmDiffBefore = (observedMZ - theorMZ) / theorMZ * 1e6;
            var ppmDiffAfter = (calibratedMZ - theorMZ) / theorMZ * 1e6;

            Console.WriteLine("input m/z= " + observedMZ);
            Console.WriteLine("calibrated m/z= " + calibratedMZ);
            Console.WriteLine("ppmDiffBeforeCalibration= " + ppmDiffBefore);
            Console.WriteLine("ppmDiffAftereCalibration= " + ppmDiffAfter);

            var theorMZToLookForInRawData = run.GetTargetMZAligned(theorMZ);
            var ppmDiffTheor = (theorMZToLookForInRawData - theorMZ) / theorMZ * 1e6;

            Console.WriteLine();
            Console.WriteLine("Theor m/z = " + theorMZ);
            Console.WriteLine("Theor m/z to look for = " + theorMZToLookForInRawData);
            Console.WriteLine("ppmDiff = " + ppmDiffTheor);

            Assert.AreEqual(4.8m, (decimal) Math.Round(ppmDiffTheor, 1));
        }

        [Test]
        public void doAlignmentTest1()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var deconToolsResultFile = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_targetedFeatures.txt");

            var importer = new UnlabeledTargetedResultFromTextImporter(deconToolsResultFile);
            var repo = importer.Import();

            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_allMassTags.txt";
            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var aligner = new NETAndMassAligner();
            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.TargetList);

            aligner.Execute(run);

            float testScan = 6005;
            var testNET1 = run.NetAlignmentInfo.GetNETValueForScan((int) testScan);

            //note - this is Multialign's
            Assert.AreEqual(0.3253423m, (decimal)testNET1);
        }

        [Test]
        public void ExportNET_andMass_AlignmentDataTest1()
        {
            var exportNETFilename = Path.Combine(FileRefs.OutputFolderPath, "exportedNETAlignmentInfo1.txt");
            var exportMassFilename = Path.Combine(FileRefs.OutputFolderPath, "exportedMassAlignmentInfo1.txt");

            var rf = new RunFactory();
            var run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var deconToolsResultFile = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_targetedFeatures.txt");

            var importer = new UnlabeledTargetedResultFromTextImporter(deconToolsResultFile);
            var repo = importer.Import();

            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_allMassTags.txt";
            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var aligner = new NETAndMassAligner();
            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.TargetList);

            aligner.Execute(run);

            var exporter = new NETAlignmentInfoToTextExporter(exportNETFilename);
            exporter.ExportAlignmentInfo(run.AlignmentInfo);

            var massInfoexporter = new MassAlignmentInfoToTextExporter(exportMassFilename);
            massInfoexporter.ExportAlignmentInfo(run.AlignmentInfo);
        }

        [Test]
        public void ImportNET_and_Try_Alignment_Test1()
        {
            var importFilename = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ScanNetAlignment.txt");

            var rf = new RunFactory();
            var run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var importer = new NETAlignmentFromTextImporter(importFilename);
            var scanNETdata = importer.Import();

            run.NetAlignmentInfo.SetScanToNETAlignmentData(scanNETdata);

            var testScan = 6005;
            var testNET1 = run.NetAlignmentInfo.GetNETValueForScan(testScan);

           // float testNET1 = run.AlignmentInfo.GetNETFromTime(testScan);
            Assert.IsTrue(run.NETIsAligned);
            Assert.AreEqual(0.3252918m, (decimal)Math.Round(testNET1, 7));
        }

        [Test]
        public void ImportNET_and_Try_Alignment_Test2()
        {
            var importFilename = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ScanNetAlignment.txt");

            var rf = new RunFactory();
            var run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var importer = new NETAlignmentFromTextImporter(importFilename);
            importer.Execute(run);

            var testScan = 6005;
            var testNET1 = run.NetAlignmentInfo.GetNETValueForScan(testScan);

            Assert.AreEqual(0.3252918m, (decimal)Math.Round(testNET1,7));
            Assert.IsTrue(run.NETIsAligned);
        }

        [Test]
        public void ImportMassAndTimePPMCorrections_and_Try_Alignment_Test1()
        {
            var importFilename = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_MZAlignment.txt");

            var rf = new RunFactory();
            var run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var importer = new MassAlignmentInfoFromTextImporter(importFilename);
            var massAlignmentData = importer.Import();

            var massAlignmentInfo = new MassAlignmentInfoLcmsWarp();
            massAlignmentInfo.SetMassAlignmentData(massAlignmentData);
            run.MassAlignmentInfo = massAlignmentInfo;

            float testScan = 6439;
            var testMZ = 698.875137f;    //QCSHEW massTag 37003; m/z 698.875137 (2+)    See Redmine issue 627:  http://redmine.pnl.gov/issues/627

            var ppmshift = run.MassAlignmentInfo.GetPpmShift(testMZ,  (int) testScan);
            Console.WriteLine("ppm shift = " + ppmshift);

            Assert.AreEqual(-4.3, (decimal)Math.Round(ppmshift, 1));
        }

        [Test]
        public void Import_NET_And_MassAlignment_Test1()
        {
            var mzAlignmentInfoFilename = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_MZAlignment.txt");
            var NETAlignmentInfoFilename = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ScanNetAlignment.txt");

            var rf = new RunFactory();
            var run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var importer = new MassAlignmentInfoFromTextImporter(mzAlignmentInfoFilename);
            var massAlignmentData = importer.Import();

            var massAlignmentInfo = new MassAlignmentInfoLcmsWarp();
            massAlignmentInfo.SetMassAlignmentData(massAlignmentData);
            run.MassAlignmentInfo = massAlignmentInfo;

            var netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            var scanNETdata = netAlignmentInfoImporter.Import();

            run.NetAlignmentInfo.SetScanToNETAlignmentData(scanNETdata);
            float testScan = 6439;
            var testMZ = 698.875137f;    //QCSHEW massTag 37003; m/z 698.875137 (2+)   See Redmine issue 627:  http://redmine.pnl.gov/issues/627

            var ppmshift = run.MassAlignmentInfo.GetPpmShift(testMZ, (int) testScan);
            Console.WriteLine("ppm shift = " + ppmshift);

            var testScan2 = 6005;
            var testNET1 = run.NetAlignmentInfo.GetNETValueForScan(testScan2);

            Assert.AreEqual(0.3252918m, (decimal)Math.Round(testNET1, 7));
            Assert.AreEqual(-4.3, (decimal)Math.Round(ppmshift, 1));
        }

        [Test]
        public void checkRetrievalOfScanValueForAGivenNET()
        {
            var NETAlignmentInfoFilename = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ScanNetAlignment.txt");

            var rf = new RunFactory();
            var run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            var scanNETdata = netAlignmentInfoImporter.Import();
            run.NetAlignmentInfo.SetScanToNETAlignmentData(scanNETdata);

            var testNET1 = 0.95f;
            var testNET2 = 0.08f;
            var scan1 =  (int) run.NetAlignmentInfo.GetScanForNet(testNET1);
            var scan2 = (int)run.NetAlignmentInfo.GetScanForNet(testNET2);

            Assert.AreEqual(18231, scan1);
            Assert.AreEqual(1113, scan2);

            //foreach (var item in scanNETdata)
            //{
            //    Console.WriteLine(item);

            //}

            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine(scan1 + " was returned for NET= " + testNET1);

        }

        [Test]
        public void check_alignment_of_MZ()
        {
            var mzAlignmentInfoFilename = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_MZAlignment.txt");
            var NETAlignmentInfoFilename = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ScanNetAlignment.txt");

            var rf = new RunFactory();
            var run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var importer = new MassAlignmentInfoFromTextImporter(mzAlignmentInfoFilename);
            var massAlignmentData = importer.Import();

            var massAlignmentInfo = new MassAlignmentInfoLcmsWarp();
            massAlignmentInfo.SetMassAlignmentData(massAlignmentData);
            run.MassAlignmentInfo = massAlignmentInfo;

            var netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            var scanNETdata = netAlignmentInfoImporter.Import();
            run.NetAlignmentInfo.SetScanToNETAlignmentData(scanNETdata);

            float testScan = 6439;
            var theorMZ = 698.875137f;    //QCSHEW massTag 37003; m/z 698.875137 (2+)   See Redmine issue 627:  http://redmine.pnl.gov/issues/627

            var ppmshift = run.MassAlignmentInfo.GetPpmShift(theorMZ, (int) testScan);
            Console.WriteLine("ppm shift = " + ppmshift);

            var observedMZ = 698.8721;
            var alignedTargetMZ = run.GetTargetMZAligned(theorMZ);

            var differenceInMZ = Math.Abs(observedMZ - alignedTargetMZ);

            Console.WriteLine("theor m/z of monoisotopic peak = " + theorMZ.ToString("0.0000"));

            Console.WriteLine("observed m/z of monoisotopic peak = " + observedMZ.ToString("0.0000"));

            Console.WriteLine("aligned theor m/z = " + alignedTargetMZ.ToString("0.00000"));

            Assert.IsTrue(differenceInMZ < 0.001);
        }

        [Test]
        public void ensure_alignmentIsBeingUsed_duringProcessing_test1()
        {
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            var mzAlignmentInfoFilename = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_MZAlignment.txt";
            var NETAlignmentInfoFilename = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_NETAlignment.txt";
            var rawDataFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16.RAW";
            var peaksDataFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_peaks.txt";

            var run =  DeconTools.Backend.Utilities.RunUtilities.CreateAndLoadPeaks(rawDataFile, peaksDataFile);

            var importer = new MassAlignmentInfoFromTextImporter(mzAlignmentInfoFilename);
            var massAlignmentData = importer.Import();

            var massAlignmentInfo = new MassAlignmentInfoLcmsWarp();
            massAlignmentInfo.SetMassAlignmentData(massAlignmentData);
            run.MassAlignmentInfo = massAlignmentInfo;

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var testMassTagID = 24817;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            //first will execute workflow on a dataset that was NOT aligned

            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.2;   //use a very wide tolerance
            parameters.ChromGenTolerance = 5;
            parameters.MSToleranceInPPM = 15;

            var workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag);

            Assert.IsTrue(result.ChromPeakSelected != null);
            Assert.AreEqual(5395, (int)result.ChromPeakSelected.XValue);

            var netDiff = result.Target.NormalizedElutionTime - result.GetNET();
            Console.WriteLine("NET diff before alignment = " + netDiff);

            var expectedNETDiff = 0.057m;
            Assert.AreEqual(expectedNETDiff, (decimal)Math.Round(netDiff, 3));

            //import NET alignment information
            var netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            var scanNETdata = netAlignmentInfoImporter.Import();
            run.NetAlignmentInfo.SetScanToNETAlignmentData(scanNETdata);

            parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.01;   //use a more narrow tolerance
            parameters.ChromGenTolerance = 5;
            parameters.MSToleranceInPPM = 15;

            workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            netDiff = result.Target.NormalizedElutionTime - result.GetNET();
            Console.WriteLine("NET diff before alignment = " + netDiff);

            Assert.IsTrue(result.ChromPeakSelected != null);
            Assert.AreEqual(5395, (int)result.ChromPeakSelected.XValue);

            var expectedNETDiffMaximum = 0.01;
            Assert.IsTrue(netDiff < expectedNETDiffMaximum);
        }

        [Test]
        public void ensure_alignmentIsBeingUsed_duringProcessing_test2()
        {
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            var mzAlignmentInfoFilename = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_MZAlignment.txt";
            var NETAlignmentInfoFilename = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_NETAlignment.txt";
            var rawDataFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16.RAW";
            var peaksDataFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_peaks.txt";

            var run = DeconTools.Backend.Utilities.RunUtilities.CreateAndLoadPeaks(rawDataFile, peaksDataFile);

            var importer = new MassAlignmentInfoFromTextImporter(mzAlignmentInfoFilename);
            var massAlignmentData = importer.Import();

            var massAlignmentInfo = new MassAlignmentInfoLcmsWarp();
            massAlignmentInfo.SetMassAlignmentData(massAlignmentData);
            run.MassAlignmentInfo = massAlignmentInfo;

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var testMassTagID = 24730;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            //first will execute workflow on a dataset that was NOT aligned

            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.2;   //use a very wide tolerance
            parameters.ChromGenTolerance = 5;
            parameters.MSToleranceInPPM = 15;

            var workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag);

            Assert.IsTrue(result.ChromPeakSelected != null);
            Assert.AreEqual(9367, (int)result.ChromPeakSelected.XValue);

            var netDiff = result.Target.NormalizedElutionTime - result.GetNET();
            Console.WriteLine("NET diff before alignment = " + netDiff);

            var expectedNETDiff = 0.071m;
            Assert.AreEqual(expectedNETDiff, (decimal)Math.Round(netDiff, 3));

            //import NET alignment information
            var netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            var scanNETdata = netAlignmentInfoImporter.Import();
            run.NetAlignmentInfo.SetScanToNETAlignmentData(scanNETdata);

            parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.01;   //use a more narrow tolerance
            parameters.ChromGenTolerance = 5;
            parameters.MSToleranceInPPM = 15;

            workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            netDiff = result.Target.NormalizedElutionTime - result.GetNET();
            Console.WriteLine("NET diff after alignment = " + netDiff);

            Assert.IsTrue(result.ChromPeakSelected != null);

            var expectedNETDiffMaximum = 0.01;
            Assert.IsTrue(netDiff < expectedNETDiffMaximum);
        }

        [Test]
        public void ApplyViperMassAlignmentDataFromViperTest1()
        {
            var testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            var run = new RunFactory().CreateRun(testFile);

            var calibrationData = new ViperMassCalibrationData();
            calibrationData.MassError = -3.6;

            var massAlignmentInfo = new MassAlignmentInfoLcmsWarp();
            massAlignmentInfo.SetMassAlignmentData(calibrationData);

            run.MassAlignmentInfo = massAlignmentInfo;

            var testMZ = 974.504343924692;
            var alignedMZ = run.GetAlignedMZ(testMZ);
            var ppmDiff = (testMZ - alignedMZ) / testMZ * 1e6;

            Console.WriteLine("input m/z= " + testMZ);
            Console.WriteLine("aligned m/z= " + alignedMZ);
            Console.WriteLine("ppmDiff= " + ppmDiff);

            Assert.AreEqual(-3.6, (decimal)Math.Round(ppmDiff, 1));
        }

        [Test]
        public void LoadAndApplyMassAlignmentFromViperDataTest1()
        {
            var testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            var run = new RunFactory().CreateRun(testFile);

            var viperMassAlignmentFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\AlignmentInfo\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_MassAndGANETErrors_BeforeRefinement.txt";
            var loader = new ViperMassCalibrationLoader(viperMassAlignmentFile);

            /*
             * From unit test: targetedWorkflow_alignUsingDataFromFiles
             *      TargetID = 	24702
                    ChargeState = 	3
                    theor monomass= 	2920.5319802
                    theor m/z= 	974.517936556667
                    obs monomass= 	2920.49120230408
                    obs m/z= 	974.504343924692
                    ppmError before= 	13.9597398284934
                    ppmError after= 	10.8899784905986
                    calibrated mass= 	2920.50017566955
                    calibrated mass2= 	2920.50017566955
                    Database NET= 0.4197696
                    Result NET= 0.42916464805603
                    Result NET Error= -0.00934833288192749
                    NumChromPeaksWithinTol= 3
             *
             *
             */

            var calibrationData = loader.ImportMassCalibrationData();

            var massAlignmentInfo = new MassAlignmentInfoLcmsWarp();
            massAlignmentInfo.SetMassAlignmentData(calibrationData);

            run.MassAlignmentInfo = massAlignmentInfo;

            var testMZ = 974.504343924692;
            var alignedMZ =  run.GetAlignedMZ(testMZ);
            var ppmDiff = (testMZ - alignedMZ)/testMZ*1e6;

            Console.WriteLine("input m/z= " + testMZ);
            Console.WriteLine("aligned m/z= "+ alignedMZ);
            Console.WriteLine("ppmDiff= " + ppmDiff);

            Assert.AreEqual(-3.6, (decimal) Math.Round(ppmDiff, 1));
        }
    }
}
