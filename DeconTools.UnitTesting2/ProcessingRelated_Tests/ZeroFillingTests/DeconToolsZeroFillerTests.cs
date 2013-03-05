using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ZeroFillingTests
{
    [TestFixture]
    public class DeconToolsZeroFillerTests
    {
        //string imfStrangeOneFilepath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\IMF\7peptides_1uM_600_50_4t_114Vpp_0000.Accum_1_recal.imf";

        [Test]
        public void ZeroFillerTest1()
        {
            Run run = new MSScanFromTextFileRun(FileRefs.RawDataMSFiles.TextFileMS_std1);
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var zeroFiller = new DeconToolsZeroFiller(3);

            msgen.Execute(run.ResultCollection);
            int numZerosToFill = 3;
            var newZeroFilled = zeroFiller.ZeroFill(run.XYData.Xvalues, run.XYData.Yvalues, numZerosToFill);

            double lowerMZ = 625.50;
            double upperMZ = 626.18;

            run.XYData = run.XYData.TrimData(lowerMZ, upperMZ);
            newZeroFilled = newZeroFilled.TrimData(lowerMZ, upperMZ);
           
            Console.WriteLine("---------- before zerofilling ---------------");
            TestUtilities.DisplayXYValues(run.XYData);

            Console.WriteLine("---------- after zerofilling ---------------");
            TestUtilities.DisplayXYValues(newZeroFilled);

            Assert.IsTrue(newZeroFilled.Xvalues.Length > run.XYData.Xvalues.Length);
        }

        [Ignore("For comparing old and new")]
        [Test]
        public void ZeroFillerCompareNewAndOld()
        {

            Run run = new MSScanFromTextFileRun(FileRefs.RawDataMSFiles.TextFileMS_std1);
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var zeroFiller = new DeconToolsZeroFiller(3);

            msgen.Execute(run.ResultCollection);
            int numZerosToFill=3;
            var newZeroFilled=  zeroFiller.ZeroFill(run.XYData.Xvalues, run.XYData.Yvalues, numZerosToFill);
            var oldZeroFilled = zeroFiller.ZeroFillOld(run.XYData.Xvalues, run.XYData.Yvalues, numZerosToFill);


            double lowerMZ = 625.48;
            double upperMZ = 626.18;

            run.XYData = run.XYData.TrimData(lowerMZ, upperMZ);
            newZeroFilled = newZeroFilled.TrimData(lowerMZ, upperMZ);
            oldZeroFilled = oldZeroFilled.TrimData(lowerMZ, upperMZ);

            Console.WriteLine("---------- before zerofilling ---------------");
            TestUtilities.DisplayXYValues(run.XYData);

            Console.WriteLine("---------- after zerofilling ---------------");
            TestUtilities.DisplayXYValues(newZeroFilled);

            Console.WriteLine("---------- after zerofilling using DeconEngine ---------------");
            TestUtilities.DisplayXYValues(oldZeroFilled);


        }




    }
}
