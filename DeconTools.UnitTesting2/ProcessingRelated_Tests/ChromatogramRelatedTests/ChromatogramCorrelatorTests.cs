using System;
using System.Diagnostics;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ChromatogramRelatedTests
{
    [TestFixture]
    public class ChromatogramCorrelatorTests
    {
        [Category("MustPass")]
        [Test]
        public void CorrelationTest1()
        {
            //TODO: test something

            var run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var peakImporter = new PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            var mt = TestUtilities.GetMassTagStandard(1);
            run.CurrentMassTag = mt;

            var unlabeledTheorGenerator = new JoshTheorFeatureGenerator();
            unlabeledTheorGenerator.GenerateTheorFeature(mt);

            double chromToleranceInPPM = 10;
            var startScan = 5460;
            var stopScan = 5755;

            var smoother = new SavitzkyGolaySmoother(3, 2);

            var peakChromGen = new PeakChromatogramGenerator(chromToleranceInPPM);
            run.XYData = peakChromGen.GenerateChromatogram(run, startScan, stopScan, mt.IsotopicProfile.Peaklist[0].XValue, chromToleranceInPPM);
            run.XYData = smoother.Smooth(run.XYData);

            var chromdata1 = run.XYData.TrimData(startScan, stopScan);


            run.XYData = peakChromGen.GenerateChromatogram(run, startScan, stopScan, mt.IsotopicProfile.Peaklist[3].XValue, chromToleranceInPPM);
            run.XYData = smoother.Smooth(run.XYData);

            var chromdata2 = run.XYData.TrimData(startScan, stopScan);

            //chromdata1.Display();
            //Console.WriteLine();
            //chromdata2.Display();

            ChromatogramCorrelatorBase correlator = new ChromatogramCorrelator(3);

            correlator.GetElutionCorrelationData(chromdata1, chromdata2, out var slope, out var intercept, out var rsquaredVal);

            Console.WriteLine(mt);

            Console.WriteLine("slope = \t" + slope);
            Console.WriteLine("intercept = \t" + intercept);
            Console.WriteLine("rsquared = \t" + rsquaredVal);


            for (var i = 0; i < chromdata1.Xvalues.Length; i++)
            {
                Console.WriteLine(chromdata1.Xvalues[i].ToString("0") + "\t" + chromdata1.Yvalues[i].ToString("0") + "\t" + chromdata2.Yvalues[i]);
            }


        }

        [Test]
        public void CorrelationTest2_UsingExecutorMethod()
        {
            var run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var peakImporter = new PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            var mt = TestUtilities.GetMassTagStandard(1);
            run.CurrentMassTag = mt;

            var unlabeledTheorGenerator = new JoshTheorFeatureGenerator();
            unlabeledTheorGenerator.GenerateTheorFeature(mt);

            double chromToleranceInPPM = 10;
            var startScan = 5460;
            var stopScan = 5755;

            var smoother = new SavitzkyGolaySmoother(3, 2);

            var peakChromGen = new PeakChromatogramGenerator(chromToleranceInPPM);
            run.XYData = peakChromGen.GenerateChromatogram(run, startScan, stopScan, mt.IsotopicProfile.Peaklist[0].XValue, chromToleranceInPPM);
            run.XYData = smoother.Smooth(run.XYData);

            var result = run.ResultCollection.GetTargetedResult(mt);

            ChromatogramCorrelatorBase correlator = new ChromatogramCorrelator(3);
            var corrData = correlator.CorrelatePeaksWithinIsotopicProfile(run, mt.IsotopicProfile, startScan, stopScan);

            Debug.Assert(corrData.CorrelationDataItems != null, "corrData.CorrelationDataItems != null");
            Assert.AreEqual(0.98m, (decimal)Math.Round((double)corrData.CorrelationDataItems[1].CorrelationRSquaredVal, 2));

            foreach (var item in corrData.CorrelationDataItems)
            {
                Console.WriteLine(item.CorrelationRSquaredVal);

            }


        }


        [Test]
        public void BadCorrelationTest1()
        {
            var dataset =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\RawData\Alz_P01_A01_097_26Apr12_Roc_12-03-15.RAW";

            var run = new RunFactory().CreateRun(dataset);

            var peaksDataFile = dataset.ToLower().Replace(".raw", "_peaks.txt");
            var peakImporter = new PeakImporterFromText(peaksDataFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            double chromToleranceInPPM = 10;
            var startScan = 2340;
            var stopScan = 2440;

            var smoother = new SavitzkyGolaySmoother(9, 2);

            var testMZVal1 = 719.80349;

            var peakChromGen = new PeakChromatogramGenerator(chromToleranceInPPM);
            run.XYData = peakChromGen.GenerateChromatogram(run, startScan, stopScan, testMZVal1, chromToleranceInPPM);
            run.XYData = smoother.Smooth(run.XYData);

            var chromdata1 = run.XYData.TrimData(startScan, stopScan);

            var testMZVal2 = 722.325;
            run.XYData = peakChromGen.GenerateChromatogram(run, startScan, stopScan, testMZVal2, chromToleranceInPPM);
            run.XYData = smoother.Smooth(run.XYData);

            var chromdata2 = run.XYData.TrimData(startScan, stopScan);



            //chromdata1.Display();
            //Console.WriteLine();
            //chromdata2.Display();

            ChromatogramCorrelatorBase correlator = new ChromatogramCorrelator(3);

            correlator.GetElutionCorrelationData(chromdata1, chromdata2, out var slope, out var intercept, out var rsquaredVal);


            Console.WriteLine("slope = \t" + slope);
            Console.WriteLine("intercept = \t" + intercept);
            Console.WriteLine("rsquared = \t" + rsquaredVal);


            for (var i = 0; i < chromdata1.Xvalues.Length; i++)
            {
                Console.WriteLine(chromdata1.Xvalues[i] + "\t" + chromdata1.Yvalues[i] + "\t" + chromdata2.Yvalues[i]);
            }


        }


    }
}
