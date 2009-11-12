using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;


namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class MSDataFromTextFileTests
    {
        string imfMSScanTextfile = "..\\..\\Testfiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt";


        [Test]
        public void test1()
        {

            MSScanFromTextFileRun textfiledata = new MSScanFromTextFileRun(imfMSScanTextfile, DeconTools.Backend.Globals.XYDataFileType.Textfile);
            textfiledata.GetMassSpectrum(new DeconTools.Backend.Core.ScanSet(0),200,2000);

            Assert.AreEqual(2596, textfiledata.XYData.Xvalues.Length);
            Assert.AreEqual(582.822204589844, Convert.ToDecimal(textfiledata.XYData.Xvalues[418]));
            Assert.AreEqual(2984, textfiledata.XYData.Yvalues[418]);
            Assert.AreEqual(1, textfiledata.GetNumMSScans());
            Assert.AreEqual(DeconTools.Backend.Globals.XYDataFileType.Textfile, textfiledata.FileType);

        }


    }
}
