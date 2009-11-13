using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend;
using System.Diagnostics;
using DeconTools.Backend.Data;

namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class BrukerRun_tests
    {
        string brukerTestFile1 = @"H:\N14N15Data\RSPH_Aonly_26_run1_20Oct07_Andromeda_07-09-02";
        string brukerTestFile2 = @"H:\N14N15Data\RSPH_Aonly_23_run1_25Oct07_Andromeda_07-09-02\0.ser"; 

        [Test]
        public void GetSpectrumTest1()
        {

            Run run = new BrukerRun(brukerTestFile1);
            run.GetMassSpectrum(new ScanSet(1005), 200, 2000);

            int numscans = run.GetNumMSScans();
            Assert.AreEqual(4276, numscans);

            StringBuilder sb = new StringBuilder();

            //for (int i = 0; i < run.XYData.Xvalues.Length; i++)
            //{
            //    sb.Append(Convert.ToDouble(run.XYData.Xvalues[i]));
            //    sb.Append("\t");
            //    sb.Append(Convert.ToDouble(run.XYData.Yvalues[i]));
            //    sb.Append(Environment.NewLine);
            //}
            //Console.Write(sb.ToString());

            Assert.AreEqual(211063, run.XYData.Xvalues.Length);
            Assert.AreEqual(211063, run.XYData.Yvalues.Length);
            //Assert.AreEqual(579.808837890625,run.XYData.Xvalues[85910]);  //TODO:  there seems to be discrepancy - 5th decimal -  between this and the result from the Decon2LS.UI.  Possibly a FT-MS preprocessing difference?
            Assert.AreEqual(26474.986328125, run.XYData.Yvalues[85910]);


            Assert.AreEqual(1, run.GetMSLevel(1005));
        }

        [Test]
        public void OpenTwoBrukerFilesTest1()
        {
            Run run1 = new BrukerRun(brukerTestFile1);
            run1.GetMassSpectrum(new ScanSet(1005), 0, 500000);
            //run1.GetMSLevel(1005);


            Run run2 = new BrukerRun(brukerTestFile2);
            //run2.GetMSLevel(1005);

            run2.GetMassSpectrum(new ScanSet(1005), 0, 500000);

        }

        [Test]
        public void runFactoryTest1()
        {
       

        }





        [Test]
        public void GetSpectrumTest2()
        {
            Run run = new BrukerRun(brukerTestFile1);
            run.GetMassSpectrum(new ScanSet(1005), 600, 700);

            int numscans = run.GetNumMSScans();
            Assert.AreEqual(4276, numscans);

            StringBuilder sb = new StringBuilder();

            //for (int i = 0; i < run.XYData.Xvalues.Length; i++)
            //{
            //    sb.Append(Convert.ToDouble(run.XYData.Xvalues[i]));
            //    sb.Append("\t");
            //    sb.Append(Convert.ToDouble(run.XYData.Yvalues[i]));
            //    sb.Append(Environment.NewLine);
            //}
            //Console.Write(sb.ToString());

            Assert.AreEqual(24330, run.XYData.Xvalues.Length);
            Assert.AreEqual(24330, run.XYData.Yvalues.Length);
            //Assert.AreEqual(579.808837890625,run.XYData.Xvalues[85910]);  //TODO:  there seems to be discrepancy - 5th decimal -  between this and the result from the Decon2LS.UI.  Possibly a FT-MS preprocessing difference?
     
            Assert.AreEqual(1, run.GetMSLevel(1005));
        }

        [Test]
        public void GetSpectrumSpeedTest1()
        {
            Run run = new BrukerRun(brukerTestFile1);
            int numScansToGet = 50;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(1005), 0, 50000);
                
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;
            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);   //133 ms
        }

        [Test]
        public void GetSpectrumSpeedTest2()
        {
            Run run = new BrukerRun(brukerTestFile1);
            int numScansToGet = 50;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(1005), 500, 600);   //  some filtering is involved... therefore time is added. 
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;
            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);   //141 ms
        }

        [Test]
        public void GetNumMSScansTest()
        {
            Run run = new BrukerRun(brukerTestFile1);
            Assert.AreEqual(4276, run.GetNumMSScans());
        }


        [Test]
        public void GetSpectrumTypesTest1()
        {
            Run run = new BrukerRun(brukerTestFile1);


            Assert.AreEqual(1, run.GetMSLevel(6067));
            Assert.AreEqual(1, run.GetMSLevel(6068));
            Assert.AreEqual(1, run.GetMSLevel(6069));
            Assert.AreEqual(1, run.GetMSLevel(6070));
        }


    }


}
