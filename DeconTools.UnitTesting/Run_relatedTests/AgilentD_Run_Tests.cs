using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;
using DeconTools.Backend;

namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class AgilentD_Run_Tests
    {


        string agilentDataset1 = @"\\pnl\projects\MSSHARE\Gord\Test_data\BSA_TOF4.d";
        string aglientDataset2 = @"\\pnl\projects\MSSHARE\Gord\Test_data\bsa_tof5.d";
        string agilentDataset3 = @"\\proto-10\IMS_DEV2\Agilent_QTOF_data\Shew_250ng-r001.d";


        string wrongFileExample1 = @"\\pnl\projects\MSSHARE\Gord\Test_data\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        string temp_agilentFile1 = @"F:\Gord\Data\UIMF\FPGA\2010_05_13\Agilent_Cal_100513.d";



          



        [Test]
        public void ConstructorTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentDataset1);

            Assert.AreEqual("BSA_TOF4", run.DatasetName);
            Assert.AreEqual(@"\\pnl\projects\MSSHARE\Gord\Test_data", run.DataSetPath);
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
        public void GetSpectrumTest2()
        {
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(aglientDataset2);

            ScanSet scanset = new ScanSet(25);

            run.GetMassSpectrum(scanset, 0, 6000);
            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);


        }


        [Test]
        public void GetBelovSpectrumTest1()
        {
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentDataset3);

            ScanSet scanset = new ScanSet(25);

            run.GetMassSpectrum(scanset, 0, 6000);
            TestUtilities.DisplayXYValues(run.XYData);
            Console.WriteLine("numPoints = " + run.XYData.Xvalues.Length);
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
        public void getTimeTest2()
        {
            Run run = new DeconTools.Backend.Runs.AgilentD_Run(agilentDataset1);
            ScanSet scanset = new ScanSet(25);
            run.GetMassSpectrum(scanset, 0, 6000);

            Assert.AreEqual(0.414033333333333m, (decimal)run.GetTime(25));
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



  



    }
}
