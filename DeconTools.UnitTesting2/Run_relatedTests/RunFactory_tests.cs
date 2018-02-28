using System;
using DeconTools.Backend;
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
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.Bruker15TFile1);

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(17, run.MaxLCScan);
        }

        [Test]
        public void createBruker_fromBruker12T_FID_Test1()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.BrukerSolarix12T_FID_File1);

            Assert.IsNotNull(run);
            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(0, run.MaxLCScan);
        }



        [Test]
        public void createBruker_fromBruker12T_Test1()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(7, run.MaxLCScan);
        }

        [Test]
        public void createBruker_fromBruker9T_Test1()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.Bruker9TStandardFile1);

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(599, run.MaxLCScan);
        }




        [Test]
        public void createBrukerSolarixRunTest1()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(7, run.MaxLCScan);

        }


        [Test]
        public void CreateMZMLRunTest1()
        {
            var testfile =
                @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzML";

            var rf = new RunFactory();
            var run = rf.CreateRun(testfile);

            Assert.AreEqual(Globals.MSFileType.MZML, run.MSFileType);
            Assert.AreEqual(18504, run.MaxLCScan);
        }


        [Test]
        public void CreateMZXMLRunTest1()
        {
            var testfile =
                @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzXML";

            var rf = new RunFactory();
            var run = rf.CreateRun(testfile);

            Assert.AreEqual(Globals.MSFileType.MZXML_Rawdata, run.MSFileType);
            Assert.AreEqual(18504, run.MaxLCScan);
        }


        [Test]
        public void CreateMZ5RunTest1()
        {
            var testfile =
                @"\\proto-2\unitTest_Files\DeconTools_TestFiles\mzXML\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mz5";

            var rf = new RunFactory();
            var run = rf.CreateRun(testfile);

            Assert.AreEqual(Globals.MSFileType.MZ5, run.MSFileType);
            Assert.AreEqual(18504, run.MaxLCScan);
        }



        [Test]
        public void createAgilentDRunTest1()
        {

            var rf = new RunFactory();
            var run = rf.CreateRun(FileRefs.RawDataMSFiles.AgilentDFile1);

            Assert.AreEqual(Globals.MSFileType.Agilent_D, run.MSFileType);

        }

        [Test]
        public void createTextFileRunTest1()
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(textFile1);

            Assert.AreEqual(Globals.MSFileType.Ascii, run.MSFileType);

        }

        [Test]
        public void CreateBrukerTOFRunTest1()
        {
            var testfile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_Maxis\2012_05_15_MN9_A_000001.d";
            var rf = new RunFactory();

            var run = rf.CreateRun(testfile);

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.IsTrue(run is BrukerTOF);
            Assert.AreEqual(1131, run.GetNumMSScans());
        }

        [Test]
        public void UnknownDatasetTypeTest1()
        {
            var testfile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Other_testFileTypes\sampleXYData.wiffer";

            var rf = new RunFactory();

            try
            {
                var run = rf.CreateRun(testfile);
            }
            catch (ApplicationException)
            {
                Assert.Pass("ApplicationException caught; this is expected");
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception caught of type {0}: {1}", ex.GetType(), ex.Message);
            }

            Assert.Fail("Exception was not caught; we expected an ApplicationException to be thrown");

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
