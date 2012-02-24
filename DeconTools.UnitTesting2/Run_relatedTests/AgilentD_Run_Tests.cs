using System;
using System.Diagnostics;
using System.IO;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;
using NUnit.Framework;

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
        public void getMSLevelTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentDRun(agilentDataset1);
            Assert.AreEqual(1, run.GetMSLevel(25)); 
        }


        [Test]
        public void GetMSLevelTest2()
        {
            string testfile =
                @"\\proto-5\BionetXfer\People\ScottK\2012_01_12 SPIN QTOF3\GLY06_11JAN12_LYNX_SN7980_TOP4wList_75000_SPIN_2.d";

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

            //   IMsdrDataReader m_reader = new MassSpecDataReader();
            // m_reader.OpenDataFile(testfile);

            
           
          
            //double scanTime = 30;




            //Console.WriteLine(m_reader.MSScanFileInformation.MSLevel);

            //m_reader.MSScanFileInformation.



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



  



    }
}
