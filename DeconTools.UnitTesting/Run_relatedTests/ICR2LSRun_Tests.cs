using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using System.Diagnostics;

namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class ICR2LSRun_Tests
    {
        private string icr2LSRawDatafile1 = "..\\..\\TestFiles\\ICR2LS_RawData\\ICR2LSRawData_Samplefile1_RSPH_PtoA_.02210";

        [Test]
        public void GetNumScansTest1()
        {
            Run run = new ICR2LSRun(icr2LSRawDatafile1);
            Console.WriteLine(run.GetNumMSScans());

        }

        public void GetMSLevelTest1()
        {
            Run run = new ICR2LSRun(icr2LSRawDatafile1);
            int scanLevel = run.GetMSLevel(2210);

            Console.WriteLine(scanLevel);
        }

        public void GetSpectrumTest1()
        {
            Run run = new ICR2LSRun(icr2LSRawDatafile1);
            run.GetMassSpectrum(new ScanSet(2210), 0, 2000);
         
            Assert.AreEqual(211062, run.XYData.Xvalues.Length);

        }

        public void GetSpectrumTest2()
        {
            Run run = new ICR2LSRun(icr2LSRawDatafile1);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            run.GetMassSpectrum(new ScanSet(2210), 500, 550);
            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);

            StringBuilder sb = new StringBuilder();

            TestUtilities.GetXYValuesToStringBuilder(sb, run.XYData.Xvalues, run.XYData.Yvalues);
            Console.Write(sb.ToString());

            Assert.AreEqual(18580, run.XYData.Xvalues.Length);

        }


        [Test]
        public void GetSpectrumSpeedTest1()
        {
            Run run = new ICR2LSRun(icr2LSRawDatafile1);
            int numScansToGet = 10;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(2210), 0, 10000000);
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;
            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);   
        }

        [Test]
        public void GetSpectrumSpeedTest2()
        {
            Run run = new ICR2LSRun(icr2LSRawDatafile1);
            int numScansToGet = 10;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(2210), 500, 600);   //
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;
            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);   //141 ms
        }

        [Test]
        public void GetTICTest1()
        {
            int scanNum = 2210;

            Run run = new ICR2LSRun(icr2LSRawDatafile1);
            run.GetMassSpectrum(new ScanSet(scanNum), 800, 2000);

            float ticVal = run.GetTIC(400, 2000);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            ticVal = run.GetTIC(400, 2000);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            Assert.AreEqual(3070655000m, (decimal)ticVal);     //TODO: need to confirm this number
        }



    }
}
