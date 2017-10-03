using System;
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
        private string xcaliburTestfile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;



        [Test]
        public void fitterOnHornDataTest1()
        {
            Run run = new XCaliburRun2(xcaliburTestfile);

            var results = new ResultCollection(run);
            run.CurrentScanSet = new ScanSet(6067);

            var isTicRequested = false;
            Task msGen = new GenericMSGenerator(1154, 1158, isTicRequested);
            msGen.Execute(results);

            Task peakDetector = new DeconToolsPeakDetectorV2(0.5, 3, Globals.PeakFitType.QUADRATIC, false);
            peakDetector.Execute(results);

            var deconParameters = new DeconToolsParameters();
            deconParameters.ThrashParameters.MinMSFeatureToBackgroundRatio = 2;     // PeptideMinBackgroundRatio

            Task decon = new HornDeconvolutor(deconParameters);
            decon.Execute(results);


            var result1 = results.ResultList[0];


            var distcreator = new MercuryDistributionCreator();
            var resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / result1.IsotopicProfile.GetFWHM();
            distcreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            distcreator.OffsetDistribution(result1.IsotopicProfile);



            var theorXYData = distcreator.Data;

            //StringBuilder sb = new StringBuilder();
            //TestUtilities.GetXYValuesToStringBuilder(sb, theorXYData.Xvalues, theorXYData.Yvalues);

            //Console.WriteLine(sb.ToString());

            var areafitter = new AreaFitter();
            var fitval = areafitter.GetFit(theorXYData, run.XYData, 10);

            Console.WriteLine(result1.IsotopicProfile.Score + "\t" + fitval);
            Console.WriteLine((result1.IsotopicProfile.Score - fitval) / result1.IsotopicProfile.Score);

            Assert.AreEqual(0.0207350903681061m, (decimal)fitval);    //TODO: fix this test... i'm getting 0.0207350903681061m

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
            Task msGen = new GenericMSGenerator(1154, 1158, isTicRequested);
            msGen.Execute(results);

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


            MercuryDistributionCreator distcreator = new MercuryDistributionCreator();
            distcreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            distcreator.OffsetDistribution(result1.IsotopicProfile);



            XYData theorXYData = distcreator.Data;

           // theorXYData.Display();

            AreaFitter areafitter = new AreaFitter();
            double fitval = areafitter.GetFit(theorXYData, run.XYData, 10);

            Console.WriteLine(result1.IsotopicProfile.Score + "\t" + fitval);
            Console.WriteLine((result1.IsotopicProfile.Score - fitval) / result1.IsotopicProfile.Score * 100);

            Assert.AreEqual(0.0207350903681061m, (decimal)fitval);

        }
#endif

        [Test]
        public void fitterOnHornDataTest2()
        {
            Run run = new XCaliburRun2(xcaliburTestfile);

            var results = new ResultCollection(run);
            run.CurrentScanSet = new ScanSet(6005);

            var isTicRequested = false;
            Task msGen = new GenericMSGenerator(579, 582, isTicRequested);
            msGen.Execute(results);

            Task peakDetector = new DeconToolsPeakDetectorV2(0.5, 3, Globals.PeakFitType.QUADRATIC, false);
            peakDetector.Execute(results);


            Task decon = new HornDeconvolutor();
            decon.Execute(results);


            var result1 = results.ResultList[1];
            var resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / result1.IsotopicProfile.GetFWHM();

            var distcreator = new MercuryDistributionCreator();
            distcreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            var sb = new StringBuilder();
            var theorXYData = distcreator.Data;

            //TestUtilities.GetXYValuesToStringBuilder(sb, theorXYData.Xvalues, theorXYData.Yvalues);



            distcreator.OffsetDistribution(result1.IsotopicProfile);





            TestUtilities.GetXYValuesToStringBuilder(sb, theorXYData.Xvalues, theorXYData.Yvalues);

            //Console.WriteLine(sb.ToString());

            var areafitter = new AreaFitter();
            var fitval = areafitter.GetFit(theorXYData, run.XYData, 10);

            Console.WriteLine(result1.IsotopicProfile.Score + "\t" + fitval);
            Console.WriteLine((result1.IsotopicProfile.Score - fitval) / result1.IsotopicProfile.Score * 100);

            Assert.AreEqual(0.0763818319332606m, (decimal)fitval);

        }


        [Test]
        public void effectOfFWHMOnFitTest()
        {
            Run run = new XCaliburRun2(xcaliburTestfile);

            var results = new ResultCollection(run);
            run.CurrentScanSet = new ScanSet(6005);

            var isTicRequested = false;
            Task msGen = new GenericMSGenerator(1154, 1160, isTicRequested);
            msGen.Execute(results);

            Task peakDetector = new DeconToolsPeakDetectorV2(0.5, 3, Globals.PeakFitType.QUADRATIC, false);
            peakDetector.Execute(results);


            Task decon = new HornDeconvolutor();
            decon.Execute(results);

            var result1 = results.ResultList[0];
            var resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / result1.IsotopicProfile.GetFWHM();



            var distcreator = new MercuryDistributionCreator();
            distcreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            distcreator.OffsetDistribution(result1.IsotopicProfile);

            var sb = new StringBuilder();

            var areafitter = new AreaFitter();
            var fitval = areafitter.GetFit(distcreator.Data, run.XYData, 10);

            sb.Append(resolution);
            sb.Append("\t");
            sb.Append(fitval);
            sb.Append("\n");

            for (var fwhm = 0.001; fwhm < 0.050; fwhm = fwhm + 0.0005)
            {
                distcreator = new MercuryDistributionCreator();
                resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / fwhm;

                distcreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
                distcreator.OffsetDistribution(result1.IsotopicProfile);
                areafitter = new AreaFitter();
                fitval = areafitter.GetFit(distcreator.Data, run.XYData, 10);

                sb.Append(resolution);
                sb.Append("\t");
                sb.Append(fitval);
                sb.Append("\n");
            }

            //Console.Write(sb.ToString());
        }


    }
}
