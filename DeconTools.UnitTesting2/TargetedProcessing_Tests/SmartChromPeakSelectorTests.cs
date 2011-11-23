using System;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.NETAlignment;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using NUnit.Framework;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;

namespace DeconTools.UnitTesting2.TargetedProcessing_Tests
{
    [TestFixture]
    public class SmartChromPeakSelectorTests
    {
        private string xcaliburTestfile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
        private string massTagTestList1 = FileRefs.RawDataBasePath + "\\TargetedWorkflowStandards\\QCShew_peptidesWithObsCountGreaterThan1000.txt";
        private TargetCollection massTagColl;
        private XCaliburRun run;

        ChromPeakDetector chromPeakDet;

        ChromPeakSelector basicChromPeakSelector;
        SmartChromPeakSelector smartChromPeakSelector;


        TomTheorFeatureGenerator theorFeatureGen;
        private MSGenerator _msgen;
        private MassTagFitScoreCalculator _fitscoreCalc;


        [SetUp]
        public void initializeTests()
        {
            run = new XCaliburRun(xcaliburTestfile);

            massTagColl = new TargetCollection();

            MassTagFromTextFileImporter masstagImporter = new MassTagFromTextFileImporter(massTagTestList1);
            massTagColl = masstagImporter.Import();

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            theorFeatureGen = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);

            chromPeakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 1);

            SmartChromPeakSelectorParameters smartchromParam = new SmartChromPeakSelectorParameters();
            smartChromPeakSelector = new SmartChromPeakSelector(smartchromParam);

            basicChromPeakSelector = new ChromPeakSelector(1);


           
            _msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            
            _tff = new IterativeTFF(new IterativeTFFParameters());

            _fitscoreCalc = new MassTagFitScoreCalculator();

        }



        [Test]
        public void smartChromPeakSelectorTest_noSumming()
        {
            string testChromatogramDataFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TargetedWorkflowStandards\massTag635428_chromatogramData.txt";

            XYData xydata = TestUtilities.LoadXYDataFromFile(testChromatogramDataFile);
            Assert.IsNotNull(xydata);

            run.XYData = xydata;
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
            run.XYData = xydata;
            chromPeakDet.Execute(run.ResultCollection);
            smartChromPeakSelector.Parameters.NETTolerance = 0.025f;
            smartChromPeakSelector.Execute(run.ResultCollection);

            _msgen.Execute(run.ResultCollection);
            _tff.Execute(run.ResultCollection);
            _fitscoreCalc.Execute(run.ResultCollection);

        
            Assert.AreEqual(9579, (int)Math.Round(result.ChromPeakSelected.XValue));
            result.DisplayToConsole();

        }


        [Test]
        public void smartChromPeakSelectorTest_withDynamicSumming()
        {
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

            _msgen.Execute(run.ResultCollection);
            _tff.Execute(run.ResultCollection);
            _fitscoreCalc.Execute(run.ResultCollection);

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

            _msgen.Execute(run.ResultCollection);
            _tff.Execute(run.ResultCollection);
            _fitscoreCalc.Execute(run.ResultCollection);

            result.DisplayToConsole();

            Assert.AreEqual(9579, (int)Math.Round(result.ChromPeakSelected.XValue));
            Assert.AreEqual(0.0249m, (decimal)(Math.Round(result.Score,4)));
            Assert.AreEqual("9575 {9568, 9575, 9582, 9589}", result.ScanSet.ToString());

        }


        public IterativeTFF _tff { get; set; }
    }
}
