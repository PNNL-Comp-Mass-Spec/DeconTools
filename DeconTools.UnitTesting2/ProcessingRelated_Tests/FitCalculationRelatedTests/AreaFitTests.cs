using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using NUnit.Framework;


namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.FitCalculationRelatedTests
{
    [TestFixture]
    public class AreaFitTests
    {
        private readonly string xcaliburTestfile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;



        [Test]
        public void fitterOnHornDataTest1()
        {
            Run run = new XCaliburRun2(xcaliburTestfile);

            var results = new ResultCollection(run);
            run.CurrentScanSet = new ScanSet(6067);

            Task generator = new GenericMSGenerator(1154, 1158, isTicRequested: false);
            generator.Execute(results);

            Task peakDetector = new DeconToolsPeakDetectorV2(0.5, 3);
            peakDetector.Execute(results);

            var deconParameters = new DeconToolsParameters {ThrashParameters = {
                    MinMSFeatureToBackgroundRatio = 2            // PeptideMinBackgroundRatio
                }
            };

            Task decon = new HornDeconvolutor(deconParameters);
            decon.Execute(results);

            if (results.ResultList.Count == 0)
                Assert.Fail("Result list is empty");

            var result1 = results.ResultList[0];


            var distCreator = new MercuryDistributionCreator();
            var resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / result1.IsotopicProfile.GetFWHM();
            distCreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            distCreator.OffsetDistribution(result1.IsotopicProfile);



            var theorXYData = distCreator.Data;

            //StringBuilder sb = new StringBuilder();
            //TestUtilities.GetXYValuesToStringBuilder(sb, theorXYData.Xvalues, theorXYData.Yvalues);

            //Console.WriteLine(sb.ToString());

            var areafitter = new AreaFitter();
            var fitValsByShift = new Dictionary<int, double>();
            var bestFitViaShifting = 1.0;

            for (var shift = -4; shift <= 4; shift++)
            {
                var offset = -1 * shift * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / result1.IsotopicProfile.ChargeState;
                var fitval = areafitter.GetFit(theorXYData, run.XYData, 10, offset);

                fitValsByShift.Add(shift, fitval);

                if (!double.IsNaN(fitval))
                    bestFitViaShifting = Math.Min(bestFitViaShifting, fitval);
            }

            Console.WriteLine("{0,-8}  {1}", "Shift", "Fit");
            foreach (var item in fitValsByShift)
            {
                Console.WriteLine("{0,2}       {1,8:F5}", item.Key, item.Value);
            }

            Console.WriteLine();
            Console.WriteLine("{0,12}   {1,8}", "Profile Score", "Best Fit");
            Console.WriteLine("{0,8:F5}       {1,8:F5}", result1.IsotopicProfile.Score, bestFitViaShifting);

            Assert.AreEqual(0.00926548, bestFitViaShifting, 0.00001);

        }

        // To include support for Rapid, you must add a reference to DeconEngine.dll, which was compiled with Visual Studio 2003 and uses MSVCP71.dll
        // Note that DeconEngine.dll also depends on xerces-c_2_7.dll while DeconEngineV2.dll depends on xerces-c_2_8.dll
#if INCLUDE_RAPID

        [Test]
        public void fitterOnRapidDataTest1()
        {
            Run run = new XCaliburRun2(xcaliburTestfile);

            ResultCollection results = new ResultCollection(run);
            run.CurrentScanSet = new ScanSet(6067);

            bool isTicRequested = false;
            Task generator = new GenericMSGenerator(1154, 1158, isTicRequested);
            generator.Execute(results);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 0.5;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;

            Task peakDetector = new DeconToolsPeakDetectorV2(detectorParams);
            peakDetector.Execute(results);


            Task decon = new RapidDeconvolutor();
            decon.Execute(results);


            IsosResult result1 = results.ResultList[0];
            double resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / result1.IsotopicProfile.GetFWHM();


            MercuryDistributionCreator distCreator = new MercuryDistributionCreator();
            distCreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            distCreator.OffsetDistribution(result1.IsotopicProfile);



            XYData theorXYData = distCreator.Data;

           // theorXYData.Display();

            AreaFitter areafitter = new AreaFitter();
            double fitval = areafitter.GetFit(theorXYData, run.XYData, 10);

            Console.WriteLine(result1.IsotopicProfile.Score + "\t" + fitval);
            Console.WriteLine((result1.IsotopicProfile.Score - fitval) / result1.IsotopicProfile.Score * 100);

            Assert.AreEqual(0.0207350903681061m, (decimal)fitval);

        }
#endif

        [Test]
        [Ignore("No results")]
        public void fitterOnHornDataTest2()
        {
            Run run = new XCaliburRun2(xcaliburTestfile);

            var results = new ResultCollection(run);
            run.CurrentScanSet = new ScanSet(6005);

            Task generator = new GenericMSGenerator(579, 582, isTicRequested: false);
            generator.Execute(results);

            Task peakDetector = new DeconToolsPeakDetectorV2(0.5, 3);
            peakDetector.Execute(results);


            Task decon = new HornDeconvolutor();
            decon.Execute(results);

            if (results.ResultList.Count == 0)
                Assert.Fail("Result list is empty");

            var result1 = results.ResultList[1];
            var resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / result1.IsotopicProfile.GetFWHM();

            var distCreator = new MercuryDistributionCreator();
            distCreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            var theorXYData = distCreator.Data;

            //TestUtilities.DisplayXYValues(theorXYData);

            distCreator.OffsetDistribution(result1.IsotopicProfile);
            
            TestUtilities.DisplayXYValues(theorXYData);

            //Console.WriteLine(sb.ToString());

            var areafitter = new AreaFitter();
            var fitval = areafitter.GetFit(theorXYData, run.XYData, 10);

            Console.WriteLine(result1.IsotopicProfile.Score + "\t" + fitval);
            Console.WriteLine((result1.IsotopicProfile.Score - fitval) / result1.IsotopicProfile.Score * 100);

            Assert.AreEqual(0.0763818319332606m, (decimal)fitval);

        }


        [Test]
        [Ignore("No results")]
        public void effectOfFWHMOnFitTest()
        {
            Run run = new XCaliburRun2(xcaliburTestfile);

            var results = new ResultCollection(run);
            run.CurrentScanSet = new ScanSet(6005);

            Task generator = new GenericMSGenerator(1154, 1160, isTicRequested: false);
            generator.Execute(results);

            Task peakDetector = new DeconToolsPeakDetectorV2(0.5, 3);
            peakDetector.Execute(results);


            Task decon = new HornDeconvolutor();
            decon.Execute(results);

            if (results.ResultList.Count == 0)
                Assert.Fail("Result list is empty");

            var result1 = results.ResultList[0];
            var resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / result1.IsotopicProfile.GetFWHM();



            var distCreator = new MercuryDistributionCreator();
            distCreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            distCreator.OffsetDistribution(result1.IsotopicProfile);

            var sb = new StringBuilder();

            var areafitter = new AreaFitter();
            var fitval = areafitter.GetFit(distCreator.Data, run.XYData, 10);

            sb.Append(resolution);
            sb.Append("\t");
            sb.Append(fitval);
            sb.Append("\n");

            for (var fwhm = 0.001; fwhm < 0.050; fwhm = fwhm + 0.0005)
            {
                distCreator = new MercuryDistributionCreator();
                resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / fwhm;

                distCreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
                distCreator.OffsetDistribution(result1.IsotopicProfile);
                areafitter = new AreaFitter();
                fitval = areafitter.GetFit(distCreator.Data, run.XYData, 10);

                sb.Append(resolution);
                sb.Append("\t");
                sb.Append(fitval);
                sb.Append("\n");
            }

            //Console.Write(sb.ToString());
        }


    }
}
