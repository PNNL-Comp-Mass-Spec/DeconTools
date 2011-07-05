using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data.Importers;
using DeconTools.Backend.ProcessingTasks.NETAlignment;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.FileIO;


namespace DeconTools.UnitTesting2.TargetedProcessing_Tests
{
    [TestFixture]
    public class TargetedProcessingOrbiDataTests
    {
        private string xcaliburTestfile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
        private string massTagTestList1 = FileRefs.RawDataBasePath + "\\TargetedWorkflowStandards\\QCShew_peptidesWithObsCountGreaterThan1000.txt";
        private string xcaliburAllPeaksFile = FileRefs.PeakDataFiles.OrbitrapPeakFile1;
        private XCaliburRun run;
        private MassTagCollection massTagColl;


        [SetUp]
        public void initialize_loadPeaks_and_massTags()
        {
            run = new XCaliburRun(xcaliburTestfile);

            massTagColl = new MassTagCollection();

            MassTagFromTextFileImporter masstagImporter = new MassTagFromTextFileImporter(massTagTestList1);
            massTagColl = masstagImporter.Import();

            Assert.AreEqual(2719, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);
        }



        [Test]
        public void find_targetMassTag_131959Test1()
        {
            //int mtID = 635428;
            int mtID = 131959;

            Task peakChromGen = new PeakChromatogramGenerator(20);
            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task zeroFill = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 1);
            Task msPeakDet = new DeconTools.Backend.ProcessingTasks.DeconToolsPeakDetector(1.3, 2, Globals.PeakFitType.QUADRATIC, true);
    
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(1, 0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);

            run.CurrentMassTag = massTagColl.MassTagList.Find(p => p.ID == mtID);
            MassTag mt = run.CurrentMassTag;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            Task theorFeatureGen = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);
            Task targetedFeatureFinder = new BasicTFF();
            Task fitScoreCalc = new MassTagFitScoreCalculator();


            theorFeatureGen.Execute(run.ResultCollection);


            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
            Console.WriteLine("monoMass = " + mt.MonoIsotopicMass.ToString("0.0000") + "; monoMZ = " + mt.MZ.ToString("0.0000") + "; ChargeState = " + mt.ChargeState + "; NET = " + mt.NETVal.ToString("0.000") + "; Sequence = " + mt.PeptideSequence + "\n");

            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            //TestUtilities.DisplayXYValues(run.ResultCollection);

            peakDet.Execute(run.ResultCollection);
            TestUtilities.DisplayPeaks(run.PeakList);

            chromPeakSel.Execute(run.ResultCollection);
            msgen.Execute(run.ResultCollection);
            //TestUtilities.DisplayXYValues(run.ResultCollection);
            msPeakDet.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            fitScoreCalc.Execute(run.ResultCollection);

            MassTagResultBase massTagResult = run.ResultCollection.MassTagResultList[mt];
            massTagResult.DisplayToConsole();



            //Console.WriteLine("------------------------------ end --------------------------");





        }

        [Test]
        public void find_targetMassTag_635428_wrong_ChromPeakSelected_Test1()
        {
            int mtID = 635428;
            //int mtID = 131959;

            Task peakChromGen = new PeakChromatogramGenerator(20);
            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task zeroFill = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 1);
            Task msPeakDet = new DeconTools.Backend.ProcessingTasks.DeconToolsPeakDetector(1.3, 2, Globals.PeakFitType.QUADRATIC, true);

            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(1, 0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);

            run.CurrentMassTag = massTagColl.MassTagList.Find(p => p.ID == mtID);
            MassTag mt = run.CurrentMassTag;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            Task theorFeatureGen = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);
            Task targetedFeatureFinder = new BasicTFF();
            Task fitScoreCalc = new MassTagFitScoreCalculator();


            theorFeatureGen.Execute(run.ResultCollection);


            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
            Console.WriteLine("monoMass = " + mt.MonoIsotopicMass.ToString("0.0000") + "; monoMZ = " + mt.MZ.ToString("0.0000") + "; ChargeState = " + mt.ChargeState + "; NET = " + mt.NETVal.ToString("0.000") + "; Sequence = " + mt.PeptideSequence + "\n");

            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);

            peakDet.Execute(run.ResultCollection);
            //TestUtilities.DisplayPeaks(run.PeakList);

            chromPeakSel.Execute(run.ResultCollection);
            msgen.Execute(run.ResultCollection);
            //TestUtilities.DisplayXYValues(run.ResultCollection);
            msPeakDet.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            fitScoreCalc.Execute(run.ResultCollection);

            MassTagResultBase massTagResult = run.ResultCollection.MassTagResultList[mt];
            //massTagResult.DisplayToConsole();



            //Console.WriteLine("------------------------------ end --------------------------");





        }

    }
}
