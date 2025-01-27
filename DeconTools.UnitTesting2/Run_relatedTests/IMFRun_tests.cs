﻿using System;
using System.IO;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
#if !Disable_DeconToolsV2
    [TestFixture]
    public class IMFRun_tests
    {
        [Test]
        public void instiatateRunTest1()
        {
            Run run = new IMFRun(FileRefs.RawDataMSFiles.IMFStdFile1);

            Assert.AreEqual(Path.GetDirectoryName(FileRefs.RawDataMSFiles.IMFStdFile1), run.DatasetDirectoryPath);
            Assert.AreEqual("50ugpmlBSA_CID_QS_16V_0000.Accum_1", run.DatasetName);
        }

        [Test]
        public void GetMassSpectrumTest1()
        {

            Run run = new IMFRun(FileRefs.RawDataMSFiles.IMFStdFile1);

            var xyData = run.GetMassSpectrum(new ScanSet(233), 200, 2000);

            var numscans = run.GetNumMSScans();
            Assert.AreEqual(600, numscans);

            xyData.GetXYValuesAsSingles(out var xvals, out var yvals);


            var sb = new StringBuilder();

            for (var i = 0; i < xvals.Length; i++)
            {
                sb.Append(xvals[i]);
                sb.Append("\t");
                sb.Append(yvals[i]);
                sb.Append(Environment.NewLine);
            }
            Console.Write(sb.ToString());

            Assert.AreEqual(2595, xvals.Length);
            Assert.AreEqual(2595, yvals.Length);


        }

        [Test]
        public void GetMassSpectrumAndSumAllTest()
        {
            Run run = new IMFRun(FileRefs.RawDataMSFiles.IMFStdFile1);
            var xyData = run.GetMassSpectrum(new ScanSet(10000, 1, 20000), 200, 2000);     // this will sum scans 1 through 20000 (if <20000 scans, then will sum all available)

            var numscans = run.GetNumMSScans();
            Assert.AreEqual(600, numscans);
            Assert.AreEqual(38326, xyData.Xvalues.Length);
            Assert.AreEqual(38326, xyData.Yvalues.Length);
        }

        [Test]
        public void GetSpectrumTypesTest1()
        {
            Run run = new IMFRun(FileRefs.RawDataMSFiles.IMFStdFile1);
            Assert.AreEqual(1, run.GetMSLevel(233));    //this function hasn't yet been implemented for IMF files
            Assert.AreEqual(1, run.GetMSLevel(234));
            Assert.AreEqual(1, run.GetMSLevel(235));
        }

    }
#endif
}
