using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class BrukerV3Tests
    {
             
        [Test]
        public void runInitiation_Bruker9T_test1()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker9TStandardFile1);
            Assert.AreEqual(0, run.MinScan);
            Assert.AreEqual(599, run.MaxScan);
            Assert.AreEqual("SWT_9t_TestDS216_Small", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.Bruker9TStandardFile1, run.DataSetPath);
        }

        [Test]
        public void runInitiation_Bruker9T_test2()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker9TStandardFile2);
            Assert.AreEqual(0, run.MinScan);
            Assert.AreEqual(4274, run.MaxScan);
            Assert.AreEqual("RSPH_Aonly_01_run1_11Oct07_Andromeda_07-09-02", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.Bruker9TStandardFile2, run.DataSetPath);
        }

        [Test]
        public void runInitiation_Bruker12T_ser_Test1()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);
            Assert.AreEqual(0, run.MinScan);
            Assert.AreEqual(7, run.MaxScan);
            Assert.AreEqual(8, run.GetNumMSScans());
            Assert.AreEqual("12Ttest_000003", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1, run.DataSetPath);
        }

        [Test]
        public void runInitiation_Bruker12T_fid_Test1()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12T_FID_File1);
            Assert.AreEqual(0, run.MinScan);
            Assert.AreEqual(0, run.MaxScan);
            Assert.AreEqual(1, run.GetNumMSScans());
            Assert.AreEqual("HVY_000001", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.BrukerSolarix12T_FID_File1, run.DataSetPath);
        }

        [Test]
        public void runInitiation_Bruker12T_dotD_Test1()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12T_dotD_File1);
            Assert.AreEqual(0, run.MinScan);
            Assert.AreEqual(0, run.MaxScan);
            Assert.AreEqual(1, run.GetNumMSScans());
            Assert.AreEqual("BSA_10082010_000003", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.BrukerSolarix12T_dotD_File1, run.DataSetPath);
        }

        [Test]
        public void runInitiation_Bruker15T_Test1()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker15TFile1);
            Assert.AreEqual(0, run.MinScan);
            Assert.AreEqual(17, run.MaxScan);
            Assert.AreEqual(18, run.GetNumMSScans());
            Assert.AreEqual("092410_ubiquitin_AutoCID_000004", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.Bruker15TFile1, run.DataSetPath);
        }


        [Test]
        public void GetSpectrum_Bruker9T_Test1()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker9TStandardFile1);

            ScanSet scanSet = new ScanSet(300);
            run.GetMassSpectrum(scanSet);
            Assert.That(run.XYData.Xvalues != null);
            Assert.That(run.XYData.Xvalues.Length > 0);
            Assert.AreEqual(98897, run.XYData.Xvalues.Length);

        }

        [Test]
        public void GetSpectrum_Bruker9T_Test2()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker9TStandardFile2);

            ScanSet scanSet = new ScanSet(1000);
            run.GetMassSpectrum(scanSet);
            Assert.That(run.XYData.Xvalues != null);
            Assert.That(run.XYData.Xvalues.Length > 0);
            Assert.AreEqual(211064, run.XYData.Xvalues.Length);

        }

        [Test]
        public void Get_summed_Spectrum_Bruker9T_Test1()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker9TStandardFile2);

            ScanSet scanSet = new ScanSet(1000,999,1001);
            run.GetMassSpectrum(scanSet);
            Assert.That(run.XYData.Xvalues != null);
            Assert.That(run.XYData.Xvalues.Length > 0);
            Assert.AreEqual(211064, run.XYData.Xvalues.Length);

        }


        [Test]
        public void GetSpectrum_Bruker12T_fid_Test1()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12T_FID_File1);

            ScanSet scanSet = new ScanSet(0);
            run.GetMassSpectrum(scanSet);
            Assert.That(run.XYData.Xvalues != null);
            Assert.That(run.XYData.Xvalues.Length > 0);
            Assert.AreEqual(369658, run.XYData.Xvalues.Length);

        }

        [Test]
        public void GetSpectrum_Bruker12T_fid_Test2()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12T_FID_File1);

            ScanSet scanSet = new ScanSet(0);
            double minMZ = 700;
            double maxMZ = 701;

            run.GetMassSpectrum(scanSet,minMZ,maxMZ);
            Assert.That(run.XYData.Xvalues != null);
            Assert.That(run.XYData.Xvalues.Length > 0);
            Assert.AreEqual(630, run.XYData.Xvalues.Length);

        }

        [Test]
        public void GetSpectrum_Bruker12T_ser_Test1()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);

            ScanSet scanSet = new ScanSet(2);
            run.GetMassSpectrum(scanSet);
            Assert.That(run.XYData.Xvalues != null);
            Assert.That(run.XYData.Xvalues.Length > 0);
            Assert.AreEqual(204162, run.XYData.Xvalues.Length);
        }


        [Test]
        public void GetSpectrum_Bruker15T_ser_Test1()
        {
            BrukerV3Run run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker15TFile1);

            ScanSet scanSet = new ScanSet(2);
            run.GetMassSpectrum(scanSet);
            Assert.That(run.XYData.Xvalues != null);
            Assert.That(run.XYData.Xvalues.Length > 0);
            Assert.AreEqual(209817, run.XYData.Xvalues.Length);
        }


    }
}
