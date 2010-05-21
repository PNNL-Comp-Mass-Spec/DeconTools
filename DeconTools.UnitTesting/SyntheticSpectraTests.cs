using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class SyntheticSpectraTests
    {

        string sourceXYData = @"\\pnl\projects\MSSHARE\Scott\To Gordon S\output.txt";


        [Test]
        public void test1()
        {
            Run run = new MSScanFromTextFileRun(sourceXYData, DeconTools.Backend.Globals.XYDataFileType.Textfile, '\t');

            MSGeneratorFactory fact = new MSGeneratorFactory();
            Task msGen = fact.CreateMSGenerator(run.MSFileType);
            msGen.Execute(run.ResultCollection);

            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);
            peakDet.Execute(run.ResultCollection);

            HornDeconvolutor decon = new HornDeconvolutor();
            decon.Execute(run.ResultCollection);

            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);
            //TestUtilities.DisplayPeaks(run.PeakList);
        }


    }
}
