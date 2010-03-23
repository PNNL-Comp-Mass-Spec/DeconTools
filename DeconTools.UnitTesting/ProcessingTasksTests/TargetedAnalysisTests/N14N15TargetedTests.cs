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
using DeconTools.Backend.Data.Importers;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks.NETAlignment;
using DeconTools.Backend;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using System.Diagnostics;

namespace DeconTools.UnitTesting.ProcessingTasksTests.TargetedAnalysisTests
{
    [TestFixture]
    public class N14N15TargetedTests
    {

        string rsph_AOnly_28_run1File = @"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_28_run1_26Oct07_Andromeda_07-09-02\acqus";

        string rsph_Aonly_28_run1_scans500_1000_peaks = @"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_28_run1_26Oct07_Andromeda_07-09-02\RSPH_Aonly_28_run1_26Oct07_Andromeda_07-09-02_scans500-1000_peaks.txt";

        List<long> fourTestMTs = new List<long>{ 23091034, 23091326, 23095273, 2320533 };


        [Test]
        public void find_unlabelledAndUnlabelledIsotopicProfilesTest1()
        {
            Run run = new BrukerRun(rsph_AOnly_28_run1File);

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


        [Test]
        public void quantifyN14N15RatioTest2()
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


        [Test]
        public void fourMassTags_fullAnalysisTest1()
        {
            Run run = new BrukerRun(rsph_AOnly_28_run1File);
            MassTagCollection massTagColl = new MassTagCollection();

            massTagColl.MassTagIDList = fourTestMTs;

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_R_sphaeroides241_P513", "Albert");
            importer.chargeStateFilterThreshold = 0.05;
            importer.Import(massTagColl);

            Assert.AreEqual(7, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(rsph_Aonly_28_run1_scans500_1000_peaks);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 0.5);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.01, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 0.75, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder(6);


            N14N15FeatureFinder finder = new N14N15FeatureFinder(0.02);
 


            N14N15QuantifierTask quant = new N14N15QuantifierTask();


            MassTagFitScoreCalculator fitScoreCalc = new MassTagFitScoreCalculator();

            List<long> timingResults = new List<long>();


            foreach (MassTag mt in massTagColl.MassTagList)
            {
                run.CurrentMassTag = mt;
                mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
                Console.WriteLine("monoMass = " + mt.MonoIsotopicMass.ToString("0.0000") + "; monoMZ = " + mt.MZ.ToString("0.0000") + "; ChargeState = " + mt.ChargeState + "; NET = " + mt.NETVal.ToString("0.000") + "; Sequence = " + mt.PeptideSequence + "\n");


                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    peakChromGen.Execute(run.ResultCollection);
                    smoother.Execute(run.ResultCollection);
                    peakDet.Execute(run.ResultCollection);
                    chromPeakSel.Execute(run.ResultCollection);
                    msgen.Execute(run.ResultCollection);
                    mspeakDet.Execute(run.ResultCollection);
                    theorFeatureGen.Execute(run.ResultCollection);
                    finder.Execute(run.ResultCollection);
                    quant.Execute(run.ResultCollection); 
                    fitScoreCalc.Execute(run.ResultCollection);
                    MassTagResultBase massTagResult = run.ResultCollection.MassTagResultList[mt];
                    massTagResult.DisplayToConsole();


                    Console.WriteLine("------------------------------ end --------------------------");
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Task failed. Message: " + ex.Message + ex.StackTrace);
                }
                sw.Stop();
                timingResults.Add(sw.ElapsedMilliseconds);

          

            }

            foreach (long tr in timingResults)
            {
                Console.WriteLine(tr);

            }
            Console.WriteLine("-------- Analysis time for all MTs = " + timingResults.Sum());
            Console.WriteLine("-------- Average time for each MT = " + timingResults.Average());

            Console.WriteLine();
       

        }


    }
}
