using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Data.Importers;
using DeconTools.Backend;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.ProcessingTasks.NETAlignment;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.UnitTesting2.TargetedProcessing_Tests
{
    [TestFixture]
    public class SmartChromPeakSelectorTests
    {
        private string xcaliburTestfile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
        private string massTagTestList1 = FileRefs.RawDataBasePath + "\\TargetedWorkflowStandards\\QCShew_peptidesWithObsCountGreaterThan1000.txt";
        private MassTagCollection massTagColl;
        private XCaliburRun run;

        ChromPeakDetector chromPeakDet;

        ChromPeakSelector basicChromPeakSelector;
        SmartChromPeakSelector smartChromPeakSelector;


        TomTheorFeatureGenerator theorFeatureGen;


        [SetUp]
        public void initializeTests()
        {
            run = new XCaliburRun(xcaliburTestfile);

            massTagColl = new MassTagCollection();

            MassTagFromTextFileImporter masstagImporter = new MassTagFromTextFileImporter(massTagTestList1);
            massTagColl = masstagImporter.Import();

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            theorFeatureGen = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);

            chromPeakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 1);
            smartChromPeakSelector = new SmartChromPeakSelector();

            basicChromPeakSelector = new ChromPeakSelector(1);
        }



        [Test]
        public void smartChromPeakSelectorTest1()
        {
            string testChromatogramDataFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TargetedWorkflowStandards\massTag635428_chromatogramData.txt";

            XYData xydata = TestUtilities.LoadXYDataFromFile(testChromatogramDataFile);
            Assert.IsNotNull(xydata);

            run.XYData = xydata;
           // run.XYData.Display();

            run.CurrentMassTag = massTagColl.MassTagList.Where(p => p.ID == 635428).First();

            Console.WriteLine("------MT = " + run.CurrentMassTag.ToString());

            theorFeatureGen.Execute(run.ResultCollection);
            chromPeakDet.Execute(run.ResultCollection);

            basicChromPeakSelector.Execute(run.ResultCollection);

            var result = run.ResultCollection.GetMassTagResult(run.CurrentMassTag);
            Assert.AreEqual(10066, (int)Math.Round(result.ChromPeakSelected.XValue));

            run.XYData = xydata;
            chromPeakDet.Execute(run.ResultCollection);
            smartChromPeakSelector.NETTolerance = 0.025f;
            smartChromPeakSelector.Execute(run.ResultCollection);
        
            Assert.AreEqual(9579, (int)Math.Round(result.ChromPeakSelected.XValue));

            result.DisplayToConsole();
        }

    }
}
