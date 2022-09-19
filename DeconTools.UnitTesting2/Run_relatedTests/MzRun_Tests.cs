using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class MzRun_Tests
    {
        [OneTimeSetUp]
        public void ConfigureDllLookup()
        {
            pwiz.ProteowizardWrapper.DependencyLoader.AddAssemblyResolver();
        }

        [Test]
        public void constructorTest1()
        {
            var testfile =
                @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzXML";

            Run run = new MzRun(testfile);
            Assert.AreEqual(Backend.Globals.MSFileType.MZXML_Rawdata, run.MSFileType);
            Assert.AreEqual(18505, run.GetNumMSScans());

            Assert.AreEqual(0, run.MinLCScan);
            Assert.AreEqual(18504, run.MaxLCScan);

            Assert.AreEqual("QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18", run.DatasetName);
            Assert.AreEqual(@"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML", run.DatasetDirectoryPath);
        }

        [Test]
        public void GetMassSpectrumTest1()
        {
            var testscan = 6004;

            var testfile =
               @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzXML";

            Run run = new MzRun(testfile);

            var scanSet = new ScanSet(testscan);
            run.XYData = run.GetMassSpectrum(scanSet);

            Assert.AreEqual(481.274514196002, (decimal)run.XYData.Xvalues[3769]);
            Assert.AreEqual(13084442, run.XYData.Yvalues[3769]);

            //TestUtilities.DisplayXYValues(run.XYData);
        }

        [Test]
        public void GetMassSpectrum_higherThanTotalScans()
        {
            var testscan = 18506;

            var testfile =
               @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzXML";

            Run run = new MzRun(testfile);

            try
            {
                var scanSet = new ScanSet(testscan);
                run.GetMassSpectrum(scanSet);
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Pass("ArgumentOutOfRangeException caught; this is expected");
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception caught of type {0}: {1}", ex.GetType(), ex.Message);
            }

            Assert.Fail("Exception was not caught; we expected an ArgumentOutOfRangeException to be thrown");
        }

        [Test]
        public void GetMSLevelsTest1()
        {
            var testscan = 6004;
            var testscan2 = 6005;

            var testfile =
               @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzXML";

            Run run = new MzRun(testfile);

            var level = run.GetMSLevel(testscan);

            var scanTime = run.GetTime(testscan);

            Assert.AreEqual(1, run.GetMSLevel(testscan));
            Assert.AreEqual(2, run.GetMSLevel(testscan2));

            Assert.AreEqual(1961.65, (decimal)run.GetTime(testscan));
        }

        [Test]
        public void ValidateDataTest1()    //purpose is to compare
        {
            var testMz5File = @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mz5";
            var testThermoFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            var mz5run = new RunFactory().CreateRun(testMz5File);

            var thermoRun = new RunFactory().CreateRun(testThermoFile);

            var testScanMz5 = 6004;
            var testScanThermo = 6005;

            var testScanSet1 = new ScanSet(testScanMz5);
            var testScanSetThermo = new ScanSet(testScanThermo);

            mz5run.CurrentScanSet = testScanSet1;
            thermoRun.CurrentScanSet = testScanSetThermo;

            mz5run.XYData = mz5run.GetMassSpectrum(testScanSet1);
            thermoRun.XYData = thermoRun.GetMassSpectrum(testScanSetThermo);

            Assert.AreEqual(mz5run.XYData.Xvalues.Length, thermoRun.XYData.Xvalues.Length);

            for (var i = 0; i < mz5run.XYData.Xvalues.Length; i++)
            {
                Assert.AreEqual(mz5run.XYData.Xvalues[i], thermoRun.XYData.Xvalues[i]);
            }

            var peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 3;
            peakDetector.SignalToNoiseThreshold = 2;

            //peakDetector.Execute(mz5run.ResultCollection);

            peakDetector.Execute(thermoRun.ResultCollection);

            //TestUtilities.DisplayPeaks(mz5run.PeakList);

            Console.WriteLine();
            Console.WriteLine();

            TestUtilities.DisplayPeaks(thermoRun.PeakList);
        }

        [Test]
        [Ignore("Local file")]
        public void ValidateDataTest_temp()    //purpose is to compare
        {
            var testMz5File = @"C:\Sipper\SipperDemo\RawDataFiles\Yellow_C13_070_23Mar10_Griffin_10-01-28.mz5";
            var testThermoFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            var mz5run = new RunFactory().CreateRun(testMz5File);
            mz5run.IsDataThresholded = true;

            var thermoRun = new RunFactory().CreateRun(testThermoFile);

            var testScanMz5 = 6010;
            var testScanThermo = 6011;

            var testScanSet1 = new ScanSet(testScanMz5);
            var testScanSetThermo = new ScanSet(testScanThermo);

            mz5run.CurrentScanSet = testScanSet1;
            thermoRun.CurrentScanSet = testScanSetThermo;

            mz5run.GetMassSpectrum(testScanSet1);
            thermoRun.GetMassSpectrum(testScanSetThermo);

            Assert.AreEqual(mz5run.XYData.Xvalues.Length, thermoRun.XYData.Xvalues.Length);

            for (var i = 0; i < mz5run.XYData.Xvalues.Length; i++)
            {
                Assert.AreEqual(mz5run.XYData.Xvalues[i], thermoRun.XYData.Xvalues[i]);
            }

            var peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 3;
            peakDetector.SignalToNoiseThreshold = 2;

            peakDetector.Execute(mz5run.ResultCollection);

            //peakDetector.Execute(thermoRun.ResultCollection);

            TestUtilities.DisplayPeaks(mz5run.PeakList);

            Console.WriteLine();
            Console.WriteLine();

            // TestUtilities.DisplayPeaks(thermoRun.PeakList);

        }

        [Test]
        public void GetScanInfoTest1()
        {
            var testfile =
             @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mz5";

            Run run = new MzRun(testfile);

            var ms2scan = 6005;

            var info = run.GetScanInfo(ms2scan);

            Console.WriteLine(info);
        }

        [Test]
        public void Speedtest1()
        {
            //string testfile =
            //  @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzXML";

            var testfile2 =
             @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mz5";

            //string testfile3 =
            //@"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzML";

            Run run = new MzRun(testfile2);

            var stopwatch = new Stopwatch();

            var startScan = 5500;
            var stopScan = 6500;

            var scanSet = new ScanSet(startScan);
            var times = new List<long>();

            run.GetMSLevel(startScan);

            for (var i = startScan; i < stopScan; i++)
            {
                scanSet = new ScanSet(i);

                if (run.GetMSLevel(i) == 2)
                {
                    continue;
                }

                stopwatch.Start();
                run.GetMassSpectrum(scanSet);
                stopwatch.Stop();

                times.Add(stopwatch.ElapsedMilliseconds);

                stopwatch.Reset();
            }

            //foreach (var time in times)
            //{
            //    Console.WriteLine(time);
            //}

            Console.WriteLine("Average = " + times.Average());
        }

        [Test]
        public void WatersMzXmlTest1()
        {
            var testfile =
                @"\\proto-2\unitTest_Files\DeconTools_TestFiles\Waters\LC_MS_pHis_Caulo_meth_110207.mzXML";

            var run = new RunFactory().CreateRun(testfile);
            TestUtilities.DisplayRunInformation(run);

            run.ScanSetCollection = new ScanSetCollection();
            run.ScanSetCollection.Create(run, 500, 600, 1, 1);

            foreach (var scanSet in run.ScanSetCollection.ScanSetList)
            {
                var level = run.GetMSLevel(scanSet.PrimaryScanNumber);

                Console.WriteLine(scanSet + "\t" + level);
            }
        }
    }
}
