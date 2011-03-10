using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.DTO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class PeakImporterTests
    {
        [Test]
        public void importPeaksFromOrbitrapFileTest1()
        {
            List<MSPeakResult> mspeaks = new List<MSPeakResult>();
            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(mspeaks);

            Assert.AreEqual(105540, mspeaks.Count);
            
        }

        //[Test]
        //public void importPeaksFromUIMFFileTest1()
        //{
        //    string testPeakFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000_Frame800_802_peaks.txt";

        //    List<MSPeakResult> mspeaks = new List<MSPeakResult>();
        //    PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(testPeakFile);
        //    peakImporter.ImportUIMFPeaks(mspeaks);

        //    Assert.AreEqual(16787, mspeaks.Count);

        //    MSPeakResult testResult = mspeaks[42];
        //    Assert.AreEqual(43, testResult.PeakID);
        //    Assert.AreEqual(800, testResult.Frame_num);
        //    Assert.AreEqual(213, testResult.Scan_num);
        //    Assert.AreEqual(357.38662, (decimal)testResult.MSPeak.XValue);
        //    Assert.AreEqual(1528, testResult.MSPeak.Height);
        //    Assert.AreEqual(100, testResult.MSPeak.SN);
        //    Assert.AreEqual(16, testResult.MSPeak.MSFeatureID);
                
        //        //43	800	213	357.38662	1528	0.071	100	16

        //}


    }
}
