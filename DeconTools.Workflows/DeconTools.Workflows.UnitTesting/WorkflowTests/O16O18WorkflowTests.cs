using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;
using System;
using DeconTools.UnitTesting2;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class O16O18WorkflowTests
    {


        [Test]
        public void testVladsData()
        {
            string testFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\RawData\Alz_P01_A01_097_26Apr12_Roc_12-03-15.RAW";

            string massTagFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\Targets\MT_Human_ALZ_O18_P836\MassTags_PMT2_First60.txt";

            string peakTestFile = testFile.Replace(".RAW", "_peaks.txt");

            Run run = RunUtilities.CreateAndAlignRun(testFile, peakTestFile);


            TargetCollection mtc = new TargetCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 24653;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 1 select n).First();

            TargetedWorkflowParameters parameters = new O16O18WorkflowParameters();
            parameters.ChromNETTolerance = 0.2;
            parameters.MSToleranceInPPM = 10;
            parameters.ChromGeneratorMode = Globals.ChromatogramGeneratorMode.O16O18_THREE_MONOPEAKS;
            parameters.ChromPeakDetectorPeakBR = 1;


            O16O18Workflow workflow = new O16O18Workflow(run, parameters);
            workflow.Execute();

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag);

            result.DisplayToConsole();

            Console.WriteLine("theor monomass= \t" + result.Target.MonoIsotopicMass);
            Console.WriteLine("monomass= \t" + result.IsotopicProfile.MonoIsotopicMass);

            Console.WriteLine("ppmError before= \t" + result.GetMassErrorBeforeAlignmentInPPM());

            Console.WriteLine("ppmError after= \t" + result.GetMassErrorAfterAlignmentInPPM());


            var calibratedMass = -1* ((result.Target.MonoIsotopicMass*result.GetMassErrorAfterAlignmentInPPM()/1e6) -
                                  result.Target.MonoIsotopicMass);


            var calibratedMass2 = result.GetCalibratedMonoisotopicMass();


            Console.WriteLine("calibrated mass= \t" + calibratedMass);
            Console.WriteLine("calibrated mass2= \t" + calibratedMass2);

            var errorInMZ = result.GetMassErrorAfterAlignmentInPPM()*result.Target.MonoIsotopicMass/1e6;
            var calcTheorMonoMass = calibratedMass + errorInMZ;


            Console.WriteLine("Theor monomass=" + calcTheorMonoMass);


        }







        [Test]
        public void findSingleMassTag_test1()
        {
            string testFile = @"D:\Data\O16O18\Weijun\TechTest_O18_02\TechTest_O18_02_RunA_10Dec09_Doc_09-11-08.RAW";

            string massTagFile = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2011\O16O18_TargetedProcessing\Targets\MassTags_MousePlasma_1709_allChargeStates_nonRedundant.txt";

            string peakTestFile = testFile.Replace(".RAW", "_peaks.txt");

            Run run = RunUtilities.CreateAndLoadPeaks(testFile, peakTestFile);


            TargetCollection mtc = new TargetCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 6643962;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            TargetedWorkflowParameters parameters = new O16O18WorkflowParameters();
            parameters.ChromNETTolerance = 0.1;
            parameters.MSToleranceInPPM = 10;

            O16O18Workflow workflow = new O16O18Workflow(run, parameters);
            workflow.Execute();


            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag);

            Assert.IsNotNull(result);
            result.DisplayToConsole();

            Assert.IsNotNull(result.IsotopicProfile);
            Assert.IsNotNull(result.ScanSet);
            Assert.IsNotNull(result.ChromPeakSelected);
            Assert.AreEqual(2, result.IsotopicProfile.ChargeState);
            Assert.AreEqual(6865, result.ScanSet.PrimaryScanNumber);


        }

        [Test]
        [Ignore("Local files used")]
        public void checkO16O18ChromGenMode_test1()
        {
            //

            string testFile = @"D:\Data\O16O18\Weijun\TechTest_O18_02\TechTest_O18_02_RunA_10Dec09_Doc_09-11-08.RAW";

            string massTagFile = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2011\O16O18_TargetedProcessing\Targets\MassTags_MousePlasma_1709_allChargeStates_nonRedundant.txt";

            string peakTestFile = testFile.Replace(".RAW", "_peaks.txt");

            Run run = RunUtilities.CreateAndLoadPeaks(testFile, peakTestFile);


            TargetCollection mtc = new TargetCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 20746149;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            TargetedWorkflowParameters parameters = new O16O18WorkflowParameters();
            parameters.ChromNETTolerance = 0.1;
            parameters.MSToleranceInPPM = 10;
            parameters.ChromGeneratorMode = DeconTools.Backend.Globals.ChromatogramGeneratorMode.O16O18_THREE_MONOPEAKS;
            parameters.ChromPeakDetectorPeakBR = 1;


            O16O18Workflow workflow = new O16O18Workflow(run, parameters);
            workflow.Execute();

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag);

            result.DisplayToConsole();
           // Assert.AreEqual(3498, result.ScanSet.PrimaryScanNumber);

            Console.WriteLine("O16O18 ChromMode PeaksWithinTol = " + result.NumChromPeaksWithinTolerance);
            Console.WriteLine("O16O18 ChromMode HQPeaksWithinTol = " + result.NumQualityChromPeaks);
            //foreach (var item in workflow.ChromPeaksDetected)
            //{
            //    Console.WriteLine("peak\t" + item.XValue.ToString("0.0") + "\t" + item.Height.ToString("0"));
            //} 
           // TestUtilities.DisplayXYValues(workflow.ChromatogramXYData);

            parameters.ChromGeneratorMode = Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            workflow = new O16O18Workflow(run, parameters);
            workflow.Execute();

           

            result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag);

            Console.WriteLine("MonoChromMode PeaksWithinTol = " + result.NumChromPeaksWithinTolerance);
            Console.WriteLine("O16O18 ChromMode HQPeaksWithinTol = " + result.NumQualityChromPeaks);
           // TestUtilities.DisplayXYValues(workflow.ChromatogramXYData);

            //foreach (var item in workflow.ChromPeaksDetected)
            //{
            //    Console.WriteLine("peak\t" + item.XValue.ToString("0.0") + "\t" + item.Height.ToString("0"));

            //}

            result.DisplayToConsole();

        }
    }
}
