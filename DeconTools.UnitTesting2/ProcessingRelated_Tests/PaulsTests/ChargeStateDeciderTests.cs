using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DeconTools.Backend.ProcessingTasks.Deconvoluters;
using DeconTools.Backend.Workflows;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.UnitTesting2;
using DeconTools.Backend;
using System.IO;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.ChargeStateDeciders;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;
using DeconTools.Backend.Data;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    class ChargeStateDeciderTests
    {

        //[Test]
        //public void TraditionalWorkflowTestOrbitrapData1()
        //{
        //    string parameterFile = FileRefs.ParameterFiles.Orbitrap_Scans6000_6050ParamFile;

        //    Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
        //    string expectedIsosFile = Path.Combine(run.DataSetPath, run.DatasetName + "_isos.csv");
        //    string expectedScansFile = Path.Combine(run.DataSetPath, run.DatasetName + "_scans.csv");
        //    string expectedPeaksFile = Path.Combine(run.DataSetPath, run.DatasetName + "_peaks.txt");

        //    if (File.Exists(expectedIsosFile)) File.Delete(expectedIsosFile);
        //    if (File.Exists(expectedScansFile)) File.Delete(expectedScansFile);
        //    if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);

        //    var parameters = new DeconToolsParameters();
        //    parameters.LoadFromOldDeconToolsParameterFile(parameterFile);
        //    parameters.ThrashParameters.UseThrashV1 = false;

        //    parameters.MSGeneratorParameters.MinLCScan = 6005;
        //    parameters.MSGeneratorParameters.MaxLCScan = 6005;


        //    var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
        //    workflow.Execute();

        //    Assert.IsTrue(File.Exists(expectedIsosFile), "Isos file was not created.");
        //    Assert.IsTrue(File.Exists(expectedScansFile), "Scans file was not created.");
        //    Assert.IsTrue(File.Exists(expectedPeaksFile), "Peaks file was not created.");

        //    IsosImporter isosImporter = new IsosImporter(expectedIsosFile, run.MSFileType);
        //    var isos = isosImporter.Import();

        //    Assert.AreEqual(186, isos.Count);

        //    PeakImporterFromText peakImporter = new PeakImporterFromText(expectedPeaksFile);

        //    List<MSPeakResult> peaklist = new List<MSPeakResult>();
        //    peakImporter.ImportPeaks(peaklist);

        //    Assert.AreEqual(809, peaklist.Count);

        //    var sumIntensities = isos.Select(p => p.IntensityAggregate).Sum();
        //    Assert.AreEqual(266185816d, Math.Round(sumIntensities));

        //    var sumPeakIntensities = peaklist.Select(p => p.Height).Sum();
        //    Assert.AreEqual(605170496.0f, sumPeakIntensities);

        //}

        public List<double> ReadScottsAnnotationsScan5509()
        {
            var fileName = @"\\pnl\projects\MSSHARE\Gord\For_Paul\Anotated from Scott\ScottsAnnotatedScan5509.txt";

            var mzList = new List<double>();

            using (var reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    double.TryParse(reader.ReadLine(), out var currentMz);
                    mzList.Add(currentMz);
                }
            }

            return mzList.OrderBy(p => p).ToList();
        }

        [Test]
        public void SpeedTest1()
        {
            var fileName = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var run = new RunFactory().CreateRun(fileName);
            run.ScanSetCollection.Create(run, 5500, 5550, 1, 1, false);
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakBr = 1.3;   //0.25;
            var peakDetector = new DeconToolsPeakDetectorV2(peakBr, 2, Globals.PeakFitType.QUADRATIC, true);

            var thrashParameters = new ThrashParameters
            {
                MinMSFeatureToBackgroundRatio = peakBr,
                MaxFit = 0.4
            };

            var newDeconvolutor = new InformedThrashDeconvolutor(thrashParameters);

            var watch = new Stopwatch();
            watch.Start();
            foreach (var scanSet in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanSet;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);
                run.CurrentScanSet.BackgroundIntensity = peakDetector.BackgroundIntensity;
                newDeconvolutor.Execute(run.ResultCollection);
            }
            watch.Stop();

            Console.WriteLine("Time per scan (INFORMED) = " + watch.ElapsedMilliseconds / run.ScanSetCollection.ScanSetList.Count);


        }


        [Test]
        public void SpeedTest_oldThrash()
        {
            var fileName = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var run = new RunFactory().CreateRun(fileName);
            run.ScanSetCollection.Create(run, 5500, 5550, 1, 1, false);
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakBr =1.3;
            var peakDetector = new DeconToolsPeakDetectorV2(peakBr, 2, Globals.PeakFitType.QUADRATIC, true);

            var thrashParameters = new ThrashParameters
            {
                MinMSFeatureToBackgroundRatio = peakBr,
                MaxFit = 0.4
            };

            var deconParams = new DeconToolsParameters {
                ThrashParameters = thrashParameters
            };

            var deconvolutor = new HornDeconvolutor(deconParams);

            var watch = new Stopwatch();
            watch.Start();

            foreach (var scanSet in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanSet;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);
                run.CurrentScanSet.BackgroundIntensity = peakDetector.BackgroundIntensity;
                deconvolutor.Execute(run.ResultCollection);
            }
            watch.Stop();

            Console.WriteLine("Time per scan = " + watch.ElapsedMilliseconds / run.ScanSetCollection.ScanSetList.Count);


        }



        [Test]
        public void CompareToScottsData1()
        {
            var fileName = FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var run = new RunFactory().CreateRun(fileName);
            run.ScanSetCollection.Create(run, 5509, 5509, 1, 1, false);
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakBr = 1.9;//1.3;
            var peakDetector = new DeconToolsPeakDetectorV2(peakBr, 2, Globals.PeakFitType.QUADRATIC, true);

            var thrashParameters = new ThrashParameters
            {
                MinMSFeatureToBackgroundRatio = peakBr,
                MaxFit = 0.4
            };

            var newDeconvolutor = new InformedThrashDeconvolutor(thrashParameters);

            var scanset = new ScanSet(5509);
            run.CurrentScanSet = scanset;

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            run.CurrentScanSet.BackgroundIntensity = peakDetector.BackgroundIntensity;
            newDeconvolutor.Execute(run.ResultCollection);


            var scottMzVals = ReadScottsAnnotationsScan5509().ToArray();

            var myMzVals = run.ResultCollection.ResultList.Select(p => p.IsotopicProfile.MonoPeakMZ).OrderBy(p => p).ToArray();

            var comparisions = new Dictionary<decimal, double>();

            foreach (var scottVal in scottMzVals)
            {
                var tolerance = 0.01;
                var indexInThrash = MathUtils.BinarySearchWithTolerance(myMzVals, scottVal, 0, myMzVals.Length - 1, tolerance);

                var myMzVal = indexInThrash == -1 ? 0.0d : myMzVals[indexInThrash];
                comparisions.Add((decimal)scottVal, myMzVal);
            }

            var numCorrect = comparisions.Values.Count(p => p > 0);

            var numMissing = scottMzVals.Length - numCorrect;

            Console.WriteLine("Total annotated= \t" + scottMzVals.Length);
            Console.WriteLine("Number correct = \t" + numCorrect);
            Console.WriteLine("Number missing = \t" + numMissing);

        }

        [Test]
        public void ReadInScottsDataTest()
        {
            GetScottsData();
        }
        private List<double> GetScottsData()
        {
            string line;
            var scottsData = new List<double>();

            var file = new StreamReader(@"\\pnl\projects\MSSHARE\Gord\For_Paul\Anotated from Scott\ScottsAnnotatedScan5509.txt");
            while ((line = file.ReadLine()) != null)
            {
                scottsData.Add(double.Parse(line));
            }
            file.Close();
            return scottsData;
        }
        [Test]
        public void TestAndComparingScottsData()
        {
            var fileName = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.raw";
            var run = new RunFactory().CreateRun(fileName);
            run.ScanSetCollection.Create(run, 5509, 5509, 1, 1, false);
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakBr = 0.5;

            var peakDetector = new DeconToolsPeakDetectorV2(peakBr, 2, Globals.PeakFitType.QUADRATIC, true);

            var thrashParameters = new ThrashParameters
            {
                MinMSFeatureToBackgroundRatio = peakBr,
                MaxFit = 0.4
            };
            //3;

            //var newDeconvolutor = new ThrashDeconvolutorV2(thrashParameters);
            var newDeconvolutor = new InformedThrashDeconvolutor(thrashParameters);

            var scanset = new ScanSet(5509);

            //For summing mass spectra:
            //scanset = new ScanSetFactory().CreateScanSet(run, 6005, 5);

            run.CurrentScanSet = scanset;

            var oldDeconvolutor = new HornDeconvolutor
            {
                MinPeptideBackgroundRatio = peakBr,
                MaxFitAllowed = 0.4
            };

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

            //run.PeakList = run.PeakList.Where(p => p.XValue > 634 && p.XValue < 642).ToList();
            //run.DeconToolsPeakList = run.DeconToolsPeakList.Where(p => p.mdbl_mz > 634 && p.mdbl_mz < 642).ToArray();

            run.CurrentScanSet.BackgroundIntensity = peakDetector.BackgroundIntensity;

            newDeconvolutor.Execute(run.ResultCollection);

            //Console.WriteLine("\n--------------New decon ------------------");
            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            var newResults = new List<IsosResult>(run.ResultCollection.ResultList);

            //TestUtilities.DisplayMSFeatures(newResults);
            //return;

            // DisplayPPMErrorsForeachPeakOfMSFeature(newResults);

            run.ResultCollection.ResultList.Clear();
            run.ResultCollection.IsosResultBin.Clear();
            oldDeconvolutor.Execute(run.ResultCollection);

            var oldResults = new List<IsosResult>(run.ResultCollection.ResultList);

            //Console.WriteLine("\n--------------Old decon ------------------");
            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            var sharedIsos = new List<IsosResult>();
            var uniqueToNew = new List<IsosResult>();
            var uniqueToOld = new List<IsosResult>();

            GetComparisons(newResults, oldResults, sharedIsos, uniqueToNew, uniqueToOld);
            var scottsData = GetScottsData();
            var scottsDataArray = scottsData.ToArray();

            GetScottComparisons(sharedIsos, uniqueToNew, uniqueToOld, scottsDataArray);

            Console.WriteLine("\n--------------Common to new and Old ------------------");
            TestUtilities.DisplayMSFeatures(sharedIsos);

            Console.WriteLine("\n--------------Unique to new ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToNew);

            var outputFilename = @"C:\Temp\ThrashTesting\exportedIsos.csv";
            var exporter = IsosExporterFactory.CreateIsosExporter(run.ResultCollection.ResultType, Globals.ExporterType.Text, outputFilename);

            exporter.ExportIsosResults(uniqueToNew);

            Console.WriteLine("\n--------------Unique to old ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToOld);
        }

        private void GetScottComparisons(List<IsosResult> sharedIsos, List<IsosResult> uniqueToNew, List<IsosResult> uniqueToOld, IReadOnlyList<double> scottsDataArray)
        {
            CompareListToScottsData(sharedIsos, scottsDataArray, out var sharedFoundInScotts, out var sharedNotFoundInScotts);
            CompareListToScottsData(uniqueToNew, scottsDataArray, out var uniqueToNewFoundInScotts, out var uniqueToNewNotFoundinScotts);
            CompareListToScottsData(uniqueToOld, scottsDataArray, out var uniqueToOldFoundInScotts, out var uniqueToOldNotFoundinScotts);

            var totalCorrectFoundByNew = sharedFoundInScotts + uniqueToNewFoundInScotts;
            var totalCorrectFoundByOld = sharedFoundInScotts + uniqueToOldFoundInScotts;
            var totalNOTFOUND_usingOld= sharedNotFoundInScotts+ uniqueToOldNotFoundinScotts;
            var totalNOTFOUND_usingNEW= sharedNotFoundInScotts+ uniqueToNewNotFoundinScotts;
            var scottsTotal = scottsDataArray.Count;
            Console.WriteLine("Total correct found new:\t" + totalCorrectFoundByNew);
            Console.WriteLine("Total correct found old:\t" + totalCorrectFoundByOld);
            Console.WriteLine("Total FALSE old:\t" + totalNOTFOUND_usingOld );
            Console.WriteLine("Total FALSE NEW:\t" + totalNOTFOUND_usingNEW);
            Console.WriteLine("Shared Found in Scott's Data\t" + sharedFoundInScotts);
            Console.WriteLine("Shared NOT FOUND in Scott's Data\t" + sharedNotFoundInScotts);
            Console.WriteLine("Total found SCOTT:\t" + scottsTotal);
            Console.WriteLine("False positives in both: \t" + sharedNotFoundInScotts);
            Console.WriteLine("False positives unique in new:\t" + uniqueToNewNotFoundinScotts);
            Console.WriteLine("False positives unique in old: \t" + uniqueToOldNotFoundinScotts);



        }
        private void CompareListToScottsData(List<IsosResult> list, IReadOnlyList<double> scottsDataArray, out int numFound, out int numNotFound)
        {
            numFound = 0;
            numNotFound = 0;
            foreach (var item in list)
            {
                var mz = item.IsotopicProfile.MonoPeakMZ;
                if (IsValueCloseEnoughToOneOfScotts(mz, scottsDataArray))
                {
                    numFound++;
                }
                else
                {
                    numNotFound++;
                }
            }
        }

        private bool IsValueCloseEnoughToOneOfScotts(double mz, IReadOnlyList<double> scottsDataArray)
        {
            var aReasonableValue = 0.01;
            int i;
            var stoppedinArray = false;
            for (i = 0; i < scottsDataArray.Count; i++)
            {
                if (scottsDataArray[i] > mz)
                {
                    stoppedinArray = true;
                    break;
                }

            }
            if (stoppedinArray)
            {
                if (Math.Abs(scottsDataArray[i] - mz) < aReasonableValue)
                {
                    return true;
                }

                if (i == 0) { return false; }

                if (Math.Abs(scottsDataArray[i - 1] - mz) < aReasonableValue)
                {
                    return true;
                }
            }
            return false;
        }

        [Test]
        public void Stolen_ThrashV2OnOrbitrapTest1()
        {
            //Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var fileName =
                             @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.raw";
            //Run run = RunUtilities.CreateAndLoadPeaks(fileName);
            var run = new RunFactory().CreateRun(fileName);
            run.ScanSetCollection.Create(run, 6005, 6005, 1, 1, false);

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, Globals.PeakFitType.QUADRATIC, true) {
                IsDataThresholded = true
            };

            var parameters = new ThrashParameters
            {
                MinMSFeatureToBackgroundRatio = 1,
                MaxFit = 0.3
            };

            var deconvolutor = new InformedThrashDeconvolutor(parameters);
            //var deconvolutor2 = new

            var scan = new ScanSet(6005);

            run.CurrentScanSet = scan;
            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            deconvolutor.Execute(run.ResultCollection);
            Console.WriteLine(run.ResultCollection.MSPeakResultList);
            // TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            Assert.IsTrue(run.ResultCollection.ResultList.Count > 0);
            //Assert.AreEqual(187, run.ResultCollection.ResultList.Count);

            var result1 = run.ResultCollection.ResultList[0];
            Assert.AreEqual(13084442, (decimal)Math.Round(result1.IntensityAggregate));
            Assert.AreEqual(960.53365m, (decimal)Math.Round(result1.IsotopicProfile.MonoIsotopicMass, 5));
            Assert.AreEqual(2, result1.IsotopicProfile.ChargeState);

        }

        [Test]
        public void Stolen_CompareOldAndNewDeconvolutorsOrbitrap()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            //string fileName = @"C:\Users\Klin638\Documents\Visual Studio 2010\Backup Files\New folder\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            //@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.raw";
            //@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\Vorbi\Yellow_C12_099_18Mar10_Griffin_10-01-13.raw";

            //@"C:\Users\Klin638\Documents\Visual Studio 2010\Backup Files\New folder\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            //
            //Run run = RunUtilities.CreateAndLoadPeaks(fileName);
            //Run run = new RunFactory().CreateRun(fileName);

            run.ScanSetCollection.Create(run, 6005, 6005, 1, 1, false);

            //to sum
            //run.ScanSetCollection.Create(run, 6005, 6005, 5, 1, false);


            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, Globals.PeakFitType.QUADRATIC, true);



            var thrashParameters = new ThrashParameters
            {
                MinMSFeatureToBackgroundRatio = 3,
                MaxFit = 0.4
            };

            var newDeconvolutor = new ThrashDeconvolutorV2(thrashParameters);
            //var newDeconvolutor = new InformedThrashDeconvolutor(thrashParameters);

            var scanset = new ScanSet(6005);

            //For summing mass spectra:
            //scanset = new ScanSetFactory().CreateScanSet(run, 6005, 5);

            run.CurrentScanSet = scanset;

            var oldDeconvolutor = new HornDeconvolutor
            {
                MinPeptideBackgroundRatio = 3,
                MaxFitAllowed = 0.4
            };

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);


            //run.PeakList = run.PeakList.Where(p => p.XValue > 634 && p.XValue < 642).ToList();
            //run.DeconToolsPeakList = run.DeconToolsPeakList.Where(p => p.mdbl_mz > 634 && p.mdbl_mz < 642).ToArray();

            run.CurrentScanSet.BackgroundIntensity = peakDetector.BackgroundIntensity;

            newDeconvolutor.Execute(run.ResultCollection);

            //Console.WriteLine("\n--------------New decon ------------------");
            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            var newResults = new List<IsosResult>(run.ResultCollection.ResultList);

            //TestUtilities.DisplayMSFeatures(newResults);
            //return;

            // DisplayPPMErrorsForeachPeakOfMSFeature(newResults);

            run.ResultCollection.ResultList.Clear();
            run.ResultCollection.IsosResultBin.Clear();
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

            var outputFilename = @"C:\Temp\ThrashTesting\exportedIsos.csv";
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


                foreach (var newresult in newResults)
                {
                    var foundMatch = false;
                    foreach (var oldResult in oldResults)
                    {
                        if (Math.Abs(newresult.IsotopicProfile.MonoIsotopicMass - oldResult.IsotopicProfile.MonoIsotopicMass) < 0.01 &&
                            newresult.IsotopicProfile.ChargeState == oldResult.IsotopicProfile.ChargeState)
                        {
                            foundMatch = true;
                        }
                    }

                    if (foundMatch)
                    {
                        sharedIsos.Add(newresult);
                    }
                    else
                    {
                        uniqueToNew.Add(newresult);
                    }
                }

                foreach (var oldResult in oldResults)
                {
                    var foundMatch = false;
                    foreach (var newresult in newResults)
                    {
                        if (Math.Abs(newresult.IsotopicProfile.MonoIsotopicMass - oldResult.IsotopicProfile.MonoIsotopicMass) < 0.01 &&
                            newresult.IsotopicProfile.ChargeState == oldResult.IsotopicProfile.ChargeState)
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
        public void stolen_peakexporterTest1()
        {

            //RunFactory rf = new RunFactory();

            //Run run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var fileName = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            //@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.raw";
            //@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\Vorbi\Yellow_C12_099_18Mar10_Griffin_10-01-13.raw";

            //
            //Run run = RunUtilities.CreateAndLoadPeaks(fileName);
            var run = new RunFactory().CreateRun(fileName);


            var parameters = new PeakDetectAndExportWorkflowParameters
            {
                LCScanMin = 5500,
                LCScanMax = 6500
            };

            var expectedPeaksFile = Path.Combine(run.DataSetPath, run.DatasetName + "_peaks.txt");
            if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);


            var workflow = new PeakDetectAndExportWorkflow(run, parameters);
            workflow.Execute();

            var fileinfo = new FileInfo(expectedPeaksFile);
            Assert.IsTrue(fileinfo.Exists);
            Assert.IsTrue(fileinfo.Length > 1000000);

        }

        [Test]
        public void EasyDecision()
        {
            var fileName =
                            @"\\Protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            //Run run = RunUtilities.CreateAndLoadPeaks(fileName);
            var run = new RunFactory().CreateRun(fileName);
            EasyDecisionSetUp(out var potentialFeatures);
            ChargeStateDecider chargestatedecider = new ChromCorrelatingChargeDecider(run);
            var msFeature = chargestatedecider.DetermineCorrectIsotopicProfile(potentialFeatures);

            Assert.AreEqual(msFeature.ChargeState, 2);
        }
        private void EasyDecisionSetUp(out List<IsotopicProfile> potentialFeatures)
        {

            var peak1_iso1 = MakeMSPeak(0.007932149f, 481.27410112055895);
            var peak2_iso1 = MakeMSPeak(0.007846226f, 481.775417927883);
            var peak3_iso1 = MakeMSPeak(0.00768206455f, 482.27682748526291);
            var peaklist1 = MakeMSPeakList(peak1_iso1, peak2_iso1, peak3_iso1);
            var iso1 = MakePotentialFeature(2, 481.27410112055895, 0, peaklist1);

            var peak1_iso2 = MakeMSPeak(0.007932149f, 481.27410112055895);
            var peak2_iso2 = MakeMSPeak(0.00768206455f, 482.27682748526291);
            var peaklist2 = MakeMSPeakList(peak1_iso2, peak2_iso2);
            var iso2 = MakePotentialFeature(1, 481.27410112055895, 0, peaklist2);



            potentialFeatures = MakePotentialFeaturesList(iso1, iso2);
        }

        [Test]
        public void MediumDecision()
        {
            var fileName = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.raw";
            //Run run = RunUtilities.CreateAndLoadPeaks(fileName);
            var run = new RunFactory().CreateRun(fileName);
            run.CurrentScanSet = new ScanSet(1000);

            MediumDecisionSetup(out var potentialFeatures);

            ChargeStateDecider chargestatedecider = new ChromCorrelatingChargeDecider(run);
            var msFeature = chargestatedecider.DetermineCorrectIsotopicProfile(potentialFeatures);

            //Assert something here.

        }
        private void MediumDecisionSetup(out List<IsotopicProfile> potentialFeatures)
        {


            var peak1_iso1 = MakeMSPeak(0.0151630929f, 746.70851219111921);
            var peak2_iso1 = MakeMSPeak(0.0151641443f, 746.87333076347306);
            var peaklist1 = MakeMSPeakList(peak1_iso1, peak2_iso1);
            var iso1 = MakePotentialFeature(6, 746.5392140968064, -1, peaklist1);

            var peak1_iso2 = MakeMSPeak(0.0151641443f, 746.87333076347306);
            var peak2_iso2 = MakeMSPeak(0.01580638f, 747.37507455840785);
            var peak3_iso2 = MakeMSPeak(0.0153686106f, 747.87687140741934);
            var peaklist2 = MakeMSPeakList(peak1_iso2, peak2_iso2, peak3_iso2);
            var iso2 = MakePotentialFeature(2, 746.87333076347306, 0, peaklist2);

            var peak1_iso3 = MakeMSPeak(0.0151641443f, 746.87333076347306);
            var peak2_iso3 = MakeMSPeak(0.0153686106f, 747.87687140741934);
            var peaklist3 = MakeMSPeakList(peak1_iso2, peak2_iso2);
            var iso3 = MakePotentialFeature(1, 746.87333076347306, 0, peaklist3);

            potentialFeatures = MakePotentialFeaturesList(iso1, iso2, iso3);
        }
        [Test]
        public void HardDecision()
        {

            //Assert.AreEqual(msFeature.ChargeState, 2);

        }
        [Test]
        public void testPeakGeneration()
        {
            var fileName =
                 @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.raw";
            var run = new RunFactory().CreateRun(fileName);

            var parameters = new PeakDetectAndExportWorkflowParameters
            {
                LCScanMin = 5000,
                LCScanMax = 7000
            };


            var expectedPeaksFile = Path.Combine(run.DataSetPath, run.DatasetName + "_peaks.txt");
            if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);


            var workflow = new PeakDetectAndExportWorkflow(run, parameters);
            workflow.Execute();

            var fileinfo = new FileInfo(expectedPeaksFile);
            Assert.IsTrue(fileinfo.Exists);
            Assert.IsTrue(fileinfo.Length > 1000000);

        }
        [Test]
        public void GetPeakChromatogram_IQStyle_Test1()
        {

            //Run run = RunUtilities.CreateAndLoadPeaks(FileRefs.RawDataMSFiles.OrbitrapStdFile1,
            //                                          FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            var fileName =
                 @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.raw";
            var run = RunUtilities.CreateAndLoadPeaks(fileName);

            var target = TestUtilities.GetIQTargetStandard(1);


            //TestUtilities.DisplayIsotopicProfileData(target.TheorIsotopicProfile);

            var chromGen = new PeakChromatogramGenerator
            {
                ChromatogramGeneratorMode = Globals.ChromatogramGeneratorMode.TOP_N_PEAKS,
                TopNPeaksLowerCutOff = 0.4,
                Tolerance = 10
            };
            //chromGen.ChromatogramGeneratorMode = Globals.ChromatogramGeneratorMode.;


            //var chromXYData = chromGen.GenerateChromatogram(run, target.TheorIsotopicProfile, target.ElutionTimeTheor);
            // var chromXYData = chromGen.GenerateChromatogram(run, new List<double> { 481.27410, 481.77542, 482.27683 }, 4000, 6500, 0.009, Globals.ToleranceUnit.MZ);
            //   var chromXYData = chromGen.GenerateChromatogram(run,500,7000,490.26483,0.005,Globals.ToleranceUnit.MZ);


            //  Assert.IsNotNull(chromXYData);

            Console.WriteLine("481.27410");
            var chromXYData1 = chromGen.GenerateChromatogram(run, 500, 7000, 481.27410, 0.005, Globals.ToleranceUnit.MZ); TestUtilities.DisplayXYValues(chromXYData1);
            //  TestUtilities.DisplayXYValues(chromXYData);
            var minx1 = chromXYData1.Xvalues.Min();
            var maxx1 = chromXYData1.Xvalues.Max();
            Console.WriteLine("481.77542");
            // chromXYData1.
            var chromXYData2 = chromGen.GenerateChromatogram(run, 500, 7000, 481.77542, 0.005, Globals.ToleranceUnit.MZ); TestUtilities.DisplayXYValues(chromXYData2);
            // TestUtilities.DisplayXYValues(chromXYData);
            var minx2 = chromXYData2.Xvalues.Min();
            var maxx2 = chromXYData2.Xvalues.Max();

            Console.WriteLine("482.27683");
            var chromXYData3 = chromGen.GenerateChromatogram(run, 500, 7000, 482.27683, 0.005, Globals.ToleranceUnit.MZ); TestUtilities.DisplayXYValues(chromXYData3);


            TestUtilities.DisplayXYValues(chromXYData3);
            var minx3 = chromXYData3.Xvalues.Min();
            var maxx3 = chromXYData3.Xvalues.Max();

            var minxhelper = Math.Max(minx1, minx2);
            var minx = Math.Max(minxhelper, minx3);
            var maxxhelper = Math.Min(maxx1, maxx2);
            var maxX = Math.Min(maxxhelper, maxx3);

            var c1start = chromXYData1.GetClosestXVal(minx);
            var c2start = chromXYData2.GetClosestXVal(minx);
            var c3start = chromXYData3.GetClosestXVal(minx);
            var c1stop = chromXYData1.GetClosestXVal(maxX);
            var c2stop = chromXYData2.GetClosestXVal(maxX);
            var c3stop = chromXYData3.GetClosestXVal(maxX);
            chromXYData1.NormalizeYData();
            chromXYData2.NormalizeYData();
            chromXYData3.NormalizeYData();
            var c1 = new double[c1stop - c1start + 1];
            var c2 = new double[c2stop - c2start + 1];
            var c3 = new double[c3stop - c3start + 1];


            for (int i = c1start, j = 0; i <= c1stop; i++, j++)
            {
                c1[j] = chromXYData1.Yvalues[i];

            }
            for (int i = c2start, j = 0; i <= c2stop; i++, j++)
            {
                c2[j] = chromXYData2.Yvalues[i];

            }
            for (int i = c3start, j = 0; i <= c3stop; i++, j++)
            {
                c3[j] = chromXYData3.Yvalues[i];

            }

            for (var i = 0; i < c1.Length; i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}", c1[i], c2[i], c3[i]);

            }

            // double c1chromEven= chromXYData1.Xvalues
            var corr1 = MathNet.Numerics.Statistics.Correlation.Pearson(c1, c2);
            var corr2 = MathNet.Numerics.Statistics.Correlation.Pearson(c1, c3);
            Console.WriteLine("HERE IS THE CORRELATION: " + corr1);
            Console.WriteLine("HERE IS THE CORRELATION: " + corr2);


            //bool alldone = false, done1 = false, done2 = false, done3 = false;
            //int onestarting=0,twostarting=0,threestarting=0;
            //while (!alldone)
            //{
            //    if (!done1)
            //    {
            //        if (chromXYData1.Xvalues[onestarting]==minx)
            //        {
            //            done1 = true;
            //        }
            //        else
            //        {

            //        }

            //    }
            //    if (!done2)
            //    {

            //    }
            //    if (!done3)
            //    {

            //    }
            //    alldone = done1 && done2 && done3;
            //}



        }

        #region private helper methods

        private MSPeak MakeMSPeak(float width, double xValue)
        {
            var mspeak = new MSPeak(xValue, 0, width);

            return mspeak;
        }

        private List<MSPeak> MakeMSPeakList(params MSPeak[] msPeaks)
        {
            return msPeaks.ToList();
        }
        private List<IsotopicProfile> MakePotentialFeaturesList(params IsotopicProfile[] isotopicProfiles)
        {
            return isotopicProfiles.ToList(); //cool...
        }
        private IsotopicProfile MakePotentialFeature(int chargeState, double monoPeakMZ, int monoIsotpoicPeakIndex, params MSPeak[] peakList)
        {
            return MakePotentialFeature(chargeState, monoPeakMZ, monoIsotpoicPeakIndex, peakList.ToList());
        }
        private IsotopicProfile MakePotentialFeature(int chargeState, double monoPeakMZ, int monoIsotpoicPeakIndex, List<MSPeak> peakList)
        {
            var isopo = new IsotopicProfile
            {
                ChargeState = chargeState,
                MonoPeakMZ = monoPeakMZ,
                MonoIsotopicPeakIndex = monoIsotpoicPeakIndex,
                Peaklist = peakList
            };
            return isopo;
        }
        #endregion
    }

}
