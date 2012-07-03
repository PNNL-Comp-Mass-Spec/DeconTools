using System.Linq;
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
        public void testVladsData()
        {
            string testFile =
                @"\\protoapps\UserData\Slysz\Data\O16O18\Vlad_O16O18\RawData\Alz_P01_D12_144_26Apr12_Roc_12-03-18.RAW";

            string massTagFile =
                @"\\protoapps\UserData\Slysz\Data\O16O18\Vlad_O16O18\Targets\MT_Human_ALZ_O18_P836\MassTags_PMT2.txt";

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
            parameters.ChromGeneratorMode = DeconTools.Backend.ProcessingTasks.ChromatogramGeneratorMode.O16O18_THREE_MONOPEAKS;
            parameters.ChromPeakDetectorPeakBR = 1;


            O16O18Workflow workflow = new O16O18Workflow(run, parameters);
            workflow.Execute();

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag);

            result.DisplayToConsole();

            
        }


        [Test]
        public void checkO16O18ChromGenMode_test1()
        {
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
            parameters.ChromGeneratorMode = DeconTools.Backend.ProcessingTasks.ChromatogramGeneratorMode.O16O18_THREE_MONOPEAKS;
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

            parameters.ChromGeneratorMode = DeconTools.Backend.ProcessingTasks.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
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
