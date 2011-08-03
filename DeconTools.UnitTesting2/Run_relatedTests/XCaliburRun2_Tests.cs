using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using System.Diagnostics;

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

                Assert.AreEqual(1, run.MinScan);
                Assert.AreEqual(18505, run.MaxScan);
            }
            
        }

        [Test]
        public void ConstructorTest2()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1, 6000, 7000);

            Assert.AreEqual(6000, run.MinScan);
            Assert.AreEqual(7000, run.MaxScan);

            run = null;
        }
     
        [Test]
        public void getNumSpectraTest1()
        {
            XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int numScans= run.GetNumMSScans();
            Assert.AreEqual(18505, numScans);
            Assert.AreEqual(1, run.MinScan);
            Assert.AreEqual(18505, run.MaxScan);

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

            Assert.AreEqual(1, run.MinScan);
            Assert.AreEqual(17773, run.MaxScan);

        }



        [Test]
        public void getSpectrum_Test1()
        {
            XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            run.GetMassSpectrum(new Backend.Core.ScanSet(6005));

            //TestUtilities.DisplayXYValues(run.XYData);

        }


        [Test]
        public void getSummedSpectrum_Test1()
        {
            XCaliburRun2 run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);


            ScanSet scanset = new ScanSet(6005, new int[] { 6005, 6012, 6019 });

            run.GetMassSpectrum(scanset);

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

            string scanInfoString = run.GetScanInfo(scan);
            Assert.AreEqual(@"ITMS + c NSI d Full ms2 543.80@cid35.00 [135.00-2000.00]", scanInfoString);

        }

       


    }
}
