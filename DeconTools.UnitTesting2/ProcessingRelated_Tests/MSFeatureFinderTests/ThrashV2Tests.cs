using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSFeatureFinderTests
{
    [TestFixture]
    public class ThrashV2Tests
    {
        [Test]
        public void ThrashV2OnOrbitrapTest1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            run.ScanSetCollection.Create(run, 6005, 7005, 1, 1, false);

            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            ThrashParameters parameters = new ThrashParameters();
            parameters.MinMSFeatureToBackgroundRatio = 1;
            
            ThrashDeconvolutorV2 deconvolutor = new ThrashDeconvolutorV2(parameters);
            


            List<IsotopicProfile> isotopicprofiles = new List<IsotopicProfile>();
            foreach (var scanSet in run.ScanSetCollection.ScanSetList)
            {
                //Console.WriteLine("-------------- scan" + scanSet);
                run.CurrentScanSet = scanSet;
                msgen.Execute(run.ResultCollection);

                //TestUtilities.DisplayXYValues(run.XYData);

                //run.XYData = run.XYData.TrimData(578, 582);
                //run.XYData = run.XYData.TrimData(520.5, 524);

                peakDetector.Execute(run.ResultCollection);



                //deconvolutor.Execute(run.ResultCollection);

                //isotopicprofiles = deconvolutor.PerformThrash(run.XYData, run.PeakList, run.CurrentBackgroundIntensity,0);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                run.CurrentScanSet.BackgroundIntensity = peakDetector.BackgroundIntensity;

                deconvolutor.Execute(run.ResultCollection);

                //isotopicprofiles = deconvolutor.PerformThrash(run.XYData, run.PeakList, run.CurrentBackgroundIntensity,0);
                stopwatch.Stop();


                Console.WriteLine("Time for decon= \t" + stopwatch.ElapsedMilliseconds);

            }

            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
        }

        [Test]
        public void OldDeconvolutorOrbitrapTest1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            run.ScanSetCollection.Create(run, 6005, 6200, 1, 1, false);


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            HornDeconvolutor deconvolutor = new HornDeconvolutor();
            deconvolutor.MinPeptideBackgroundRatio = 3;


            //deconvolutor.IsMZRangeUsed = true;
            //deconvolutor.MinMZ = 575;
            //deconvolutor.MaxMZ = 585;


            foreach (var scanSet in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanSet;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);

                run.CurrentScanSet.BackgroundIntensity = peakDetector.BackgroundIntensity;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                deconvolutor.Execute(run.ResultCollection);
                stopwatch.Stop();

                Console.WriteLine("Time for decon= \t" + stopwatch.ElapsedMilliseconds);

            }
            //Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            ////order and get the most intense msfeature
            //run.ResultCollection.ResultList = run.ResultCollection.ResultList.OrderByDescending(p => p.IsotopicProfile.IntensityAggregate).ToList();
            //IsosResult testIso = run.ResultCollection.ResultList[0];
            //Assert.AreEqual(13084442, testIso.IsotopicProfile.IntensityAggregate);
            //Assert.AreEqual(2, testIso.IsotopicProfile.ChargeState);
            //Assert.AreEqual(0.01012m, (decimal)Math.Round(testIso.IsotopicProfile.Score, 5));
            //Assert.AreEqual(3, testIso.IsotopicProfile.Peaklist.Count);
            //Assert.AreEqual(481.274105402604m, (decimal)testIso.IsotopicProfile.Peaklist[0].XValue);
            //Assert.AreEqual(481.775412188198m, (decimal)testIso.IsotopicProfile.Peaklist[1].XValue);
            //Assert.AreEqual(482.276820274024m, (decimal)testIso.IsotopicProfile.Peaklist[2].XValue);

            //TestUtilities.DisplayIsotopicProfileData(testIso.IsotopicProfile);

            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
            //TestUtilities.DisplayPeaks(run.PeakList);

        }


        [Test]
        public void CompareOldAndNewDeconvolutorsOrbitrap()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            run.ScanSetCollection.Create(run, 6005, 6005, 1, 1, false);

            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            


            var thrashParameters = new ThrashParameters();
            thrashParameters.MinMSFeatureToBackgroundRatio = 3;
            thrashParameters.MaxFit = 0.4;

            var newDeconvolutor = new ThrashDeconvolutorV2(thrashParameters);
            

            ScanSet scanset = new ScanSet(6005);
            run.CurrentScanSet = scanset;

            HornDeconvolutor oldDeconvolutor = new HornDeconvolutor();
            oldDeconvolutor.MinPeptideBackgroundRatio = 3;
            oldDeconvolutor.MaxFitAllowed = 0.4;

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);


            run.PeakList = run.PeakList.Where(p => p.XValue > 634 && p.XValue < 642).ToList();
            run.DeconToolsPeakList = run.DeconToolsPeakList.Where(p => p.mdbl_mz > 634 && p.mdbl_mz < 642).ToArray();

            run.CurrentScanSet.BackgroundIntensity = peakDetector.BackgroundIntensity;

            newDeconvolutor.Execute(run.ResultCollection);

            //Console.WriteLine("\n--------------New decon ------------------");
            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            List<IsosResult> newResults = new List<IsosResult>(run.ResultCollection.ResultList);

            //TestUtilities.DisplayMSFeatures(newResults);
            //return;

           // DisplayPPMErrorsForeachPeakOfMSFeature(newResults);

            run.ResultCollection.ResultList.Clear();
            run.ResultCollection.IsosResultBin.Clear();
            oldDeconvolutor.Execute(run.ResultCollection);

            List<IsosResult> oldResults = new List<IsosResult>(run.ResultCollection.ResultList);


            //Console.WriteLine("\n--------------Old decon ------------------");
            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);


            var sharedIsos = new List<IsosResult>();
            var uniqueToNew = new List<IsosResult>();
            var uniqueToOld = new List<IsosResult>();


            foreach (var newresult in newResults)
            {
                bool foundMatch = false;
                for (int i = 0; i < oldResults.Count; i++)
                {
                    var oldResult = oldResults[i];

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
                bool foundMatch = false;
                for (int i = 0; i < newResults.Count; i++)
                {
                    var newresult = newResults[i];

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


            Console.WriteLine("\n--------------Common to new and Old ------------------");
            TestUtilities.DisplayMSFeatures(sharedIsos);


            Console.WriteLine("\n--------------Unique to new ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToNew);

            string outputFilename = @"D:\temp\exportedIsos.csv";
            var exporter = IsosExporterFactory.CreateIsosExporter(run.ResultCollection.ResultType, Globals.ExporterType.Text ,outputFilename);

            exporter.ExportIsosResults(uniqueToNew);


            Console.WriteLine("\n--------------Unique to old ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToOld);

        }


        [Test]
        public void CompareOldAndNewDeconvolutorsUIMF()
        {
            string uimfFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS2_90_6Apr11_Cheetah_11-02-19.uimf";



            Run run = new RunFactory().CreateRun(uimfFile);

            int testLCScan = 500;
            int testIMSScan = 120;

            int numIMSScanSummed = 7;
            int lowerIMSScan = testIMSScan - (numIMSScanSummed - 1) / 2;
            int upperIMSScan = testIMSScan + (numIMSScanSummed - 1) / 2;

            var scanSet = new ScanSetFactory().CreateScanSet(run, testLCScan, 1);


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, Globals.PeakFitType.QUADRATIC, true);

            SavitzkyGolaySmoother smoother = new SavitzkyGolaySmoother(3, 2);


            var zeroFiller = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);


            var thrashParameters = new ThrashParameters();
            thrashParameters.MinMSFeatureToBackgroundRatio = 1;
            thrashParameters.MaxFit = 0.4;

            var newDeconvolutor = new ThrashDeconvolutorV2(thrashParameters);

            
            HornDeconvolutor oldDeconvolutor = new HornDeconvolutor();
            oldDeconvolutor.MinPeptideBackgroundRatio = 1;
            oldDeconvolutor.MaxFitAllowed = 0.4;

            run.CurrentScanSet = scanSet;
            ((UIMFRun)run).CurrentIMSScanSet = new IMSScanSet(testIMSScan, lowerIMSScan, upperIMSScan);


            msgen.Execute(run.ResultCollection);
            zeroFiller.Execute(run.ResultCollection);
            //smoother.Execute(run.ResultCollection);
            
            peakDetector.Execute(run.ResultCollection);
            
            newDeconvolutor.Execute(run.ResultCollection);

            //Console.WriteLine("\n--------------New decon ------------------");
            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            var newResults = new List<IsosResult>(run.ResultCollection.ResultList);

            TestUtilities.DisplayMSFeatures(newResults);

            //DisplayPPMErrorsForeachPeakOfMSFeature(newResults);

            return;

            run.ResultCollection.ResultList.Clear();
            run.ResultCollection.IsosResultBin.Clear();
            oldDeconvolutor.Execute(run.ResultCollection);

            List<IsosResult> oldResults = new List<IsosResult>(run.ResultCollection.ResultList);


            //Console.WriteLine("\n--------------Old decon ------------------");
            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);


            var sharedIsos = new List<IsosResult>();
            var uniqueToNew = new List<IsosResult>();
            var uniqueToOld = new List<IsosResult>();


            foreach (var newresult in newResults)
            {
                bool foundMatch = false;
                for (int i = 0; i < oldResults.Count; i++)
                {
                    var oldResult = oldResults[i];

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
                bool foundMatch = false;
                for (int i = 0; i < newResults.Count; i++)
                {
                    var newresult = newResults[i];

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


            Console.WriteLine("\n--------------Common to new and Old ------------------");
            TestUtilities.DisplayMSFeatures(sharedIsos);


            Console.WriteLine("\n--------------Unique to new ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToNew);

            Console.WriteLine("\n--------------Unique to old ------------------");
            TestUtilities.DisplayMSFeatures(uniqueToOld);

        }

        private void DisplayPPMErrorsForeachPeakOfMSFeature(List<IsosResult> newResults)
        {

            StringBuilder sb = new StringBuilder();
            var delimiter = '\t';

            foreach (var isosResult in newResults)
            {
                var indexOfMostIntensePeak = isosResult.IsotopicProfile.GetIndexOfMostIntensePeak();

                var maxPeak = isosResult.IsotopicProfile.Peaklist[indexOfMostIntensePeak];

                sb.Append(maxPeak.XValue);
                sb.Append(delimiter);
                sb.Append(isosResult.IsotopicProfile.ChargeState);
                sb.Append(delimiter);

                List<double> ppmErrors = new List<double>();

                for (int i = 0; i < isosResult.IsotopicProfile.Peaklist.Count; i++)
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
            string uimfFile =
             @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS2_90_6Apr11_Cheetah_11-02-19.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);

            int testLCScan = 500;
            int testIMSScan = 120;

            int numIMSScanSummed = 7;
            int lowerIMSScan = testIMSScan - (numIMSScanSummed - 1) / 2;
            int upperIMSScan = testIMSScan + (numIMSScanSummed - 1) / 2;

            var scanSet = new ScanSetFactory().CreateScanSet(run, testLCScan, 1);


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(2, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            var thrashParameters = new ThrashParameters();
            thrashParameters.MinMSFeatureToBackgroundRatio = 2;
            thrashParameters.MaxFit = 0.4;

            var newDeconvolutor = new ThrashDeconvolutorV2(thrashParameters);


            run.CurrentScanSet = scanSet;
            ((UIMFRun)run).CurrentIMSScanSet = new IMSScanSet(testIMSScan, lowerIMSScan, upperIMSScan);

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

            run.PeakList = (from n in run.PeakList where n.XValue > 554 && n.XValue < 559 select n).ToList();

            newDeconvolutor.Execute(run.ResultCollection);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            //TODO: figure out why duplicate MSFeatures are being created

        }


        [Test]
        public void OldDeconvolutorTest_temp1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            OldDecon2LSParameters parameters = new OldDecon2LSParameters();
            string paramFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash_MaxFit1.xml";
            parameters.Load(paramFile);

            ScanSet scanSet = new ScanSetFactory().CreateScanSet(run, 6005, 1);

            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);

            var deconvolutor = new HornDeconvolutor(parameters.HornTransformParameters);
            run.CurrentScanSet = scanSet;
            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            deconvolutor.Execute(run.ResultCollection);

            run.ResultCollection.ResultList = run.ResultCollection.ResultList.OrderByDescending(p => p.IntensityAggregate).ToList();

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            //IsosResult testIso = run.ResultCollection.ResultList[0];

            //TestUtilities.DisplayIsotopicProfileData(testIso.IsotopicProfile);

            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
            //TestUtilities.DisplayPeaks(run.PeakList);



        }
    }
}
