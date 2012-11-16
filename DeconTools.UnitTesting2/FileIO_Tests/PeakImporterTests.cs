using System.Collections.Generic;
using DeconTools.Backend.Data;
using DeconTools.Backend.DTO;
using NUnit.Framework;

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

        [Test]
        public void importPeaksFromUIMFFileTest1()     //NOTE:  2012_11_15 - The importer imports UIMF peaks as if they were orbi peaks.  All IMS scan info is ignored
        {
            string testPeakFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\UIMF_O16O18Testing\RawData\Alz_O18_Run03_7Sep12_Cheetah_11-12-23_peaks.txt";

            List<MSPeakResult> mspeaks = new List<MSPeakResult>();
            var peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(testPeakFile);
            peakImporter.ImportPeaks(mspeaks);

            Assert.AreEqual(202928, mspeaks.Count);

            MSPeakResult testResult = mspeaks[0];
            Assert.AreEqual(1, testResult.PeakID);
            Assert.AreEqual(-1, testResult.FrameNum);
            Assert.AreEqual(1, testResult.Scan_num);
            Assert.AreEqual(386.74464, (decimal)testResult.MSPeak.XValue);
            Assert.AreEqual(49599, testResult.MSPeak.Height);
            Assert.AreEqual(199.19, (decimal)testResult.MSPeak.SN);
            Assert.AreEqual(-1, testResult.MSPeak.MSFeatureID);

           // 43	800	213	357.38662	1528	0.071	100	16

        }


    }
}
