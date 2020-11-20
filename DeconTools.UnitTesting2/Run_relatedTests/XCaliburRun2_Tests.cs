using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using NUnit.Framework;
using PNNLOmics.Data;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class XCaliburRun2_Tests
    {
        [Test]
        public void ConstructorTest1()
        {
            var fileName = Path.GetFileName(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            using (Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1))
            {
                Console.WriteLine("Scan range for {0} is {1:N0} to {2:N0}", fileName, run.MinLCScan, run.MaxLCScan);

                Assert.AreEqual(1, run.MinLCScan);
                Assert.AreEqual(18505, run.MaxLCScan);
            }
        }

        [Test]
        public void TestGetNumMSScans()
        {
            var rawFile = new FileInfo(@"\\proto-3\QExactP02\2020_1\QC_Shew_20_01_Run-1_30Feb20_Merry_19-11-03\QC_Shew_20_01_Run-1_30Feb20_Merry_19-11-03.raw");

            var run = new XCaliburRun2(rawFile.FullName);
            Assert.IsNotNull(run);

            var scanCount = run.GetNumMSScans();
            Console.WriteLine("File {0} has {1:N0} spectra", rawFile.Name, scanCount);

            Assert.AreEqual(56864, scanCount);
        }

        [Test]
        public void TempTest1()
        {
            using (var run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1, true))
            {
                var tuneData = run.GetTuneData();
                Console.WriteLine(tuneData);
            }
        }

        [Test]
        public void ConstructorTest2()
        {
            var fileName = Path.GetFileName(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1, 6000, 7000);

            Console.WriteLine("Scan range for {0} is {1:N0} to {2:N0}", fileName, run.MinLCScan, run.MaxLCScan);

            Assert.AreEqual(6000, run.MinLCScan);
            Assert.AreEqual(7000, run.MaxLCScan);

            run = null;
        }

        [Test]
        public void getNumSpectraTest1()
        {
            var fileName = Path.GetFileName(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            var run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            Console.WriteLine("Scan range for {0} is {1:N0} to {2:N0}", fileName, run.MinLCScan, run.MaxLCScan);

            var numScans = run.GetNumMSScans();
            Assert.AreEqual(18505, numScans);
            Assert.AreEqual(1, run.MinLCScan);
            Assert.AreEqual(18505, run.MaxLCScan);
        }

        [Test]
        public void checkDataSetNamesAndPathsTest()
        {
            var testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            Run run = new XCaliburRun2(testFile);

            Console.WriteLine("Opened " + testFile);

            Assert.AreEqual("QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18", run.DatasetName);
        }

        [Test]
        public void initializeVelosOrbiFile_Test1()
        {
            var fileName = Path.GetFileName(FileRefs.RawDataMSFiles.VOrbiFile1);
            var testFile = FileRefs.RawDataMSFiles.VOrbiFile1;

            Run run = new XCaliburRun2(testFile);

            Console.WriteLine("Scan range for {0} is {1:N0} to {2:N0}", fileName, run.MinLCScan, run.MaxLCScan);

            Assert.AreEqual(1, run.MinLCScan);
            Assert.AreEqual(17773, run.MaxLCScan);
        }

        [Test]
        public void GetIonInjectionTimeTest1()
        {
            var run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var scan = 6005;
            var ionInjectionTime = run.GetIonInjectionTimeInMilliseconds(scan);

            Console.WriteLine("Scan " + scan + "; ion injection time = " + ionInjectionTime);

            Assert.AreEqual(2.84m, (decimal)Math.Round(ionInjectionTime, 2));
        }

        [Test]
        public void GetMS2IsolationWidthTest1()
        {
            var run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var scan = 6006;
            var isolationWidth = run.GetMS2IsolationWidth(scan);

            Console.WriteLine("Scan " + scan + "; MS2IsolationWidth = " + isolationWidth);

            Assert.AreEqual(3.0m, (decimal)Math.Round(isolationWidth, 1));
        }

        [Test]
        public void getSpectrum_Test1()
        {
            const int SCAN_NUMBER = 6005;

            var fileName = Path.GetFileName(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            var run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var xyData = run.GetMassSpectrum(new ScanSet(SCAN_NUMBER));

            Console.WriteLine("In file {0}, scan {1:N0} has {2:N0} points", fileName, SCAN_NUMBER, xyData.Xvalues.Length);

            Assert.IsTrue(xyData.Xvalues.Length > 1000);

            TestUtilities.DisplayXYValues(xyData, 600, 603);
        }

        [Test]
        public void getSummedSpectrum_Test1()
        {
            var fileName = Path.GetFileName(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            var run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var scanSet = new ScanSet(6005, new int[] { 6005, 6012, 6019 });

            var xyData = run.GetMassSpectrum(scanSet);

            Console.WriteLine("In file {0}, scans {1} sum to give {2:N0} points", fileName, scanSet, xyData.Xvalues.Length);

            Assert.IsTrue(xyData.Xvalues.Length > 1000);

            Assert.AreEqual(46378, xyData.Xvalues.Length, 5, "Data point count is not close to the expected value");

            TestUtilities.DisplayXYValues(xyData, 400, 402);
        }

        [Test]
        public void getSummedSpectrum_SpeedTest1()
        {
            using (var run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1))
            {
                var scanSet = new ScanSet(6005, new int[] { 6005, 6012, 6019 });

                var numIterations = 50;

                var watch = new Stopwatch();
                var timeStats = new List<long>();

                for (var i = 0; i < numIterations; i++)
                {
                    watch.Start();
                    run.GetMassSpectrum(scanSet);
                    watch.Stop();
                    timeStats.Add(watch.ElapsedMilliseconds);
                    watch.Reset();
                }

                Console.WriteLine("Average reading time when summing 3 spectra = " + timeStats.Average());
            }
        }

        [Test]
        public void GetMSLevel_Test1()
        {
            var run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            Assert.AreEqual(1, run.GetMSLevel(6005));
            Assert.AreEqual(2, run.GetMSLevel(6006));
            Assert.AreEqual(2, run.GetMSLevel(6007));
            Assert.AreEqual(2, run.GetMSLevel(6008));
            Assert.AreEqual(2, run.GetMSLevel(6009));
            Assert.AreEqual(2, run.GetMSLevel(6010));
            Assert.AreEqual(2, run.GetMSLevel(6011));
            Assert.AreEqual(1, run.GetMSLevel(6012));
        }

        [Test]
        public void GetScanInfoTest1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var msmsScanNum1 = 6000;

            var scan = new ScanSet(msmsScanNum1);

            var scanInfoString = run.GetScanInfo(scan.PrimaryScanNumber);
            Assert.AreEqual(@"ITMS + c NSI d Full ms2 543.80@cid35.00 [135.00-2000.00]", scanInfoString);
        }

        [Test]
        public void GetPrecursorInfo()
        {
            var run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var precursor1 = run.GetPrecursorInfo(6005);
            ShowPrecursorInfo(6005, precursor1);

            Assert.AreEqual(1, precursor1.MSLevel, "MSLevel is not {0} instead of 1", precursor1.MSLevel);
            Assert.AreEqual(-1, precursor1.PrecursorCharge, "Charge is {0} instead of -1", precursor1.PrecursorCharge);
            Assert.AreEqual(0, precursor1.PrecursorIntensity, "Intensity is {0} instead of 0", precursor1.PrecursorMZ);
            Assert.AreEqual(0, precursor1.PrecursorMZ, "Precursor m/z is {0} instead of -1", precursor1.PrecursorIntensity);
            Assert.AreEqual(6005, precursor1.PrecursorScan, "Scan is {0} instead of 600", precursor1.PrecursorScan);

            Console.WriteLine();

            var precursor2 = run.GetPrecursorInfo(6006);
            ShowPrecursorInfo(6006, precursor2);

            Assert.AreEqual(2, precursor2.MSLevel, "MSLevel is {0} instead of 2", precursor1.MSLevel);
            Assert.AreEqual(-1, precursor2.PrecursorCharge, "Charge is {0} instead of -1", precursor1.PrecursorCharge);
            Assert.AreEqual(0, precursor2.PrecursorIntensity, "Intensity is {0} instead of 0", precursor1.PrecursorIntensity);
            Assert.AreEqual(408.25, precursor2.PrecursorMZ, "Precursor m/z is {0} instead of 408.25", precursor1.PrecursorMZ);
            Assert.AreEqual(6005, precursor2.PrecursorScan, "Scan is {0} instead of 6005", precursor1.PrecursorScan);
        }

        private void ShowPrecursorInfo(int scanNumber, PrecursorInfo precursor)
        {
            Console.WriteLine("Precursor info for scan  {0}", scanNumber);
            Console.WriteLine("Precursor scan: {0}", precursor.PrecursorScan);
            Console.WriteLine("MSLevel:        {0}", precursor.MSLevel);
            Console.WriteLine("Charge:         {0}", precursor.PrecursorCharge);
            Console.WriteLine("m/z:            {0:F4}", precursor.PrecursorMZ);
            Console.WriteLine("Intensity:      {0:F2}", precursor.PrecursorIntensity);
        }

        [Test]
        public void GetTICFromInstrumentInfoTest1()
        {
            var run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var scan = 6005;

            var ticIntensity = run.GetTICFromInstrumentInfo(scan);

            Assert.IsTrue(ticIntensity > 0);
        }
    }
}
