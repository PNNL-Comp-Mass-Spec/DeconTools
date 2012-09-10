using System;
using System.Diagnostics;
using System.IO;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;
using NUnit.Framework;
using PNNLOmics.Data;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class AgilentD_Run_Tests
    {


        string agilentDataset1 = FileRefs.RawDataMSFiles.AgilentDFile1;

        string wrongFileExample1 = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

  
        [Test]
        public void ConstructorTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentDRun(agilentDataset1);

            Assert.AreEqual("BSA_TOF4", run.DatasetName);
            Assert.AreEqual("\\\\protoapps\\UserData\\Slysz\\DeconTools_TestFiles\\AgilentD\\BSA_TOF4\\BSA_TOF4.D", run.DataSetPath);
           
            Assert.AreEqual(61, run.MaxScan);
            Assert.AreEqual(0, run.MinScan);
            Assert.AreEqual(Globals.MSFileType.Agilent_D, run.MSFileType);
        }

        [Test]
        public void getNumMSScansTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentDRun(agilentDataset1);
            Assert.AreEqual(62, run.GetNumMSScans());
        }

        [Test]
        public void GetSpectrumTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentDRun(agilentDataset1);

            ScanSet scanset = new ScanSet(25);

            run.GetMassSpectrum(scanset, 0, 6000);
            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);
            Assert.AreEqual(156721, run.XYData.Xvalues.Length);
        }

        [Test]
        ///checks the length of a non-summed and summed spectra
        public void GetSummedSpectrumTest1()
        {
            string testfile =
               @"\\proto-5\BionetXfer\People\ScottK\2012_01_12 SPIN QTOF3\GLY06_11JAN12_LYNX_SN7980_TOP4wList_75000_SPIN_2.d";

            Stopwatch stopWatch = new Stopwatch();
            

            Run run = new DeconTools.Backend.Runs.AgilentDRun(testfile);

            ScanSet scanset = new ScanSet(25);

            stopWatch.Start();

            run.GetMassSpectrum(scanset);

            stopWatch.Stop();
            TimeSpan singleSpectraLoadTime = stopWatch.Elapsed;

            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);
            Assert.AreEqual(258899, run.XYData.Xvalues.Length);


            ScanSet scansetSum = new ScanSet(25,24,26);

            stopWatch.Start();

            run.GetMassSpectrum(scansetSum);

            stopWatch.Stop();
            TimeSpan threeScanSumSpectraLoadTime = stopWatch.Elapsed;


            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);
            Console.WriteLine("This took " + singleSpectraLoadTime + " seconds to load one scan");
            Console.WriteLine("This took " + threeScanSumSpectraLoadTime + " seconds to load and sum three scans");
            Assert.AreEqual(258899, run.XYData.Xvalues.Length);
        }

        [Test]
        ///checks the length of a non-summed and summed spectra and restrict the mass range to 100Da
        public void GetSummedSpectrumTest2()
        {
            string testfile =
               @"\\proto-5\BionetXfer\People\ScottK\2012_01_12 SPIN QTOF3\GLY06_11JAN12_LYNX_SN7980_TOP4wList_75000_SPIN_2.d";

            Stopwatch stopWatch = new Stopwatch();


            Run run = new DeconTools.Backend.Runs.AgilentDRun(testfile);

            ScanSet scanset = new ScanSet(25);

            stopWatch.Start();

            run.GetMassSpectrum(scanset, 1000,1100);

            stopWatch.Stop();
            TimeSpan singleSpectraLoadTime = stopWatch.Elapsed;

            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);
            Assert.AreEqual(8926, run.XYData.Xvalues.Length);


            ScanSet scansetSum = new ScanSet(25, 24, 26);

            stopWatch.Start();

            run.GetMassSpectrum(scansetSum, 1000,1100);

            stopWatch.Stop();
            TimeSpan threeScanSumSpectraLoadTime = stopWatch.Elapsed;


            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);
            Console.WriteLine("This took " + singleSpectraLoadTime + " seconds to load one scan");
            Console.WriteLine("This took " + threeScanSumSpectraLoadTime + " seconds to load and sum three scans");
            Assert.AreEqual(8926, run.XYData.Xvalues.Length);
        }

        [Test]
        ///  Writes three spectra files to disk so that the data can be compared before and after summing
        public void GetSummedSpectrumTestWriteToDisk()
        {
            string testfile =
               @"\\proto-5\BionetXfer\People\ScottK\2012_01_12 SPIN QTOF3\GLY06_11JAN12_LYNX_SN7980_TOP4wList_75000_SPIN_2.d";

            Run run = new DeconTools.Backend.Runs.AgilentDRun(testfile);

            int primaryScan = 10000;
            //Scan A
            ScanSet scansetA = new ScanSet(primaryScan-1);

            run.GetMassSpectrum(scansetA, 0, 6000);

            TestUtilities.WriteToFile(run.XYData,@"P:\ScanA.txt");

            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);
            Assert.AreEqual(258899, run.XYData.Xvalues.Length);

            //Scan B
            ScanSet scansetB = new ScanSet(primaryScan);

            run.GetMassSpectrum(scansetB, 0, 6000);

            TestUtilities.WriteToFile(run.XYData, @"P:\ScanB.txt");

            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);
            Assert.AreEqual(258899, run.XYData.Xvalues.Length);

            //Scan C
            ScanSet scansetC = new ScanSet(primaryScan+1);

            run.GetMassSpectrum(scansetC, 0, 6000);

            TestUtilities.WriteToFile(run.XYData, @"P:\ScanC.txt");

            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);
            Assert.AreEqual(258899, run.XYData.Xvalues.Length);

            //Scan ABC summed
            ScanSet scansetSum = new ScanSet(primaryScan, primaryScan-1, primaryScan+1);//primary, min, max

            run.GetMassSpectrum(scansetSum, 0, 6000);

            TestUtilities.WriteToFile(run.XYData, @"P:\ScanABC.txt");

            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);
            Assert.AreEqual(258899, run.XYData.Xvalues.Length);
        }

       
        [Test]
        public void getMSLevelTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentDRun(agilentDataset1);
            Assert.AreEqual(1, run.GetMSLevel(25)); 
        }

        [Test]
        public void GetMSLevelTest2()
        {
            string testfile =
                @"\\proto-7\AgQTOF03\2012_3\GLY06_11JAN12_LYNX_SN7980_TOP4wList_75000_SPIN_2\GLY06_11JAN12_LYNX_SN7980_TOP4wList_75000_SPIN_2.d";

            Run run = new AgilentDRun(testfile);

            int scanStart = 3000;
            int scanStop = 3500;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int scan = scanStart; scan <= scanStop; scan++)
            {
                int mslevel = run.GetMSLevel(scan);
            }

            stopwatch.Stop();
            Console.WriteLine("Total time in milliseconds to get MSLevel for " + (scanStop - scanStart + 1) + " spectra = " +
                              stopwatch.ElapsedMilliseconds);

            Console.WriteLine("Average time (ms) to get MSLevel = " + (double)stopwatch.ElapsedMilliseconds/(scanStop-scanStart+1));


            Assert.AreEqual(1, run.GetMSLevel(612));
            Assert.AreEqual(2, run.GetMSLevel(613)); 
        }

        [Test]
        public void GetPrecursorTest1()
        {
            string testfile =
                @"\\proto-7\AgQTOF03\2012_3\GLY06_11JAN12_LYNX_SN7980_TOP4wList_75000_SPIN_2\GLY06_11JAN12_LYNX_SN7980_TOP4wList_75000_SPIN_2.d";

            Run run = new AgilentDRun(testfile);
            
            PrecursorInfo precursor = run.GetPrecursorInfo(612);

            Assert.AreEqual(1, precursor.MSLevel);
            Assert.AreEqual(-1, precursor.PrecursorCharge);
            Assert.AreEqual(0, precursor.PrecursorIntensity);
            Assert.AreEqual(0, precursor.PrecursorMZ);
            Assert.AreEqual(612, precursor.PrecursorScan);

            precursor = run.GetPrecursorInfo(613);

            Assert.AreEqual(2, precursor.MSLevel);
            Assert.AreEqual(1, precursor.PrecursorCharge);
            Assert.AreEqual(291638.781f, precursor.PrecursorIntensity);
            Assert.AreEqual(534.26422119140625, precursor.PrecursorMZ);
            Assert.AreEqual(613, precursor.PrecursorScan);
        }

        [Test]
        public void getTimeTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentDRun(agilentDataset1);
            Assert.AreEqual(0.414033333333333m, (decimal)run.GetTime(25));
        }

        [Test]
        public void disposeTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentDRun(agilentDataset1);

            using (Run disposedRun = run )
            {
                Assert.AreEqual(62, disposedRun.GetNumMSScans());
            }  
        }

       



   // ----------------------------- Exception tests -----------------------------------------

        [Test]
        public void ConstructorError_wrongKindOfInputTest1()
        {
            PreconditionException ex = Assert.Throws<PreconditionException>(delegate
            {
                Run run = new DeconTools.Backend.Runs.AgilentDRun(wrongFileExample1);
            });
            Assert.That(ex.Message, Is.EqualTo("Dataset's inputted name refers to a file, but should refer to a Folder"));
        }


        [Test]
        public void ConstructorError_wrongKindOfInputTest2()
        {
            PreconditionException ex = Assert.Throws<PreconditionException>(delegate
            {
                Run run = new DeconTools.Backend.Runs.AgilentDRun(wrongFileExample1 + ".txt");
            });
            Assert.That(ex.Message, Is.EqualTo("Dataset not found."));
        }

        [Test]
        public void ConstructorError_wrongKindOfInputTest3()
        {
            PreconditionException ex = Assert.Throws<PreconditionException>(delegate
            {
                Run run = new DeconTools.Backend.Runs.AgilentDRun(@"J:\test");
            });
            Assert.That(ex.Message, Is.EqualTo("Dataset not found."));
        }

        [Test]
        public void ConstructorError_wrongKindOfInputTest4()
        {
            PreconditionException ex = Assert.Throws<PreconditionException>(delegate
            {
                DirectoryInfo dirinfo = new DirectoryInfo(agilentDataset1);

                string agilentFileButMissingDotD = dirinfo.Parent.FullName;

                Run run = new DeconTools.Backend.Runs.AgilentDRun(agilentFileButMissingDotD);
            });
            Assert.That(ex.Message, Is.EqualTo("Agilent_D dataset folders must end with with the suffix '.d'. Check your folder name."));
        }


        [Test]
        public void Test1()
        {
            string testfile = @"\\protoapps\UserData\Nikola\DDD_Milk\D6.1.forExpRepAnal_3.14.2012.d";

            Run run = new RunFactory().CreateRun(testfile);
            Assert.IsNotNull(run);
            string info = TestUtilities.DisplayRunInformation(run);

            Console.WriteLine(info);


            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var scanSet = new ScanSet(1000);

            run.CurrentScanSet = scanSet;

            msgen.Execute(run.ResultCollection);


            TestUtilities.DisplayXYValues(run.XYData);






        }



    }
}
