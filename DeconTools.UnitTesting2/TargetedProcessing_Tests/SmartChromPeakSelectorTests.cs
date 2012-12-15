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
            smartChromPeakSelector.Parameters.MaxScansSummedInDynamicSumming = 51;
            smartChromPeakSelector.Parameters.SummingMode = SummingModeEnum.SUMMINGMODE_DYNAMIC;
            smartChromPeakSelector.Parameters.AreaOfPeakToSumInDynamicSumming = 1;

            smartChromPeakSelector.Execute(run.ResultCollection);

            msgen.Execute(run.ResultCollection);
            iterativeTff.Execute(run.ResultCollection);
            fitscoreCalc.Execute(run.ResultCollection);


            Console.WriteLine(result.ScanSet);
            Assert.AreEqual(14, result.ScanSet.IndexValues.Count);
            Assert.AreEqual(0.0104m, (decimal)Math.Round(result.Score, 4));
            Assert.AreEqual("9575 {9534, 9540, 9547, 9554, 9561, 9568, 9575, 9582, 9589, 9596, 9603, 9610, 9617, 9624}", result.ScanSet.ToString());

            run.XYData = new XYData();
            run.XYData.Xvalues = xydata.Xvalues;
            run.XYData.Yvalues = xydata.Yvalues;
            chromPeakDet.Execute(run.ResultCollection);
            smartChromPeakSelector.Parameters.NETTolerance = 0.025f;
            smartChromPeakSelector.Parameters.MaxScansSummedInDynamicSumming = 51;
            smartChromPeakSelector.Parameters.SummingMode = SummingModeEnum.SUMMINGMODE_DYNAMIC;
            smartChromPeakSelector.Parameters.AreaOfPeakToSumInDynamicSumming = 2;

            smartChromPeakSelector.Execute(run.ResultCollection);

            msgen.Execute(run.ResultCollection);
            iterativeTff.Execute(run.ResultCollection);
            fitscoreCalc.Execute(run.ResultCollection);

            result.DisplayToConsole();

            Assert.AreEqual(9579, (int)Math.Round(result.ChromPeakSelected.XValue));
            Assert.AreEqual(0.0251m, (decimal)(Math.Round(result.Score,4)));
            //Console.WriteLine(result.ScanSet);
            Assert.AreEqual(26, result.ScanSet.IndexValues.Count);
            Assert.AreEqual("9575 {9493, 9500, 9506, 9513, 9520, 9527, 9534, 9540, 9547, 9554, 9561, 9568, 9575, 9582, 9589, 9596, 9603, 9610, 9617, 9624, 9631, 9638, 9645, 9652, 9658, 9665}", result.ScanSet.ToString());

           
        }


       
    }
}
