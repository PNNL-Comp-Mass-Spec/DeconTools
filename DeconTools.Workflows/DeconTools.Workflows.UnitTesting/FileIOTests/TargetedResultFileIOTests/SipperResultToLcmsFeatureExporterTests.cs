using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.FileIOTests.TargetedResultFileIOTests
{
    [TestFixture]
    public class SipperResultToLcmsFeatureExporterTests
    {
        [Test]
        public void executeWorkflowTest1()
        {
            var testFile = FileRefs.SipperRawDataFile;

            var exportedResultFile = Path.Combine(FileRefs.OutputFolderPath, "ExportedSipperResults1.txt");

            var peaksFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_peaks.txt";

            var run = RunUtilities.CreateAndLoadPeaks(testFile, peaksFile);

            var lcmsfeaturesFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_LCMSFeatures.txt";

            // load LCMSFeatures as targets
            var importer =
                new LcmsTargetFromFeaturesFileImporter(lcmsfeaturesFile);

            var lcmsTargetCollection = importer.Import();

            // load MassTags
            var massTagFile1 =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_MassTags.txt";

            var massTagImporter = new MassTagFromTextFileImporter(massTagFile1);
            var massTagCollection = massTagImporter.Import();

            //enriched
            var testMassTags = new int[] { 355116553, 355129038, 355160150, 355162540, 355163371 };

            var filteredLcmsFeatureTargets = (from n in lcmsTargetCollection.TargetList
                                              where testMassTags.Contains(((LcmsFeatureTarget)n).FeatureToMassTagID)
                                              select n).ToList();

            TargetCollection.UpdateTargetsWithMassTagInfo(filteredLcmsFeatureTargets, massTagCollection.TargetList);

            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromPeakSelectorMode = Globals.PeakSelectorMode.ClosestToTarget;

            var workflow = new SipperTargetedWorkflow(run, parameters);

            foreach (var target in filteredLcmsFeatureTargets)
            {
                run.CurrentMassTag = target;

                workflow.Execute();
            }

            var results = run.ResultCollection.GetMassTagResults();

            var repo = new TargetedResultRepository();
            repo.AddResults(run.ResultCollection.GetMassTagResults());

            var exporter = new SipperResultToLcmsFeatureExporter(exportedResultFile);
            exporter.ExportResults(repo.Results);
        }
    }
}
