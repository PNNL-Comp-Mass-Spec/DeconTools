using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class IMFRun_tests
    {
        public string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";

        [Test]
        public void GetMassSpectrumTest1()
        {

            Run run = new IMFRun(imfFilepath);

            run.MSParameters = new DeconEngine_MSParameters(233, 1);

            run.GetMassSpectrum(new ScanSet(233),200,2000);

            int numscans = run.GetNumMSScans();
            Assert.AreEqual(600, numscans);

            float[] xvals = new float[1];
            float[] yvals = new float[1];
            run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);


            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < xvals.Length; i++)
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


            Run run = new IMFRun(imfFilepath);


            run.MSParameters = new DeconEngine_MSParameters(200, 1000000);
            run.GetMassSpectrum(new ScanSet(10000,1,20000),200,2000);     // this will sum scans 1 through 20000 (if <20000 scans, then will sum all available)

            int numscans = run.GetNumMSScans();
            Assert.AreEqual(600, numscans);

            float[] xvals = new float[1];
            float[] yvals = new float[1];
            run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);

            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < xvals.Length; i++)
            //{
            //    sb.Append(xvals[i]);
            //    sb.Append("\t");
            //    sb.Append(yvals[i]);
            //    sb.Append(Environment.NewLine);
            //}
            //Console.Write(sb.ToString());


            Assert.AreEqual(38326, xvals.Length);
            Assert.AreEqual(38326, yvals.Length);


        }

        [Test]
        public void CreateScanSetCollectionTest1()
        {
            //Run run = new IMFRun(imfFilepath);
            //run.CreateScanSetCollection(230, 233);        //June 20, 2009 --  decommissioned
            ////run.GetMassSpectrum();

            //Assert.AreEqual(4, run.ScanSetCollection.ScanSetList.Count);
            //Assert.AreEqual(230, run.ScanSetCollection.ScanSetList[0].IndexValues[0]);
            //Assert.AreEqual(231, run.ScanSetCollection.ScanSetList[1].IndexValues[0]);
            //Assert.AreEqual(232, run.ScanSetCollection.ScanSetList[2].IndexValues[0]);
            //Assert.AreEqual(233, run.ScanSetCollection.ScanSetList[3].IndexValues[0]);

        }

        //[Test]
        //public void CreateScanSetCollectionTest2()     //June 20 2009 -- decommissioned
        //{
        //    Run run = new IMFRun(imfFilepath);

        //    run.CreateScanSetCollection();

        //    Assert.AreEqual(600, run.ScanSetCollection.ScanSetList.Count);
        //    Assert.AreEqual(230, run.ScanSetCollection.ScanSetList[230].IndexValues[0]);
        //}

        [Test]
        public void GetSpectrumTypesTest1()
        {
            Run run = new IMFRun(imfFilepath);
            Assert.AreEqual(1, run.GetMSLevel(233));    //this function hasn't yet been implemented for IMF files
            Assert.AreEqual(1, run.GetMSLevel(234));
            Assert.AreEqual(1, run.GetMSLevel(235));  
        }

    }
}
