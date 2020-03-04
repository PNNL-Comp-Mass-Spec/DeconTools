using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSFeatureFinderTests
{
    [TestFixture]
    public class ThrashV2Tests
    {

        [Category("MustPass")]
        [Test]
        public void ThrashV2OnOrbitrapTest1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            run.ScanSetCollection.Create(run, 6005, 6005, 1, 1, false);

            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, Globals.PeakFitType.QUADRATIC, true) {
                IsDataThresholded = true
            };

            var parameters = new ThrashParameters {
                MinMSFeatureToBackgroundRatio = 1,
                MaxFit = 0.3
            };

            var deconvolutor = new ThrashDeconvolutorV2(parameters);

            var scan = new ScanSet(6005);

            run.CurrentScanSet = scan;

            // Populate run.ResultCollection.Run.XYData
            generator.Execute(run.ResultCollection);

            // Find peaks, populating run.ResultCollection.Run.PeakList
            peakDetector.Execute(run.ResultCollection);

            // Find isotopic distributions
            deconvolutor.Execute(run.ResultCollection);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            Assert.IsTrue(run.ResultCollection.ResultList.Count > 0);
            //Assert.AreEqual(187, run.ResultCollection.ResultList.Count);

            var result1 = run.ResultCollection.ResultList[0];
            Assert.AreEqual(13084442, (decimal)Math.Round(result1.IntensityAggregate));
            Assert.AreEqual(960.53365m, (decimal)Math.Round(result1.IsotopicProfile.MonoIsotopicMass, 5));
            Assert.AreEqual(2, result1.IsotopicProfile.ChargeState);
        }

        [Test]
        public void ThrashV2OnIMSDataTest1()
        {
            var uimfFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS2_90_6Apr11_Cheetah_11-02-19.uimf";

            var run = new RunFactory().CreateRun(uimfFile);
            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, Globals.PeakFitType.QUADRATIC, true)
            {
                PeaksAreStored = true
            };

            var zeroFiller = new Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);

            var thrashParameters = new ThrashParameters {
                MinMSFeatureToBackgroundRatio = 1,
                MaxFit = 0.4
            };

            var newDeconvolutor = new ThrashDeconvolutorV2(thrashParameters);

            var testLCScan = 500;
            var testIMSScan = 120;
            var numIMSScanSummed = 7;
            var lowerIMSScan = testIMSScan - (numIMSScanSummed - 1) / 2;
            var upperIMSScan = testIMSScan + (numIMSScanSummed - 1) / 2;

            var scanSet = new ScanSetFactory().CreateScanSet(run, testLCScan, 1);

            run.CurrentScanSet = scanSet;
            ((UIMFRun)run).CurrentIMSScanSet = new IMSScanSet(testIMSScan, lowerIMSScan, upperIMSScan);

            // Populate run.ResultCollection.Run.XYData
            generator.Execute(run.ResultCollection);

            // Zero fill run.ResultCollection.Run.XYData
            zeroFiller.Execute(run.ResultCollection);

            // Find peaks, populating run.ResultCollection.Run.PeakList
            peakDetector.Execute(run.ResultCollection);

            // Find isotopic distributions
            newDeconvolutor.Execute(run.ResultCollection);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            Assert.IsTrue(run.ResultCollection.ResultList.Count > 0);

            // Previously 33, now 30
            Assert.AreEqual(30, run.ResultCollection.ResultList.Count);

            var result1 = run.ResultCollection.ResultList[0];

            // Previously 13084442, now 573290m
            Assert.AreEqual(573290m, (decimal)Math.Round(result1.IntensityAggregate));

            // Previously 960.53365m, now 1160.55077m
            Assert.AreEqual(1160.55077m, (decimal)Math.Round(result1.IsotopicProfile.MonoIsotopicMass, 5));

            Assert.AreEqual(2, result1.IsotopicProfile.ChargeState);

        }

        [Test]
        public void ThrashPreferPlusOneChargeStateTest1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            run.ScanSetCollection.Create(run, 6005, 6005, 1, 1, false);

            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(0.5, 2, Globals.PeakFitType.QUADRATIC, true) {
                IsDataThresholded = true,
                PeaksAreStored = true
            };

            var parameters = new ThrashParameters {
                MinMSFeatureToBackgroundRatio = 1,
                MaxFit = 0.3,
                CheckAllPatternsAgainstChargeState1 = true
            };

            var deconvolutor = new ThrashDeconvolutorV2(parameters);

            var scan = new ScanSet(6005);

            run.CurrentScanSet = scan;

            // Populate run.ResultCollection.Run.XYData
            generator.Execute(run.ResultCollection);

            // Find peaks, populating run.ResultCollection.Run.PeakList
            peakDetector.Execute(run.ResultCollection);

            run.PeakList = run.PeakList.Where(p => p.XValue > 750 && p.XValue < 753).ToList();

            // Find isotopic distributions
            deconvolutor.Execute(run.ResultCollection);

            Assert.IsTrue(run.ResultCollection.ResultList.Count > 0);
            var result1 = run.ResultCollection.ResultList.First();
            Assert.AreEqual(1, result1.IsotopicProfile.ChargeState);

            Console.WriteLine("--------- Prefer +1 charge state ----------------");
            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            run.ResultCollection.ResultList.Clear();
            run.ResultCollection.IsosResultBin.Clear();

            deconvolutor.Parameters.CheckAllPatternsAgainstChargeState1 = false;

            // Find isotopic distributions
            deconvolutor.Execute(run.ResultCollection);

            Console.WriteLine("\n--------- No charge state bias ----------------");
            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            Assert.IsTrue(run.ResultCollection.ResultList.Count > 0);
            result1 = run.ResultCollection.ResultList.First();

            Assert.AreEqual(3, result1.IsotopicProfile.ChargeState);
        }

        [Ignore("For testing only")]
        [Test]
        public void OldDeconvolutorOrbitrapTest1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            run.ScanSetCollection.Create(run, 6005, 6200, 1, 1, false);

            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, Globals.PeakFitType.QUADRATIC, true);

            var deconvolutor = new HornDeconvolutor {
                MinPeptideBackgroundRatio = 3
            };

            //deconvolutor.IsMZRangeUsed = true;
            //deconvolutor.MinMZ = 575;
            //deconvolutor.MaxMZ = 585;


            foreach (var scanSet in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanSet;

                // Populate run.ResultCollection.Run.XYData
                generator.Execute(run.ResultCollection);

                // Find peaks, populating run.ResultCollection.Run.PeakList
                peakDetector.Execute(run.ResultCollection);

                run.CurrentScanSet.BackgroundIntensity = peakDetector.BackgroundIntensity;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                deconvolutor.Execute(run.ResultCollection);
                stopwatch.Stop();

                Console.WriteLine("Time for decon= \t" + stopwatch.ElapsedMilliseconds);

            }
            //Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            ////order and get the most intense ms feature
            //run.ResultCollection.ResultList = run.ResultCollection.ResultList.OrderByDescending(p => p.IsotopicProfile.IntensityAggregate).ToList();
            //IsosResult testIso = run.ResultCollection.ResultList[0];
            //Assert.AreEqual(13084442, testIso.IsotopicProfile.IntensityAggregate);
            //Assert.AreEqual(2, testIso.IsotopicProfile.ChargeState);
            //Assert.AreEqual(0.01012m, (decimal)Math.Round(testIso.IsotopicProfile.Score, 5));
            //Assert.AreEqual(3, testIso.IsotopicProfile.PeakList.Count);
            //Assert.AreEqual(481.274105402604m, (decimal)testIso.IsotopicProfile.PeakList[0].XValue);
            //Assert.AreEqual(481.775412188198m, (decimal)testIso.IsotopicProfile.PeakList[1].XValue);
            //Assert.AreEqual(482.276820274024m, (decimal)testIso.IsotopicProfile.PeakList[2].XValue);

            //TestUtilities.DisplayIsotopicProfileData(testIso.IsotopicProfile);

            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
            //TestUtilities.DisplayPeaks(run.PeakList);

        }

        //[Ignore("For testing only")]
        [Test]
        public void CompareOldAndNewDeconvolutorsOrbitrap()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            //run.ScanSetCollection.Create(run, 6005, 6005, 1, 1, false);

            //to sum
            run.ScanSetCollection.Create(run, 6005, 6005, 5, 1, false);

            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, Globals.PeakFitType.QUADRATIC, true)
            {
                PeaksAreStored = true
            };

            var thrashParameters = new ThrashParameters {
                MinMSFeatureToBackgroundRatio = 3,
                MaxFit = 0.4
            };

            var newDeconvolutor = new ThrashDeconvolutorV2(thrashParameters);

            var scanSet = new ScanSet(6005);

            //For summing mass spectra:
            //scanSet = new ScanSetFactory().CreateScanSet(run, 6005, 5);

            run.CurrentScanSet = scanSet;

            var oldDeconvolutor = new HornDeconvolutor {
                MinPeptideBackgroundRatio = 3, MaxFitAllowed = 0.4
            };

            // Populate run.ResultCollection.Run.XYData
            generator.Execute(run.ResultCollection);

            // Find peaks, populating run.ResultCollection.Run.PeakList
            peakDetector.Execute(run.ResultCollection);

            //run.PeakList = run.PeakList.Where(p => p.XValue > 634 && p.XValue < 642).ToList();
            //run.DeconToolsPeakList = run.DeconToolsPeakList.Where(p => p.mz > 634 && p.mz < 642).ToArray();

            run.CurrentScanSet.BackgroundIntensity = peakDetector.BackgroundIntensity;

            // Find isotopic distributions
            newDeconvolutor.Execute(run.ResultCollection);

            //Console.WriteLine("\n--------------New decon ------------------");
            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            var newResults = new List<IsosResult>(run.ResultCollection.ResultList);

            //TestUtilities.DisplayMSFeatures(newResults);
            //return;

            // DisplayPPMErrorsForeachPeakOfMSFeature(newResults);

            run.ResultCollection.ResultList.Clear();
            run.ResultCollection.IsosResultBin.Clear();

            // Find isotopic distributions
            oldDeconvolutor.Execute(run.ResultCollection);

            var oldResults = new List<IsosResult>(run.ResultCollection.ResultList);

            //Console.WriteLine("\n--------------Old decon ------------------");
            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            var sharedIsos = new List<IsosResult>();
            var uniqueToNew = new List<IsosResult>();
            var uniqueToOld = new List<IsosResult>();

            GetComparisons(newResults, oldResults, sharedIsos, uniqueToNew, uniqueToOld);

            Console.WriteLine("\n--------------Common to new and Old ------------------");
            TestUtilities.DisplayMSFeatures(sharedIsos);


            Console.WriteLine("\n--------------Unique to new ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToNew);

            var outputDirectory = new DirectoryInfo(@"C:\Temp\ThrashTesting");
            if (!outputDirectory.Exists)
                outputDirectory.Create();

            var outputFilename = Path.Combine(outputDirectory.FullName,  "exportedIsos.csv");
            var exporter = IsosExporterFactory.CreateIsosExporter(run.ResultCollection.ResultType, Globals.ExporterType.Text, outputFilename);

            exporter.ExportIsosResults(uniqueToNew);


            Console.WriteLine("\n--------------Unique to old ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToOld);

        }

        private void GetComparisons(
            IReadOnlyCollection<IsosResult> allNewResults,
            IReadOnlyCollection<IsosResult> allOldResults,
            ICollection<IsosResult> sharedIsos,
            ICollection<IsosResult> uniqueToNew,
            ICollection<IsosResult> uniqueToOld)
        {

            var scans = allNewResults.Select(p => p.ScanSet.PrimaryScanNumber).Distinct().OrderBy(p => p).ToList();

            foreach (var scan in scans)
            {
                var newResults = allNewResults.Where(p => p.ScanSet.PrimaryScanNumber == scan).ToList();
                var oldResults = allOldResults.Where(p => p.ScanSet.PrimaryScanNumber == scan).ToList();


                foreach (var newResult in newResults)
                {
                    var foundMatch = false;
                    foreach (var oldResult in oldResults)
                    {
                        if (Math.Abs(newResult.IsotopicProfile.MonoIsotopicMass - oldResult.IsotopicProfile.MonoIsotopicMass) < 0.01 &&
                            newResult.IsotopicProfile.ChargeState == oldResult.IsotopicProfile.ChargeState)
                        {
                            foundMatch = true;
                        }
                    }

                    if (foundMatch)
                    {
                        sharedIsos.Add(newResult);
                    }
                    else
                    {
                        uniqueToNew.Add(newResult);
                    }
                }

                foreach (var oldResult in oldResults)
                {
                    var foundMatch = false;
                    foreach (var newResult in newResults)
                    {
                        if (Math.Abs(newResult.IsotopicProfile.MonoIsotopicMass - oldResult.IsotopicProfile.MonoIsotopicMass) < 0.01 &&
                            newResult.IsotopicProfile.ChargeState == oldResult.IsotopicProfile.ChargeState)
                        {
                            foundMatch = true;
                        }
                    }

                    if (!foundMatch)
                    {
                        uniqueToOld.Add(oldResult);
                    }
                }

            }

        }

        [Test]
        public void CompareOldAndNewDeconvolutorsUIMF()
        {
            var uimfFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS2_90_6Apr11_Cheetah_11-02-19.uimf";

            var run = new RunFactory().CreateRun(uimfFile);

            var testLCScan = 500;
            var testIMSScan = 120;

            var numIMSScanSummed = 7;
            var lowerIMSScan = testIMSScan - (numIMSScanSummed - 1) / 2;
            var upperIMSScan = testIMSScan + (numIMSScanSummed - 1) / 2;

            var scanSet = new ScanSetFactory().CreateScanSet(run, testLCScan, 1);

            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, Globals.PeakFitType.QUADRATIC, true) {
                PeaksAreStored = true
            };

            var zeroFiller = new Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);

            var thrashParameters = new ThrashParameters {
                MinMSFeatureToBackgroundRatio = 1,
                MaxFit = 0.4
            };

            var newDeconvolutor = new ThrashDeconvolutorV2(thrashParameters);

            var oldDeconvolutor = new HornDeconvolutor {
                MinPeptideBackgroundRatio = 1,
                MaxFitAllowed = 0.4
            };

            run.CurrentScanSet = scanSet;
            ((UIMFRun)run).CurrentIMSScanSet = new IMSScanSet(testIMSScan, lowerIMSScan, upperIMSScan);

            // Populate run.ResultCollection.Run.XYData
            generator.Execute(run.ResultCollection);

            // Zero fill run.ResultCollection.Run.XYData
            zeroFiller.Execute(run.ResultCollection);

            // Optionally smooth the data
            //smoother.Execute(run.ResultCollection);

            // Find peaks, populating run.ResultCollection.Run.PeakList
            peakDetector.Execute(run.ResultCollection);

            // Find isotopic distributions
            newDeconvolutor.Execute(run.ResultCollection);

            //Console.WriteLine("\n--------------New decon ------------------");
            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            var newResults = new List<IsosResult>(run.ResultCollection.ResultList);

            // TestUtilities.DisplayMSFeatures(newResults);

            //DisplayPPMErrorsForeachPeakOfMSFeature(newResults);

            //return;

            run.ResultCollection.ResultList.Clear();
            run.ResultCollection.IsosResultBin.Clear();

            // Find isotopic distributions
            oldDeconvolutor.Execute(run.ResultCollection);

            var oldResults = new List<IsosResult>(run.ResultCollection.ResultList);


            //Console.WriteLine("\n--------------Old decon ------------------");
            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);


            var sharedIsos = new List<IsosResult>();
            var uniqueToNew = new List<IsosResult>();
            var uniqueToOld = new List<IsosResult>();

            var toleranceForComparison = 0.05;

            foreach (var newResult in newResults)
            {
                var foundMatch = false;
                foreach (var oldResult in oldResults)
                {
                    if (Math.Abs(newResult.IsotopicProfile.MonoIsotopicMass - oldResult.IsotopicProfile.MonoIsotopicMass) < toleranceForComparison &&
                        newResult.IsotopicProfile.ChargeState == oldResult.IsotopicProfile.ChargeState)
                    {
                        foundMatch = true;
                    }
                }

                if (foundMatch)
                {
                    sharedIsos.Add(newResult);
                }
                else
                {
                    uniqueToNew.Add(newResult);
                }

            }

            foreach (var oldResult in oldResults)
            {
                var foundMatch = false;
                foreach (var newResult in newResults)
                {
                    if (Math.Abs(newResult.IsotopicProfile.MonoIsotopicMass - oldResult.IsotopicProfile.MonoIsotopicMass) < toleranceForComparison &&
                        newResult.IsotopicProfile.ChargeState == oldResult.IsotopicProfile.ChargeState)
                    {
                        foundMatch = true;
                    }
                }

                if (!foundMatch)
                {
                    uniqueToOld.Add(oldResult);
                }

            }

            Console.WriteLine("\n--------------Common to new and Old ------------------");
            TestUtilities.DisplayMSFeatures(sharedIsos);

            Console.WriteLine("\n--------------Unique to new ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToNew);

            Console.WriteLine("\n--------------Unique to old ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToOld);

        }

        [Obsolete("Unused")]
        private void DisplayPPMErrorsForeachPeakOfMSFeature(IEnumerable<IsosResult> newResults)
        {

            var sb = new StringBuilder();
            var delimiter = '\t';

            foreach (var isosResult in newResults)
            {
                var indexOfMostIntensePeak = isosResult.IsotopicProfile.GetIndexOfMostIntensePeak();

                var maxPeak = isosResult.IsotopicProfile.Peaklist[indexOfMostIntensePeak];

                sb.Append(maxPeak.XValue);
                sb.Append(delimiter);
                sb.Append(isosResult.IsotopicProfile.ChargeState);
                sb.Append(delimiter);

                var ppmErrors = new List<double>();

                for (var i = 0; i < isosResult.IsotopicProfile.Peaklist.Count; i++)
                {
                    var currentPeak = isosResult.IsotopicProfile.Peaklist[i];

                    var expectedMZ = isosResult.IsotopicProfile.MonoPeakMZ +
                                     Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / isosResult.IsotopicProfile.ChargeState * i;


                    var ppmError = (expectedMZ - currentPeak.XValue) / expectedMZ * 1e6;
                    ppmErrors.Add(Math.Abs(ppmError));

                    //sb.Append(currentPeak.XValue);
                    //sb.Append(delimiter);
                    //sb.Append(currentPeak.Height);
                    //sb.Append(delimiter);
                    //sb.Append(ppmError.ToString("0.0"));
                    //sb.Append(delimiter);

                }
                sb.Append(ppmErrors.Average().ToString("0.0"));
                sb.Append(Environment.NewLine);



            }

            Console.WriteLine(sb.ToString());
        }

        [Test]
        public void UIMFTesting1()
        {
            var uimfFile =
             @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS2_90_6Apr11_Cheetah_11-02-19.uimf";

            var run = new RunFactory().CreateRun(uimfFile);

            var testLCScan = 500;
            var testIMSScan = 120;

            var numIMSScanSummed = 7;
            var lowerIMSScan = testIMSScan - (numIMSScanSummed - 1) / 2;
            var upperIMSScan = testIMSScan + (numIMSScanSummed - 1) / 2;

            var scanSet = new ScanSetFactory().CreateScanSet(run, testLCScan, 1);


            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(2, 2, Globals.PeakFitType.QUADRATIC, true)
            {
                PeaksAreStored = true
            };

            var thrashParameters = new ThrashParameters {
                MinMSFeatureToBackgroundRatio = 2,
                MaxFit = 0.4
            };

            var newDeconvolutor = new ThrashDeconvolutorV2(thrashParameters);


            run.CurrentScanSet = scanSet;
            ((UIMFRun)run).CurrentIMSScanSet = new IMSScanSet(testIMSScan, lowerIMSScan, upperIMSScan);

            // Populate run.ResultCollection.Run.XYData
            generator.Execute(run.ResultCollection);

            // Find peaks, populating run.ResultCollection.Run.PeakList
            peakDetector.Execute(run.ResultCollection);

            //6	500	728.6907	729.69800	1	34678	0.252	0.000

            run.PeakList = (from n in run.PeakList where n.XValue > 729 && n.XValue < 731 select n).ToList();

            // Find isotopic distributions
            newDeconvolutor.Execute(run.ResultCollection);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
        }


        [Ignore("Uses old decon engine")]
        [Test]
        public void OldDeconvolutorTest_temp1()
        {
            // Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

#if !Disable_DeconToolsV2
            var parameters = new DeconEngineClasses.OldDecon2LSParameters();
            var paramFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash_MaxFit1.xml";
            parameters.Load(paramFile);

            var scanSet = new ScanSetFactory().CreateScanSet(run, 6005, 1);

            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, Globals.PeakFitType.QUADRATIC, true);

            var deconvolutor = new HornDeconvolutor(parameters.GetDeconToolsParameters());
            run.CurrentScanSet = scanSet;
            generator.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            deconvolutor.Execute(run.ResultCollection);

            run.ResultCollection.ResultList = run.ResultCollection.ResultList.OrderByDescending(p => p.IntensityAggregate).ToList();

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            //IsosResult testIso = run.ResultCollection.ResultList[0];

            //TestUtilities.DisplayIsotopicProfileData(testIso.IsotopicProfile);

            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
            //TestUtilities.DisplayPeaks(run.PeakList);
#endif


        }

        [Ignore("Local testing only")]
        [Test]
        public void CompareOldAndNew2()
        {
            var newResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\Standard_Testing\DeconTools\Orbitrap\Test_Results\Version_1.0.XX_Jan6_AfterThrashRefining\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_isos.csv";

            var oldResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\Standard_Testing\DeconTools\Orbitrap\Test_Results\Version_1.0.4745_Dec28_AfterThrashRefactor\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_isos.csv";

            var importer = new IsosImporter(newResultsFile, Globals.MSFileType.Thermo_Raw);
            var newResults = importer.Import();

            importer = new IsosImporter(oldResultsFile, Globals.MSFileType.Thermo_Raw);
            var oldResults = importer.Import();

            //newResults = newResults.Where(p => p.ScanSet.PrimaryScanNumber == 6005).ToList();
            //oldResults = oldResults.Where(p => p.ScanSet.PrimaryScanNumber == 6005).ToList();

            var sharedIsos = new List<IsosResult>();
            var uniqueToNew = new List<IsosResult>();
            var uniqueToOld = new List<IsosResult>();


            GetComparisons(newResults, oldResults, sharedIsos, uniqueToNew, uniqueToOld);


            Console.WriteLine("Summary----------------------------");
            Console.WriteLine("Shared =\t" + sharedIsos.Count);
            Console.WriteLine("Unique to new =\t" + uniqueToNew.Count);
            Console.WriteLine("Unique to old =\t" + uniqueToOld.Count);

            Console.WriteLine("\n--------------Common to new and Old ------------------");
            TestUtilities.DisplayMSFeatures(sharedIsos);


            Console.WriteLine("\n--------------Unique to new ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToNew);

            var outputFilename = @"D:\temp\exportedIsos.csv";
            var exporter = IsosExporterFactory.CreateIsosExporter(Globals.ResultType.BASIC_TRADITIONAL_RESULT, Globals.ExporterType.Text, outputFilename);

            exporter.ExportIsosResults(uniqueToNew);


            Console.WriteLine("\n--------------Unique to old ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToOld);

        }
    }
}
