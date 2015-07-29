using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.FileIO.TargetedResultFileIO;
using DeconTools.Backend.ProcessingTasks.NETAlignment;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.NET_Mass_AlignmentTests
{
    [TestFixture]
    public class AlignmentTests
    {
        [Test]
        public void doAlignmentTest1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            string deconToolsResultFile = @"D:\Temp\output7\targetedFeatures.txt";

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

            Assert.AreEqual(0.3253423m, (decimal)testNET1);



        }

        [Test]
        public void ExportNET_andMass_AlignmentDataTest1()
        {
            string exportNETFilename = Path.Combine(FileRefs.OutputFolderPath, "exportedNETAlignmentInfo1.txt");
            string exportMassFilename = Path.Combine(FileRefs.OutputFolderPath, "exportedMassAlignmentInfo1.txt");


            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            string deconToolsResultFile = @"D:\Temp\output7\targetedFeatures.txt";

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
            string importFilename = Path.Combine(FileRefs.OutputFolderPath, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ScanNetAlignment.txt");

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            NETAlignmentFromTextImporter importer = new NETAlignmentFromTextImporter(importFilename);
            run.AlignmentInfo = importer.Import();

            float testScan = 6005;
            float testNET1 = run.AlignmentInfo.GetNETFromTime(testScan);

            Assert.AreEqual(0.3253423m, (decimal)testNET1);

        }

        [Test]
        public void ImportMassAndTimePPMCorrections_and_Try_Alignment_Test1()
        {
            string importFilename = Path.Combine(FileRefs.OutputFolderPath, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_MZAlignment.txt");

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            MassAlignmentInfoFromTextImporter importer = new MassAlignmentInfoFromTextImporter(importFilename);
            run.AlignmentInfo = importer.Import();

            float testScan = 6439;
            float testMZ = 698.875137f;    //QCSHEW massTag 37003; m/z 698.875137 (2+)    See Redmine issue 627:  http://redmine.pnl.gov/issues/627

            float ppmshift = run.AlignmentInfo.GetPPMShiftFromTimeMZ(testScan, testMZ);
            Console.WriteLine("ppm shift = " + ppmshift);

            Assert.AreEqual(-4.3, (decimal)Math.Round(ppmshift, 1));

        }


        [Test]
        public void Import_NET_And_MassAlignment_Test1()
        {
            string mzAlignmentInfoFilename = Path.Combine(FileRefs.OutputFolderPath, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_MZAlignment.txt");
            string NETAlignmentInfoFilename = Path.Combine(FileRefs.OutputFolderPath, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_ScanNetAlignment.txt");

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            MassAlignmentInfoFromTextImporter importer = new MassAlignmentInfoFromTextImporter(mzAlignmentInfoFilename);
            run.AlignmentInfo = importer.Import();

            NETAlignmentFromTextImporter netAlignmentInfoImporter = new NETAlignmentFromTextImporter(NETAlignmentInfoFilename);
            netAlignmentInfoImporter.ImportIntoAlignmentInfo(run.AlignmentInfo);   //this will append the NET alignment info to the AlignmentInfo object

            float testScan = 6439;
            float testMZ = 698.875137f;    //QCSHEW massTag 37003; m/z 698.875137 (2+)   See Redmine issue 627:  http://redmine.pnl.gov/issues/627

            float ppmshift = run.AlignmentInfo.GetPPMShiftFromTimeMZ(testScan, testMZ);
            Console.WriteLine("ppm shift = " + ppmshift);

            float testScan2 = 6005;
            float testNET1 = run.AlignmentInfo.GetNETFromTime(testScan2);

            Assert.AreEqual(0.3253423m, (decimal)testNET1);
            Assert.AreEqual(-4.3, (decimal)Math.Round(ppmshift, 1));

        }


    }
}
