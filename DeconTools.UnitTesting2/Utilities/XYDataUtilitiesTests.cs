using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2.QuantificationTests;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Utilities
{
    [TestFixture]
    public class XYDataUtilitiesTests
    {
        [Test]
        public void Test1()
        {

            double featureFinderTol = 15;

             var n14n15Util = new N14N15TestingUtilities();
            //get MS
            var massSpectrum = n14n15Util.GetSpectrumAMTTag23140708_Z3_Sum3();  //this is the diff b/w previous test and this one


            var mt23140708 = n14n15Util.CreateMT23140708_Z3();


            var featureGen2 = new JoshTheorFeatureGenerator();
            featureGen2.GenerateTheorFeature(mt23140708);




            //get ms peaks
            var peakDet = new DeconToolsPeakDetectorV2(1.3, 2);
            var msPeakList = peakDet.FindPeaks(massSpectrum);

            var bff = new BasicTFF(featureFinderTol, requiresMonoPeak: false);
            var n14Profile = bff.FindMSFeature(msPeakList, mt23140708.IsotopicProfile);

            var theorXYData=   mt23140708.IsotopicProfile.GetTheoreticalIsotopicProfileXYData(n14Profile.GetFWHM());


            double obsMaxY = n14Profile.getMostIntensePeak().Height;


            for (var i = 0; i < theorXYData.Xvalues.Length; i++)
            {
                theorXYData.Yvalues[i] = theorXYData.Yvalues[i] * obsMaxY;
            }

            offsetDistribution(theorXYData, mt23140708.IsotopicProfile, n14Profile);


            var subtracted=   XYDataUtilities.SubtractXYData(massSpectrum, theorXYData, n14Profile.MonoPeakMZ - 1,
                                           n14Profile.MonoPeakMZ + 3, 0.01);



            var outputFileMS = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TextFile\massSpectrum1.txt";
            var outputFileMSSubtracted = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TextFile\massSpectrum1_withDataSubtracted.txt";

            TestUtilities.WriteToFile(massSpectrum, outputFileMS);
            TestUtilities.WriteToFile(subtracted, outputFileMSSubtracted);



            // XYDataUtilities.NormalizeXYData()



        }


        private void offsetDistribution(XYData theorXYData, IsotopicProfile theorIsotopicProfile, IsotopicProfile observedIsotopicProfile)
        {
            double offset;
            if (theorIsotopicProfile?.Peaklist == null || theorIsotopicProfile.Peaklist.Count == 0) return;

            var mostIntensePeak = theorIsotopicProfile.getMostIntensePeak();
            var indexOfMostIntensePeak = theorIsotopicProfile.Peaklist.IndexOf(mostIntensePeak);

            if (observedIsotopicProfile.Peaklist == null || observedIsotopicProfile.Peaklist.Count == 0) return;

            var enoughPeaksInTarget = (indexOfMostIntensePeak <= observedIsotopicProfile.Peaklist.Count - 1);

            if (enoughPeaksInTarget)
            {
                var targetPeak = observedIsotopicProfile.Peaklist[indexOfMostIntensePeak];
                offset = targetPeak.XValue - mostIntensePeak.XValue;
                //offset = observedIsotopicProfile.Peaklist[0].XValue - theorIsotopicProfile.Peaklist[0].XValue;   //want to test to see if Thrash is same as rapid

            }
            else
            {
                offset = observedIsotopicProfile.Peaklist[0].XValue - theorIsotopicProfile.Peaklist[0].XValue;
            }

            for (var i = 0; i < theorXYData.Xvalues.Length; i++)
            {
                theorXYData.Xvalues[i] = theorXYData.Xvalues[i] + offset;
            }

            foreach (var peak in theorIsotopicProfile.Peaklist)
            {
                peak.XValue = peak.XValue + offset;

            }


        }



    }
}
