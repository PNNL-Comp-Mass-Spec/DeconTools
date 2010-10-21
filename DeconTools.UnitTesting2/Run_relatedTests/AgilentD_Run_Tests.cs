using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;
using DeconTools.Backend;
using System.IO;

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
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentDataset1);

            Assert.AreEqual("BSA_TOF4", run.DatasetName);
            Assert.AreEqual("\\\\protoapps\\UserData\\Slysz\\DeconTools_TestFiles\\AgilentD\\BSA_TOF4\\BSA_TOF4.D", run.DataSetPath);
           
            Assert.AreEqual(61, run.MaxScan);
            Assert.AreEqual(0, run.MinScan);
            Assert.AreEqual(Globals.MSFileType.Agilent_D, run.MSFileType);
        }

        [Test]
        public void getNumMSScansTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentDataset1);
            Assert.AreEqual(62, run.GetNumMSScans());

        }

        [Test]
        public void GetSpectrumTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentDataset1);

            ScanSet scanset = new ScanSet(25);

            run.GetMassSpectrum(scanset, 0, 6000);
            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);
            Assert.AreEqual(156721, run.XYData.Xvalues.Length);
        }

       
        [Test]
        public void getMSLevelTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentDataset1);
            Assert.AreEqual(1, run.GetMSLevel(25)); 
        }

        [Test]
        public void getTimeTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentDataset1);
            Assert.AreEqual(0.414033333333333m, (decimal)run.GetTime(25));
        }


        [Test]
        public void disposeTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentDataset1);

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
                Run run = new DeconTools.Backend.Runs.AgilentD_Run(wrongFileExample1);
            });
            Assert.That(ex.Message, Is.EqualTo("Dataset's inputted name refers to a file, but should refer to a Folder"));
        }


        [Test]
        public void ConstructorError_wrongKindOfInputTest2()
        {
            PreconditionException ex = Assert.Throws<PreconditionException>(delegate
            {
                Run run = new DeconTools.Backend.Runs.AgilentD_Run(wrongFileExample1 + ".txt");
            });
            Assert.That(ex.Message, Is.EqualTo("Dataset not found."));
        }

        [Test]
        public void ConstructorError_wrongKindOfInputTest3()
        {
            PreconditionException ex = Assert.Throws<PreconditionException>(delegate
            {
                Run run = new DeconTools.Backend.Runs.AgilentD_Run("J:\test");
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

                Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentFileButMissingDotD);
            });
            Assert.That(ex.Message, Is.EqualTo("Agilent_D dataset folders must end with with the suffix '.d'. Check your folder name."));
        }



  



    }
}
