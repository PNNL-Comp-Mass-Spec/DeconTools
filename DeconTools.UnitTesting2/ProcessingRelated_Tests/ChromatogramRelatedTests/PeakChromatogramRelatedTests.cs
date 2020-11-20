using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ChromatogramRelatedTests
{
    [TestFixture]
    public class PeakChromatogramRelatedTests
    {
        [Test]
        public void GetPeakChromatogram_IQStyle_Test1()
        {
            var run = RunUtilities.CreateAndLoadPeaks(FileRefs.RawDataMSFiles.OrbitrapStdFile1,
                                                      FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);

            var target = TestUtilities.GetIQTargetStandard(1);

            //TestUtilities.DisplayIsotopicProfileData(target.TheorIsotopicProfile);

            var chromGen = new PeakChromatogramGenerator
            {
                ChromatogramGeneratorMode = Globals.ChromatogramGeneratorMode.TOP_N_PEAKS,
                TopNPeaksLowerCutOff = 0.4,
                Tolerance = 10
            };

            var chromXYData = chromGen.GenerateChromatogram(run, target.TheorIsotopicProfile, target.ElutionTimeTheor);

            Assert.IsNotNull(chromXYData);

            TestUtilities.DisplayXYValues(chromXYData);
        }

        [Test]
        public void GetPeakChromatogram_useMZTolerance_Test1()
        {
            var run = RunUtilities.CreateAndLoadPeaks(FileRefs.RawDataMSFiles.OrbitrapStdFile1,
                                                      FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);

            var targetMZ = 759.4032;
            var startScan = 5500;
            var stopScan = 6500;

            var toleranceInMZ = 0.02;

            var chromGen = new PeakChromatogramGenerator();
            var xyData = chromGen.GenerateChromatogram(run, startScan, stopScan, targetMZ, toleranceInMZ, Globals.ToleranceUnit.MZ);

            Assert.IsNotNull(xyData);
            TestUtilities.DisplayXYValues(xyData);
        }

        [Test]
        public void GetPeakChromatogram_wideMZTolerance_Test1()
        {
            var run = RunUtilities.CreateAndLoadPeaks(FileRefs.RawDataMSFiles.OrbitrapStdFile1,
                                                      FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);

            var targetMZ = 759.4032;
            var startScan = 5500;
            var stopScan = 6500;

            var toleranceInMZ = 1.0;

            var chromGen = new PeakChromatogramGenerator();
            var xyData = chromGen.GenerateChromatogram(run, startScan, stopScan, targetMZ, toleranceInMZ, Globals.ToleranceUnit.MZ);

            Assert.IsNotNull(xyData);
            TestUtilities.DisplayXYValues(xyData);
        }

        [Test]
        public void GetPeakChromatogram_usePPMTolerance_Test1()
        {
            var run = RunUtilities.CreateAndLoadPeaks(FileRefs.RawDataMSFiles.OrbitrapStdFile1,
                                                      FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);

            var targetMZ = 759.4032;
            var startScan = 5500;
            var stopScan = 6500;

            double toleranceInPPM = 20;

            var chromGen = new PeakChromatogramGenerator();
            var xyData = chromGen.GenerateChromatogram(run, startScan, stopScan, targetMZ, toleranceInPPM);

            Assert.IsNotNull(xyData);
            TestUtilities.DisplayXYValues(xyData);
        }

        [Category("MustPass")]
        [Test]
        public void getPeakChromatogramTest1()
        {
            var run = RunUtilities.CreateAndLoadPeaks(FileRefs.RawDataMSFiles.OrbitrapStdFile1,
                                                    FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);

            double toleranceInPPM = 20;

            var mt = TestUtilities.GetMassTagStandard(1);
            run.CurrentMassTag = mt;

            var unlabeledTheorGenerator = new JoshTheorFeatureGenerator();
            unlabeledTheorGenerator.GenerateTheorFeature(mt);

            var peakChromGen = new PeakChromatogramGenerator(toleranceInPPM);
            peakChromGen.Execute(run.ResultCollection);

            run.XYData.Display();

            Assert.AreEqual(133, run.XYData.Xvalues.Length);
            Assert.AreEqual(5543, (int)run.XYData.Xvalues[35]);
            Assert.AreEqual(7319569, (int)run.XYData.Yvalues[35]);

            //make sure that the 0's are not trimmed off
            Assert.AreEqual(0, run.XYData.Yvalues[1]);
        }

        //SK
        [Test]
        public void GetPeakChromatogram_IQStyle_Test2()
        {
            var run = RunUtilities.CreateAndLoadPeaks(FileRefs.RawDataMSFiles.OrbitrapStdFile1, FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);

            var target = TestUtilities.GetIQTargetStandard(1);

            var chromGen = new PeakChromatogramGenerator
            {
                ChromatogramGeneratorMode = Globals.ChromatogramGeneratorMode.TOP_N_PEAKS,
                TopNPeaksLowerCutOff = 0.4,
                Tolerance = 100
            };

            var lowerScan = 6087;
            var upperScan = 6418;

            var chromXYData = chromGen.GenerateChromatogram(run, target.TheorIsotopicProfile, lowerScan, upperScan, chromGen.Tolerance, chromGen.ToleranceUnit);
            Assert.IsNotNull(chromXYData);
            Assert.AreEqual(105, chromXYData.Xvalues.Length);

            TestUtilities.DisplayXYValues(chromXYData);
        }

        //SK
        [Test]
        public void GetPeakChromatogram_IQStyle_Test3()
        {
            var run = RunUtilities.CreateAndLoadPeaks(FileRefs.RawDataMSFiles.OrbitrapStdFile1, FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);

            var target = TestUtilities.GetIQTargetStandard(1);

            var chromGen = new PeakChromatogramGenerator
            {
                ChromatogramGeneratorMode = Globals.ChromatogramGeneratorMode.TOP_N_PEAKS,
                TopNPeaksLowerCutOff = 0.4,
                Tolerance = 100
            };

            var lowerScan = 6087;
            var upperScan = 6418;
            var massOfInterest = new List<double> {
                target.TheorIsotopicProfile.Peaklist[0].XValue,
                target.TheorIsotopicProfile.Peaklist[1].XValue
            };

            var chromXYData = chromGen.GenerateChromatogram(run, massOfInterest, lowerScan, upperScan, chromGen.Tolerance, chromGen.ToleranceUnit);
            Assert.IsNotNull(chromXYData);
            Assert.AreEqual(96, chromXYData.Xvalues.Length);

            TestUtilities.DisplayXYValues(chromXYData);
        }

        [Test]
        public void getPeakChromatogramInTheFormOfMSPeakResult_Test1()
        {
            double chromToleranceInPPM = 20;
            var targetMZ = 831.48;

            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var peakImporter = new Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            var chromGen = new ChromatogramGenerator();
            var filteredMSPeaks = chromGen.GeneratePeakChromatogram(run.ResultCollection.MSPeakResultList, run.MinLCScan, run.MaxLCScan, targetMZ, chromToleranceInPPM);

            //Assert.AreEqual(56, filteredMSPeaks.Count);

            TestUtilities.DisplayMSPeakResults(filteredMSPeaks);
        }

        [Test]
        public void getPeakChromAndStoreInPeakChromObject_Test1()
        {
            double chromToleranceInPPM = 20;
            var targetMZ = 831.48;

            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var peakImporter = new Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            var chromGen = new ChromatogramGenerator();

            PeakChrom chrom = new BasicPeakChrom();
            chrom.ChromSourceData = chromGen.GeneratePeakChromatogram(run.ResultCollection.MSPeakResultList, run.MinLCScan, run.MaxLCScan, targetMZ, chromToleranceInPPM);

            Assert.AreEqual(59, chrom.ChromSourceData.Count);
        }

        [Test]
        public void getPeakChromatogramTest2()
        {
            var mt = TestUtilities.GetMassTagStandard(1);

            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var peakImporter = new Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.CurrentMassTag = mt;

            var unlabeledTheorGenerator = new TomTheorFeatureGenerator();
            unlabeledTheorGenerator.GenerateTheorFeature(mt);

            var peakChromGen = new PeakChromatogramGenerator(10, Globals.ChromatogramGeneratorMode.TOP_N_PEAKS)
            {
                TopNPeaksLowerCutOff = 0.4
            };

            peakChromGen.Execute(run.ResultCollection);

            Assert.AreEqual(133, run.XYData.Xvalues.Length);
            Assert.AreEqual(5543, (int)run.XYData.Xvalues[35]);
            //Assert.AreEqual(7319569, (int)run.XYData.Yvalues[35]);
            run.XYData.Display();
        }

        [Test]
        public void getPeakChromatogramUsingChromGenTest1()
        {
            var targetMZ = 831.48;
            double toleranceInPPM = 20;

            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var peakImporter = new Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            var chromGen = new ChromatogramGenerator();
            run.XYData = chromGen.GenerateChromatogram(run.ResultCollection.MSPeakResultList, run.MinLCScan, run.MaxLCScan, targetMZ, toleranceInPPM, 1);

            for (var i = 0; i < run.XYData.Xvalues.Length; i++)
            {
                run.XYData.Xvalues[i] = run.GetTime(i);
            }

            // run.XYData.Display();

            var sb = new StringBuilder();
            var sortedList = run.ResultCollection.MSPeakResultList.OrderBy(p => p.MSPeak.XValue);
            foreach (var peak in sortedList)
            {
                if (peak.MSPeak.XValue > (targetMZ - 0.1) && peak.MSPeak.XValue < (targetMZ + 0.1))
                {
                    sb.Append(peak.ChromID);
                    sb.Append("\t");
                    sb.Append(peak.PeakID);
                    sb.Append("\t");
                    sb.Append(peak.Scan_num);
                    sb.Append("\t");
                    sb.Append(peak.MSPeak.XValue);
                    sb.Append("\t");
                    sb.Append(peak.MSPeak.Height);
                    sb.Append(Environment.NewLine);
                }
            }

            Console.WriteLine(sb.ToString());
        }

        [Test]
        public void getChromatogramsSpeedTest1()
        {
            var targetMZ = 831.48;
            double toleranceInPPM = 20;

            var totalIterations = 40;

            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var peakImporter = new Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            var chromGen = new ChromatogramGenerator();

            var speedResults = new List<long>();

            var sw = new Stopwatch();

            var startScan = 5500;
            var stopScan = 8500;

            for (var i = 0; i < totalIterations; i++)
            {
                sw.Reset();
                sw.Start();

                run.XYData = chromGen.GenerateChromatogram(run.ResultCollection.MSPeakResultList, startScan, stopScan, (targetMZ + totalIterations), toleranceInPPM, 1);
                sw.Stop();

                speedResults.Add(sw.ElapsedMilliseconds);
            }

            Console.WriteLine("Average time = " + speedResults.Average());
        }

        //[Test]
        //public void getPeakChromatogramsForManyPeaks_Test1()
        //{
        //    Dictionary<long, int> peakFrequency = new Dictionary<long, int>();

        //    Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

        //    PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
        //    peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

        //    ChromatogramGenerator chromGen = new ChromatogramGenerator();
        //    // PeakChromatogramGenerator peakChromGen = new PeakChromatogramGenerator(

        //    long timeForSort = 0;
        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();

        //    var sortedList = run.ResultCollection.MSPeakResultList.OrderByDescending(p => p.MSPeak.Height);
        //    sw.Stop();
        //    timeForSort = sw.ElapsedMilliseconds;

        //    long timeToList = 0;
        //    sw = new Stopwatch();
        //    sw.Start();

        //    List<MSPeakResult> sortedMSPeakResultList = sortedList.ToList();
        //    sw.Stop();
        //    timeToList = sw.ElapsedMilliseconds;

        //    List<long> chromGenTimes = new List<long>();

        //    Stopwatch allChromsStopwatch = new Stopwatch();
        //    allChromsStopwatch.Start();

        //    Stopwatch chromGenStopwatch = new Stopwatch();

        //    //iterate over the MSPeakResults and pull out chromatograms

        //    int counter = -1;
        //    foreach (var peakResult in sortedMSPeakResultList)
        //    {
        //        counter++;

        //        if (counter > 1000) break;
        //        if (peakResult.ChromID != -1)
        //        {
        //            continue;
        //        }
        //        chromGenStopwatch.Reset();
        //        chromGenStopwatch.Start();
        //        run.XYData = chromGen.GenerateChromatogram(run.ResultCollection.MSPeakResultList, run.MinScan, run.MaxScan, peakResult.MSPeak.XValue, 20, counter);
        //        chromGenStopwatch.Stop();
        //        chromGenTimes.Add(chromGenStopwatch.ElapsedMilliseconds);

        //    }

        //    allChromsStopwatch.Stop();

        //    Console.WriteLine("Original peaklist count = " + run.ResultCollection.MSPeakResultList.Count);
        //    Console.WriteLine("Sort time = " + timeForSort);
        //    Console.WriteLine("Conversion to List = " + timeToList);
        //    Console.WriteLine("Number of times chrom generated = " + chromGenTimes.Count);
        //    Console.WriteLine("Average chrom times = " + chromGenTimes.Average());
        //    Console.WriteLine("Total chrom time in seconds = " + (double)allChromsStopwatch.ElapsedMilliseconds / 1000d);

        //}

        //[Test]
        //public void getPeakChromatogramsForManyPeaks_Test2()
        //{
        //    Dictionary<long, int> peakFrequency = new Dictionary<long, int>();

        //    Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

        //    PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
        //    peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();

        //    List<int> indexArray = new List<int>();

        //    for (int i = 3000; i < 5000; i++)
        //    {
        //        IEnumerable<MSPeakResult> query = (from n in run.ResultCollection.MSPeakResultList where n.Scan_num == i select n);

        //        //List<MSPeakResult> filteredResults = query.ToList();
        //        var indexQuery = (from n in query select n.PeakID);

        //        foreach (var peak in query)
        //        {
        //        }

        //    }
        //    sw.Stop();

        //    Console.WriteLine("time = " + sw.ElapsedMilliseconds);

        //}

        //[Test]
        //public void getPeakChromatogramsForManyPeaks_Test3()
        //{
        //    Dictionary<long, int> peakFrequency = new Dictionary<long, int>();

        //    Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

        //    PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
        //    peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();

        //    List<int> indexArray = new List<int>();

        //    for (int i = 3000; i < 5000; i++)
        //    {
        //        IEnumerable<MSPeakResult> query = (from n in run.ResultCollection.MSPeakResultList where n.Scan_num == i select n);

        //        //List<MSPeakResult> filteredResults = query.ToList();
        //        var indexQuery = (from n in query select n.PeakID);

        //        foreach (var peak in query)
        //        {
        //        }

        //    }
        //    sw.Stop();

        //    Console.WriteLine("time = " + sw.ElapsedMilliseconds);

        //}

    }
}
