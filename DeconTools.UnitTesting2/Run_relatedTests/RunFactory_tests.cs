using System;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class RunFactory_tests
    {
        string textFile1 = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TextFile\sampleXYData.txt";

        [Test]
        public void createBruker_fromBruker15T_Test1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.Bruker15TFile1);

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(17, run.MaxScan);
        }

        [Test]
        public void createBruker_fromBruker12T_FID_Test1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.BrukerSolarix12T_FID_File1);

            Assert.IsNotNull(run);
            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(0, run.MaxScan);
        }



        [Test]
        public void createBruker_fromBruker12T_Test1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(7, run.MaxScan);
        }

        [Test]
        public void createBruker_fromBruker9T_Test1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.Bruker9TStandardFile1);

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(599, run.MaxScan);
        }




        [Test]
        public void createBrukerSolarixRunTest1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(7, run.MaxScan);

        }


        [Test]
        public void createBrukerSolarixRunTest2()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(Globals.MSFileType.Bruker, FileRefs.RawDataMSFiles.BrukerSolarix12TFile1, new OldDecon2LSParameters());

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(7, run.MaxScan);

        }




        [Test]
        public void createAgilentDRunTest1()
        {

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.AgilentDFile1);

            Assert.AreEqual(Globals.MSFileType.Agilent_D, run.MSFileType);

        }

        [Test]
        public void createTextFileRunTest1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(textFile1);

            Assert.AreEqual(Globals.MSFileType.Ascii, run.MSFileType);

        }

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void UnknownDatasetTypeTest1()
        {
            string testfile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Other_testFileTypes\sampleXYData.wiffer";

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(testfile);
        }


        // [Test]
        //[ExpectedException(typeof(ApplicationException))]
        //public void InvalidThermoDataTest1()
        // {
        //    string testfile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Other_testFileTypes\invalidRawData.raw";

        //    RunFactory rf = new RunFactory();
        //    Run run = rf.CreateRun(testfile);
        //}


        

    }
}
