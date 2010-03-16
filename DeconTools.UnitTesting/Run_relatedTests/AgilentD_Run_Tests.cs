using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class AgilentD_Run_Tests
    {


        string agilentDataset1 = @"\\pnl\projects\MSSHARE\Gord\Test_data\BSA_TOF4.d";
        string wrongFileExample1 = @"\\pnl\projects\MSSHARE\Gord\Test_data\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        [Test]
        public void ConstructorTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentDataset1);

            Assert.AreEqual("BSA_TOF4", run.DatasetName);
            Assert.AreEqual(@"\\pnl\projects\MSSHARE\Gord\Test_data", run.DataSetPath);
        }


        [Test]
        public void getNumMSScansTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentDataset1);

            Assert.AreEqual(62, run.GetNumMSScans());

        }


        [Test]
        public void ConstructorError_wrongKindOfInputTest1()
        {
            PreconditionException ex =  Assert.Throws<PreconditionException>(delegate
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
                Run run = new DeconTools.Backend.Runs.AgilentD_Run(wrongFileExample1+".txt");
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


    }
}
