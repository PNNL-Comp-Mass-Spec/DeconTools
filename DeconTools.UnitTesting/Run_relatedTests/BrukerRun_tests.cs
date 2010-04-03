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
        string brukerTestFile1 = @"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_22_run1_19Oct07_Andromeda_07-09-02";
        string brukerTestFile2 = @"\\Proto-4\SWT_ICR2\SWT_9t_TestDS216_Small\0.ser";
        string brukerTestFile1b = @"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_22_run1_19Oct07_Andromeda_07-09-02\acqus";

        string brukerTestfile3 = @"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_28_run2_14Jan08_Raptor_07-11-11\acqus";


        [Test]
        public void checkDataSetNamesAndPathsTest1()
        {
            string testFile = brukerTestFile1;

            Run run = new BrukerRun(testFile);

            Assert.AreEqual("RSPH_Aonly_22_run1_19Oct07_Andromeda_07-09-02", run.DatasetName);

            Assert.AreEqual(@"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_22_run1_19Oct07_Andromeda_07-09-02", run.DataSetPath);


        }

 
        [Test]
        public void checkDataSetNamesAndPathsTest2()
        {
            string testFile = brukerTestFile1b;

            Run run = new BrukerRun(testFile);

            Assert.AreEqual("RSPH_Aonly_22_run1_19Oct07_Andromeda_07-09-02", run.DatasetName);

            Assert.AreEqual(@"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_22_run1_19Oct07_Andromeda_07-09-02", run.DataSetPath);


        }

        [Test]
        public void GetSpectrumTest1()
        {
            string testFile = brukerTestFile1;
            int testScan = 2501;

            Run run = new BrukerRun(testFile);
            run.GetMassSpectrum(new ScanSet(testScan), 200, 2000);

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
            TestUtilities.GetXYValuesToStringBuilder(sb, run.XYData.Xvalues, run.XYData.Yvalues);
            Console.WriteLine(sb.ToString());

            Assert.AreEqual(211063, run.XYData.Xvalues.Length);
            Assert.AreEqual(211063, run.XYData.Yvalues.Length);
            Assert.AreEqual(579.808837890625m, (decimal)run.XYData.Xvalues[85910]);  //TODO:  there seems to be discrepancy - 5th decimal -  between this and the result from the Decon2LS.UI.  Possibly a FT-MS preprocessing difference?
            //Assert.AreEqual(61576.37109375d, run.XYData.Yvalues[85910]);


            Assert.AreEqual(1, run.GetMSLevel(testScan));
        }

        [Test]
        public void GetSpectrum_FromNetwork_Test1()
        {
            string testFile = brukerTestFile2;
            int testScan = 500;

            Run run = new BrukerRun(testFile);
            run.GetMassSpectrum(new ScanSet(testScan), 200, 2000);

            int numscans = run.GetNumMSScans();
            Assert.AreEqual(600, numscans);
            Assert.AreEqual(98896, run.XYData.Xvalues.Length);
            Assert.AreEqual(98896, run.XYData.Yvalues.Length);
            Assert.AreEqual(1, run.GetMSLevel(testScan));
        }



        public void GetSummedSpectrumTest1()
        {
            string testFile = brukerTestFile1;
            int testScan = 2500;

            Run run = new BrukerRun(testFile);

            ScanSet scan2499 = new ScanSet(2499);
            run.GetMassSpectrum(scan2499, 200, 2000);
            double scan2499testVal = run.XYData.Yvalues[85910];
            Assert.AreEqual(19073.291015625d, scan2499testVal);

            ScanSet scan2500 = new ScanSet(2500);
            run.GetMassSpectrum(scan2500, 200, 2000);
            double scan2500testVal = run.XYData.Yvalues[85910];
            Assert.AreEqual(61576.37109375d, scan2500testVal);

            ScanSet scan2501 = new ScanSet(2501);
            run.GetMassSpectrum(scan2501, 200, 2000);
            double scan2501testVal = run.XYData.Yvalues[85910];
            Assert.AreEqual(12558.935546875d, scan2501testVal);

            double summedVal = scan2499testVal + scan2500testVal + scan2501testVal;

            ScanSet summedScanSet1 = new ScanSet(testScan, new int[] { 2499, 2500, 2501 });

            run.GetMassSpectrum(summedScanSet1, 200, 2000);
            Assert.AreEqual(211063, run.XYData.Xvalues.Length);
            Assert.AreEqual(211063, run.XYData.Yvalues.Length);
            Assert.AreEqual(summedVal, run.XYData.Yvalues[85910]);

            StringBuilder sb = new StringBuilder();
            TestUtilities.GetXYValuesToStringBuilder(sb, run.XYData.Xvalues, run.XYData.Yvalues);
            Console.WriteLine(sb.ToString());
        }



        //[Test]
        //public void OpenTwoBrukerFilesTest1()
        //{
        //    Run run1 = new BrukerRun(brukerTestFile1);
        //    run1.GetMassSpectrum(new ScanSet(1005), 0, 500000);
        //    //run1.GetMSLevel(1005);


        //    Run run2 = new BrukerRun(brukerTestFile2);
        //    //run2.GetMSLevel(1005);

        //    run2.GetMassSpectrum(new ScanSet(1005), 0, 500000);

        //}

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
        public void GetSpectrumSpeedTest1_fromNetwork()
        {
            Run run = new BrukerRun(brukerTestFile2);
            int numScansToGet = 50;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(500), 0, 5000000);   //  some filtering is involved... therefore time is added. 
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;
            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);   //141 ms
        }




        [Test]
        public void GetSpectrumSpeedTest_summedSpectra_Test1()
        {
            Run run = new BrukerRun(brukerTestFile1);
            int numScansToGet = 20;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(1005, 1003, 1007), 0, 50000);   //  sum five scans
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;
            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);   //970 ms / summed scan
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


        [Test]
        public void getClosestMSLevelScanTest1()
        {
            Run run = new BrukerRun(brukerTestFile1);

            Assert.AreEqual(2000, run.GetClosestMSScan(2000, Globals.ScanSelectionMode.CLOSEST));
            Assert.AreEqual(2001, run.GetClosestMSScan(2001, Globals.ScanSelectionMode.CLOSEST));
            Assert.AreEqual(2002, run.GetClosestMSScan(2002, Globals.ScanSelectionMode.CLOSEST));

            Assert.AreEqual(2002, run.GetClosestMSScan(2002, Globals.ScanSelectionMode.ASCENDING));
            Assert.AreEqual(2002, run.GetClosestMSScan(2002, Globals.ScanSelectionMode.DESCENDING));




        }


    }


}
