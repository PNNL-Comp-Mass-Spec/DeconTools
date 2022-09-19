using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.FileIOTests.TargetedResultFileIOTests
{
    [TestFixture]
    public class UnlabeledTargetedResultFileIOTests
    {
        [Test]
        public void exporterTest1()
        {
            var testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_NETVals0.3-0.33.txt";

            var exportedResultFile = Path.Combine(FileRefs.OutputFolderPath, "UnlabeledTargetedResultsExporterOutput1.txt");

            if (File.Exists(exportedResultFile))
            {
                File.Delete(exportedResultFile);
            }

            var run = RunUtilities.CreateAndAlignRun(testFile, peaksTestFile);

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var selectedMassTags = mtc.TargetList.OrderBy(p => p.ID).Take(10).ToList();

            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            var workflow = new BasicTargetedWorkflow(run, parameters);

            foreach (var mt in selectedMassTags)
            {
                run.CurrentMassTag = mt;
                workflow.Execute();
            }

            var repo = new TargetedResultRepository();
            repo.AddResults(run.ResultCollection.GetMassTagResults());

            var exporter = new UnlabeledTargetedResultToTextExporter(exportedResultFile);
            exporter.ExportResults(repo.Results);
        }

        [Test]
        public void importerTest1()
        {
            var importedResultFile = Path.Combine(FileRefs.OutputFolderPath, "UnlabeledTargetedResultsExporterOutput1.txt");

            var importer = new UnlabeledTargetedResultFromTextImporter(importedResultFile);
            var repo = importer.Import();

            Assert.IsNotNull(repo);
            Assert.IsTrue(repo.Results.Count > 0);

            var testResult1 = repo.Results[0];

            Assert.AreEqual("QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18", testResult1.DatasetName);
            Assert.AreEqual(24698, testResult1.TargetID);
            Assert.AreEqual(3, testResult1.ChargeState);
            Assert.AreEqual(5880, testResult1.ScanLC);
            Assert.AreEqual(5876, testResult1.ScanLCStart);
            Assert.AreEqual(5888, testResult1.ScanLCEnd);
            Assert.AreEqual(0.3160, (decimal)testResult1.NET);
            Assert.AreEqual(1, testResult1.NumChromPeaksWithinTol);
            Assert.AreEqual(2311.07759, (decimal)testResult1.MonoMass);
            Assert.AreEqual(771.36647, (decimal)testResult1.MonoMZ);
            Assert.AreEqual(8247913, (decimal)testResult1.Intensity);
            Assert.AreEqual(0.0119, (decimal)testResult1.FitScore);
            Assert.AreEqual(0, (decimal)testResult1.IScore);
        }
    }
}
