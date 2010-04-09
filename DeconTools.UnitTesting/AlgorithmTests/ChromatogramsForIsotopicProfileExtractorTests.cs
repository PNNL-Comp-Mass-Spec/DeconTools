using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend;
using System.Linq;

namespace DeconTools.UnitTesting.AlgorithmTests
{
    [TestFixture]
    public class ChromatogramsForIsotopicProfileExtractorTests
    {
        private string xcaliburPeakDataFile = "..\\..\\TestFiles\\XCaliburPeakDataScans5500-6500.txt";
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        [Test]
        public void test1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);
            MassTag mt = TestUtilities.GetMassTagStandard(1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburPeakDataFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.CurrentMassTag = mt;

            Task theorFeatureGen = new TomTheorFeatureGenerator();
            theorFeatureGen.Execute(run.ResultCollection);


            int numPeaksForExtractingChrom = 3;
            int toleranceInPPM = 25;

            IsotopicProfileMultiChromatogramExtractor multiChromExtractor = new IsotopicProfileMultiChromatogramExtractor(
                numPeaksForExtractingChrom, toleranceInPPM);

            List<int> ms1Levels = run.GetMSLevelScanValues();

            Dictionary<MSPeak, XYData> chromDataForIsotopicProfile = multiChromExtractor.GetChromatogramsForIsotopicProfilePeaks(run.ResultCollection.MSPeakResultList, run.CurrentMassTag.IsotopicProfile, true, ms1Levels);

            multiChromExtractor.SmoothChromatograms(chromDataForIsotopicProfile, new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11,11,2));
            XYData xydata1 = chromDataForIsotopicProfile.Values.First();
            XYData xydata2 = chromDataForIsotopicProfile.Values.ElementAt(1);
            XYData xydata3 = chromDataForIsotopicProfile.Values.ElementAt(2);


            

            TestUtilities.DisplayXYValues(xydata3);

        }


    }
}
