using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DeconTools.Backend;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class YAFMS_Tests
    {
        string m_testFile = FileRefs.RawDataMSFiles.YAFMSStandardFile1;

        // string tempFile = @"D:\Data\MEND\3_c_elegans_large_eqd_zero.yafms";


        [Test]
        public void checkDataSetNamesAndPathsTest()
        {
            Run run = new YAFMSRun(m_testFile);

            Assert.AreEqual(FileRefs.RawDataMSFiles.YAFMSStandardFile1, run.Filename);
            Assert.AreEqual("QC_Shew_09_01_pt5_a_20Mar09_Earth_09-01-01", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataBasePath.TrimEnd('\\'), run.DataSetPath);
        }

        [Test]
        public void checkMinAndMaxScansTest1()
        {
            Run run = new YAFMSRun(m_testFile);

            //Need to confirm these numbers
            //Assert.AreEqual(14428, run.GetNumMSScans());
            //Assert.AreEqual(0, run.MinScan);
            //Assert.AreEqual(14427, run.MaxScan);
        }


        [Test]
        public void GetSpectrumTest1()
        {
            int testScan = 6009;

            Run run = new YAFMSRun(m_testFile);

            ScanSet scanset = new ScanSet(testScan);

            XYData xydata= run.GetMassSpectrum(scanset);

            Assert.AreNotEqual(xydata, null);
            Assert.AreNotEqual(xydata.Xvalues, null);
            Assert.AreNotEqual(xydata.Xvalues.Length, 0);
            Assert.AreEqual(1576,xydata.Xvalues.Length);

            //xydata.Display();
        }


        [Test]
        public void GetSpectrumTest2_WideMZRange()
        {
            int testScan = 6009;
            double minMZ = 0;
            double maxMZ = 50000;

            Run run = new YAFMSRun(m_testFile);

            ScanSet scanset = new ScanSet(testScan);

            var xydata =run.GetMassSpectrum(scanset, minMZ, maxMZ);

            Assert.AreNotEqual(xydata, null);
            Assert.AreNotEqual(xydata.Xvalues, null);
            Assert.AreNotEqual(xydata.Xvalues.Length, 0);
        }

        [Test]
        public void GetSpectrumTest3_NarrowMZRange()
        {
            int testScan = 6009;
            double minMZ = 800;
            double maxMZ = 850;

            Run run = new YAFMSRun(m_testFile);

            ScanSet scanset = new ScanSet(testScan);

            var  xydata =run.GetMassSpectrum(scanset, minMZ, maxMZ);

            Assert.AreNotEqual(xydata, null);
            Assert.AreNotEqual(xydata.Xvalues, null);
            Assert.AreNotEqual(xydata.Xvalues.Length, 0);
            //Assert.AreEqual(xydata.Xvalues.Length, 1000);
        }

        [Test]
        public void GetSpectrumTest4_SummedSpectra()
        {
            int testScan = 6009;
            double minMZ = 800;
            double maxMZ = 850;

            Run run = new YAFMSRun(m_testFile);

            ScanSet scanset = new ScanSet(testScan, 6009, 6020);

            var xydata=  run.GetMassSpectrum(scanset, minMZ, maxMZ);

            Assert.AreNotEqual(xydata, null);
            Assert.AreNotEqual(xydata.Xvalues, null);
            Assert.AreNotEqual(xydata.Xvalues.Length, 0);
            Assert.AreEqual(xydata.Xvalues.Length, 462);
        }

        [Test]
        public void GetSpectrumTypesTest1()
        {
            Run run = new YAFMSRun(m_testFile);

            Assert.AreEqual(1, run.GetMSLevel(6009));
            Assert.AreEqual(2, run.GetMSLevel(6010));
            Assert.AreEqual(2, run.GetMSLevel(6011));
        }


        [Test]
        public void GetSpectrumTypesTest2()
        {
            Run run = new YAFMSRun(m_testFile);

            //for (int i = 6000; i < 7000; i++)
            //{
            //    Console.WriteLine("scan " + i + "; mslevel = " + run.GetMSLevel(i));

            //}

        }


        [Test]
        public void getPrecursorScanAndMZTest1()
        {
            YAFMSRun run = new YAFMSRun(FileRefs.RawDataMSFiles.YAFMSStandardFile3);

            int testScan = 2000;


            int scanLevel = run.GetMSLevel(testScan);

            Assert.AreEqual(1, scanLevel);


        }


        [Test]
        public void getPrecursorScanAndMZTest2()
        {
            YAFMSRun run = new YAFMSRun(FileRefs.RawDataMSFiles.YAFMSStandardFile3);

            //for (int i = 2000; i < 3000; i++)
            //{
            //    Console.WriteLine("scan " + i + "; mslevel = " + run.GetMSLevel(i));

            //}


        }
    }
}
