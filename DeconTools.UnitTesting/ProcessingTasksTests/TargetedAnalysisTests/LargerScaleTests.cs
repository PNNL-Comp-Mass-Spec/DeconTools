using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.NETAlignment;
using DeconTools.Backend.ProcessingTasks.ResultExporters.MassTagResultExporters;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting.ProcessingTasksTests.TargetedAnalysisTests
{
    [TestFixture]
    public class LargerScaleTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private string cysteineAnalysisOutput1 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_cysteine_isos.db3";
        private string methionineAnalysisOutput1 = @"D:\My Documents\PNNL\My_DataAnalysis\2010\2010_01_TargetedDataProcessing\Shewenella_MT_PMT2_Cleav2_MetContaining_results.db3";
        private string xcaliburAllPeaksFile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_peaks.txt";

        private string cysteineMassTagSourceFile1 = "..\\..\\TestFiles\\MT_Shewanella_ProdTest_P352_massTags_LessThan2000Da_ContainingCys.txt";
        private string methioninMassTagSourceFile1 = @"D:\My Documents\PNNL\My_DataAnalysis\2010\2010_01_TargetedDataProcessing\Shewenella_MT_PMT2_Cleav2_MetContaining.txt";


        private string smartMTSourceFile1 = @"D:\My Documents\PNNL\My_DataAnalysis\2010\2010_01_TargetedDataProcessing\SMART_output_greaterThan_90.txt";
        private string smartMethionineMatches = @"D:\My Documents\PNNL\My_DataAnalysis\2010\2010_01_TargetedDataProcessing\SMART_output_greaterThan_90_methionine.txt";

        private string targetedMethionineMatches = @"D:\My Documents\PNNL\My_DataAnalysis\2010\2010_01_TargetedDataProcessing\Shewenella_MT_PMT2_Cleav2_MetContaining_Matches.txt";


        [Test]
        public void test1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);
            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(cysteineMassTagSourceFile1, '\t');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.Import(massTagColl);

            Assert.AreEqual(737, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(826, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 0.5);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(1,0.01, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 0.75, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFF(6);
            Task exporter = new BasicMTResultSQLiteExporter(cysteineAnalysisOutput1);


            IsotopicProfileFitScoreCalculator fitScoreCalc = new IsotopicProfileFitScoreCalculator();

            int successCounter = 0;
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
                    targetedFeatureFinder.Execute(run.ResultCollection);
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

                if (mt == massTagColl.MassTagList.Last())
                {
                    exporter.Execute(run.ResultCollection);
                }


            }

            exporter.Cleanup();

            //List<IMassTagResult> successfulResults = run.ResultCollection.GetSuccessfulMassTagResults();

            foreach (long tr in timingResults)
            {
                Console.WriteLine(tr);

            }
            Console.WriteLine("-------- Analysis time for all MTs = " + timingResults.Sum());
            Console.WriteLine("-------- Average time for each MT = " + timingResults.Average());

            Console.WriteLine();

        }


        [Test]
        public void test2()
        {
            Run run = new XCaliburRun(xcaliburTestfile);
            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(methioninMassTagSourceFile1, '\t');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.Import(massTagColl);

            Assert.AreEqual(6256, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(8315, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 0.5);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(1,0.01, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 0.75, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFF(6);
            Task exporter = new BasicMTResultSQLiteExporter(methionineAnalysisOutput1);


            IsotopicProfileFitScoreCalculator fitScoreCalc = new IsotopicProfileFitScoreCalculator();

            int successCounter = 0;
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
                    targetedFeatureFinder.Execute(run.ResultCollection);
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

                if (mt == massTagColl.MassTagList.Last())
                {
                    exporter.Execute(run.ResultCollection);
                }


            }

            exporter.Cleanup();

            //List<IMassTagResult> successfulResults = run.ResultCollection.GetSuccessfulMassTagResults();

            foreach (long tr in timingResults)
            {
                Console.WriteLine(tr);

            }
            Console.WriteLine("-------- Analysis time for all MTs = " + timingResults.Sum());
            Console.WriteLine("-------- Average time for each MT = " + timingResults.Average());

            Console.WriteLine();

        }


        [Test]
        public void tester()
        {
            Run run = new XCaliburRun(xcaliburTestfile);
            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(smartMethionineMatches, '\t');
            mtidImporter.Import(massTagColl);
            Console.WriteLine("SMART matches = " + massTagColl.MassTagIDList.Count());

            MassTagCollection matchedMassTag = new MassTagCollection();
            MassTagIDGenericImporter matchImporter = new MassTagIDGenericImporter(targetedMethionineMatches, '\t');
            matchImporter.Import(matchedMassTag);
            Console.WriteLine("Targeted matches = " + matchedMassTag.MassTagIDList.Distinct().Count());


            //List<long> common_massTags = massTagColl.MassTagIDList.Intersect(matchedMassTag.MassTagIDList).ToList();
            List<long> common_massTags = matchedMassTag.MassTagIDList.Intersect(massTagColl.MassTagIDList).ToList();

            List<long> targetedUniqueTags = matchedMassTag.MassTagIDList.Except(massTagColl.MassTagIDList).ToList();

            List<long> smartUniqueTags = massTagColl.MassTagIDList.Except(matchedMassTag.MassTagIDList).ToList();


            string listOfMTsForSMARTUnique = getStringListOfMassTags(smartUniqueTags);

            Console.WriteLine("Common matches = " + common_massTags.Count);
            Console.WriteLine("Unique to targeted = " + targetedUniqueTags.Count);
            Console.WriteLine("Unique to SMART = " + smartUniqueTags.Count);

            Console.WriteLine();
            Console.WriteLine(listOfMTsForSMARTUnique);

        }

        private string getStringListOfMassTags(List<long> mtList)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in mtList)
            {
                sb.Append(item);

                if (item == mtList.Last())
                {
                    
                }
                else
                {
                    sb.Append(", ");
                }

                
            }
            return sb.ToString();
        }


        [Test]
        public void test3()
        {
            Run run = new XCaliburRun(xcaliburTestfile);
            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(smartMethionineMatches, '\t');
            mtidImporter.Import(massTagColl);


            MassTagCollection matchedMassTag = new MassTagCollection();
            MassTagIDGenericImporter matchImporter = new MassTagIDGenericImporter(targetedMethionineMatches, '\t');


            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.Import(massTagColl);

            Assert.AreEqual(6256, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(8315, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 0.5);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(1,0.01, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 0.75, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFF(6);
            Task exporter = new BasicMTResultSQLiteExporter(cysteineAnalysisOutput1);


            IsotopicProfileFitScoreCalculator fitScoreCalc = new IsotopicProfileFitScoreCalculator();

            int successCounter = 0;
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
                    targetedFeatureFinder.Execute(run.ResultCollection);
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

                if (mt == massTagColl.MassTagList.Last())
                {
                    exporter.Execute(run.ResultCollection);
                }


            }

            exporter.Cleanup();

            //List<IMassTagResult> successfulResults = run.ResultCollection.GetSuccessfulMassTagResults();

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
