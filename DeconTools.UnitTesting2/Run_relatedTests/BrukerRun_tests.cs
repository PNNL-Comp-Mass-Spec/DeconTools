using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend;
using System.Diagnostics;
using DeconTools.Backend.Data;
using DeconTools.UnitTesting2;

namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class BrukerRun_tests
    {

        string bruker9TFolderRef = FileRefs.RawDataMSFiles.Bruker9TStandardFile1;
        string bruker9T_ser_FolderRef = FileRefs.RawDataMSFiles.Bruker9TStandardFile1 + @"\0.ser";             //Bruker files can be referenced by their 0.ser folder
        string bruker9T_ACQUS_alternateRef = FileRefs.RawDataMSFiles.Bruker9TStandardFile1 + @"\0.ser\acqus";   //Bruker files can be referenced by their acqus file

        string brukerFIDFile = @"\\pnl\projects\MSSHARE\BrukerTestFiles\LIFT_CID\0_G3\1\1347.7400.LIFT_2\1SRef\fid";


        [Test]
        public void checkDataSetNamesAndPathsTest1()
        {
            Run run = new BrukerRun(bruker9TFolderRef);

            Assert.AreEqual("SWT_9t_TestDS216_Small", run.DatasetName);
            Assert.AreEqual(bruker9TFolderRef, run.DataSetPath);
            Assert.AreEqual(bruker9T_ser_FolderRef, run.Filename);
        }


        [Test]
        public void checkDataSetNamesAndPaths_fromAccessing_serFolder_Test1()
        {
            Run run = new BrukerRun(bruker9T_ser_FolderRef);
            Assert.AreEqual("SWT_9t_TestDS216_Small", run.DatasetName);

            Assert.AreEqual(bruker9TFolderRef, run.DataSetPath);
        }

        [Test]
        public void checkDataSetNamesAndPaths_fromAccessing_acqusFile_Test1()
        {
            Run run = new BrukerRun(bruker9T_ACQUS_alternateRef);
            Assert.AreEqual("SWT_9t_TestDS216_Small", run.DatasetName);
            Assert.AreEqual(bruker9TFolderRef, run.DataSetPath);
        }


        [Test]
        public void checkMinMaxScansFromAnotherBrukerFileTest1()
        {
            Run run = new BrukerRun(@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_9T\RSPH_Aonly_01_run1_11Oct07_Andromeda_07-09-02\acqus");
            Assert.AreEqual(1, run.MinScan);
            Assert.AreEqual(4275, run.MaxScan);
        }

        






        [Test]
        public void GetNumMSScansTest1()
        {
            Run run = new BrukerRun(bruker9T_ACQUS_alternateRef);
            int numScans = run.GetNumMSScans();
            Assert.AreEqual(600, numScans);

            run = new BrukerRun(bruker9T_ser_FolderRef);
            numScans = run.GetNumMSScans();
            Assert.AreEqual(600, numScans);

            run = new BrukerRun(bruker9TFolderRef);
            numScans = run.GetNumMSScans();
            Assert.AreEqual(600, numScans);
       }




        [Test]
        public void GetSpectrumTest1()
        {
            int testScan = 300;

            Run run = new BrukerRun(bruker9TFolderRef);
            run.GetMassSpectrum(new ScanSet(testScan),0,50000);

            Assert.AreEqual(98896, run.XYData.Xvalues.Length);
            Assert.AreEqual(98896, run.XYData.Yvalues.Length);

            //TestUtilities.DisplayXYValues(run.XYData);

            //TODO: validate m/z values
            Assert.AreEqual(955.164162718754m, (decimal)run.XYData.Xvalues[63689]);  
            Assert.AreEqual(439030.5625m, (decimal)run.XYData.Yvalues[63689]);      //btw - most intense in the spectrum
            //Assert.AreEqual(579.808837890625m, (decimal)run.XYData.Xvalues[85910]);  //TODO:  there seems to be discrepancy - 5th decimal -  between this and the result from the Decon2LS.UI.  Possibly a FT-MS preprocessing difference?
            //Assert.AreEqual(61576.37109375d, run.XYData.Yvalues[85910]);


            Assert.AreEqual(1, run.GetMSLevel(testScan));
        }

    
        [Test]
        public void GetSummedSpectrumTest1()
        {
            int testScan = 300;

            Run run = new BrukerRun(bruker9TFolderRef);

            ScanSet scan299 = new ScanSet(299);
            run.GetMassSpectrum(scan299, 200, 2000);
            double scan299testVal = run.XYData.Yvalues[63689];
            Assert.AreEqual(267637.84375m, (decimal)scan299testVal);

            ScanSet scan300 = new ScanSet(300);
            run.GetMassSpectrum(scan300, 200, 2000);
            double scan300testVal = run.XYData.Yvalues[63689];
            Assert.AreEqual(439030.5625m,(decimal)scan300testVal);

            ScanSet scan301 = new ScanSet(301);
            run.GetMassSpectrum(scan301, 200, 2000);
            double scan301testVal = run.XYData.Yvalues[63689];
            Assert.AreEqual(205542.765625m, (decimal)scan301testVal);

            double summedVal = scan299testVal + scan300testVal + scan301testVal;

            ScanSet summedScanSet1 = new ScanSet(testScan, new int[] { 299,300,301 });

            run.GetMassSpectrum(summedScanSet1, 200, 2000);
            Assert.AreEqual(98896, run.XYData.Xvalues.Length);
            Assert.AreEqual(98896, run.XYData.Yvalues.Length);
            Assert.AreEqual(summedVal, run.XYData.Yvalues[63689]);

            //TestUtilities.DisplayXYValues(run.XYData);
        }

  
        [Test]
        public void GetSpectrumSpeedTest1()
        {
            Run run = new BrukerRun(bruker9TFolderRef);
            int numScansToGet = 50;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(300), 0, 50000);

            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;
            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);   // approx 62 ms
        }

        [Test]
        public void GetSpectrumSpeedTest2()
        {
            Run run = new BrukerRun(bruker9TFolderRef);
            int numScansToGet = 50;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(1005), 500, 600);   //  some filtering is involved... therefore time is added. 
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;
            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);   //62 ms
        }


      


        [Test]
        public void GetSpectrumSpeedTest_summedSpectra_Test1()
        {
            Run run = new BrukerRun(bruker9TFolderRef);
            int numScansToGet = 20;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < numScansToGet; i++)
            {
                run.GetMassSpectrum(new ScanSet(300, 298, 302), 0, 50000);   //  sum five scans
            }
            sw.Stop();

            double avgTime = ((double)sw.ElapsedMilliseconds) / (double)numScansToGet;
            Console.WriteLine("Average GetScans() time in milliseconds for " + numScansToGet + " scans = \t" + avgTime);   //343 ms / summed scan
        }



        [Test]
        public void GetSpectrumTypesTest1()
        {
            Run run = new BrukerRun(bruker9TFolderRef);


            Assert.AreEqual(1, run.GetMSLevel(6067));
            Assert.AreEqual(1, run.GetMSLevel(6068));
            Assert.AreEqual(1, run.GetMSLevel(6069));
            Assert.AreEqual(1, run.GetMSLevel(6070));
        }


        [Test]
        public void getClosestMSLevelScanTest1()
        {
            Run run = new BrukerRun(bruker9TFolderRef);

            Assert.AreEqual(300, run.GetClosestMSScan(300, Globals.ScanSelectionMode.CLOSEST));
            Assert.AreEqual(300, run.GetClosestMSScan(300, Globals.ScanSelectionMode.CLOSEST));
            Assert.AreEqual(300, run.GetClosestMSScan(300, Globals.ScanSelectionMode.CLOSEST));

            Assert.AreEqual(300, run.GetClosestMSScan(300, Globals.ScanSelectionMode.ASCENDING));
            Assert.AreEqual(300, run.GetClosestMSScan(300, Globals.ScanSelectionMode.DESCENDING));




        }


    }


}
