using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
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
        [Test]
        public void doAlignmentTest1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            string deconToolsResultFile = FileRefs.ImportedData + "\\" + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_targetedFeatures.txt";

            UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(deconToolsResultFile);
            TargetedResultRepository repo = importer.Import();

            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_allMassTags.txt";
            MassTagCollection mtc = new MassTagCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            NETAndMassAligner aligner = new NETAndMassAligner();
            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.MassTagList);

            aligner.Execute(run);

            Assert.IsNotNull(run.AlignmentInfo);
            Assert.IsNotNull(run.AlignmentInfo.marrNETFncTimeInput);
            Assert.AreEqual(2273.0f, run.AlignmentInfo.marrNETFncTimeInput[0]);

            float testScan = 6005;
            float testNET1 = run.AlignmentInfo.GetNETFromTime(testScan);

           
            //note - this is Multialign's 
            Assert.AreEqual(0.3253423m, (decimal)testNET1);



        }

        [Test]
        public void ExportNET_andMass_AlignmentDataTest1()
        {
            string exportNETFilename = FileRefs.OutputFolderPath + "\\" + "exportedNETAlignmentInfo1.txt";
            string exportMassFilename = FileRefs.OutputFolderPath + "\\" + "exportedMassAlignmentInfo1.txt";


            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            string deconToolsResultFile = FileRefs.ImportedData + "\\" + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_targetedFeatures.txt";

            UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(deconToolsResultFile);
            TargetedResultRepository repo = importer.Import();

            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_allMassTags.txt";
            MassTagCollection mtc = new MassTagCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            NETAndMassAligner aligner = new NETAndMassAligner();
            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.MassTagList);

            aligner.Execute(run);

            NETAlignmentInfoToTextExporter exporter = new NETAlignmentInfoToTextExporter(exportNETFilename);
            exporter.ExportAlignmentInfo(run.AlignmentInfo);

            MassAlignmentInfoToTextExporter massInfoexporter = new MassAlignmentInfoToTextExporter(exportMassFilename);
            massInfoexporter.ExportAlignmentInfo(run.AlignmentInfo);
        }


        [Test]
        public void ImportNET_and_Try_Alignment_Test1()
        {
            string importFilename = FileRefs.ImportedData + "\\" + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ScanNetAlignment.txt";

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            NETAlignmentFromTextImporter importer = new NETAlignmentFromTextImporter(importFilename);
            List<ScanNETPair> scanNETdata = importer.Import();
            run.SetScanToNETAlignmentData(scanNETdata);

            float testScan = 6005;
            float testNET1 = run.GetNETValueForScan((int)testScan);

            
           // float testNET1 = run.AlignmentInfo.GetNETFromTime(testScan);

            Assert.AreEqual(0.3252918m, (decimal)testNET1);

        }

        [Test]
        public void ImportMassAndTimePPMCorrections_and_Try_Alignment_Test1()
        {
            string importFilename = FileRefs.ImportedData + "\\" + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_MZAlignment.txt";

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            MassAlignmentInfoFromTextImporter importer = new MassAlignmentInfoFromTextImporter(importFilename);
            List<MassAlignmentDataItem> massAlignmentData = importer.Import();
            run.SetMassAlignmentData(massAlignmentData);

            float testScan = 6439;
            float testMZ = 698.875137f;    //QCSHEW massTag 37003; m/z 698.875137 (2+)    See Redmine issue 627:  http://redmine.pnl.gov/issues/627

            float ppmshift = run.AlignmentInfo.GetPPMShiftFromTimeMZ(testScan, testMZ);
            Console.WriteLine("ppm shift = " + ppmshift);




            Assert.AreEqual(-4.3, (decimal)Math.Round(ppmshift, 1));

        }


        [Test]
        public void Import_NET_And_MassAlignment_Test1()
        {
            string mzAlignmentInfoFilename = FileRefs.ImportedData + "\\" + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_MZAlignment.txt";
            string NETAlignmentInfoFilename = FileRefs.ImportedData + "\\" + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ScanNetAlignment.txt";

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            MassAlignmentInfoFromTextImporter importer = new MassAlignmentInfoFromTextImporter(mzAlignmentInfoFilename);
            List<MassAlignmentDataItem> massAlignmentData = importer.Import();
            run.SetMassAlignmentData(massAlignmentData);

            NETAlignmentFromTextImporter netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            List<ScanNETPair> scanNETdata = netAlignmentInfoImporter.Import();
            run.SetScanToNETAlignmentData(scanNETdata);

            float testScan = 6439;
            float testMZ = 698.875137f;    //QCSHEW massTag 37003; m/z 698.875137 (2+)   See Redmine issue 627:  http://redmine.pnl.gov/issues/627

            float ppmshift = run.AlignmentInfo.GetPPMShiftFromTimeMZ(testScan, testMZ);
            Console.WriteLine("ppm shift = " + ppmshift);

            float testScan2 = 6005;
            float testNET1 = run.GetNETValueForScan((int)testScan2);

            Assert.AreEqual(0.3252918m, (decimal)testNET1);
            Assert.AreEqual(-4.3, (decimal)Math.Round(ppmshift, 1));

        }


        [Test]
        public void checkRetrievalOfScanValueForAGivenNET()
        {
            string NETAlignmentInfoFilename = FileRefs.ImportedData + "\\" + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ScanNetAlignment.txt";

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            
            NETAlignmentFromTextImporter netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            List<ScanNETPair> scanNETdata = netAlignmentInfoImporter.Import();
            run.SetScanToNETAlignmentData(scanNETdata);

            float testNET1 = 0.95f;
            float testNET2 = 0.08f;
            int scan1 =  run.GetScanValueForNET(testNET1);
            int scan2 = run.GetScanValueForNET(testNET2);

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
        public void checkRetrievalOfScanValueForAGivenNET_test2()
        {
            string NETAlignmentInfoFilename = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_NETAlignment.txt";

            string rawdatafile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16.RAW"; 

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(rawdatafile);

            NETAlignmentFromTextImporter netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            List<ScanNETPair> scanNETdata = netAlignmentInfoImporter.Import();
            run.SetScanToNETAlignmentData(scanNETdata);

            float testNET1 = 0.66f;
            int scan1 = run.GetScanValueForNET(testNET1);

            Assert.AreEqual(9986, scan1);

            //foreach (var item in scanNETdata)
            //{
            //    Console.WriteLine(item.Scan + "\t" + item.NET);

            //}

            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine(scan1 + " was returned for NET= " + testNET1);

        }



        [Test]
        public void check_alignment_of_MZ()
        {
            string mzAlignmentInfoFilename = FileRefs.ImportedData + "\\" + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_MZAlignment.txt";
            string NETAlignmentInfoFilename = FileRefs.ImportedData + "\\" + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ScanNetAlignment.txt";

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            MassAlignmentInfoFromTextImporter importer = new MassAlignmentInfoFromTextImporter(mzAlignmentInfoFilename);
            List<MassAlignmentDataItem> massAlignmentData = importer.Import();
            run.SetMassAlignmentData(massAlignmentData);

            NETAlignmentFromTextImporter netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            List<ScanNETPair> scanNETdata = netAlignmentInfoImporter.Import();
            run.SetScanToNETAlignmentData(scanNETdata);


            float testScan = 6439;
            float theorMZ = 698.875137f;    //QCSHEW massTag 37003; m/z 698.875137 (2+)   See Redmine issue 627:  http://redmine.pnl.gov/issues/627

            float ppmshift = run.AlignmentInfo.GetPPMShiftFromTimeMZ(testScan, theorMZ);
            Console.WriteLine("ppm shift = " + ppmshift);

           
            double observedMZ = 698.8721;
            double alignedTargetMZ = run.GetTargetMZAligned(theorMZ);

            double differenceInMZ = Math.Abs(observedMZ - alignedTargetMZ);


            Console.WriteLine("theor m/z of monoisotopic peak = " + theorMZ.ToString("0.0000"));
         
            Console.WriteLine("observed m/z of monoisotopic peak = " + observedMZ.ToString("0.0000"));

            Console.WriteLine("aligned theor m/z = " + alignedTargetMZ.ToString("0.00000"));


            Assert.IsTrue(differenceInMZ < 0.001);



        }



        [Test]
        public void ensure_alignmentIsBeingUsed_duringProcessing_test1()
        {
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            string mzAlignmentInfoFilename = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_MZAlignment.txt";
            string NETAlignmentInfoFilename = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_NETAlignment.txt";
            string rawDataFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16.RAW";
            string peaksDataFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_peaks.txt";

            Run run =  DeconTools.Backend.Utilities.RunUtilities.CreateAndLoadPeaks(rawDataFile, peaksDataFile);
          
          
            MassAlignmentInfoFromTextImporter importer = new MassAlignmentInfoFromTextImporter(mzAlignmentInfoFilename);
            List<MassAlignmentDataItem> massAlignmentData = importer.Import();
            run.SetMassAlignmentData(massAlignmentData);

            MassTagCollection mtc = new MassTagCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 24817;
            run.CurrentMassTag = (from n in mtc.MassTagList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            //first will execute workflow on a dataset that was NOT aligned

            DeconToolsTargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.2;   //use a very wide tolerance
            parameters.ChromToleranceInPPM = 5;
            parameters.MSToleranceInPPM = 15;

            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            MassTagResultBase result = run.ResultCollection.GetMassTagResult(run.CurrentMassTag);

            Assert.IsTrue(result.ChromPeakSelected != null);
            Assert.AreEqual(5395, (int)result.ChromPeakSelected.XValue);


            double netDiff = result.MassTag.NETVal - result.GetNET();
            Console.WriteLine("NET diff before alignment = " + netDiff);
            
            decimal expectedNETDiff = 0.057m;
            Assert.AreEqual(expectedNETDiff, (decimal)Math.Round(netDiff, 3));


            //import NET alignment information
            NETAlignmentFromTextImporter netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            List<ScanNETPair> scanNETdata = netAlignmentInfoImporter.Import();
            run.SetScanToNETAlignmentData(scanNETdata);

            parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.01;   //use a more narrow tolerance
            parameters.ChromToleranceInPPM = 5;
            parameters.MSToleranceInPPM = 15;

            workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();


            netDiff = result.MassTag.NETVal - result.GetNET();
            Console.WriteLine("NET diff before alignment = " + netDiff);


            Assert.IsTrue(result.ChromPeakSelected != null);
            Assert.AreEqual(5395, (int)result.ChromPeakSelected.XValue);
            
            double expectedNETDiffMaximum = 0.01;
            Assert.IsTrue(netDiff < expectedNETDiffMaximum);

            
        }

        [Test]
        public void ensure_alignmentIsBeingUsed_duringProcessing_test2()
        {
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";
            string mzAlignmentInfoFilename = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_MZAlignment.txt";
            string NETAlignmentInfoFilename = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_NETAlignment.txt";
            string rawDataFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16.RAW";
            string peaksDataFile = @"D:\Data\Orbitrap\QC_Shew_08_04-pt1-3_15Apr09_Sphinx_09-02-16_peaks.txt";

            Run run = DeconTools.Backend.Utilities.RunUtilities.CreateAndLoadPeaks(rawDataFile, peaksDataFile);


            MassAlignmentInfoFromTextImporter importer = new MassAlignmentInfoFromTextImporter(mzAlignmentInfoFilename);
            List<MassAlignmentDataItem> massAlignmentData = importer.Import();
            run.SetMassAlignmentData(massAlignmentData);

            MassTagCollection mtc = new MassTagCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 24730;
            run.CurrentMassTag = (from n in mtc.MassTagList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            //first will execute workflow on a dataset that was NOT aligned

            DeconToolsTargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.2;   //use a very wide tolerance
            parameters.ChromToleranceInPPM = 5;
            parameters.MSToleranceInPPM = 15;

            BasicTargetedWorkflow workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();

            MassTagResultBase result = run.ResultCollection.GetMassTagResult(run.CurrentMassTag);

            Assert.IsTrue(result.ChromPeakSelected != null);
            Assert.AreEqual(9367, (int)result.ChromPeakSelected.XValue);


            double netDiff = result.MassTag.NETVal - result.GetNET();
            Console.WriteLine("NET diff before alignment = " + netDiff);

            decimal expectedNETDiff = 0.071m;
            Assert.AreEqual(expectedNETDiff, (decimal)Math.Round(netDiff, 3));


            //import NET alignment information
            NETAlignmentFromTextImporter netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            List<ScanNETPair> scanNETdata = netAlignmentInfoImporter.Import();
            run.SetScanToNETAlignmentData(scanNETdata);

            parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromNETTolerance = 0.01;   //use a more narrow tolerance
            parameters.ChromToleranceInPPM = 5;
            parameters.MSToleranceInPPM = 15;

            workflow = new BasicTargetedWorkflow(run, parameters);
            workflow.Execute();


            netDiff = result.MassTag.NETVal - result.GetNET();
            Console.WriteLine("NET diff before alignment = " + netDiff);


            Assert.IsTrue(result.ChromPeakSelected != null);
            Assert.AreEqual(5395, (int)result.ChromPeakSelected.XValue);

            double expectedNETDiffMaximum = 0.01;
            Assert.IsTrue(netDiff < expectedNETDiffMaximum);


        }
    }
}
