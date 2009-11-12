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
        string imfMSScanTextfile = "..\\..\\Testfiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt";
        string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";

        [Test]
        public void MSGeneratorOnTextfileTest1()
        {
            Run run = new MSScanFromTextFileRun(imfMSScanTextfile, Globals.XYDataFileType.Textfile);
            ResultCollection resultcollection = new ResultCollection(run);
            
            Task msgen = new GenericMSGenerator();
            msgen.Execute(resultcollection);

            Assert.AreEqual(2596, resultcollection.Run.XYData.Xvalues.Length);
        }

        [Test]
        public void MSGeneratorOnIMFFileTest1()
        {
            Run run = new IMFRun(imfFilepath);
            run.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            run.CurrentScanSet = new ScanSet(233,229, 237);

            run.GetMassSpectrum(run.CurrentScanSet,200,2000);

            Assert.AreEqual(9908, run.XYData.Xvalues.Length);

           
        }



    }
}
