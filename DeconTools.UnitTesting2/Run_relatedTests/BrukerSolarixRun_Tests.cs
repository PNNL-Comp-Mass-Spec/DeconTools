using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class BrukerSolarixRun_Tests
    {
        [Test]
        public void checkDataSetNames_andPaths()
        {
            BrukerSolarixRun run = new BrukerSolarixRun(FileRefs.RawDataMSFiles.BrukerSolarixFile1);
            Assert.AreEqual("12Ttest_000003", run.DatasetName);
            Assert.AreEqual(@"\\pnl\projects\MSSHARE\BrukerTestFiles\12Ttest_000003", run.DataSetPath);
        }

        [Test]
        public void settingsTest1()
        {
            BrukerSolarixRun run = new BrukerSolarixRun(FileRefs.RawDataMSFiles.BrukerSolarixFile1);

            Assert.AreEqual(184346289.692624m, (decimal)run.CalibrationData.ML1);
            Assert.AreEqual(12.8161378555916m, (decimal)run.CalibrationData.ML2);
            Assert.AreEqual(416666.666666667m, (decimal)run.CalibrationData.SW_h);
            Assert.AreEqual(524288, run.CalibrationData.TD);
            Assert.AreEqual(73725.699739194m, (decimal)run.CalibrationData.FR_Low);
            Assert.AreEqual(0, run.CalibrationData.ByteOrder);
        }



    }
}
