using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Quantifiers;

namespace DeconTools.UnitTesting.ProcessingTasksTests.TargetedAnalysisTests
{
    [TestFixture]
    public class N14N15TargetedTests
    {

        [Test]
        public void find_unlabelledAndUnlabelledIsotopicProfilesTest1()
        {
            Run run = new BrukerRun(@"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_28_run1_26Oct07_Andromeda_07-09-02\acqus");

            MSGeneratorFactory fact = new MSGeneratorFactory();
            Task msgen=  fact.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.PeakBackgroundRatio = 4;
            peakDet.SigNoiseThreshold = 3;
            peakDet.IsDataThresholded = false;


            Task theorFeature = new DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator.TomTheorFeatureGenerator();


            List<MassTag> massTags = TestUtilities.CreateN14N15TestMassTagList();
            run.CurrentMassTag = massTags[0];
            run.CurrentScanSet = new ScanSet(1210);
            run.ResultCollection.MassTagResultType = DeconTools.Backend.Globals.MassTagResultType.N14N15_MASSTAG_RESULT;


            msgen.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);

            theorFeature.Execute(run.ResultCollection);

            N14N15FeatureFinder finder = new N14N15FeatureFinder(0.02);
            finder.Execute(run.ResultCollection);

            //TestUtilities.DisplayIsotopicProfileData(finder.LabeledTheorProfile);

            N14N15_TResult massTagResult =(N14N15_TResult)run.ResultCollection.GetMassTagResult(run.CurrentMassTag);

            TestUtilities.DisplayIsotopicProfileData(massTagResult.IsotopicProfile);
            Console.WriteLine("------------------");
            TestUtilities.DisplayIsotopicProfileData(massTagResult.N15IsotopicProfile);



        }


        [Test]
        public void quantifyN14N15RatioTest1()
        {
            Run run = new BrukerRun(@"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_28_run1_26Oct07_Andromeda_07-09-02\acqus");

            MSGeneratorFactory fact = new MSGeneratorFactory();
            Task msgen = fact.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.PeakBackgroundRatio = 4;
            peakDet.SigNoiseThreshold = 3;
            peakDet.IsDataThresholded = false;

            Task theorFeature = new DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator.TomTheorFeatureGenerator();
            List<MassTag> massTags = TestUtilities.CreateN14N15TestMassTagList();
            run.CurrentMassTag = massTags[0];
            run.CurrentScanSet = new ScanSet(1210);
            run.ResultCollection.MassTagResultType = DeconTools.Backend.Globals.MassTagResultType.N14N15_MASSTAG_RESULT;


            msgen.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);

            theorFeature.Execute(run.ResultCollection);

            N14N15FeatureFinder finder = new N14N15FeatureFinder(0.02);
            finder.Execute(run.ResultCollection);


            N14N15QuantifierTask quant = new N14N15QuantifierTask();
            quant.Execute(run.ResultCollection);


            N14N15_TResult massTagResult = (N14N15_TResult)run.ResultCollection.GetMassTagResult(run.CurrentMassTag);

            TestUtilities.DisplayIsotopicProfileData(massTagResult.IsotopicProfile);
            Console.WriteLine("------------------");
            TestUtilities.DisplayIsotopicProfileData(massTagResult.N15IsotopicProfile);
            Console.WriteLine("------------------");
            Console.WriteLine("Ratio N14/N15 = " + massTagResult.RatioN14N15);



        }

    }
}
