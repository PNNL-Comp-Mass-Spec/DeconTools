using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;


namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class MSDataFromTextFileTests
    {
        string imfMSScanTextfile = "..\\..\\Testfiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt";

        string textFileContainingMultipleSpaceDelimiters = @"\\pnl\projects\MSSHARE\Yan\textfile\DN_2_1000.txt";


        string textFileContainingMulipleHeaderLines = @"F:\Gord\Data\Sang-Won\sampleIMS_Data_09103034.TXT";

        [Test]
        public void test1()
        {

            MSScanFromTextFileRun textfiledata = new MSScanFromTextFileRun(imfMSScanTextfile);
            textfiledata.GetMassSpectrum(new DeconTools.Backend.Core.ScanSet(0),200,2000);

            Assert.AreEqual(2596, textfiledata.XYData.Xvalues.Length);
            Assert.AreEqual(582.822204589844, Convert.ToDecimal(textfiledata.XYData.Xvalues[418]));
            Assert.AreEqual(2984, textfiledata.XYData.Yvalues[418]);
            Assert.AreEqual(1, textfiledata.GetNumMSScans());

        }

        //[Test]
        //public void readInTextFileContainingMultipleSpaceDelimitersTest1()
        //{
        //    Run textfiledata = new MSScanFromTextFileRun(textFileContainingMultipleSpaceDelimiters, DeconTools.Backend.Globals.XYDataFileType.Textfile, ' ');
        //    textfiledata.GetMassSpectrum(new DeconTools.Backend.Core.ScanSet(0), 200, 2000);

        //    TestUtilities.DisplayXYValues(textfiledata.XYData);

        //}


        [Test]
        public void readInTextFileContainingMultipleHeaderLinesTest1()
        {
            Run textfiledata = new MSScanFromTextFileRun(textFileContainingMulipleHeaderLines, '\t',1,2);
            textfiledata.GetMassSpectrum(new DeconTools.Backend.Core.ScanSet(0), 200, 2000);

            TestUtilities.DisplayXYValues(textfiledata.XYData);

        }


        //[Test]
        //public void readInTextFileContainingMultipleSpaceDelimitersTest2()
        //{
        //    Run textfiledata = new MSScanFromTextFileRun(textFileContainingMultipleSpaceDelimiters, DeconTools.Backend.Globals.XYDataFileType.Textfile, ' ');
            
        //    MSGeneratorFactory fact=new MSGeneratorFactory();
        //    Task msGen = fact.CreateMSGenerator(textfiledata.MSFileType);
        //    msGen.Execute(textfiledata.ResultCollection);
          
        //    TestUtilities.DisplayXYValues(textfiledata.XYData);

        //}



    }
}
