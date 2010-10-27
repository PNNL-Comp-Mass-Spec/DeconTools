using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    public class YAFMS_FileIntegrityTests
    {

        string targetFile = @"D:\Data\MEND\3_c_elegans_large_eqd_zero.yafms";
        [Test]
        public void checkDataSetNamesAndPathsTest()
        {
            Run run = new YAFMSRun(targetFile);

            Assert.AreEqual(targetFile, run.Filename);
            Assert.AreEqual("3_c_elegans_large_eqd_zero", run.DatasetName);
        }

        [Test]
        public void checkMinAndMaxScansTest1()
        {
            Run run = new YAFMSRun(targetFile);

            Assert.AreEqual(2984, run.GetNumMSScans());
            Assert.AreEqual(0, run.MinScan);
            Assert.AreEqual(2983, run.MaxScan);
        }


        [Test]
        public void GetSpectrumTest1()
        {
            int testScan = 2000;

            Run run = new YAFMSRun(targetFile);

            ScanSet scanset = new ScanSet(testScan);

            run.GetMassSpectrum(scanset);

            Assert.AreNotEqual(run.XYData, null);
            Assert.AreNotEqual(run.XYData.Xvalues, null);
            Assert.AreNotEqual(run.XYData.Xvalues.Length, 0);
            Assert.AreEqual(53348, run.XYData.Xvalues.Length);

            run.XYData.Display();
        }

    
    }
}
