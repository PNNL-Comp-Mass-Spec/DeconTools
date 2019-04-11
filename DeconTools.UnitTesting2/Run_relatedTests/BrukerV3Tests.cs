using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class BrukerV3Tests
    {
             
        [Test]
        public void runInitiation_Bruker9T_test1()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker9TStandardFile1);
            Assert.AreEqual(0, run.MinLCScan);
            Assert.AreEqual(599, run.MaxLCScan);
            Assert.AreEqual("SWT_9t_TestDS216_Small", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.Bruker9TStandardFile1, run.DataSetPath);
        }

        [Test]
        public void runInitiation_Bruker9T_test2()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker9TStandardFile2);
            Assert.AreEqual(0, run.MinLCScan);
            Assert.AreEqual(4274, run.MaxLCScan);
            Assert.AreEqual("RSPH_Aonly_01_run1_11Oct07_Andromeda_07-09-02", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.Bruker9TStandardFile2, run.DataSetPath);
        }

        [Test]
        public void runInitiation_Bruker12T_ser_Test1()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);
            Assert.AreEqual(0, run.MinLCScan);
            Assert.AreEqual(7, run.MaxLCScan);
            Assert.AreEqual(8, run.GetNumMSScans());
            Assert.AreEqual("12Ttest_000003", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1, run.DataSetPath);
        }

        [Test]
        public void runInitiation_Bruker12T_fid_Test1()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12T_FID_File1);
            Assert.AreEqual(0, run.MinLCScan);
            Assert.AreEqual(0, run.MaxLCScan);
            Assert.AreEqual(1, run.GetNumMSScans());
            Assert.AreEqual("HVY_000001", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.BrukerSolarix12T_FID_File1, run.DataSetPath);
        }

        [Test]
        public void runInitiation_Bruker12T_dotD_Test1()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12T_dotD_File1);
            Assert.AreEqual(0, run.MinLCScan);
            Assert.AreEqual(0, run.MaxLCScan);
            Assert.AreEqual(1, run.GetNumMSScans());
            Assert.AreEqual("BSA_10082010_000003", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.BrukerSolarix12T_dotD_File1, run.DataSetPath);
        }

        [Test]
        public void runInitiation_Bruker15T_Test1()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker15TFile1);
            Assert.AreEqual(0, run.MinLCScan);
            Assert.AreEqual(17, run.MaxLCScan);
            Assert.AreEqual(18, run.GetNumMSScans());
            Assert.AreEqual("092410_ubiquitin_AutoCID_000004", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataMSFiles.Bruker15TFile1, run.DataSetPath);
        }


        [Test]
        public void GetSpectrum_Bruker9T_Test1()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker9TStandardFile1);
            var xyData = new XYData();
            var scanSet = new ScanSet(300);
            xyData =run.GetMassSpectrum(scanSet);
            Assert.That(xyData.Xvalues != null);
            Assert.That(xyData.Xvalues.Length > 0);
            Assert.AreEqual(98897, xyData.Xvalues.Length);

        }

        [Test]
        public void GetSpectrum_Bruker9T_Test2()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker9TStandardFile2);

            var xyData = new XYData();

            var scanSet = new ScanSet(1000);
            xyData =run.GetMassSpectrum(scanSet);
            Assert.That(xyData.Xvalues != null);
            Assert.That(xyData.Xvalues.Length > 0);
            Assert.AreEqual(211064, xyData.Xvalues.Length);

        }

        [Test]
        public void Get_summed_Spectrum_Bruker9T_Test1()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker9TStandardFile2);

            var xyData = new XYData();

            var scanSet = new ScanSet(1000,999,1001);
            xyData =run.GetMassSpectrum(scanSet);
            Assert.That(xyData.Xvalues != null);
            Assert.That(xyData.Xvalues.Length > 0);
            Assert.AreEqual(211064, xyData.Xvalues.Length);

        }


        [Test]
        public void GetSpectrum_Bruker12T_fid_Test1()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12T_FID_File1);
            var xyData = new XYData();
            var scanSet = new ScanSet(0);
            xyData =run.GetMassSpectrum(scanSet);
            Assert.That(xyData.Xvalues != null);
            Assert.That(xyData.Xvalues.Length > 0);
            Assert.AreEqual(369658, xyData.Xvalues.Length);

        }

        [Test]
        public void GetSpectrum_Bruker12T_fid_Test2()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12T_FID_File1);
            var xyData = new XYData();
            var scanSet = new ScanSet(0);
            double minMZ = 700;
            double maxMZ = 701;

            xyData=  run.GetMassSpectrum(scanSet,minMZ,maxMZ);
            Assert.That(xyData.Xvalues != null);
            Assert.That(xyData.Xvalues.Length > 0);
            Assert.AreEqual(630, xyData.Xvalues.Length);

        }

        [Test]
        public void GetSpectrum_Bruker12T_ser_Test1()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);
            var xyData = new XYData();
            var scanSet = new ScanSet(2);
            xyData =run.GetMassSpectrum(scanSet);
            Assert.That(xyData.Xvalues != null);
            Assert.That(xyData.Xvalues.Length > 0);
            Assert.AreEqual(204162, xyData.Xvalues.Length);
        }


        [Test]
        public void GetSpectrum_Bruker15T_ser_Test1()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker15TFile1);
            var xyData = new XYData();
            var scanSet = new ScanSet(2);
            xyData =run.GetMassSpectrum(scanSet);
            Assert.That(xyData.Xvalues != null);
            Assert.That(xyData.Xvalues.Length > 0);
            Assert.AreEqual(209817, xyData.Xvalues.Length);

            //Assert.AreEqual(28962756691,(long)xyData.Yvalues.Sum());
        }


        [Test]
        public void GetSpectrum_Bruker15T_SummedMS_Test1()
        {
            var run = new BrukerV3Run(FileRefs.RawDataMSFiles.Bruker15TFile1);
            var xyData = new XYData();
            var scanSet = new ScanSet(2,1,3);
            xyData =run.GetMassSpectrum(scanSet);
            Assert.That(xyData.Xvalues != null);
            Assert.That(xyData.Xvalues.Length > 0);
            Assert.AreEqual(209817, xyData.Xvalues.Length);

            //TODO: confirm this value:   (currently different)
            //Assert.AreEqual(86318972269, (long)xyData.Yvalues.Sum());

        }



    }
}
