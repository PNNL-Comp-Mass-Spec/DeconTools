using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            using (Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1))
            {

                Assert.AreEqual(1, run.MinLCScan);
                Assert.AreEqual(18505, run.MaxLCScan);
            }
            
        }

        [Test]
        public void checkVersion_2pt2_xcalibur_dll_test1()
        {
            var run = new XCaliburRun2(@"\\proto-7\VOrbiETD01\2012_3\QC_Shew_12_02_Run-03_26Jul12_Roc_12-04-08\QC_Shew_12_02_Run-03_26Jul12_Roc_12-04-08.raw");

            Assert.AreEqual(8904, run.GetNumMSScans());

            Assert.IsNotNull(run);
        }


        [Test]
        public void TempTest1()
        {
            using (XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1))
            {

                run.GetTuneData();
            }

        }




        [Test]
        public void ConstructorTest2()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1, 6000, 7000);

            Assert.AreEqual(6000, run.MinLCScan);
            Assert.AreEqual(7000, run.MaxLCScan);

            run = null;
        }
     
        [Test]
        public void getNumSpectraTest1()
        {
            XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int numScans= run.GetNumMSScans();
            Assert.AreEqual(18505, numScans);
            Assert.AreEqual(1, run.MinLCScan);
            Assert.AreEqual(18505, run.MaxLCScan);

            //TestUtilities.DisplayXYValues(run.XYData);

        }

        [Test]
        public void checkDataSetNamesAndPathsTest()
        {
            string testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            Run run = new XCaliburRun2(testFile);

            Assert.AreEqual("QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18", run.DatasetName);
        }

        [Test]
        public void initializeVelosOrbiFile_Test1()
        {
            string testFile = FileRefs.RawDataMSFiles.VOrbiFile1;

            Run run = new XCaliburRun2(testFile);

            Assert.AreEqual(1, run.MinLCScan);
            Assert.AreEqual(17773, run.MaxLCScan);

        }

        [Test]
        public  void GetIonInjectionTimeTest1()
        {
            XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int scan = 6005;
            var ionInjectionTime = run.GetIonInjectionTimeInMilliseconds(scan);

            Assert.AreEqual(2.84m, (decimal) Math.Round(ionInjectionTime, 2));

            Console.WriteLine("Scan "+ scan + "; ion injection time = " + ionInjectionTime);

        }


        [Test]
        public void GetMS2IsolationWidthTest1()
        {
            XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int scan = 6006;
            var isolationWidth = run.GetMS2IsolationWidth(scan);

            Assert.AreEqual(3.0m, (decimal)Math.Round(isolationWidth,1));

            Console.WriteLine("Scan " + scan + "; MS2IsolationWidth = " + isolationWidth);
        }



        [Test]
        public void getSpectrum_Test1()
        {
            XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var xydata=  run.GetMassSpectrum(new Backend.Core.ScanSet(6005));

            Assert.IsTrue(xydata.Xvalues.Length > 1000);
         

           // TestUtilities.DisplayXYValues(xydata);

        }


        [Test]
        public void getSummedSpectrum_Test1()
        {
            XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);


            ScanSet scanset = new ScanSet(6005, new int[] { 6005, 6012, 6019 });

            var xydata= run.GetMassSpectrum(scanset);
            Assert.IsTrue(xydata.Xvalues.Length > 1000);
            //TestUtilities.DisplayXYValues(run.XYData);

        }



        [Test]
        public void getSummedSpectrum_SpeedTest1()
        {
            using (XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1))
            {

                ScanSet scanset = new ScanSet(6005, new int[] { 6005, 6012, 6019 });

                int numIterations = 50;

                Stopwatch watch = new Stopwatch();
                List<long> timeStats = new List<long>();

                for (int i = 0; i < numIterations; i++)
                {
                    watch.Start();
                    run.GetMassSpectrum(scanset);
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
            XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

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

            int msmsScanNum1 = 6000;

            ScanSet scan = new ScanSet(msmsScanNum1);

            string scanInfoString = run.GetScanInfo(scan.PrimaryScanNumber);
            Assert.AreEqual(@"ITMS + c NSI d Full ms2 543.80@cid35.00 [135.00-2000.00]", scanInfoString);

        }

        [Test]
        public void GetPrecursorInfo()
        {
            XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            
            PrecursorInfo precursor;

            precursor = run.GetPrecursorInfo(6005);

            Assert.AreEqual(1, precursor.MSLevel);
            Assert.AreEqual(-1, precursor.PrecursorCharge);
            Assert.AreEqual(0, precursor.PrecursorIntensity);
            Assert.AreEqual(-1, precursor.PrecursorMZ);
            Assert.AreEqual(6005, precursor.PrecursorScan);

            precursor = run.GetPrecursorInfo(6006);

            Assert.AreEqual(2, precursor.MSLevel);
            Assert.AreEqual(-1, precursor.PrecursorCharge);
            Assert.AreEqual(0, precursor.PrecursorIntensity);
            Assert.AreEqual(408.25, precursor.PrecursorMZ);
            Assert.AreEqual(6005, precursor.PrecursorScan);
        }


        [Test]
        public void GetTICFromInstrumentInfoTest1()
        {
            XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int scan = 6005;

           var ticIntensity =   run.GetTICFromInstrumentInfo(scan);

            Assert.IsTrue(ticIntensity > 0);

        }
       


    }
}
