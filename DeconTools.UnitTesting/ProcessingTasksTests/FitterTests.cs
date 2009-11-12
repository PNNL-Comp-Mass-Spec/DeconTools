using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend;


namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class FitterTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        [Test]
        public void fitterOnHornDataTest1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            ResultCollection results = new ResultCollection(run);
            run.CurrentScanSet = new ScanSet(6067);


            Task msGen = new GenericMSGenerator(1154, 1158);
            msGen.Execute(results);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 0.5;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;

            Task peakDetector = new DeconToolsPeakDetector(detectorParams);
            peakDetector.Execute(results);


            DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            hornParameters.PeptideMinBackgroundRatio = 2;

            Task decon = new HornDeconvolutor(hornParameters);
            decon.Execute(results);


            IsosResult result1 = results.ResultList[0];


            MercuryDistributionCreator distcreator = new MercuryDistributionCreator();
            double resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / result1.IsotopicProfile.GetFWHM();
            distcreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            distcreator.OffsetDistribution(result1.IsotopicProfile);



            XYData theorXYData = distcreator.Data;

            StringBuilder sb = new StringBuilder();
            TestUtilities.GetXYValuesToStringBuilder(sb, theorXYData.Xvalues, theorXYData.Yvalues);

            Console.WriteLine(sb.ToString());

            AreaFitter areafitter = new AreaFitter(theorXYData, run.XYData, 10);
            double fitval = areafitter.getFit();

            Console.WriteLine(result1.IsotopicProfile.Score + "\t" + fitval);
            Console.WriteLine((result1.IsotopicProfile.Score - fitval) / result1.IsotopicProfile.Score);

            Assert.AreEqual(0.0209385414928986, (decimal)fitval);    //TODO: fix this test... i'm getting 0.0207350903681061m

        }

        [Test]
        public void fitterOnRapidDataTest1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            ResultCollection results = new ResultCollection(run);
            run.CurrentScanSet = new ScanSet(6067);


            Task msGen = new GenericMSGenerator(1154, 1158);
            msGen.Execute(results);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 0.5;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;

            Task peakDetector = new DeconToolsPeakDetector(detectorParams);
            peakDetector.Execute(results);


            Task decon = new RapidDeconvolutor();
            decon.Execute(results);


            IsosResult result1 = results.ResultList[0];
            double resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / result1.IsotopicProfile.GetFWHM();


            MercuryDistributionCreator distcreator = new MercuryDistributionCreator();
            distcreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            distcreator.OffsetDistribution(result1.IsotopicProfile);



            XYData theorXYData = distcreator.Data;

            StringBuilder sb = new StringBuilder();
            TestUtilities.GetXYValuesToStringBuilder(sb, theorXYData.Xvalues, theorXYData.Yvalues);

            Console.WriteLine(sb.ToString());

            AreaFitter areafitter = new AreaFitter(theorXYData, run.XYData, 10);
            double fitval = areafitter.getFit();

            Console.WriteLine(result1.IsotopicProfile.Score + "\t" + fitval);
            Console.WriteLine((result1.IsotopicProfile.Score - fitval) / result1.IsotopicProfile.Score * 100);

            Assert.AreEqual(0.021245316579128, (decimal)fitval);

        }


        [Test]
        public void fitterOnHornDataTest2()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            ResultCollection results = new ResultCollection(run);
            run.CurrentScanSet = new ScanSet(6005);


            Task msGen = new GenericMSGenerator(579, 582);
            msGen.Execute(results);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 0.5;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;

            Task peakDetector = new DeconToolsPeakDetector(detectorParams);
            peakDetector.Execute(results);


            Task decon = new HornDeconvolutor();
            decon.Execute(results);


            IsosResult result1 = results.ResultList[1];
            double resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / result1.IsotopicProfile.GetFWHM();

            MercuryDistributionCreator distcreator = new MercuryDistributionCreator();
            distcreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            StringBuilder sb = new StringBuilder();
            XYData theorXYData = distcreator.Data;

            //TestUtilities.GetXYValuesToStringBuilder(sb, theorXYData.Xvalues, theorXYData.Yvalues);



            distcreator.OffsetDistribution(result1.IsotopicProfile);





            TestUtilities.GetXYValuesToStringBuilder(sb, theorXYData.Xvalues, theorXYData.Yvalues);

            Console.WriteLine(sb.ToString());

            AreaFitter areafitter = new AreaFitter(theorXYData, run.XYData, 10);
            double fitval = areafitter.getFit();

            Console.WriteLine(result1.IsotopicProfile.Score + "\t" + fitval);
            Console.WriteLine((result1.IsotopicProfile.Score - fitval) / result1.IsotopicProfile.Score * 100);

            Assert.AreEqual(0.0704215577242672, (decimal)fitval);

        }


        [Test]
        public void effectOfFWHMOnFitTest()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            ResultCollection results = new ResultCollection(run);
            run.CurrentScanSet = new ScanSet(6005);


            Task msGen = new GenericMSGenerator(1154, 1160);
            msGen.Execute(results);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 0.5;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;

            Task peakDetector = new DeconToolsPeakDetector(detectorParams);
            peakDetector.Execute(results);


            Task decon = new HornDeconvolutor();
            decon.Execute(results);

            IsosResult result1 = results.ResultList[0];
            double resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / result1.IsotopicProfile.GetFWHM();



            MercuryDistributionCreator distcreator = new MercuryDistributionCreator();
            distcreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
            distcreator.OffsetDistribution(result1.IsotopicProfile);

            StringBuilder sb = new StringBuilder();

            AreaFitter areafitter = new AreaFitter(distcreator.Data, run.XYData, 10);
            double fitval = areafitter.getFit();

            sb.Append(resolution);
            sb.Append("\t");
            sb.Append(fitval);
            sb.Append("\n");

            for (double fwhm = 0.001; fwhm < 0.050; fwhm = fwhm + 0.0005)
            {
                distcreator = new MercuryDistributionCreator();
                resolution = result1.IsotopicProfile.GetMZofMostAbundantPeak() / fwhm;
                
                distcreator.CreateDistribution(result1.IsotopicProfile.MonoIsotopicMass, result1.IsotopicProfile.ChargeState, resolution);
                distcreator.OffsetDistribution(result1.IsotopicProfile);
                areafitter = new AreaFitter(distcreator.Data, run.XYData, 10);
                fitval = areafitter.getFit();

                sb.Append(resolution);
                sb.Append("\t");
                sb.Append(fitval);
                sb.Append("\n");
            }

            //Console.Write(sb.ToString());
        }

    }
}
