﻿using System;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    [Ignore("Ignore since uses YAFMS")]
    [Obsolete("Old file format")]
    public class YAFMS_Tests
    {
        readonly string m_testFile = FileRefs.RawDataMSFiles.YAFMSStandardFile1;

        // string tempFile = @"D:\Data\MEND\3_c_elegans_large_eqd_zero.yafms";


        [Test]
        public void checkDataSetNamesAndPathsTest()
        {
            Run run = new YAFMSRun(m_testFile);

            Assert.AreEqual(FileRefs.RawDataMSFiles.YAFMSStandardFile1, run.Filename);
            Assert.AreEqual("QC_Shew_09_01_pt5_a_20Mar09_Earth_09-01-01", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataBasePath.TrimEnd('\\'), run.DatasetDirectoryPath);
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
            var testScan = 6009;

            Run run = new YAFMSRun(m_testFile);

            var scanset = new ScanSet(testScan);

            var xyData= run.GetMassSpectrum(scanset);

            Assert.AreNotEqual(xyData, null);
            Assert.AreNotEqual(xyData.Xvalues, null);
            Assert.AreNotEqual(xyData.Xvalues.Length, 0);
            Assert.AreEqual(1576,xyData.Xvalues.Length);

            //xyData.Display();
        }


        [Test]
        public void GetSpectrumTest2_WideMZRange()
        {
            var testScan = 6009;
            double minMZ = 0;
            double maxMZ = 50000;

            Run run = new YAFMSRun(m_testFile);

            var scanset = new ScanSet(testScan);

            var xyData =run.GetMassSpectrum(scanset, minMZ, maxMZ);

            Assert.AreNotEqual(xyData, null);
            Assert.AreNotEqual(xyData.Xvalues, null);
            Assert.AreNotEqual(xyData.Xvalues.Length, 0);
        }

        [Test]
        public void GetSpectrumTest3_NarrowMZRange()
        {
            var testScan = 6009;
            double minMZ = 800;
            double maxMZ = 850;

            Run run = new YAFMSRun(m_testFile);

            var scanset = new ScanSet(testScan);

            var  xyData =run.GetMassSpectrum(scanset, minMZ, maxMZ);

            Assert.AreNotEqual(xyData, null);
            Assert.AreNotEqual(xyData.Xvalues, null);
            Assert.AreNotEqual(xyData.Xvalues.Length, 0);
            //Assert.AreEqual(xyData.Xvalues.Length, 1000);
        }

        [Test]
        public void GetSpectrumTest4_SummedSpectra()
        {
            var testScan = 6009;
            double minMZ = 800;
            double maxMZ = 850;

            Run run = new YAFMSRun(m_testFile);

            var scanset = new ScanSet(testScan, 6009, 6020);

            var xyData=  run.GetMassSpectrum(scanset, minMZ, maxMZ);

            Assert.AreNotEqual(xyData, null);
            Assert.AreNotEqual(xyData.Xvalues, null);
            Assert.AreNotEqual(xyData.Xvalues.Length, 0);
            Assert.AreEqual(xyData.Xvalues.Length, 462);
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
            var run = new YAFMSRun(FileRefs.RawDataMSFiles.YAFMSStandardFile3);

            var testScan = 2000;


            var scanLevel = run.GetMSLevel(testScan);

            Assert.AreEqual(1, scanLevel);


        }


        [Test]
        public void getPrecursorScanAndMZTest2()
        {
            var run = new YAFMSRun(FileRefs.RawDataMSFiles.YAFMSStandardFile3);

            //for (int i = 2000; i < 3000; i++)
            //{
            //    Console.WriteLine("scan " + i + "; mslevel = " + run.GetMSLevel(i));

            //}


        }
    }
}
