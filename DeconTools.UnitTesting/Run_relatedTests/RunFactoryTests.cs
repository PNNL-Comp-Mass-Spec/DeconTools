using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Data;
using DeconTools.Backend.Core;
using DeconTools.Backend;

namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class RunFactoryTests
    {
        string brukerTestFile1 = @"H:\N14N15Data\RSPH_Aonly_26_run1_20Oct07_Andromeda_07-09-02\Acqu";
        string agilentDFile1 = @"F:\Gord\Data\AgilentD\BSA_TOF4.d";

        [Test]
        public void createBrukerRunTest1()
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(brukerTestFile1);

            Assert.AreEqual(Globals.MSFileType.Bruker, run.MSFileType);
            Assert.AreEqual(4276, run.MaxScan);
        }

        [Test]
        public void createAgilentDRunTest1()
        {

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(agilentDFile1);

            Assert.AreEqual(Globals.MSFileType.Agilent_D, run.MSFileType);

        }



    }
}
