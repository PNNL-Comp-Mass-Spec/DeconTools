using System;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.NETAlignment;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.TargetedProcessing_Tests
{
    [TestFixture]
    public class SmartChromPeakSelectorTests
    {
        private string xcaliburTestfile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
        private string massTagTestList1 = FileRefs.RawDataBasePath + "\\TargetedWorkflowStandards\\QCShew_peptidesWithObsCountGreaterThan1000.txt";
       
      
        [Test]
        public void SmartChromPeakSelectorParameterTest1()
        {
            SmartChromPeakSelectorParameters parameters = new SmartChromPeakSelectorParameters();
            Assert.AreEqual(20, parameters.NumChromPeaksAllowed);
            Assert.AreEqual(0.05m, (decimal)parameters.NETTolerance);
        }


        [Test]
        public void smartChromPeakSelectorTest_noSumming()
        {

            Run run = new RunFactory().CreateRun(xcaliburTestfile);
            run.Close();

            run = new RunFactory().CreateRun(xcaliburTestfile);
           

            var massTagColl = new TargetCollection();

            MassTagFromTextFileImporter masstagImporter = new MassTagFromTextFileImporter(massTagTestList1);
            massTagColl = masstagImporter.Import();

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            var theorFeatureGen = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);

            var chromPeakDet = new ChromPeakDetector(0.5, 1);

            SmartChromPeakSelectorParameters smartchromParam = new SmartChromPeakSelectorParameters();
            var smartChromPeakSelector = new SmartChromPeakSelector(smartchromParam);

            ChromPeakSelectorParameters basicChromParam = new ChromPeakSelectorParameters();
            var basicChromPeakSelector = new BasicChromPeakSelector(basicChromParam);



            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var iterativeTff = new IterativeTFF(new IterativeTFFParameters());

            var fitscoreCalc = new MassTagFitScoreCalculator();



            string testChromatogramDataFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TargetedWorkflowStandards\massTag635428_chromatogramData.txt";

            XYData xydata = TestUtilities.LoadXYDataFromFile(testChromatogramDataFile);
            Assert.IsNotNull(xydata);

            run.XYData = xydata;
           // run.XYData.Display();

            run.CurrentMassTag = massTagColl.TargetList.Where(p => p.ID == 635428).First();

            Console.WriteLine("------MT = " + run.CurrentMassTag.ToString());

            theorFeatureGen.Execute(run.ResultCollection);
            chromPeakDet.Execute(run.ResultCollection);

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag);

            //first run the standard peak selector
            basicChromPeakSelector.Execute(run.ResultCollection);
            Assert.AreEqual(10066, (int)Math.Round(result.ChromPeakSelected.XValue));


            //now run the smart chrom peak selector
            run.XYData = xydata;
            chromPeakDet.Execute(run.ResultCollection);
            smartChromPeakSelector.Parameters.NETTolerance = 0.025f;
            smartChromPeakSelector.Execute(run.ResultCollection);

            msgen.Execute(run.ResultCollection);
            iterativeTff.Execute(run.ResultCollection);
            fitscoreCalc.Execute(run.ResultCollection);

        
            Assert.AreEqual(9579, (int)Math.Round(result.ChromPeakSelected.XValue));
            result.DisplayToConsole();

        }


        [Test]
        public void smartChromPeakSelectorTest_withDynamicSumming()
        {
            Run run = new RunFactory().CreateRun(xcaliburTestfile);

            var massTagColl = new TargetCollection();

            MassTagFromTextFileImporter masstagImporter = new MassTagFromTextFileImporter(massTagTestList1);
            massTagColl = masstagImporter.Import();

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            var theorFeatureGen = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);

            var chromPeakDet = new ChromPeakDetector(0.5, 1);

            SmartChromPeakSelectorParameters smartchromParam = new SmartChromPeakSelectorParameters();
            var smartChromPeakSelector = new SmartChromPeakSelector(smartchromParam);

            var basicChromPeakSelector = new BasicChromPeakSelector(new ChromPeakSelectorParameters());



            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var iterativeTff = new IterativeTFF(new IterativeTFFParameters());

            var fitscoreCalc = new MassTagFitScoreCalculator();



            string testChromatogramDataFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TargetedWorkflowStandards\massTag635428_chromatogramData.txt";

            XYData xydata = TestUtilities.LoadXYDataFromFile(testChromatogramDataFile);
            Assert.IsNotNull(xydata);

            run.XYData = new XYData();
            run.XYData.Xvalues = xydata.Xvalues;
            run.XYData.Yvalues = xydata.Yvalues;
            // run.XYData.Display();

            run.CurrentMassTag = massTagColl.TargetList.Where(p => p.ID == 635428).First();

            Console.WriteLine("------MT = " + run.CurrentMassTag.ToString());

            theorFeatureGen.Execute(run.ResultCollection);
            chromPeakDet.Execute(run.ResultCollection);

            //first run the standard peak selector



            basicChromPeakSelector.Execute(run.ResultCollection);

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag);
            Assert.AreEqual(10066, (int)Math.Round(result.ChromPeakSelected.XValue));


            //now run the smart chrom peak selector
            run.XYData = new XYData();
            run.XYData.Xvalues = xydata.Xvalues;
            run.XYData.Yvalues = xydata.Yvalues;
            chromPeakDet.Execute(run.ResultCollection);
            smartChromPeakSelector.Parameters.NETTolerance = 0.025f;
            smartChromPeakSelector.Parameters.MaxScansSummedInDynamicSumming = 11;
            smartChromPeakSelector.Parameters.SummingMode = SummingModeEnum.SUMMINGMODE_DYNAMIC;
            smartChromPeakSelector.Parameters.AreaOfPeakToSumInDynamicSumming = 1;

            smartChromPeakSelector.Execute(run.ResultCollection);

            msgen.Execute(run.ResultCollection);
            iterativeTff.Execute(run.ResultCollection);
            fitscoreCalc.Execute(run.ResultCollection);

            Assert.AreEqual(0.032m, (decimal)(Math.Round(result.Score, 4)));
            Assert.AreEqual("9575 {9575, 9582}", result.ScanSet.ToString());

            run.XYData = new XYData();
            run.XYData.Xvalues = xydata.Xvalues;
            run.XYData.Yvalues = xydata.Yvalues;
            chromPeakDet.Execute(run.ResultCollection);
            smartChromPeakSelector.Parameters.NETTolerance = 0.025f;
            smartChromPeakSelector.Parameters.MaxScansSummedInDynamicSumming = 11;
            smartChromPeakSelector.Parameters.SummingMode = SummingModeEnum.SUMMINGMODE_DYNAMIC;
            smartChromPeakSelector.Parameters.AreaOfPeakToSumInDynamicSumming = 2;

            smartChromPeakSelector.Execute(run.ResultCollection);

            msgen.Execute(run.ResultCollection);
            iterativeTff.Execute(run.ResultCollection);
            fitscoreCalc.Execute(run.ResultCollection);

            result.DisplayToConsole();

            Assert.AreEqual(9579, (int)Math.Round(result.ChromPeakSelected.XValue));
            Assert.AreEqual(0.0249m, (decimal)(Math.Round(result.Score,4)));
            Assert.AreEqual("9575 {9568, 9575, 9582, 9589}", result.ScanSet.ToString());

            //TODO: update test - currently peak width is not being properly reported. It is always 4 or 5 scans wide it seems

        }


       
    }
}
