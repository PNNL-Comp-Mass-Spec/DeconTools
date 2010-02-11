using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using UIMFLibrary;


namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class UIMFRun_Tests
    {
        public string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000.uimf";
        public string uimfFilepath2 = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";

        [Test]
        public void getUIMFFileTest1()
        {

            UIMFRun uimfRun = new UIMFRun(uimfFilepath2);



        }


        [Test]
        public void GetNumberOfFramesTest()
        {
            UIMFRun test = new UIMFRun();



            UIMFRun uimfRun = new UIMFRun(uimfFilepath2);
            int numframes = uimfRun.GetNumFrames();
            int numScans = uimfRun.GetNumMSScans();


            Console.WriteLine("Number of frames = " + numframes);
            Console.WriteLine("Number of scans = " + numScans);
            Assert.AreEqual(2400, numframes);
            Assert.AreEqual(1440000, numScans);


        }

        [Test]
        public void UIMFLibraryDirectAccessTest1()
        {
            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(uimfFilepath2);
            DataReader datareader = adapter.Datareader;

            int numFrames = Convert.ToInt32(datareader.GetGlobalParameters("NumFrames"));

            Console.WriteLine("Number of frames = " + numFrames);
            Assert.AreEqual(2400, numFrames);
        }

        [Test]
        public void UIMFLibrarySumScansTest1()
        {
            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(uimfFilepath2);
            DataReader datareader = adapter.Datareader;

            double[] xvals = new double[100000];
            int[] yvals = new int[100000];

            int summedTotal = datareader.SumScans(xvals, yvals, 0, 100, 105, 200, 300);
            Assert.AreEqual(91796, summedTotal);
        }



        /// <summary>
        /// we fixed the error here... should be able to delete this test later. 
        /// </summary>
        [Test]
        public void Weird_MZ_zeroValues_for_lowFrame_whenSummedTest1()
        {
            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(uimfFilepath2);
            DataReader datareader = adapter.Datareader;

            double[] xvals = new double[100000];
            int[] yvals = new int[100000];

            int numWeirdZeroMZPoints = 0;

            int summedTotal = datareader.SumScans(xvals, yvals, 0, 1, 3, 225, 225);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < xvals.Length; i++)
            {
                if (xvals[i] == 0 && yvals[i] > 0)
                {
                    sb.Append(xvals[i]);
                    sb.Append("\t");
                    sb.Append(yvals[i]);
                    sb.Append("\n");
                    numWeirdZeroMZPoints++;
                }


            }
            Console.Write(sb.ToString());

            Assert.AreEqual(0, numWeirdZeroMZPoints);


        }

        [Test]
        public void getNumMSScansTest1()
        {
            Run uimfRun = new UIMFRun(uimfFilepath2);
            int numScans = uimfRun.GetNumMSScans();

            int numScans2 = uimfRun.GetNumMSScans();

            Assert.AreEqual(1440000, numScans);
            Assert.AreEqual(1440000, numScans2);
        }

        [Test]
        public void getSummedMSTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath2);

            ScanSet scanset = new ScanSet(500025, 500000, 500050);

            uimfRun.GetMassSpectrum(scanset, 100, 2000);

            Console.WriteLine(uimfRun.XYData.Xvalues.Length);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < uimfRun.XYData.Xvalues.Length; i++)
            {
                sb.Append(uimfRun.XYData.Xvalues[i]);
                sb.Append("\t");
                sb.Append(uimfRun.XYData.Yvalues[i]);
                sb.Append(Environment.NewLine);

            }
            //Console.WriteLine(sb.ToString());

            Assert.AreEqual(4268, uimfRun.XYData.Xvalues.Length);
            Assert.AreEqual(4268, uimfRun.XYData.Yvalues.Length);
        }

        [Test]
        public void getSummedFramesMSTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath2);

            uimfRun.GetMassSpectrum(new ScanSet(300, 299, 301), new FrameSet(1200, 1199, 1201), 100, 2000);
            Console.WriteLine(uimfRun.XYData.Xvalues.Length);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < uimfRun.XYData.Xvalues.Length; i++)
            {
                sb.Append(uimfRun.XYData.Xvalues[i]);
                sb.Append("\t");
                sb.Append(uimfRun.XYData.Yvalues[i]);
                sb.Append(Environment.NewLine);
            }
            Console.WriteLine(sb.ToString());
            Assert.AreEqual(3066, uimfRun.XYData.Xvalues.Length);
            Assert.AreEqual(3066, uimfRun.XYData.Yvalues.Length);
        }

        [Test]
        public void getSummedFramesMSTest2()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath2);

            for (int i = 1200; i < (600 * 2400 / 4); i = i + 600)
            {
                uimfRun.GetMassSpectrum(new ScanSet(i), new FrameSet(1200, 1199, 1201), 100, 2000);
                //Console.WriteLine(uimfRun.XYData.Xvalues.Length);
            }
        }


        [Test]
        public void getOneFrameOneScanTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath2);

            for (int i = 200; i < 301; i++)
            {
                uimfRun.GetMassSpectrum(new ScanSet(i), new FrameSet(1200), 200, 2000);
                //Console.WriteLine(uimfRun.XYData.Xvalues.Length);
            }





        }


        public void getSummedFramesAndScansMSTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath2);

            uimfRun.GetMassSpectrum(new ScanSet(300, 1, 599), new FrameSet(1200, 1199, 1201), 100, 2000);
            Console.WriteLine(uimfRun.XYData.Xvalues.Length);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < uimfRun.XYData.Xvalues.Length; i++)
            {
                sb.Append(uimfRun.XYData.Xvalues[i]);
                sb.Append("\t");
                sb.Append(uimfRun.XYData.Yvalues[i]);
                sb.Append(Environment.NewLine);

            }
            //Console.WriteLine(sb.ToString());
            Assert.AreEqual(23505, uimfRun.XYData.Xvalues.Length);
            Assert.AreEqual(23505, uimfRun.XYData.Yvalues.Length);
        }

        [Test]
        public void getFramePressureTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath2);

            double framePressure = uimfRun.GetFramePressure(1000);

            Assert.AreNotEqual(0, framePressure);
            Assert.AreEqual(4.0065, (Decimal)framePressure);

        }

        [Test]
        public void getFramePressuresUsingLibraryTest1()
        {
            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(uimfFilepath2);
            DataReader datareader = adapter.Datareader;

            int numFrames = Convert.ToInt32(datareader.GetGlobalParameters("NumFrames"));

            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < numFrames; i = i + 200)
            {
                sb.Append(Convert.ToDouble(datareader.GetFrameParameters(i, "PressureFront")));
                sb.Append("\t");
                sb.Append(Convert.ToDouble(datareader.GetFrameParameters(i, "PressureBack")));
                sb.Append(Environment.NewLine);
            }




            Console.WriteLine(sb.ToString());
        }


        [Test]
        public void getDriftTimesTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath2);

            



            int startScan = 0;
            int stopScan = 599;

            double[] driftTimes = new double[stopScan - startScan + 1];


            StringBuilder sb = new StringBuilder();
            for (int i = startScan; i <= stopScan; i++)
            {
                driftTimes[i - startScan] = uimfRun.GetDriftTime(1300, i);
                sb.Append(driftTimes[i - startScan]);
                sb.Append(Environment.NewLine);

            }

            //Console.WriteLine(sb.ToString());
        }

        [Test]
        public void getDriftTimes2()
        {
            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(uimfFilepath2);
            DataReader datareader = adapter.Datareader;

            int numFrames = Convert.ToInt32(datareader.GetGlobalParameters("NumFrames"));

            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < numFrames; i++)
            {
                sb.Append(Convert.ToDouble(datareader.GetFrameParameters(i, "AverageTOFLength")));
                sb.Append(Environment.NewLine);
            }

            double avgTOFLength = Convert.ToDouble(datareader.GetFrameParameters(1000, "AverageTOFLength"));

            //Console.WriteLine(sb.ToString());
        }

        [Test]
        public void getDriftTimeForScanZero()
        {
            //UIMF scan numbers are 0-based.  But first scan should have a drifttime above 0. 

            UIMFRun uimfRun = new UIMFRun(uimfFilepath2);

            double driftTime = uimfRun.GetDriftTime(1300, 0);
            Assert.Greater((decimal)driftTime, 0);
        }

        [Test]
        public void GetMSLevelTest1()
        {
            Run run = new UIMFRun(uimfFilepath);
            Assert.AreEqual(1, run.GetMSLevel(233));
            Assert.AreEqual(1, run.GetMSLevel(234));
            Assert.AreEqual(1, run.GetMSLevel(2000));
        }

        [Test]
        public void GetNumBinsTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath2);
            int numBins = uimfRun.GetNumBins();
            Assert.AreEqual(92000, numBins);
        }

    }
}
