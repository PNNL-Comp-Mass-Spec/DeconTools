using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Core;
using DeconTools.Backend;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.MSGenerators;


namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class MSGeneratorTests
    {
        string textFile1 = "..\\..\\TestFiles\\TextFileXYData1.txt";

        string imfMSScanTextfile = "..\\..\\Testfiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt";
        string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private string xcaliburTestfile2 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        private string agilentDTestFile = @"F:\Gord\Data\AgilentD\BSA_TOF4.d";

        [Test]
        public void MSGeneratorOnTextfileTest1()
        {
            Run run = new MSScanFromTextFileRun(imfMSScanTextfile);
            ResultCollection resultcollection = new ResultCollection(run);

            Task msgen = new GenericMSGenerator();
            msgen.Execute(resultcollection);

            Assert.AreEqual(2596, resultcollection.Run.XYData.Xvalues.Length);
        }


        [Test]
        public void MSGeneratoronTextFileTest2()
        {
            Run run = new MSScanFromTextFileRun(textFile1);

            Task msgen = new GenericMSGenerator();
            msgen.Execute(run.ResultCollection);

            Assert.AreEqual(19716, run.XYData.Xvalues.Length);

        }



        [Test]
        public void MSGeneratorOnIMFFileTest1()
        {
            Run run = new IMFRun(imfFilepath);
            run.CurrentScanSet = new ScanSet(233, 229, 237);

            run.GetMassSpectrum(run.CurrentScanSet, 200, 2000);

            Assert.AreEqual(9908, run.XYData.Xvalues.Length);
        }

        [Test]
        public void MSGeneratorOnIMFFileTest2()
        {
            Run run = new IMFRun(imfFilepath);
            run.CurrentScanSet = new ScanSet(233, 229, 237);

            Task msgen = new GenericMSGenerator();
            msgen.Execute(run.ResultCollection);

            Assert.AreEqual(9917, run.XYData.Xvalues.Length);
            Assert.AreEqual(1, run.GetMSLevel(run.CurrentScanSet.PrimaryScanNumber));
        }


        [Test]
        public void MSGeneratorOnXCaliburFileTest1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);
            run.CurrentScanSet = new ScanSet(6067);

            ResultCollection resultcollection = new ResultCollection(run);
            Task msgen = new GenericMSGenerator();
            msgen.Execute(resultcollection);

            Assert.AreEqual(27053, run.XYData.Xvalues.Length);
            Assert.AreEqual(1,run.GetMSLevel(run.CurrentScanSet.PrimaryScanNumber));
            Assert.AreEqual(3558996224, (long)run.CurrentScanSet.TICValue);
        }


        [Test]
        public void MSGeneratorOnXCaliburFileTest2()
        {
            Run run = new XCaliburRun(xcaliburTestfile, 6000, 7000);

            ScanSetCollection scansetCollection = new ScanSetCollection();
            for (int i = 6000; i < 6015; i++)
            {
                scansetCollection.ScanSetList.Add(new ScanSet(i));
            }

            ResultCollection results = new ResultCollection(run);
            foreach (ScanSet scanset in scansetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanset;
                Task msgen = new GenericMSGenerator();
                msgen.Execute(results);
            }

            Assert.AreEqual(0, results.ScanResultList.Count);   //Scan results are now created by the ScanResultUpdaterTask
        }

        [Test]
        public void MSGeneratorOnAgilentDFileTest1()
        {
            Run run = new AgilentD_Run(agilentDTestFile);

            run.CurrentScanSet = new ScanSet(25);

            Task msGen = new GenericMSGenerator();
            msGen.Execute(run.ResultCollection);

        }





    }
}
