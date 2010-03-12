using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using System.Linq;

namespace DeconTools.UnitTesting.ResultValidatorTests
{
    [TestFixture]
    public class LeftOfMonoPeakLookerTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        string uimfFile1 = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";


        [Test]
        public void xcaliburTest1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);
            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();
            Task suspPeakFlagger = new LeftOfMonoPeakLooker();

            run.CurrentScanSet = new ScanSet(6005);
            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            foreach (var result in run.ResultCollection.ResultList)
            {
                ((LeftOfMonoPeakLooker)suspPeakFlagger).CurrentResult = result;
                suspPeakFlagger.Execute(run.ResultCollection);

            }


            Assert.AreEqual(93, run.ResultCollection.ResultList.Count);

            List<IsosResult> flaggedResults = run.ResultCollection.ResultList.Where(p => p.Flags.Count > 0).ToList();
            
            Assert.AreEqual(2, flaggedResults.Count);

            TestUtilities.DisplayIsotopicProfileData(flaggedResults[0].IsotopicProfile);
            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(flaggedResults[1].IsotopicProfile);
        }

        [Test]
        public void uimfTest1()
        {
            UIMFRun run = new UIMFRun(uimfFile1);
            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();
            Task suspPeakFlagger = new LeftOfMonoPeakLooker();

            run.CurrentFrameSet = new FrameSet(501, 500, 502);
            run.CurrentScanSet = new ScanSet(250,246,254);
            
            
            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            foreach (var result in run.ResultCollection.ResultList)
            {
                ((LeftOfMonoPeakLooker)suspPeakFlagger).CurrentResult = result;
                suspPeakFlagger.Execute(run.ResultCollection);

            }


            Assert.AreEqual(38, run.ResultCollection.ResultList.Count);

            List<IsosResult> flaggedResults = run.ResultCollection.ResultList.Where(p => p.Flags.Count > 0).ToList();

            Assert.AreEqual(13, flaggedResults.Count);

            TestUtilities.DisplayIsotopicProfileData(flaggedResults[0].IsotopicProfile);
            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(flaggedResults[1].IsotopicProfile);


        }



    }
}
