using System;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]



    public class N14N15WorkflowTests
    {
        private string bruker9t_samplefile1 =
            @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\RawData\RSPH_PtoA_L_28_rep1_28Feb08_Raptor_08-01-01";

        private string bruker9t_peaksfile1 =
            @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\RawData\RSPH_PtoA_L_28_rep1_28Feb08_Raptor_08-01-01\RSPH_PtoA_L_28_rep1_28Feb08_Raptor_08-01-01_scans1450_1800_peaks.txt";




        [Test]
        public void exportParametersTest1()
        {
            string exportedParametersFile = FileRefs.OutputFolderPath + "\\" + "exportedN14N15WorkflowParameters.xml";

            exportedParametersFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\N14N15WorkflowParameters1.xml";

            var parameters = new N14N15Workflow2Parameters();



            parameters.SaveParametersToXML(exportedParametersFile);


        }


        [Test]
        public void importParametersTest1()
        {
            string importedParametersFile = FileRefs.ImportedData + "\\" + "importedN14N15WorkflowParameters.xml";

            var wp = WorkflowParameters.CreateParameters(importedParametersFile);

            Assert.AreEqual("N14N15Targeted1", wp.WorkflowType.ToString());
            Assert.IsTrue(wp is N14N15Workflow2Parameters);

        }


        [Category("MustPass")]
        [Test]
        public void WorkflowTest1()
        {


            // See:  https://jira.pnnl.gov/jira/browse/OMCS-409

            Run run = RunUtilities.CreateAndAlignRun(bruker9t_samplefile1, bruker9t_peaksfile1);

            string targetsFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Targets\POnly_MassTagsMatchingInHalfOfDatasets_Filtered0.45-0.47NET_first18.txt";


            MassTagFromTextFileImporter importer = new MassTagFromTextFileImporter(targetsFile);
            var targetCollection = importer.Import();


            run.CurrentMassTag = targetCollection.TargetList.FirstOrDefault(p => p.ChargeState == 1);

            N14N15Workflow2Parameters parameters = new N14N15Workflow2Parameters();
            parameters.LoadParameters(FileRefs.ImportedData + "\\" + "importedN14N15WorkflowParameters.xml");
            parameters.ChromGenTolerance = 25;
            parameters.MSToleranceInPPM = 25;
            parameters.TargetedFeatureFinderToleranceInPPM = 25;
            parameters.MultipleHighQualityMatchesAreAllowed = true;
            parameters.NumMSScansToSum = 5;
            parameters.SaveParametersToXML(
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Parameters\N14N15WorkflowParameters1_test.xml");


            Console.WriteLine(parameters.ToStringWithDetails());

            N14N15Workflow2 workflow = new N14N15Workflow2(run, parameters);
            workflow.Execute();
            Assert.IsTrue(run.ResultCollection.ResultType == Globals.ResultType.N14N15_TARGETED_RESULT);

            //TestUtilities.DisplayXYValues(workflow.ChromatogramXYData);

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as N14N15_TResult;

            result.DisplayToConsole();

            Assert.AreEqual(23085448, result.Target.ID);
            Assert.AreEqual(1, result.IsotopicProfile.ChargeState);

            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.IsNotNull(result.ChromPeakSelectedN15);

            Assert.AreEqual(1639.3m, (decimal)Math.Round(result.ChromPeakSelected.XValue, 1));
            Assert.AreEqual(1638.5m, (decimal)Math.Round(result.ChromPeakSelectedN15.XValue, 1));

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.IsotopicProfileLabeled);

            Console.WriteLine("theor monomass= \t" + result.Target.MonoIsotopicMass);
            Console.WriteLine("monomass= \t" + result.IsotopicProfile.MonoIsotopicMass);
            Console.WriteLine("monomassN15= \t" + result.IsotopicProfileLabeled.MonoIsotopicMass);

            Console.WriteLine("monoMZ= \t" + result.IsotopicProfile.MonoPeakMZ);
            Console.WriteLine("monoMZN15= \t" + result.IsotopicProfileLabeled.MonoPeakMZ);

            Console.WriteLine("ppmError= \t" + result.GetMassErrorBeforeAlignmentInPPM());

            Console.WriteLine("Database NET= " + result.Target.NormalizedElutionTime);
            Console.WriteLine("Result NET= " + result.GetNET());
            Console.WriteLine("Result NET Error= " + result.GetNETAlignmentError());
            Console.WriteLine("NumChromPeaksWithinTol= " + result.NumChromPeaksWithinTolerance);
            Console.WriteLine("NumChromPeaksWithinTolN15= " + result.NumChromPeaksWithinToleranceForN15Profile);

        }

    }
}
