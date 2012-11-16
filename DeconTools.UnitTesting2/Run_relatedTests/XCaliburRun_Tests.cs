#define includeLargeTests

using System;
using System.Diagnostics;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class XCaliburRun_Tests
    {
     

        [Test]
        public void checkDataSetNamesAndPathsTest()
        {
            string testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            Run run = new XCaliburRun(testFile);

            Assert.AreEqual("QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18", run.DatasetName);
        }

     
        [Test]
        public void checkMinAndMaxScansTest2()
        {
            string testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            Run run = new XCaliburRun(testFile);

            Assert.AreEqual(1, run.MinLCScan);
            Assert.AreEqual(18505, run.MaxLCScan);
        }


        [Test]
        public void initializeVelosOrbiFile_Test1()
        {
            string testFile = FileRefs.RawDataMSFiles.VOrbiFile1;

            Run run = new XCaliburRun(testFile);

            Assert.AreEqual(1, run.MinLCScan);
            Assert.AreEqual(17773, run.MaxLCScan);

        }


        
        
        [Test]
        public void GetSpectrumTest1()
        {

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            
            run.GetMassSpectrum(new ScanSet(6067), 200, 2000);

            int numscans = run.GetNumMSScans();
            Assert.AreEqual(18505, numscans);

            float[] xvals = new float[1];
            float[] yvals = new float[1];
            run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);


            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < xvals.Length; i++)
            {
                sb.Append(Convert.ToDouble(xvals[i]));
                sb.Append("\t");
                sb.Append(Convert.ToDouble(yvals[i]));
                sb.Append(Environment.NewLine);
            }
            //Console.Write(sb.ToString());

            Assert.AreEqual(27053, xvals.Length);
            Assert.AreEqual(27053, yvals.Length);
            Assert.AreEqual(1, run.GetMSLevel(6067));
        }

        [Test]
        public void GetSummedSpectrumTest1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            run.GetMassSpectrum(new ScanSet(5998), 500.02, 500.037);
            Assert.AreEqual(0, run.XYData.Xvalues.Length);

            TestUtilities.DisplayXYValues(run.ResultCollection);
            run.GetMassSpectrum(new ScanSet(6005), 500.02, 500.037);
            Assert.AreEqual(500.024411148371m, (decimal)run.XYData.Xvalues[2]);
            Assert.AreEqual(36509.9296875m, (decimal)run.XYData.Yvalues[2]);

            TestUtilities.DisplayXYValues(run.ResultCollection);
            run.GetMassSpectrum(new ScanSet(6012), 500.02, 500.037);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            Assert.AreEqual(500.02441114589m, (decimal)run.XYData.Xvalues[2]);
            Assert.AreEqual(103000.921875m, (decimal)run.XYData.Yvalues[2]);

            run.GetMassSpectrum(new ScanSet(6005, new int[] { 5998, 6005, 6012 }), 500.02, 500.037);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            Assert.AreEqual(500.02441m, (decimal)run.XYData.Xvalues[2]);
            Assert.AreEqual(139510.8515625m, (decimal)run.XYData.Yvalues[2]);
        }

        [Test]
        public void getSummedSpectrumSimpleTest1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            ScanSet scanset = new ScanSet(6005, new int[] { 5991, 5998, 6005, 6012, 6019 });
            run.GetMassSpectrum(scanset, 400, 1500);

            Assert.AreEqual(70544321.0d, run.XYData.getMaxY());
        }
        


      

#if includeLargeTests
        [Test]
        public void GetSpectrumSpeedTest1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int numScansToGet = 100;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(6005), 0, 6000);
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;

            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);    //11.61 msec


        }

        [Test]
        public void GetSummedSpectrumSpeedTest_5_summedSpectra()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int numScansToGet = 50;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(6005, new int[] { 5991, 5998, 6005, 6012, 6019 }), 400, 1500);
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;

            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);    //330 msec


        }

        [Test]
        public void GetSummedSpectrumSpeedTest_3_summedSpectra()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int numScansToGet = 50;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(6005, new int[] { 5998, 6005, 6012 }), 400, 1500);
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;

            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);    //198 msec


        }


        [Test]
        public void GetSpectrumSpeedTest2()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int numScansToGet = 100;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(6005), 500, 600);
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;

            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);    //13 msec


        }

#endif

        [Test]
        public void GetSpectrumTypesTest1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);


            Assert.AreEqual(1, run.GetMSLevel(6067));
            Assert.AreEqual(2, run.GetMSLevel(6068));
            Assert.AreEqual(2, run.GetMSLevel(6069));
            Assert.AreEqual(2, run.GetMSLevel(6070));
        }

        [Test]
        public void ConstructorTest1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            Assert.AreEqual(1, run.MinLCScan);
            Assert.AreEqual(18505, run.MaxLCScan);

            run = null;
        }

        [Test]
        public void ConstructorTest2()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1, 6000, 7000);

            Assert.AreEqual(6000, run.MinLCScan);
            Assert.AreEqual(7000, run.MaxLCScan);

            run = null;
        }


        [Test]
        public void GetNextSpectrumTest1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1, 6000, 7000);

            ScanSetCollection scansetCollection = new ScanSetCollection();
            for (int i = 6000; i < 6015; i++)
            {
                scansetCollection.ScanSetList.Add(new ScanSet(i));
            }

            foreach (ScanSet scanset in scansetCollection.ScanSetList)
            {
                int msLevel = ((XCaliburRun)(run)).RawData.GetMSLevel(scanset.PrimaryScanNumber);
                run.GetMassSpectrum(scanset, 0, 2000);

            }






        }

        [Test]
        public void GetTICTest1()
        {

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            run.GetMassSpectrum(new ScanSet(6067), 400, 2000);

            float ticVal = run.GetTIC(400, 2000);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            ticVal = run.GetTIC(400, 2000);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            Assert.AreEqual(3558996000, (decimal)ticVal);
        }

        [Test]
        public void getClosestMSLevelScanTest1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            Assert.AreEqual(6005, run.GetClosestMSScan(6005, Globals.ScanSelectionMode.CLOSEST));
            Assert.AreEqual(6005, run.GetClosestMSScan(6006, Globals.ScanSelectionMode.CLOSEST));
            Assert.AreEqual(6005, run.GetClosestMSScan(6007, Globals.ScanSelectionMode.CLOSEST));
            Assert.AreEqual(6005, run.GetClosestMSScan(6008, Globals.ScanSelectionMode.CLOSEST));
            Assert.AreEqual(6012, run.GetClosestMSScan(6009, Globals.ScanSelectionMode.CLOSEST));
            Assert.AreEqual(6012, run.GetClosestMSScan(6010, Globals.ScanSelectionMode.CLOSEST));
            Assert.AreEqual(6012, run.GetClosestMSScan(6011, Globals.ScanSelectionMode.CLOSEST));
            Assert.AreEqual(6012, run.GetClosestMSScan(6012, Globals.ScanSelectionMode.CLOSEST));

            Assert.AreEqual(6005, run.GetClosestMSScan(6005, Globals.ScanSelectionMode.ASCENDING));
            Assert.AreEqual(6012, run.GetClosestMSScan(6006, Globals.ScanSelectionMode.ASCENDING));

            Assert.AreEqual(6012, run.GetClosestMSScan(6012, Globals.ScanSelectionMode.DESCENDING));
            Assert.AreEqual(6005, run.GetClosestMSScan(6011, Globals.ScanSelectionMode.DESCENDING));
            Assert.AreEqual(6005, run.GetClosestMSScan(6010, Globals.ScanSelectionMode.DESCENDING));


            Assert.AreEqual(1, run.GetClosestMSScan(1, Globals.ScanSelectionMode.DESCENDING));
            Assert.AreEqual(1, run.GetClosestMSScan(1, Globals.ScanSelectionMode.CLOSEST));

            Assert.AreEqual(18505, run.GetClosestMSScan(run.MaxLCScan, Globals.ScanSelectionMode.ASCENDING));
            Assert.AreEqual(18499, run.GetClosestMSScan(run.MaxLCScan, Globals.ScanSelectionMode.DESCENDING));


        }


        [Test]
        public void GetScanInfoTest1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            int msmsScanNum1 = 6000;

            ScanSet scan = new ScanSet(msmsScanNum1);

            string scanInfoString = run.GetScanInfo(scan.PrimaryScanNumber);
            Assert.AreEqual(@"ITMS + c NSI d Full ms2 543.80@cid35.00 [135.00-2000.00]", scanInfoString);

        }



        //[Test]
        //public void GetSpectrumTestAndSum5()       //currently this stalls - takes forever...  
        //{
        //    ParameterLoader loader = new ParameterLoader();
        //    loader.LoadParametersFromFile(parameterFilename);

        //    Run run = new XCaliburRun(xcaliburTestfile);

        //    run.MSParameters = new DeconEngine_MSParameters(6067, 5);

        //    run.GetMassSpectrum();

        //    int numscans = run.GetNumMSScans();
        //    Assert.AreEqual(18505, numscans);

        //    float[] xvals = new float[1];
        //    float[] yvals = new float[1];
        //    run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);

        //    StringBuilder sb = new StringBuilder();

        //    for (int i = 0; i < xvals.Length; i++)
        //    {
        //        sb.Append(Convert.ToDouble(xvals[i]));
        //        sb.Append("\t");
        //        sb.Append(Convert.ToDouble(yvals[i]));
        //        sb.Append(Environment.NewLine);
        //    }
        //    Console.Write(sb.ToString());

        //    Assert.AreEqual(27053, xvals.Length);
        //    Assert.AreEqual(27053, yvals.Length);
        //}

    }
}
