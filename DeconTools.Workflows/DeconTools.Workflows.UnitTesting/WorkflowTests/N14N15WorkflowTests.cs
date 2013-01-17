using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
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



        [Test]
        public void WorkflowTest1()
        {
            // See:  https://jira.pnnl.gov/jira/browse/OMCS-409

            Run run = RunUtilities.CreateAndAlignRun(bruker9t_samplefile1, bruker9t_peaksfile1);

            string targetsFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\N14N15_standard_testing\Targets\POnly_MassTagsMatchingInHalfOfDatasets_Filtered0.45-0.47NET_first18.txt";


            MassTagFromTextFileImporter importer = new MassTagFromTextFileImporter(targetsFile);
           var targetCollection=    importer.Import();
            

            run.CurrentMassTag = targetCollection.TargetList.FirstOrDefault(p => p.ChargeState == 1);

            N14N15Workflow2Parameters parameters = new N14N15Workflow2Parameters();
            parameters.LoadParameters(FileRefs.ImportedData + "\\" + "importedN14N15WorkflowParameters.xml");
            parameters.ChromToleranceInPPM = 25;
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

            var resultOutput=  run.ResultCollection.GetTargetedResult(run.CurrentMassTag) as N14N15_TResult;

            resultOutput.DisplayToConsole();

            Assert.AreEqual(23085448, resultOutput.Target.ID);
            Assert.AreEqual(1, resultOutput.IsotopicProfile.ChargeState);

            Assert.IsNotNull(resultOutput.ScanSet);
            
            Assert.AreEqual(1639, resultOutput.ScanSet.PrimaryScanNumber);

            Assert.AreEqual(1638, resultOutput.ScanSetForN15Profile.PrimaryScanNumber);


            //Assert.AreEqual(0.462700009346008m, (decimal)resultOutput.GetNET());
            //Assert.AreEqual(0.462300002574921m, (decimal)resultOutput.GetNETN15());

            Assert.AreEqual(1, resultOutput.NumChromPeaksWithinTolerance);
            Assert.AreEqual(1, resultOutput.NumChromPeaksWithinToleranceForN15Profile);

            Assert.AreEqual(1518.82660900043m, (decimal)resultOutput.IsotopicProfile.MonoIsotopicMass);
            Assert.AreEqual(1534.7929741093m, (decimal)resultOutput.IsotopicProfileLabeled.MonoIsotopicMass);

            Assert.AreEqual(1519.83389m, (decimal)Math.Round(resultOutput.IsotopicProfile.MonoPeakMZ, 5));
            Assert.AreEqual(1535.80025m, (decimal)Math.Round(resultOutput.IsotopicProfileLabeled.MonoPeakMZ, 5));

            Assert.AreEqual(3901522, resultOutput.IntensityAggregate);
            Assert.AreEqual(3083117, resultOutput.IntensityAggregate);

            Assert.AreEqual(0.04m, (decimal)Math.Round(resultOutput.Score, 3));
            Assert.AreEqual(0.074m, (decimal)Math.Round(resultOutput.ScoreN15, 3));

            //Assert.AreEqual(0.208, (decimal)Math.Round(resultOutput.IScore1, 3));
            //Assert.AreEqual(0.110, (decimal)Math.Round(resultOutput.IScore2, 3));

            //Assert.AreEqual(1.027, (decimal)Math.Round(resultOutput.Iso1RatioContrib, 3));
            //Assert.AreEqual(1.278, (decimal)Math.Round(resultOutput.Iso2RatioContrib, 3));

            Assert.AreEqual(0.745532811554022m, (decimal)resultOutput.RatioN14N15);






        }

    }
}
