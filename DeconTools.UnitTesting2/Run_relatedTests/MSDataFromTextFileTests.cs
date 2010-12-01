using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;


namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class MSDataFromTextFileTests
    {

        [Test]
        public void test1()
        {

            MSScanFromTextFileRun textfiledata = new MSScanFromTextFileRun(FileRefs.RawDataMSFiles.TextFileMS_std1);
            textfiledata.GetMassSpectrum(new DeconTools.Backend.Core.ScanSet(0),200,2000);

            Assert.AreEqual(2596, textfiledata.XYData.Xvalues.Length);
            Assert.AreEqual(582.822204589844, Convert.ToDecimal(textfiledata.XYData.Xvalues[418]));
            Assert.AreEqual(2984, textfiledata.XYData.Yvalues[418]);
            Assert.AreEqual(1, textfiledata.GetNumMSScans());

        }

        [Test]
        public void readInTextFileContainingMultipleSpaceDelimitersTest1()
        {
            Run textfiledata = new MSScanFromTextFileRun(FileRefs.RawDataMSFiles.TextFileMS_multipleDelimiters, ' ');
            textfiledata.GetMassSpectrum(new DeconTools.Backend.Core.ScanSet(0), 200, 2000);

            TestUtilities.DisplayXYValues(textfiledata.XYData);

        }


        [Test]
        public void readInTextFileContainingMultipleHeaderLinesTest1()
        {
            Run textfiledata = new MSScanFromTextFileRun(FileRefs.RawDataMSFiles.TextFileMS_multipleHeaderLines, '\t',1,2);
            textfiledata.GetMassSpectrum(new DeconTools.Backend.Core.ScanSet(0), 200, 2000);

            TestUtilities.DisplayXYValues(textfiledata.XYData);

        }


        [Test]
        public void readInTextFileContainingMultipleSpaceDelimitersTest2()
        {
            Run textfiledata = new MSScanFromTextFileRun(FileRefs.RawDataMSFiles.TextFileMS_multipleDelimiters,' ');

            MSGeneratorFactory fact = new MSGeneratorFactory();
            Task msGen = fact.CreateMSGenerator(textfiledata.MSFileType);
            msGen.Execute(textfiledata.ResultCollection);

            TestUtilities.DisplayXYValues(textfiledata.XYData);

        }



    }
}
