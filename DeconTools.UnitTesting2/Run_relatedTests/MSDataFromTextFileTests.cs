using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using NUnit.Framework;


namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class MSDataFromTextFileTests
    {

        [Test]
        public void test1()
        {

            var textfiledata = new MSScanFromTextFileRun(FileRefs.RawDataMSFiles.TextFileMS_std1);
            var xyData=  textfiledata.GetMassSpectrum(new DeconTools.Backend.Core.ScanSet(0),0,2000);

            TestUtilities.DisplayXYValues(xyData);

            Assert.AreEqual(2596, xyData.Xvalues.Length);
            Assert.AreEqual(582.822204589844, Convert.ToDecimal(xyData.Xvalues[418]));
            Assert.AreEqual(2984, xyData.Yvalues[418]);
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
            Run run = new MSScanFromTextFileRun(FileRefs.RawDataMSFiles.TextFileMS_multipleDelimiters,' ');

            var generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            generator.Execute(run.ResultCollection);

            TestUtilities.DisplayXYValues(run.XYData);

        }



    }
}
