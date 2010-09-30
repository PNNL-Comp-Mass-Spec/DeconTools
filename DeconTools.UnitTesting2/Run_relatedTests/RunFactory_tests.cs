using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    public class RunFactory_tests
    {

        string agilentDFile1 = @"F:\Gord\Data\AgilentD\BSA_TOF4.d";

        string textFile1 = "..\\..\\TestFiles\\TextFileXYData.txt";


        [Test]
        public void createBrukerRunTest1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.Bruker9TStandardFile1AlternateRef);

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(600, run.MaxScan);
        }


        [Test]
        public void createBrukerSolarixRunTest1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);

            Assert.AreEqual(Globals.MSFileType.Bruker_12T_Solarix, run.MSFileType);
            Assert.AreEqual(8, run.MaxScan);

        }




        [Test]
        public void createAgilentDRunTest1()
        {

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(agilentDFile1);

            Assert.AreEqual(Globals.MSFileType.Agilent_D, run.MSFileType);

        }

        [Test]
        public void createTextFileRunTest1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(textFile1);

            Assert.AreEqual(Globals.MSFileType.Ascii, run.MSFileType);

        }

    }
}
